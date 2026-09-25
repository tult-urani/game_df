using System;
using LaMuralla.Core.Config;

namespace LaMuralla.Core.Match
{
    /// <summary>
    /// Mua / nâng / bán tướng. Là chỗ DUY NHẤT `SlotManager` (ai đứng đâu) gặp
    /// `EconomyService` (tiền).
    ///
    /// Vì sao cần lớp riêng thay vì để UI gọi thẳng cả hai: mỗi thao tác phải đổi
    /// CẢ HAI hoặc KHÔNG ĐỔI GÌ. Trừ tiền xong mới phát hiện ô đã đầy = mất tiền
    /// không có tướng. Đặt tướng xong mới phát hiện thiếu tiền = tướng miễn phí.
    /// Ở đây: hỏi hết mọi điều kiện TRƯỚC, chỉ chạm state khi chắc chắn cả hai đều
    /// thuận.
    ///
    /// Cột "không làm" ở docs/05 bảng §3 của module này: **"Biết UI"**.
    /// </summary>
    public sealed class UpgradeService
    {
        private readonly GameConfig _cfg;
        private readonly SlotManager _slots;
        private readonly EconomyService _economy;

        public UpgradeService(GameConfig cfg, SlotManager slots, EconomyService economy)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _slots = slots ?? throw new ArgumentNullException(nameof(slots));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
        }

        public TowerDef TowerById(string towerId)
        {
            foreach (TowerDef t in _cfg.Towers)
                if (t.Id == towerId) return t;
            throw new ConfigException($"không có tướng `{towerId}` trong towers.json");
        }

        /// <summary>
        /// Mua tướng cấp 1 đặt vào ô. `false` + lý do nếu không được — không ném:
        /// người chơi bấm nút không đủ tiền là chuyện thường.
        /// </summary>
        public bool TryBuy(string slotId, string towerId, out string? error)
        {
            TowerDef tower = TowerById(towerId);

            // Hỏi luật ô TRƯỚC khi đụng tiền. Ngược lại là trừ tiền rồi mới phát
            // hiện Dibu không đứng được ở ô sân → mất 140 Peso, không có tướng.
            if (!_slots.CanPlace(slotId, tower, out error)) return false;
            if (!_economy.TryPurchase(slotId, tower, out error)) return false;

            _slots.Place(slotId, tower);
            return true;
        }

        /// <summary>Nâng lên cấp kế tiếp. Giá lấy từ `levels[toLevel-1].cost` —
        /// đó là giá NÂNG, không phải giá tích luỹ.</summary>
        public bool TryUpgrade(string slotId, out string? error)
        {
            TowerInstance? t = _slots.At(slotId);
            if (t == null) { error = $"ô `{slotId}` chưa có tướng"; return false; }
            if (t.Level >= 3) { error = $"`{t.TowerId}` đã Lv3, hết cấp"; return false; }

            TowerDef def = TowerById(t.TowerId);
            int to = t.Level + 1;
            if (!_economy.TryUpgrade(slotId, def, to, out error)) return false;

            _slots.SetLevel(slotId, to);
            return true;
        }

        /// <summary>
        /// Bán tướng. Hoàn `sellRefundRatio` × TỔNG đã đầu tư vào ô.
        ///
        /// `balance_sim.py` chứng minh người chơi tối ưu PHẢI dùng nước này ở Act 3
        /// ("ô đặt hết, phải bán và xây lại" — waves.json). Thiếu nó, build kẹt ở
        /// Batigol maxed trong khi còn dư 2500 Peso.
        /// </summary>
        public int Sell(string slotId)
        {
            if (_slots.At(slotId) == null)
                throw new InvalidOperationException($"ô `{slotId}` không có gì để bán");

            int refund = _economy.Sell(slotId);   // ném nếu ví không biết ô này
            _slots.Remove(slotId);
            return refund;
        }

        /// <summary>Tiền hoàn nếu bán ô này ngay bây giờ, hoặc 0 nếu ô trống.
        /// UI cần con số này để vẽ nút "Bán +N" — cùng khuôn với NextUpgradeCost.</summary>
        public int SellRefund(string slotId) => _economy.PreviewSellRefund(slotId);

        /// <summary>Giá nâng lên cấp kế tiếp, hoặc `null` nếu đã Lv3 / ô trống.
        /// UI cần con số này để vẽ nút.</summary>
        public int? NextUpgradeCost(string slotId)
        {
            TowerInstance? t = _slots.At(slotId);
            if (t == null || t.Level >= 3) return null;
            return TowerById(t.TowerId).Levels[t.Level].Cost;
        }

        /// <summary>Chỉ số hiện hành của tướng ở ô. `Levels[Level - 1]` — lệch một
        /// đơn vị ở đây là tướng Lv1 dùng chỉ số Lv2, không crash, chỉ sai âm thầm.</summary>
        public TowerLevel StatsAt(string slotId)
        {
            TowerInstance t = _slots.At(slotId)
                ?? throw new InvalidOperationException($"ô `{slotId}` chưa có tướng");
            return TowerById(t.TowerId).Levels[t.Level - 1];
        }

        /// <summary>Kỹ năng ĐANG chạy ở ô đó. Theo B-01: đúng một, của cấp hiện tại.</summary>
        public AbilityDef? ActiveAbilityAt(string slotId)
        {
            TowerInstance? t = _slots.At(slotId);
            return t == null ? null : TowerById(t.TowerId).AbilityAt(t.Level);
        }
    }
}
