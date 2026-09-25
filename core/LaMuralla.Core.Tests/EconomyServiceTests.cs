using System;
using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    public class EconomyServiceTests
    {
        private readonly ITestOutputHelper _out;
        public EconomyServiceTests(ITestOutputHelper o) => _out = o;

        private static GameConfig Cfg() => RealConfigValidateTests.Load();
        private static EconomyService Eco(GameConfig c) => new EconomyService(c.Economy);
        private static TowerDef T(GameConfig c, string id) => c.Towers.Single(t => t.Id == id);

        [Fact]
        public void Bat_dau_bang_dung_startingCash_cua_config()
        {
            GameConfig c = Cfg();
            Assert.Equal(700, Eco(c).Balance);
        }

        [Fact]
        public void Khong_mua_duoc_cap_manh_nhat_ngay_tu_dau_la_CO_Y()
        {
            // 🔵 VÒNG 22 — bài này giờ canh đúng cái TÊN NÓ NÓI, không hơn.
            //
            // Bản cũ canh "không đủ tiền mua tướng thứ hai" (550 vs Pulga 300 + D10S
            // 280 = 580). Cả hai vế đều đã đổi: D10S còn 240 và startingCash lên 700
            // (đo từ engine — xem economy.json → _startingCashNote), nên mở màn hai
            // tướng GIỜ LÀ CHUYỆN BÌNH THƯỜNG và không còn gì để canh ở đó.
            //
            // Ràng buộc CÒN SỐNG là ràng buộc trong tên hàm: không ai lên thẳng CẤP
            // MẠNH NHẤT ngay từ đầu. La Pulga full = 300 + 240 + 480 = 1020 > 700.
            GameConfig c = Cfg();
            EconomyService e = Eco(c);

            Assert.True(e.TryPurchase("f01", T(c, "la_pulga"), out _));      // −300 → 400
            Assert.True(e.TryUpgrade("f01", T(c, "la_pulga"), 2, out _));    // −240 → 160
            Assert.False(e.TryUpgrade("f01", T(c, "la_pulga"), 3, out string? err));  // cần 480
            _out.WriteLine(err);
            Assert.Equal(160, e.Balance);
        }

        [Fact]
        public void Tru_qua_so_du_thi_nem_chu_khong_am_ví()
        {
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            e.TryPurchase("f01", T(c, "la_pulga"), out _);   // 700 − 300 → 400
            e.TryPurchase("f02", T(c, "la_pulga"), out _);   // 400 − 300 → 100
            Assert.False(e.TryPurchase("f03", T(c, "la_pulga"), out _));
            Assert.Equal(100, e.Balance);   // không đổi, KHÔNG âm
        }

        [Fact]
        public void Mua_vao_o_da_co_tuong_thi_tu_choi()
        {
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            Assert.True(e.TryPurchase("f01", T(c, "batigol"), out _));
            Assert.False(e.TryPurchase("f01", T(c, "batigol"), out string? err));
            Assert.Contains("đã có tướng", err);
        }

        [Fact]
        public void Ban_hoan_60_phan_tram_TONG_von_chu_khong_phai_gia_mua()
        {
            // Batigol 120 + Lv2 96 + Lv3 192 = 408 vốn → 60% = 244.8 → half_up = 245
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            TowerDef b = T(c, "batigol");
            e.TryPurchase("f01", b, out _);
            e.TryUpgrade("f01", b, 2, out _);
            e.TryUpgrade("f01", b, 3, out _);
            Assert.Equal(408, e.InvestedIn("f01"));
            Assert.Equal(245, e.Sell("f01"));
            Assert.Equal(0, e.InvestedIn("f01"));
        }

        [Fact]
        public void Ban_KHONG_tinh_vao_tien_kiem_ca_doi()
        {
            // Bán là hoàn vốn, không phải nguồn thu. Tính vào sẽ thổi phồng con số
            // đối chiếu với economy.json → maxBuildCost.lifetimeCashExpected.
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            int before = e.LifetimeEarned;
            e.TryPurchase("f01", T(c, "batigol"), out _);
            e.Sell("f01");
            Assert.Equal(before, e.LifetimeEarned);
        }

        [Fact]
        public void Ban_o_trong_thi_nem()
        {
            GameConfig c = Cfg();
            Assert.Throws<InvalidOperationException>(() => Eco(c).Sell("f09"));
        }

        [Fact]
        public void Thuong_ha_quan_dung_bang_bang_can_bang()
        {
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            EnemyDef tambor = c.Enemies.Single(x => x.Id == "tambor");
            e.EarnKill(tambor, 20, c);                 // 24 × 2.2 = 52.8 → half_up 53
            Assert.Equal(700 + 53, e.Balance);
        }

        [Fact]
        public void Thuong_boss_ghi_thang_khong_nhan_he_so_act()
        {
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            BossDef b = c.Bosses.Single(x => x.Id == "o_capitao");
            e.EarnBossKill(b, 5);
            e.EarnBossKill(b, 10);
            e.EarnBossKill(b, 15);
            e.EarnBossKill(b, 15);
            e.EarnBossKill(b, 20);
            e.EarnBossKill(b, 20);
            // 40 + 80 + 2×110 + 2×180 = 700: tăng số boss nhưng không bơm
            // thêm tiền cả trận, nếu không chính mốc Hard lại tài trợ sức mạnh.
            Assert.Equal(700 + 700, e.Balance);
        }

        [Fact]
        public void Thuong_boss_o_wave_khong_co_boss_thi_nem()
        {
            GameConfig c = Cfg();
            Assert.Throws<ArgumentException>(() => Eco(c).EarnBossKill(c.Bosses[0], 6));
        }

        [Fact]
        public void Clear_wave_dung_cong_thuc_va_W20_la_ngoai_le()
        {
            Assert.Equal(20, EconomyService.WaveClearBonus(1));
            Assert.Equal(110, EconomyService.WaveClearBonus(19));   // 20 + 5×18
            Assert.Equal(150, EconomyService.WaveClearBonus(20));   // override
        }

        [Fact]
        public void Gia_sua_cau_mon_tang_cap_so_nhan_chu_khong_co_dinh()
        {
            // Giá cố định 200 → người chơi cố tình thả quân lọt lưới sớm để dồn tiền
            // xây tướng rồi mua máu bù sau → kinh tế vỡ. Xem docs/04 §1.
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            e.EarnKill(c.Enemies[0], 1, c);
            int[] want = { 200, 300, 450, 675, 1013 };
            for (int i = 0; i < want.Length; i++)
            {
                Assert.Equal(want[i], e.NextRepairCost());
                while (!e.CanAfford(e.NextRepairCost())) e.EarnWaveClear(20);
                Assert.True(e.TryBuyGoalRepair(out _));
            }
        }

        [Fact]
        public void So_cai_ghi_lai_moi_dong_tien_de_truy_khi_vi_sai()
        {
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            e.TryPurchase("f01", T(c, "batigol"), out _);
            e.EarnWaveClear(1);
            foreach (CashEntry x in e.Ledger) _out.WriteLine(x.ToString());

            Assert.Equal(3, e.Ledger.Count);   // starting + purchase + waveclear
            Assert.Equal(e.Balance, e.Ledger[^1].BalanceAfter);
            // Số dư cuối phải bằng tổng mọi biến động — sổ cái không được nói dối.
            Assert.Equal(e.Balance, e.Ledger.Sum(x => x.Delta));
        }

        [Fact]
        public void Skip_nghi_toi_da_24_moi_wave()
        {
            GameConfig c = Cfg();
            EconomyService e = Eco(c);
            e.EarnSkipRest(8.0);            // 8 × 3
            Assert.Equal(700 + 24, e.Balance);
        }
    }
}
