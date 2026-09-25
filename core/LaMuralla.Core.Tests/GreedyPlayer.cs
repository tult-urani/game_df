using System;
using System.Collections.Generic;
using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;

namespace LaMuralla.Core.Tests
{
    /// <summary>
    /// Người chơi tự động, tham lam — dùng cho bài kiểm "map này có chơi được không".
    ///
    /// 🔴 VÌ SAO LỚP NÀY TỒN TẠI. Bản trước nằm ngay trong `MatchControllerTests` và
    /// GÕ CỨNG danh sách ô (`crowd = f01,f11,f07,...`). Với một map thì tạm được; với
    /// 11 map bố cục khác nhau thì mỗi map lại phải sửa tay một fixture, và cái đó
    /// chính là món nợ vừa trả xong ở vòng 22 (`docs/09-MAPS-REVIEW.md` R3).
    ///
    /// Nay ô được CHỌN TỪ HÌNH HỌC: đo `chord` — độ dài đường nằm trong tầm — cho từng
    /// ô, rồi xếp hạng. Thêm map mới không phải đụng file này.
    ///
    /// KHÔNG phải AI giỏi: nó không tính synergy, không bán-xây-lại, không né boss.
    /// Nó chỉ đủ giỏi để câu hỏi "map có chơi được không" có nghĩa.
    /// </summary>
    internal sealed class GreedyPlayer
    {
        // Tầm tham chiếu để phân loại ô. Không phải số bịa: đúng tầm Lv1 của hai
        // tướng mà lớp này dùng — D10S 1.2 và La Pulga 1.4.
        private const double CrowdRange = 1.2;
        private const double ReachRange = 1.4;

        private readonly List<string> _crowd;   // ô D10S với tới ngay Lv1, xếp theo chord giảm dần
        private readonly List<string> _reach;   // ô chỉ La Pulga (tầm xa hơn) với tới

        private readonly List<string> _laneIds = new();
        private readonly Dictionary<string, double> _need = new();                       // tỉ lệ quân mỗi tuyến
        private readonly Dictionary<string, Dictionary<string, double>> _chordCrowd = new(); // ô → tuyến → chord@1.2
        private readonly Dictionary<string, Dictionary<string, double>> _chordReach = new(); // ô → tuyến → chord@1.4

        internal IReadOnlyList<string> CrowdSlots => _crowd;
        internal IReadOnlyList<string> ReachSlots => _reach;

        internal GreedyPlayer(GameConfig cfg)
        {
            // 🔴 MỌI TUYẾN, không chỉ tuyến đầu. Chord của một ô là TỔNG qua các tuyến
            // nó với tới — ô canh được hai tuyến đáng giá gấp đôi, và bỏ qua điều đó
            // thì người chơi tự động đánh giá sai mọi map đa tuyến (nó sẽ bỏ trống
            // đúng những ô mạnh nhất).
            var paths = new List<EnemyPath>();
            foreach (LaneDef ln in cfg.Path.Lanes) paths.Add(new EnemyPath(ln.Waypoints.ToList()));

            _laneIds = cfg.Path.Lanes.Select(l => l.Id).ToList();

            // Nhu cầu từng tuyến = số quân đi qua nó cả trận. Một tuyến nhận 60% quân
            // đáng được 60% hoả lực; chia đều là sai khi bảng wave không chia đều.
            _need = _laneIds.ToDictionary(id => id, _ => 0.0);
            foreach (WaveDef w in cfg.Waves)
                foreach (SpawnGroup g in w.Spawns)
                    if (_need.ContainsKey(g.Lane)) _need[g.Lane] += g.Count;
            double needTot = _need.Values.Sum();
            if (needTot <= 0) foreach (string id in _laneIds) _need[id] = 1;
            else foreach (string id in _laneIds) _need[id] = Math.Max(1e-6, _need[id] / needTot);

            var scored = new List<(string Id, double Crowd, double Reach)>();
            foreach (SlotDef s in cfg.Path.Slots)
            {
                if (!s.IsField) continue;
                var perCrowd = new Dictionary<string, double>();
                var perReach = new Dictionary<string, double>();
                for (int i = 0; i < paths.Count; i++)
                {
                    perCrowd[_laneIds[i]] = Chord(paths[i], s.Position, CrowdRange);
                    perReach[_laneIds[i]] = Chord(paths[i], s.Position, ReachRange);
                }
                _chordCrowd[s.Id] = perCrowd;
                _chordReach[s.Id] = perReach;
                scored.Add((s.Id, perCrowd.Values.Sum(), perReach.Values.Sum()));
            }

            // 🔴 NGƯỠNG, KHÔNG PHẢI "> 0". Ô mà D10S Lv1 chỉ quét được một mẩu đường
            // ngắn hơn chính tầm của nó là ô nhìn vào GÓC CUA — 240 Peso mua một khẩu
            // gần như không bắn được ai. Đo trên m00: `f06` chord@1.2 = 0.97 nên bị
            // luật "> 0" xếp nhầm vào D10S, và bài kiểm 20 wave THUA ở W6; đưa nó về
            // cho La Pulga (tầm 1.8 ⇒ chord 3.00) thì thắng.
            //
            // Ngưỡng = đúng bằng tầm: đường quét được phải dài ít nhất bằng tầm tướng.
            // Không phải số bịa, và không phụ thuộc map.
            _crowd = scored.Where(x => x.Crowd >= CrowdRange)
                           .OrderByDescending(x => x.Reach).Select(x => x.Id).ToList();

            // Còn lại về La Pulga: tầm xa hơn nên ô lệch đường vẫn dùng được, và đây
            // là hoả lực ĐƠN MỤC TIÊU duy nhất — thứ bắt buộc để hạ boss.
            _reach = scored.Where(x => x.Crowd < CrowdRange)
                           .OrderByDescending(x => x.Reach).Select(x => x.Id).ToList();
        }

        /// <summary>Độ dài đường nằm trong bán kính `r` quanh `p`, lấy mẫu đều.</summary>
        private static double Chord(EnemyPath path, Vec2 p, double r)
        {
            const int n = 1000;
            double step = path.Length / n, inside = 0;
            for (int i = 0; i <= n; i++)
                if (Vec2.Distance(path.PositionAt(i * step), p) <= r) inside += step;
            return inside;
        }

        /// <summary>
        /// Tiêu hết tiền tiêu được. Vòng lặp MỖI VÒNG MỘT NƯỚC, không phải "nâng hết
        /// rồi mua hết": nâng D10S lên Lv2 rẻ hơn mua ô mới (192 &lt; 240) mà vừa ×1.6
        /// dame vừa nới bán kính nổ 1.2 → 1.7, nên phải xen kẽ mới ra được build đúng.
        /// </summary>
        internal void Spend(MatchController m)
        {
            bool moved = true;
            while (moved)
            {
                moved = false;

                // 1. Nâng D10S lên Lv2 — nước đi lãi nhất trong game.
                foreach (string s in _crowd)
                    if (m.Slots.At(s)?.Level == 1 && m.Upgrades.TryUpgrade(s, out _)) moved = true;

                // 2. Mở thêm MỘT ô đám đông rồi quay lại bước 1.
                //
                // 🔴 CHỌN THEO TUYẾN YẾU NHẤT, không theo tổng chord. Bản trước xếp ô
                // theo TỔNG chord qua mọi tuyến, nên trên map luân phiên (M03: wave lẻ
                // ra `L1`, wave chẵn ra `L2`) nó mua liền hai ô chỉ canh `L1` rồi để
                // `L2` trống trơn — và thua ở W2. Đó là điểm mù của con bot, không phải
                // độ khó của map; đo bằng nó là đo nhầm thứ.
                //
                // Luật: tối đa hoá mức phủ của tuyến đang THIẾU nhất, trong đó "phủ"
                // được chia cho tỉ lệ quân của tuyến (`_need`).
                string? pick = ChonOTotNhat(m, _crowd, _chordCrowd);
                if (pick != null && m.Upgrades.TryBuy(pick, "d10s", out _)) { moved = true; }

                // 3. Ô xa: La Pulga, nâng hết cấp — Lv1 tầm 1.4 nhiều khi chưa với tới,
                //    Lv2 (1.6) / Lv3 (1.8) mới biến chúng thành ô có ích. Cũng là hoả
                //    lực ĐƠN MỤC TIÊU duy nhất, thứ cần để hạ boss.
                foreach (string s in _reach)
                    if (m.Slots.At(s) is { Level: < 3 } && m.Upgrades.TryUpgrade(s, out _)) moved = true;

                string? pickR = ChonOTotNhat(m, _reach, _chordReach);
                if (pickR != null && m.Upgrades.TryBuy(pickR, "la_pulga", out _)) { moved = true; }

                // 4. Dibu SAU CÙNG — nó không bắn, mua sớm là bỏ 140 Peso không sát
                //    thương vào giai đoạn tiền khan nhất.
                if (m.Slots.Towers.Count(t => t.Level >= 2) >= 4 &&
                    m.Slots.IsEmpty("gk01") && m.Upgrades.TryBuy("gk01", "dibu", out _)) moved = true;
            }
        }

        /// <summary>
        /// Ô trống đáng mua nhất, hoặc `null` nếu hết ô.
        ///
        /// Điểm = Σ (đường canh được trên tuyến × tỉ lệ quân đi tuyến đó).
        ///
        /// 🔴 VÌ SAO KHÔNG XẾP THEO TỔNG CHORD như bản trước: trên map đa tuyến, tổng
        /// chord coi 1 unit đường ở tuyến vắng bằng 1 unit ở tuyến đông. M03 cho quân
        /// ra LUÂN PHIÊN từng wave, M10 mở dần ba tuyến — ở cả hai, "tổng chord" xếp
        /// hạng sai hẳn. Đã thử luật mạnh hơn (ép cân tuyến yếu nhất) và nó TỆ HƠN:
        /// dàn mỏng sớm làm hoả lực loãng, M03 thua sớm hơn 2 wave. Cân tuyến là việc
        /// của bảng wave, không phải của thứ tự mua.
        /// </summary>
        private string? ChonOTotNhat(MatchController m, List<string> ung,
                                         Dictionary<string, Dictionary<string, double>> chord)
        {
            // Trọng số tuyến = một nửa nhu cầu CẢ TRẬN, một nửa nhu cầu WAVE SAU.
            //
            // 🔴 Vì sao có nửa "wave sau": HUD báo trước wave tới ra cửa nào
            // (`MatchController.LanesOfWave`, yêu cầu bắt buộc của map luân phiên —
            // `docs/maps/M03` §3). Người chơi ĐỌC được thông tin đó và xây theo. Một
            // con bot mua mù trên map luân phiên sẽ cần gấp đôi tiền so với người
            // thật, và khi đó nó đo điểm mù của chính nó chứ không đo map.
            //
            // Chỉ MỘT NỬA vì tướng xây ra là vĩnh viễn: bám sát wave kế tiếp mà bỏ
            // qua cả trận là cái bẫy ngược lại.
            var w = new Dictionary<string, double>(_need);
            var gan = new Dictionary<string, double>();
            foreach (string id in _laneIds) gan[id] = 0;
            double tong = 0;
            // BA wave tới, không phải một. Nhìn đúng một wave là CẬN THỊ: ở wave 0
            // nó dồn hết tiền vào tuyến của W1 rồi trắng tuyến kia ở W2 (đo trên m08:
            // W1 ra 8 con toàn tuyến `W`, W2 ra 6/6). Người chơi thật không mù đến thế —
            // họ thấy nhãn `CỬA:` và cũng nhớ map khi chơi lại.
            for (int d = 1; d <= 3; d++)
                foreach (LaneLoad l in m.LanesOfWave(m.Wave + d))
                    if (gan.ContainsKey(l.Lane)) { gan[l.Lane] += l.Count; tong += l.Count; }

            if (tong > 0)
                foreach (string id in _laneIds) w[id] = 0.5 * w[id] + 0.5 * gan[id] / tong;

            string? best = null;
            double bestScore = double.NegativeInfinity;
            foreach (string s in ung)
            {
                if (!m.Slots.IsEmpty(s)) continue;
                var per = chord[s];
                // Đường canh được nhân với TỈ LỆ QUÂN đi qua tuyến đó: một khẩu canh
                // tuyến vắng thì ngồi không nửa trận.
                double score = 0;
                foreach (string id in _laneIds) score += per[id] * w[id];
                if (score > bestScore) { bestScore = score; best = s; }
            }
            return best;
        }

        /// <summary>Chạy trọn 20 wave. Trả về (kết cục, wave cuối, máu còn, đỉnh quân đồng thời).</summary>
        internal static (MatchPhase Phase, int Wave, int Goal, int PeakLive, int Earned)
            PlayFullMatch(GameConfig cfg, int seed = 7, bool useRewardedRecovery = false)
        {
            var m = new MatchController(cfg, seed);
            var p = new GreedyPlayer(cfg);
            int peak = 0;

            for (int guard = 0; guard < 400_000; guard++)
            {
                if (m.Phase == MatchPhase.Lost)
                {
                    if (useRewardedRecovery && m.RewardedRecovery.TryContinue()) continue;
                    break;
                }
                if (m.Phase == MatchPhase.Won) break;
                if (m.Phase == MatchPhase.Preparing)
                {
                    if (useRewardedRecovery) m.RewardedRecovery.TryHeal();
                    p.Spend(m);
                    if (m.Wave == 0) m.StartNextWave(); else m.SkipRest();
                    continue;
                }
                m.Tick(1.0 / 60);
                if (m.Enemies.Count > peak) peak = m.Enemies.Count;
            }
            return (m.Phase, m.Wave, m.Goal.Current, peak, m.Economy.LifetimeEarned);
        }
    }
}
