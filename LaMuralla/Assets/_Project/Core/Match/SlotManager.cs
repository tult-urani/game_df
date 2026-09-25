using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LaMuralla.Core.Config;

// State chạy của tướng (cooldown, đếm đòn) để `internal set`: chỉ core được sửa,
// UI và lớp vỏ Unity chỉ được đọc. Nhưng test cần dựng tình huống "tướng đang còn
// 7.5s cooldown" mà chưa có API công khai nào tua thời gian — API đó thuộc hệ
// chiến đấu, chưa viết.
//
// Mở cho ĐÚNG assembly test, thay vì nới thành `public`. Nới public thì bất biến
// mất vĩnh viễn để đổi lấy sự tiện lợi của một test.
//
// Unity không có assembly tên này (test sống ở core/, ngoài Assets/) — thuộc tính
// trỏ tới assembly không tồn tại là vô hại, chỉ là metadata.
[assembly: InternalsVisibleTo("LaMuralla.Core.Tests")]

namespace LaMuralla.Core.Match
{
    /// <summary>Một tướng đang đứng trên sân.</summary>
    public sealed class TowerInstance
    {
        public string SlotId { get; }
        public string TowerId { get; }

        /// <summary>1..3. Đây là cấp NGƯỜI CHƠI thấy; chỉ số nằm ở
        /// `TowerDef.Levels[Level - 1]`. Lệch một đơn vị ở đây là tướng Lv1 dùng
        /// chỉ số Lv2 — không crash, chỉ sai âm thầm.</summary>
        public int Level { get; internal set; }

        /// <summary>Còn bao nhiêu giây nữa kỹ năng `cooldown` sẵn sàng.</summary>
        public double CooldownRemaining { get; internal set; }

        /// <summary>Đếm đòn đã bắn — cho kỹ năng `every_nth_attack`
        /// (`la_pulga.so_10` n=4).</summary>
        public int AttacksFired { get; internal set; }

        /// <summary>Còn bao nhiêu giây nữa được bắn đòn thường tiếp theo.</summary>
        public double AttackCooldownRemaining { get; internal set; }

        /// <summary>Kỹ năng `cooldown` có THỜI LƯỢNG còn lại bao nhiêu giây.
        /// Chỉ `la_pulga.solo_run` dùng (`durationSec: 4.0`) — nó bật ×2 sát thương
        /// trong 4s, khác với các kỹ năng cooldown khác nổ tức thì rồi tắt.</summary>
        public double AbilityActiveRemaining { get; internal set; }

        /// <summary>
        /// Kỹ năng đã nạp xong, phát bắn TIẾP THEO được tăng sức — rồi tắt.
        ///
        /// Tách khỏi <see cref="AbilityActiveRemaining"/> vì hai cơ chế khác nhau:
        /// `solo_run` khai `durationSec: 4.0` → bật theo THỜI GIAN, mọi phát trong
        /// 4s đều ×2. `cu_vo_le` KHÔNG khai duration → nó là MỘT PHÁT, ×2.5 rồi hết.
        /// Dùng chung một trường thì hoặc cú vô lê kéo dài vô hạn, hoặc solo_run
        /// chỉ còn một phát.
        /// </summary>
        public bool AbilityCharged { get; internal set; }

        /// <summary>Đã cản thành công bao nhiêu quả — `dibu.nguoi_hung_luan_luu`
        /// hồi 1 máu cầu môn mỗi 3 lần cản.</summary>
        public int BlocksMade { get; internal set; }

        internal TowerInstance(string slotId, string towerId, int level)
        {
            SlotId = slotId; TowerId = towerId; Level = level;
        }

        public override string ToString() => $"{TowerId}@{SlotId} Lv{Level}";
    }

    /// <summary>
    /// 11 ô sân + 1 ô thủ môn. Ai đứng đâu, và ai KHÔNG được đứng đâu.
    ///
    /// Cột "không làm" ở docs/05 bảng §3: **"Mua tướng"**. Lớp này không đụng tiền
    /// — nó chỉ biết ô trống hay đầy và luật ô. `UpgradeService` mới là chỗ nối nó
    /// với `EconomyService`.
    ///
    /// Danh sách ô đến từ `path.json`, KHÔNG gõ tay: `economy.json` khai
    /// `fieldSlots: 11` và `goalkeeperSlots: 1`, và constructor đối chiếu hai nguồn.
    /// Hai file nói khác nhau thì ném ngay lúc dựng, không phải lúc người chơi bấm.
    /// </summary>
    public sealed class SlotManager
    {
        private readonly Dictionary<string, SlotDef> _slots = new Dictionary<string, SlotDef>();
        private readonly Dictionary<string, TowerInstance> _occupied = new Dictionary<string, TowerInstance>();
        private readonly Dictionary<string, int> _instanceCount = new Dictionary<string, int>();

        public IReadOnlyCollection<SlotDef> Slots => _slots.Values;
        public IReadOnlyCollection<TowerInstance> Towers => _occupied.Values;
        public int OccupiedCount => _occupied.Count;

        public SlotManager(GameConfig cfg)
        {
            if (cfg == null) throw new ArgumentNullException(nameof(cfg));

            int field = 0, keeper = 0;
            foreach (SlotDef s in cfg.Path.Slots)
            {
                _slots[s.Id] = s;
                if (s.IsField) field++; else keeper++;
            }

            // path.json và economy.json phải khớp. Lệch nhau thì mô hình cân bằng
            // (tính theo 11 ô) và bản đồ thật nói về hai trận khác nhau.
            if (field != cfg.Economy.FieldSlots)
                throw new ConfigException(
                    $"path.json có {field} ô sân nhưng economy.json khai {cfg.Economy.FieldSlots}");
            if (keeper != cfg.Economy.GoalkeeperSlots)
                throw new ConfigException(
                    $"path.json có {keeper} ô thủ môn nhưng economy.json khai {cfg.Economy.GoalkeeperSlots}");
        }

        public SlotDef SlotOf(string slotId) =>
            _slots.TryGetValue(slotId, out SlotDef? s)
                ? s
                : throw new ArgumentException($"không có ô `{slotId}` trong path.json", nameof(slotId));

        public TowerInstance? At(string slotId) =>
            _occupied.TryGetValue(slotId, out TowerInstance? t) ? t : null;

        public bool IsEmpty(string slotId) => _slots.ContainsKey(slotId) && !_occupied.ContainsKey(slotId);

        /// <summary>Số con của một loại tướng đang trên sân. Dùng để ép `maxInstances`.</summary>
        public int InstancesOf(string towerId) =>
            _instanceCount.TryGetValue(towerId, out int n) ? n : 0;

        /// <summary>
        /// Đặt được không, và vì sao không. Trả `false` + lý do thay vì ném: người
        /// chơi bấm vào ô đã đầy là chuyện thường, không phải lỗi lập trình.
        /// </summary>
        public bool CanPlace(string slotId, TowerDef tower, out string? error)
        {
            if (!_slots.TryGetValue(slotId, out SlotDef? slot))
            {
                error = $"không có ô `{slotId}`";
                return false;
            }
            if (_occupied.ContainsKey(slotId))
            {
                error = $"ô `{slotId}` đã có {_occupied[slotId].TowerId}";
                return false;
            }

            // 🔴 Luật "Dibu chỉ ở ô thủ môn" (docs/02 §55, docs/05 §3). Đây là luật
            // HAI CHIỀU: thủ môn không ra sân, VÀ tướng sân không đứng khung thành.
            // Chỉ ép một chiều thì Batigol đứng ở gk01 — nó nằm sau cầu môn, tầm 1.0,
            // gần như không canh được gì, và người chơi mất 120 Peso vì một luật thiếu.
            if (slot.Type != tower.SlotType)
            {
                error = $"`{tower.Id}` là tướng `{tower.SlotType}`, ô `{slotId}` là `{slot.Type}`";
                return false;
            }

            if (tower.MaxInstances > 0 && InstancesOf(tower.Id) >= tower.MaxInstances)
            {
                error = $"`{tower.Id}` tối đa {tower.MaxInstances} con, đang có {InstancesOf(tower.Id)}";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>Đặt tướng ở cấp 1. Ném nếu <see cref="CanPlace"/> nói không —
        /// người gọi phải hỏi trước.</summary>
        public TowerInstance Place(string slotId, TowerDef tower)
        {
            if (!CanPlace(slotId, tower, out string? err))
                throw new InvalidOperationException($"không đặt được: {err}");

            var t = new TowerInstance(slotId, tower.Id, 1);
            _occupied[slotId] = t;
            _instanceCount[tower.Id] = InstancesOf(tower.Id) + 1;
            return t;
        }

        /// <summary>Gỡ tướng khỏi ô (bán). Trả về tướng đã gỡ.</summary>
        public TowerInstance Remove(string slotId)
        {
            if (!_occupied.TryGetValue(slotId, out TowerInstance? t))
                throw new InvalidOperationException($"ô `{slotId}` không có gì để gỡ");

            _occupied.Remove(slotId);
            _instanceCount[t.TowerId] = InstancesOf(t.TowerId) - 1;
            return t;
        }

        /// <summary>Nâng cấp lên `toLevel`. KHÔNG đụng tiền — `UpgradeService` lo.</summary>
        public void SetLevel(string slotId, int toLevel)
        {
            if (toLevel < 1 || toLevel > 3)
                throw new ArgumentOutOfRangeException(nameof(toLevel), toLevel, "cấp phải trong 1..3");
            TowerInstance t = At(slotId) ?? throw new InvalidOperationException($"ô `{slotId}` chưa có tướng");
            if (toLevel <= t.Level)
                throw new InvalidOperationException(
                    $"`{slotId}` đang Lv{t.Level}, không nâng xuống Lv{toLevel} — " +
                    "hạ cấp không tồn tại trong game này, muốn đổi thì bán rồi xây lại");

            // Nâng cấp reset cooldown kỹ năng: theo B-01, cấp mới chạy một kỹ năng
            // KHÁC hẳn, nên cooldown của kỹ năng cũ vô nghĩa. `01` §8 **FM-06** cũng
            // đòi "kỹ năng reset cooldown" khi nâng đúng lúc mục tiêu chết.
            t.Level = toLevel;
            t.CooldownRemaining = 0;
            t.AttacksFired = 0;
        }
    }
}
