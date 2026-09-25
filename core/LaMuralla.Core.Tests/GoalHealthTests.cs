using System;
using System.Collections.Generic;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class GoalHealthTests
    {
        private static GoalHealth New(int max = 20) => new(max);

        [Fact]
        public void Khoi_dau_day_mau_va_clean_sheet()
        {
            GoalHealth g = New();
            Assert.Equal(20, g.Current);
            Assert.True(g.CleanSheet);
            Assert.False(g.IsLost);
        }

        [Fact]
        public void Lot_luoi_tru_mau_va_phat_event()
        {
            GoalHealth g = New();
            var seen = new List<GoalDamagedInfo>();
            g.GoalDamaged += i => seen.Add(i);

            bool lost = g.Leak(2, "tambor");

            Assert.False(lost);
            Assert.Equal(18, g.Current);
            Assert.False(g.CleanSheet);
            Assert.Single(seen);
            Assert.Equal("tambor", seen[0].EnemyId);
            Assert.Equal(18, seen[0].HealthAfter);
        }

        /// <summary>
        /// 🔴 `01` §8 **FM-09**: "`O Capitão` lọt lưới khi máu cầu môn ≤ 5 → Thua.
        /// Boss trừ 5 — KHÔNG có luật 'trừ tối đa còn 1'."
        ///
        /// Cài "để lại 1 máu cho người chơi cơ hội nữa" nghe nhân đạo nhưng phá
        /// điều kiện thua, và làm boss vô hại đúng lúc nó đáng sợ nhất.
        /// </summary>
        [Fact]
        public void Boss_lot_luoi_khi_mau_con_5_la_THUA_khong_de_lai_1()
        {
            GoalHealth g = New();
            g.Leak(15, "adepto");
            Assert.Equal(5, g.Current);

            bool lost = false;
            g.GoalLost += () => lost = true;

            bool ret = g.Leak(5, "o_capitao");   // boss trừ đúng 5

            Assert.True(ret);
            Assert.True(lost);
            Assert.Equal(0, g.Current);
            Assert.True(g.IsLost);
        }

        [Fact]
        public void Tru_qua_0_thi_ket_o_0_khong_am()
        {
            GoalHealth g = New(3);
            g.Leak(99, "o_capitao");
            Assert.Equal(0, g.Current);
            Assert.True(g.IsLost);
        }

        [Fact]
        public void GoalLost_chi_phat_dung_mot_lan()
        {
            GoalHealth g = New(2);
            int n = 0;
            g.GoalLost += () => n++;

            g.Leak(2, "adepto");
            g.Leak(5, "tambor");     // đã thua rồi
            g.Leak(5, "o_capitao");

            Assert.Equal(1, n);
        }

        [Fact]
        public void Hoi_mau_khong_vuot_toi_da()
        {
            GoalHealth g = New();
            g.Leak(3, "tambor");
            g.Heal(99);
            Assert.Equal(20, g.Current);
        }

        /// <summary>
        /// Clean sheet là "CHƯA BAO GIỜ thủng lưới", KHÔNG phải "hiện đang đầy máu".
        /// Docs `01` §3 định nghĩa 3 sao bằng ngoặc đơn: "(clean sheet — không thủng
        /// lưới lần nào)".
        ///
        /// Người thủng lưới rồi mua "Sửa cầu môn" về lại 20 → máu đúng 20 nhưng
        /// KHÔNG phải clean sheet → 2 sao. Nếu chấm bằng `Current == Max` thì tiền
        /// mua được sao, và ngôn ngữ bóng đá mất nghĩa.
        /// </summary>
        [Fact]
        public void Thung_luoi_roi_hoi_day_van_KHONG_phai_clean_sheet()
        {
            GoalHealth g = New();
            g.Leak(1, "adepto");
            g.Heal(1);

            Assert.Equal(20, g.Current);      // đầy máu
            Assert.False(g.CleanSheet);       // nhưng đã thủng
            Assert.Equal(StarRating.Two, g.RatingOnWin());   // 2 sao, không phải 3
        }

        [Theory]
        [InlineData(0, StarRating.Three)]   // không thủng lần nào
        [InlineData(1, StarRating.Two)]     // còn 19
        [InlineData(10, StarRating.Two)]    // còn 10
        [InlineData(11, StarRating.One)]    // còn 9
        [InlineData(19, StarRating.One)]    // còn 1
        public void Xep_hang_theo_mau_con_lai(int leaked, StarRating expected)
        {
            GoalHealth g = New();
            if (leaked > 0) g.Leak(leaked, "adepto");
            Assert.Equal(expected, g.RatingOnWin());
        }

        [Fact]
        public void Thua_thi_khong_co_sao()
        {
            GoalHealth g = New();
            g.Leak(20, "adepto");
            Assert.Equal(StarRating.None, g.RatingOnWin());
        }

        [Fact]
        public void Hoi_mau_sau_khi_thua_thi_nem()
        {
            GoalHealth g = New(1);
            g.Leak(1, "adepto");
            Assert.Throws<InvalidOperationException>(() => g.Heal(5));
        }

        [Fact]
        public void Tham_so_vo_ly_thi_nem()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GoalHealth(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => New().Leak(0, "adepto"));
            Assert.Throws<ArgumentOutOfRangeException>(() => New().Heal(0));
        }

        /// <summary>Ranh giới 10 là chỗ dễ lệch một đơn vị. `01` §3: ⭐⭐ = "máu còn
        /// 10–19", ⭐ = "máu còn 1–9". Máu còn đúng 10 → 2 sao.</summary>
        [Fact]
        public void Ranh_gioi_10_thuoc_ve_hai_sao()
        {
            GoalHealth a = New(); a.Leak(10, "adepto");   // còn 10
            GoalHealth b = New(); b.Leak(11, "adepto");   // còn 9
            Assert.Equal(StarRating.Two, a.RatingOnWin());
            Assert.Equal(StarRating.One, b.RatingOnWin());
        }
    }
}
