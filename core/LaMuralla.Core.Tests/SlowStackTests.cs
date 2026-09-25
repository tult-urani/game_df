using System;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    public class SlowStackTests
    {
        private readonly ITestOutputHelper _out;
        public SlowStackTests(ITestOutputHelper o) => _out = o;

        private const double Cap = 70;      // economy.json → slowCapPercent
        private const double BossResist = 75;   // enemies.json → o_capitao

        [Fact]
        public void Lay_MAX_chu_khong_cong_don()
        {
            // Cộng dồn thì 3 Árbitro = 150% = quân đi lùi.
            var s = new SlowStack(Cap);
            s.Apply("a", 50, SlowScope.Permanent);
            s.Apply("b", 60, SlowScope.Permanent);
            s.Apply("c", 30, SlowScope.Permanent);
            Assert.Equal(60, s.EffectiveSlowPercent(0));
        }

        [Fact]
        public void Cap_cat_o_70_phan_tram()
        {
            var s = new SlowStack(Cap);
            s.Apply("the_do", 95, SlowScope.Permanent);
            Assert.Equal(70, s.EffectiveSlowPercent(0));
        }

        [Fact]
        public void FM14_thu_tu_phep_tinh_TAO_RA_khac_biet_that()
        {
            // Đây là test quan trọng nhất file này: chứng minh thứ tự KHÔNG phải
            // chuyện phong cách. Hai nguồn vượt cap + boss kháng 75%.
            var s = new SlowStack(Cap);
            s.Apply("the_do", 70, SlowScope.Permanent);
            s.Apply("coi", 60, SlowScope.Permanent);

            double dung = s.EffectiveSlowPercent(BossResist);        // MAX→cap→kháng
            double sai = Math.Min((70 + 60) * (1 - BossResist / 100), Cap);  // cộng dồn→kháng→cap

            _out.WriteLine($"đúng (MAX→cap→kháng) : {dung:0.##}%");
            _out.WriteLine($"sai  (cộng→kháng→cap): {sai:0.##}%");
            Assert.Equal(17.5, dung);
            Assert.Equal(32.5, sai);
            Assert.True(sai / dung > 1.8, "nếu hai cách ra gần nhau thì test này vô dụng");
        }

        [Fact]
        public void Boss_khang_75_thi_the_do_chi_con_17_phay_5()
        {
            // Con số này chống đỡ cả thiết kế boss: vòng 3 kháng 50% làm Árbitro
            // đưa boss W20 từ 1.18× lên 1.68× → phải nâng lên 75%.
            var s = new SlowStack(Cap);
            s.Apply("the_do", 70, SlowScope.Permanent);
            Assert.Equal(17.5, s.EffectiveSlowPercent(BossResist));
            Assert.Equal(0.825, s.SpeedMultiplier(BossResist), 10);
        }

        [Fact]
        public void Quan_thuong_khang_0_thi_the_do_chậm_du_70()
        {
            var s = new SlowStack(Cap);
            s.Apply("the_do", 70, SlowScope.Permanent);
            Assert.Equal(0.30, s.SpeedMultiplier(0), 10);
        }

        [Fact]
        public void Timed_het_han_thi_tu_go()
        {
            var s = new SlowStack(Cap);
            s.Apply("ban_tay", 50, SlowScope.Timed, durationSec: 3.0);
            s.Tick(2.9);
            Assert.Equal(50, s.EffectiveSlowPercent(0));
            s.Tick(0.2);
            Assert.Equal(0, s.EffectiveSlowPercent(0));
            Assert.Equal(0, s.SourceCount);
        }

        [Fact]
        public void Tick_khong_dung_toi_Permanent_va_InRange()
        {
            var s = new SlowStack(Cap);
            s.Apply("the_vang", 50, SlowScope.Permanent);
            s.Apply("aura", 30, SlowScope.InRange);
            s.Tick(9999);
            Assert.Equal(2, s.SourceCount);
            Assert.Equal(50, s.EffectiveSlowPercent(0));
        }

        [Fact]
        public void Ra_khoi_tam_thi_go_nguon_InRange()
        {
            var s = new SlowStack(Cap);
            s.Apply("aura_dibu", 30, SlowScope.InRange);
            Assert.Equal(30, s.EffectiveSlowPercent(0));
            s.Remove("aura_dibu");
            Assert.Equal(0, s.EffectiveSlowPercent(0));
        }

        [Fact]
        public void Cung_nguon_ap_lai_thi_ghi_de_chu_khong_chong_them()
        {
            var s = new SlowStack(Cap);
            s.Apply("the_vang", 50, SlowScope.Permanent);
            s.Apply("the_vang", 50, SlowScope.Permanent);
            Assert.Equal(1, s.SourceCount);
            Assert.Equal(50, s.EffectiveSlowPercent(0));
        }

        [Fact]
        public void Timed_thieu_duration_thi_nem_ngay()
        {
            var s = new SlowStack(Cap);
            Assert.Throws<ArgumentException>(() => s.Apply("x", 50, SlowScope.Timed));
        }

        [Fact]
        public void Khang_100_thi_mien_nhiem_hoan_toan()
        {
            var s = new SlowStack(Cap);
            s.Apply("the_do", 70, SlowScope.Permanent);
            Assert.Equal(0, s.EffectiveSlowPercent(100));
            Assert.Equal(1.0, s.SpeedMultiplier(100));
        }

        [Fact]
        public void Khong_co_nguon_nao_thi_di_binh_thuong()
        {
            Assert.Equal(1.0, new SlowStack(Cap).SpeedMultiplier(0));
        }
    }
}
