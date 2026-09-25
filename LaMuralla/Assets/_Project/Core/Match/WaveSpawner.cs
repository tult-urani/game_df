using System;
using System.Collections.Generic;
using LaMuralla.Core.Config;

namespace LaMuralla.Core.Match
{
    /// <summary>Một con quân sẽ ra sân tại <see cref="TimeSec"/> giây tính từ lúc
    /// wave bắt đầu.</summary>
    public readonly struct ScheduledSpawn
    {
        public string EnemyId { get; }
        public double TimeSec { get; }
        /// <summary>Máu đã nhân hệ số wave — người gọi không phải tính lại.</summary>
        public int Hp { get; }
        public bool IsBoss { get; }

        /// <summary>Tuyến con này chạy. Map một tuyến thì luôn là `L1`.</summary>
        public string LaneId { get; }

        public ScheduledSpawn(string enemyId, double timeSec, int hp, bool isBoss, string laneId)
        {
            EnemyId = enemyId; TimeSec = timeSec; Hp = hp; IsBoss = isBoss; LaneId = laneId;
        }

        public override string ToString() =>
            $"{TimeSec,6:0.0}s {LaneId,-3} {EnemyId,-10} {Hp,6} hp{(IsBoss ? " BOSS" : "")}";
    }

    /// <summary>
    /// Đọc `waves.json` → lịch spawn.
    ///
    /// Sinh TOÀN BỘ lịch một lần thay vì tick từng frame. Hai lý do:
    ///   · tất định — test kiểm được từng mốc thời gian, không phải mô phỏng 40 giây
    ///   · lịch là dữ liệu, không phải trạng thái — lớp vỏ Unity chỉ việc đi dọc nó
    ///
    /// Cột "không làm" ở docs/05 bảng §3: **"Biết quân chết thế nào"**. Lớp này
    /// không cầm DamageSystem, không biết ai còn sống.
    /// </summary>
    public sealed class WaveSpawner
    {
        private readonly GameConfig _cfg;

        public WaveSpawner(GameConfig cfg) => _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));

        /// <summary>
        /// Lịch spawn của một wave, sắp theo thời gian tăng dần.
        ///
        /// 🔴 BOSS RA CUỐI CÙNG — `waves.json._bossSpawnNote`: "Boss spawn SAU khi
        /// toàn bộ spawns của wave ra hết. Đây là giả định nền của mô hình hoả lực
        /// tập trung (docs/04 §3.3) — đổi cái này là đổi cả mô hình."
        ///
        /// Mô hình đó giả định boss đi MỘT MÌNH: không ai chia hoả lực, D10S mất
        /// hệ số ×n mục tiêu, v_boss = 0.4. Cho boss ra giữa đám đông là biến nó
        /// thành "wave thường nhưng dồn máu vào một túi", và cả cơ chế mất nghĩa.
        /// </summary>
        public IReadOnlyList<ScheduledSpawn> Schedule(int wave)
        {
            WaveDef w = WaveAt(wave);
            var outp = new List<ScheduledSpawn>();

            // 🔵 ĐA TUYẾN. Mỗi tuyến có ĐỒNG HỒ RIÊNG, bắt đầu từ `delaySec` của nó —
            // đó là cách "ba tuyến lệch nhịp" được diễn tả. Hai tuyến cùng `delaySec = 0`
            // thì chạy ĐỒNG THỜI; wave lẻ chỉ khai tuyến A, wave chẵn chỉ khai tuyến B
            // thì thành LUÂN PHIÊN. Cả ba kiểu ra quân đều là DỮ LIỆU, không cần code riêng.
            var byLane = new Dictionary<string, List<SpawnGroup>>(StringComparer.Ordinal);
            var laneOrder = new List<string>();
            foreach (SpawnGroup g in w.Spawns)
            {
                if (g.Count <= 0) continue;
                if (!byLane.TryGetValue(g.Lane, out List<SpawnGroup>? list))
                {
                    byLane[g.Lane] = list = new List<SpawnGroup>();
                    laneOrder.Add(g.Lane);
                }
                list.Add(g);
            }
            // Sắp tên tuyến để lịch KHÔNG phụ thuộc thứ tự khoá trong Dictionary.
            laneOrder.Sort(StringComparer.Ordinal);

            double lastSpawn = 0;
            foreach (string lane in laneOrder)
            {
                List<SpawnGroup> groups = byLane[lane];
                double t = groups[0].DelaySec;   // luật 22 ép mọi nhóm cùng tuyến khai giống nhau
                foreach (string id in InterleaveOrder(groups))
                {
                    EnemyDef e = EnemyById(id);
                    outp.Add(new ScheduledSpawn(id, t, _cfg.HpOf(e, wave), false, lane));
                    t += _cfg.SpawnIntervalSec;
                }
                if (t > lastSpawn) lastSpawn = t;
            }

            // Boss ra SAU KHI MỌI TUYẾN nhả hết quân — xem chú thích trên. Nhiều boss
            // thì ra CÙNG LÚC ở các tuyến khác nhau (map cuối).
            foreach (BossSpawn bs in w.Bosses)
            {
                BossDef b = BossById(bs.Id);
                outp.Add(new ScheduledSpawn(b.Id, lastSpawn, BossHpAt(b, wave), true, bs.Lane));
            }

            outp.Sort((a, b) => a.TimeSec.CompareTo(b.TimeSec));
            return outp;
        }

        /// <summary>
        /// Thứ tự ra sân của quân thường: XEN KẼ ĐỀU theo tỉ lệ, tất định.
        ///
        /// ⚠️ LUẬT NÀY KHÔNG CÓ TRONG DOCS. `waves.json` chỉ khai số lượng mỗi loại
        /// (`{adepto: 14, tifoso: 16, tambor: 7}`), không khai thứ tự. `_bossSpawnNote`
        /// chốt boss ra cuối, nhưng quân thường thì im lặng — nên đây là quyết định
        /// của người viết code, không phải của người thiết kế.
        ///
        /// Chọn xen kẽ vì:
        ///   · Mô hình cân bằng (`04`) tính `S = (n−1) × interval × v̄` — coi wave là
        ///     một DÒNG ĐỀU. Gom theo loại thì 7 con tambor (máu ×2) dồn cuối wave
        ///     thành một cục, và mô hình nói về một trận đấu khác.
        ///   · Tất định → bảng cân bằng tái hiện được. Xáo ngẫu nhiên thì cùng một
        ///     wave khó dễ khác nhau mỗi lần chơi, và không ai debug được.
        ///
        /// 🔴 CHƯA ĐO TRÊN MÁY. Gom theo loại tạo nhịp "sóng" có thể hay hơn về cảm
        /// giác. Đổi ở `waves.json → spawnOrder` (chưa cài) sau khi chơi thử.
        ///
        /// Cách xen kẽ: mỗi loại giữ một "nợ" tăng đều; con nào nợ nhiều nhất ra
        /// trước. Với {6 adepto, 3 tifoso} ra: A T A A T A A T — không phải AAAAAATTT.
        /// </summary>
        private static IEnumerable<string> InterleaveOrder(List<SpawnGroup> groups)
        {
            var ids = new List<string>();
            var remaining = new Dictionary<string, int>();
            int total = 0;
            foreach (SpawnGroup g in groups)
            {
                if (g.Count <= 0) continue;
                // Cùng loại khai làm hai nhóm trên cùng tuyến → cộng dồn, không ghi đè.
                if (remaining.ContainsKey(g.Enemy)) remaining[g.Enemy] += g.Count;
                else { ids.Add(g.Enemy); remaining[g.Enemy] = g.Count; }
                total += g.Count;
            }
            if (total == 0) yield break;

            // Sắp id để thứ tự KHÔNG phụ thuộc thứ tự khoá trong Dictionary —
            // nếu không thì lịch spawn đổi theo cách JSON được parse.
            ids.Sort(StringComparer.Ordinal);

            var credit = new Dictionary<string, double>();
            foreach (string id in ids) credit[id] = 0;

            for (int n = 0; n < total; n++)
            {
                string? pick = null;
                double best = double.NegativeInfinity;
                foreach (string id in ids)
                {
                    if (remaining[id] <= 0) continue;
                    credit[id] += remaining[id] / (double)total;
                    if (credit[id] > best) { best = credit[id]; pick = id; }
                }
                if (pick == null) yield break;
                credit[pick] -= 1.0;
                remaining[pick]--;
                yield return pick;
            }
        }

        /// <summary>Máu boss lấy THẲNG từ `enemies.json → appearances`, KHÔNG nhân
        /// hpScaling. Boss là số gõ tay có chủ đích (W10 = 2100, W20 = 6700) — nhân
        /// thêm hệ số wave nữa là nhân đôi độ khó mà không ai định.</summary>
        private static int BossHpAt(BossDef b, int wave)
        {
            foreach (BossAppearance a in b.Appearances)
                if (a.Wave == wave) return a.Hp;
            throw new ConfigException($"boss `{b.Id}` không có appearance ở wave {wave}");
        }

        /// <summary>Tổng số con của wave, kể cả boss.</summary>
        public int CountAt(int wave) => Schedule(wave).Count;

        /// <summary>Wave kéo dài bao lâu tính từ con đầu tới con cuối RA SÂN — chưa
        /// tính thời gian chúng đi hết đường.</summary>
        public double SpawnDurationOf(int wave)
        {
            IReadOnlyList<ScheduledSpawn> s = Schedule(wave);
            return s.Count == 0 ? 0 : s[s.Count - 1].TimeSec;
        }

        private WaveDef WaveAt(int wave)
        {
            foreach (WaveDef w in _cfg.Waves)
                if (w.Wave == wave) return w;
            throw new ConfigException($"không có wave {wave} trong waves.json");
        }

        private EnemyDef EnemyById(string id)
        {
            foreach (EnemyDef e in _cfg.Enemies)
                if (e.Id == id) return e;
            throw new ConfigException($"không có quân `{id}` trong enemies.json");
        }

        private BossDef BossById(string id)
        {
            foreach (BossDef b in _cfg.Bosses)
                if (b.Id == id) return b;
            throw new ConfigException($"không có boss `{id}` trong enemies.json");
        }
    }
}
