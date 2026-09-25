using System;
using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class SlotAndUpgradeTests
    {
        private static readonly GameConfig Cfg = RealConfigValidateTests.Load();

        private static (SlotManager, EconomyService, UpgradeService) New(int cash = 100_000)
        {
            var slots = new SlotManager(Cfg);
            var econ = new EconomyService(new EconomyDef
            {
                StartingCash = cash,
                GoalHealth = Cfg.Economy.GoalHealth,
                SlowCapPercent = Cfg.Economy.SlowCapPercent,
                SkipBonusPerSecond = Cfg.Economy.SkipBonusPerSecond,
                SellRefundRatio = Cfg.Economy.SellRefundRatio,
                FieldSlots = Cfg.Economy.FieldSlots,
                GoalkeeperSlots = Cfg.Economy.GoalkeeperSlots,
                RoundingMode = Cfg.Economy.RoundingMode,
            });
            return (slots, econ, new UpgradeService(Cfg, slots, econ));
        }

        [Fact]
        public void Doc_dung_11_o_san_va_1_o_thu_mon()
        {
            // VÒNG 21: f11 thêm lại (đảo vòng 20) → 11 ô sân + 1 ô thủ môn.
            var s = new SlotManager(Cfg);
            Assert.Equal(12, s.Slots.Count);
            Assert.Equal(11, s.Slots.Count(x => x.IsField));
            Assert.Single(s.Slots, x => x.Type == "goalkeeper");
        }

        /// <summary>🔴 Luật HAI CHIỀU (docs/02 §55, docs/05 §3). Chỉ ép một chiều
        /// thì Batigol đứng được ở gk01 — nó nằm sau cầu môn, tầm 1.0, gần như
        /// không canh được gì, và người chơi mất 120 Peso vì một luật thiếu.</summary>
        [Fact]
        public void Dibu_chi_o_o_thu_mon_VA_tuong_san_khong_dung_khung_thanh()
        {
            var (_, _, up) = New();

            Assert.False(up.TryBuy("f01", "dibu", out string? e1));
            Assert.Contains("goalkeeper", e1);

            Assert.False(up.TryBuy("gk01", "batigol", out string? e2));
            Assert.Contains("field", e2);

            Assert.True(up.TryBuy("gk01", "dibu", out _));
            Assert.True(up.TryBuy("f01", "batigol", out _));
        }

        /// <summary>docs/02 §55: "Dibu là ngoại lệ duy nhất: tối đa 1 con".
        /// Chỉ có 1 ô thủ môn nên luật này khó vi phạm — nhưng nó phải được ép ở
        /// tầng luật, không phải nhờ may mắn về số ô.</summary>
        [Fact]
        public void Dibu_toi_da_1_con()
        {
            var (slots, _, up) = New();
            TowerDef dibu = up.TowerById("dibu");
            Assert.Equal(1, dibu.MaxInstances);

            Assert.True(up.TryBuy("gk01", "dibu", out _));
            Assert.Equal(1, slots.InstancesOf("dibu"));
            Assert.False(slots.CanPlace("gk01", dibu, out _));
        }

        [Fact]
        public void Tuong_san_khong_gioi_han_so_con()
        {
            var (slots, _, up) = New();
            Assert.Equal(0, up.TowerById("batigol").MaxInstances);   // 0 = vô hạn
            foreach (string s in new[] { "f01", "f02", "f03", "f04", "f05" })
                Assert.True(up.TryBuy(s, "batigol", out _));
            Assert.Equal(5, slots.InstancesOf("batigol"));
        }

        [Fact]
        public void O_da_day_thi_khong_dat_duoc()
        {
            var (_, _, up) = New();
            Assert.True(up.TryBuy("f01", "batigol", out _));
            Assert.False(up.TryBuy("f01", "la_pulga", out string? e));
            Assert.Contains("đã có", e);
        }

        /// <summary>
        /// 🔴 Mua phải là ALL-OR-NOTHING. Không đủ tiền → KHÔNG được đặt tướng.
        /// Đặt trước rồi mới hỏi tiền = tướng miễn phí.
        /// </summary>
        [Fact]
        public void Thieu_tien_thi_khong_dat_tuong()
        {
            var (slots, econ, up) = New(cash: 50);   // Batigol Lv1 = 120

            Assert.False(up.TryBuy("f01", "batigol", out string? e));

            Assert.Contains("cần 120", e);
            Assert.True(slots.IsEmpty("f01"));   // KHÔNG có tướng
            Assert.Equal(50, econ.Balance);      // KHÔNG mất tiền
        }

        /// <summary>
        /// 🔴 Chiều ngược lại: luật ô hỏng → KHÔNG được trừ tiền.
        /// Trừ tiền rồi mới phát hiện Dibu không đứng được ở ô sân = mất 140 Peso,
        /// không có tướng. Đây là lý do UpgradeService tồn tại.
        /// </summary>
        [Fact]
        public void Luat_o_hong_thi_khong_tru_tien()
        {
            var (_, econ, up) = New(cash: 1000);

            Assert.False(up.TryBuy("f01", "dibu", out _));

            Assert.Equal(1000, econ.Balance);   // ví nguyên vẹn
        }

        [Fact]
        public void Nang_cap_tang_dung_1_cap_va_dung_gia()
        {
            var (slots, econ, up) = New(cash: 1000);
            up.TryBuy("f01", "batigol", out _);          // −120 → 880
            Assert.Equal(880, econ.Balance);

            Assert.Equal(96, up.NextUpgradeCost("f01")); // levels[1].cost
            Assert.True(up.TryUpgrade("f01", out _));

            Assert.Equal(2, slots.At("f01")!.Level);
            Assert.Equal(784, econ.Balance);             // 880 − 96
            Assert.Equal(192, up.NextUpgradeCost("f01"));
        }

        [Fact]
        public void Lv3_la_het_cap()
        {
            var (_, _, up) = New();
            up.TryBuy("f01", "batigol", out _);
            up.TryUpgrade("f01", out _);
            up.TryUpgrade("f01", out _);

            Assert.False(up.TryUpgrade("f01", out string? e));
            Assert.Contains("Lv3", e);
            Assert.Null(up.NextUpgradeCost("f01"));
        }

        [Fact]
        public void Thieu_tien_nang_cap_thi_khong_doi_cap()
        {
            var (slots, econ, up) = New(cash: 150);
            up.TryBuy("f01", "batigol", out _);          // −120 → 30
            Assert.False(up.TryUpgrade("f01", out _));   // cần 96
            Assert.Equal(1, slots.At("f01")!.Level);
            Assert.Equal(30, econ.Balance);
        }

        /// <summary>Bán hoàn theo TỔNG đã đầu tư, không phải giá mua. Nước đi mà
        /// balance_sim chứng minh người chơi tối ưu phải dùng ở Act 3.</summary>
        [Fact]
        public void Ban_hoan_theo_tong_da_dau_tu()
        {
            var (slots, econ, up) = New(cash: 1000);
            up.TryBuy("f01", "batigol", out _);   // 120
            up.TryUpgrade("f01", out _);          // +96  → tổng vốn 216
            int before = econ.Balance;

            int refund = up.Sell("f01");

            Assert.Equal(Round.HalfUp(216 * Cfg.Economy.SellRefundRatio), refund);
            Assert.Equal(before + refund, econ.Balance);
            Assert.True(slots.IsEmpty("f01"));
            Assert.Equal(0, slots.InstancesOf("batigol"));
        }

        [Fact]
        public void Ban_roi_xay_lai_o_do_duoc()
        {
            var (_, _, up) = New(cash: 1000);
            up.TryBuy("f01", "batigol", out _);
            up.Sell("f01");
            Assert.True(up.TryBuy("f01", "la_pulga", out _));
        }

        /// <summary>B-01: ô Lv1 chạy kỹ năng unlockLevel=1, Lv3 chạy unlockLevel=3.
        /// Đúng một, không cộng dồn.</summary>
        [Fact]
        public void Ky_nang_dang_chay_doi_theo_cap()
        {
            var (_, _, up) = New();
            up.TryBuy("f01", "batigol", out _);
            Assert.Equal("cu_dam", up.ActiveAbilityAt("f01")!.Id);

            up.TryUpgrade("f01", out _);
            Assert.Equal("cu_da", up.ActiveAbilityAt("f01")!.Id);

            up.TryUpgrade("f01", out _);
            Assert.Equal("cu_dap", up.ActiveAbilityAt("f01")!.Id);
        }

        /// <summary>`01` §8 **FM-06**: "Tướng nâng cấp đúng lúc mục tiêu chết → kỹ năng
        /// reset cooldown". Theo B-01 lý do còn mạnh hơn: cấp mới chạy kỹ năng KHÁC
        /// HẲN, nên cooldown của kỹ năng cũ vô nghĩa.</summary>
        [Fact]
        public void Nang_cap_reset_cooldown_va_dem_don()
        {
            var (slots, _, up) = New();
            up.TryBuy("f01", "la_pulga", out _);
            TowerInstance t = slots.At("f01")!;
            t.CooldownRemaining = 7.5;
            t.AttacksFired = 4;

            up.TryUpgrade("f01", out _);

            Assert.Equal(0, t.CooldownRemaining);
            Assert.Equal(0, t.AttacksFired);
        }

        [Fact]
        public void Chi_so_lay_dung_cap_khong_lech_mot_don_vi()
        {
            var (_, _, up) = New();
            up.TryBuy("f01", "batigol", out _);
            Assert.Equal(20, up.StatsAt("f01").Damage);    // Lv1
            up.TryUpgrade("f01", out _);
            Assert.Equal(30, up.StatsAt("f01").Damage);   // Lv2
            up.TryUpgrade("f01", out _);
            Assert.Equal(45, up.StatsAt("f01").Damage);   // Lv3
        }

        [Fact]
        public void Khong_ha_cap()
        {
            var (slots, _, up) = New();
            up.TryBuy("f01", "batigol", out _);
            up.TryUpgrade("f01", out _);
            Assert.Throws<InvalidOperationException>(() => slots.SetLevel("f01", 1));
        }

        [Fact]
        public void O_khong_ton_tai_thi_bao_loi()
        {
            var (_, _, up) = New();
            Assert.False(up.TryBuy("f99", "batigol", out string? e));
            Assert.Contains("f99", e);
        }
    }
}
