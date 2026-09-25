using System;
using System.Collections.Generic;
using LaMuralla.Core.Config;

namespace LaMuralla.Core.Match
{
    public enum CashReason
    {
        Starting, Kill, BossKill, WaveClear, SkipRest, Sell, Purchase, Upgrade, GoalRepair,
    }

    public readonly struct CashEntry
    {
        public CashReason Reason { get; }
        public int Delta { get; }
        public int BalanceAfter { get; }
        public string Detail { get; }

        public CashEntry(CashReason reason, int delta, int balanceAfter, string detail)
        {
            Reason = reason; Delta = delta; BalanceAfter = balanceAfter; Detail = detail;
        }

        public override string ToString() => $"{Reason,-11} {Delta,+6} → {BalanceAfter,6}  {Detail}";
    }

    /// <summary>
    /// 🔴 NGUỒN CHÂN LÝ DUY NHẤT CỦA VÍ — luật bất khả xâm phạm #1, docs/05 §3.
    ///
    /// Không module nào được `wallet += x`. Trong TD, tiền được cộng từ ~5 chỗ
    /// (hạ quân, clear wave, skip, bán, hoàn nâng cấp). Nếu mỗi chỗ tự cộng thì
    /// khi ví sai 12 Peso ở wave 14, bạn mất một buổi chiều để tìm chỗ nào.
    /// Một cổng vào duy nhất = một breakpoint duy nhất.
    ///
    /// Giữ luôn sổ cái (<see cref="Ledger"/>) vì lý do đó: ví sai thì đọc sổ,
    /// không phải đoán.
    /// </summary>
    public sealed class EconomyService
    {
        private readonly List<CashEntry> _ledger = new List<CashEntry>();
        private readonly Dictionary<string, int> _investedBySlot = new Dictionary<string, int>();
        private readonly EconomyDef _def;
        private int _repairsBought;

        public int Balance { get; private set; }
        public IReadOnlyList<CashEntry> Ledger => _ledger;

        /// <summary>Tổng tiền đã KIẾM cả trận (không trừ chi). Dùng để đối chiếu
        /// với `lifetimeCashExpected` ở economy.json.</summary>
        public int LifetimeEarned { get; private set; }

        public EconomyService(EconomyDef def)
        {
            _def = def ?? throw new ArgumentNullException(nameof(def));
            Balance = def.StartingCash;
            LifetimeEarned = def.StartingCash;
            _ledger.Add(new CashEntry(CashReason.Starting, def.StartingCash, Balance, "tiền khởi đầu"));
        }

        private void Credit(int amount, CashReason reason, string detail)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "cộng tiền phải ≥ 0");
            Balance += amount;
            LifetimeEarned += amount;
            _ledger.Add(new CashEntry(reason, amount, Balance, detail));
        }

        private void Debit(int amount, CashReason reason, string detail)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), amount, "trừ tiền phải ≥ 0");
            if (amount > Balance)
                throw new InvalidOperationException(
                    $"trừ {amount} nhưng ví chỉ có {Balance} — người gọi phải hỏi CanAfford trước");
            Balance -= amount;
            _ledger.Add(new CashEntry(reason, -amount, Balance, detail));
        }

        public bool CanAfford(int cost) => cost <= Balance;

        // ── Tiền vào ────────────────────────────────────────────────────────

        /// <summary>Thưởng hạ quân. Gọi ĐÚNG MỘT LẦN cho mỗi con, và chỉ từ
        /// DamageSystem — nó là nơi duy nhất quyết định ai gây đòn kết liễu
        /// (luật bất khả xâm phạm #2). Xem docs/01 §8 FM-07: hai tướng cùng bắn
        /// phát cuối → cộng tiền hai lần → kinh tế vỡ sau 10 wave.</summary>
        public void EarnKill(EnemyDef enemy, int wave, GameConfig cfg) =>
            Credit(cfg.BountyOf(enemy, wave), CashReason.Kill, $"{enemy.Id} W{wave}");

        /// <summary>Thưởng hạ boss. Ghi thẳng ở enemies.json, KHÔNG nhân hệ số act.</summary>
        public void EarnBossKill(BossDef boss, int wave)
        {
            foreach (BossAppearance ap in boss.Appearances)
                if (ap.Wave == wave) { Credit(ap.Bounty, CashReason.BossKill, $"{boss.Id} W{wave}"); return; }
            throw new ArgumentException($"boss `{boss.Id}` không có appearance ở wave {wave}", nameof(wave));
        }

        /// <summary>`20 + 5 × (wave − 1)`, riêng W20 = 150. Xem docs/04 §1.</summary>
        public void EarnWaveClear(int wave) =>
            Credit(WaveClearBonus(wave), CashReason.WaveClear, $"clear W{wave}");

        public static int WaveClearBonus(int wave) => wave == 20 ? 150 : 20 + 5 * (wave - 1);

        /// <summary>Skip thời gian nghỉ: `giây_còn_lại × 3`.</summary>
        public void EarnSkipRest(double secondsRemaining)
        {
            if (secondsRemaining < 0) throw new ArgumentOutOfRangeException(nameof(secondsRemaining));
            Credit(Round.HalfUp(secondsRemaining * _def.SkipBonusPerSecond),
                   CashReason.SkipRest, $"skip {secondsRemaining:0.#}s");
        }

        // ── Tiền ra ─────────────────────────────────────────────────────────

        /// <summary>Mua tướng vào ô. Trả false nếu không đủ tiền — KHÔNG ném, vì
        /// người chơi bấm nút không đủ tiền là chuyện thường, không phải lỗi.</summary>
        public bool TryPurchase(string slotId, TowerDef tower, out string? error)
        {
            int cost = tower.Levels[0].Cost;
            if (_investedBySlot.ContainsKey(slotId)) { error = $"ô `{slotId}` đã có tướng"; return false; }
            if (!CanAfford(cost)) { error = $"cần {cost}, có {Balance}"; return false; }

            Debit(cost, CashReason.Purchase, $"{tower.Id} → {slotId}");
            _investedBySlot[slotId] = cost;
            error = null;
            return true;
        }

        /// <summary>Nâng cấp lên `toLevel` (2 hoặc 3). Cộng dồn vào vốn của ô —
        /// tiền bán sau này tính trên TỔNG đã đầu tư, không phải trên giá mua.</summary>
        public bool TryUpgrade(string slotId, TowerDef tower, int toLevel, out string? error)
        {
            if (toLevel < 2 || toLevel > 3) throw new ArgumentOutOfRangeException(nameof(toLevel), toLevel, "cấp 2 hoặc 3");
            if (!_investedBySlot.ContainsKey(slotId)) { error = $"ô `{slotId}` chưa có tướng"; return false; }

            int cost = tower.Levels[toLevel - 1].Cost;
            if (!CanAfford(cost)) { error = $"cần {cost}, có {Balance}"; return false; }

            Debit(cost, CashReason.Upgrade, $"{tower.Id} {slotId} → Lv{toLevel}");
            _investedBySlot[slotId] += cost;
            error = null;
            return true;
        }

        /// <summary>Bán: hoàn `sellRefundRatio` × TỔNG đã đầu tư vào ô đó.
        /// Đây là nước đi mà balance_sim.py chứng minh người chơi tối ưu PHẢI dùng
        /// ở Act 3 ("ô đặt hết, phải bán và xây lại").</summary>
        public int Sell(string slotId)
        {
            if (!_investedBySlot.TryGetValue(slotId, out int invested))
                throw new InvalidOperationException($"ô `{slotId}` không có gì để bán");

            int refund = PreviewSellRefund(slotId);
            _investedBySlot.Remove(slotId);
            // Bán KHÔNG phải nguồn tiền mới — nó là hoàn vốn. Tính vào LifetimeEarned
            // sẽ thổi phồng con số đối chiếu với economy.json → maxBuildCost.
            Balance += refund;
            _ledger.Add(new CashEntry(CashReason.Sell, refund, Balance, $"{slotId} (vốn {invested})"));
            return refund;
        }

        public int InvestedIn(string slotId) => _investedBySlot.TryGetValue(slotId, out int v) ? v : 0;

        /// <summary>Tiền hoàn NẾU bán ô này bây giờ — KHÔNG đổi ví. UI vẽ nút "Bán +N"
        /// cần con số này trước khi người chơi bấm. Là nguồn CHÂN LÝ của công thức
        /// hoàn: `Sell` gọi lại chính nó để hai chỗ không bao giờ lệch.</summary>
        public int PreviewSellRefund(string slotId) =>
            _investedBySlot.TryGetValue(slotId, out int invested)
                ? Round.HalfUp(invested * _def.SellRefundRatio)
                : 0;

        /// <summary>Giá sửa cầu môn lần tới: 200 × 1.5^(số lần đã sửa).
        /// Tăng cấp số nhân để nó là phao cứu sinh, KHÔNG phải chiến lược — giá cố
        /// định thì người chơi cố tình thả quân lọt lưới sớm để dồn tiền xây tướng
        /// rồi mua máu bù sau. Xem docs/04 §1.</summary>
        public int NextRepairCost() => Round.HalfUp(200 * Math.Pow(1.5, _repairsBought));

        public bool TryBuyGoalRepair(out string? error)
        {
            int cost = NextRepairCost();
            if (!CanAfford(cost)) { error = $"cần {cost}, có {Balance}"; return false; }
            Debit(cost, CashReason.GoalRepair, $"lần thứ {_repairsBought + 1}");
            _repairsBought++;
            error = null;
            return true;
        }
    }
}
