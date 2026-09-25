using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class AbilityEngineTests
    {
        private static readonly GameConfig Cfg = RealConfigValidateTests.Load();

        private sealed class Fake : ITarget
        {
            public int Id { get; set; } = 1;
            public Vec2 Position { get; set; } = new(1, 0);
            public double DistanceTravelled { get; set; }
            public double Hp { get; set; } = 100;
            public double MaxHp { get; set; } = 100;
            public bool IsAlive { get; set; } = true;
            public CardState Card { get; set; } = CardState.None;
        }

        private sealed class Ctx : IAbilityContext
        {
            public double Slow { get; set; }
            public bool AnyGlobalSlowedDamageBuff { get; set; }
            public double GlobalSlowedDamageBonusPercent { get; set; } = 20;

            /// <summary>Trần chậm — mẫu số của buff tỉ lệ. Khớp `economy.json` (70).</summary>
            public double SlowCapPercent { get; set; } = 70;

            public double SlowPercentOn(int enemyId) => Slow;
        }

        private static TowerDef Def(string id) => Cfg.Towers.First(t => t.Id == id);

        private static (TowerInstance, TowerDef) Tower(string id, int level)
        {
            var slots = new SlotManager(Cfg);
            TowerDef def = Def(id);
            string slot = def.IsGoalkeeper ? "gk01" : "f01";
            TowerInstance t = slots.Place(slot, def);
            if (level > 1) slots.SetLevel(slot, level);
            return (t, def);
        }

        private static AttackPlan Plan(string towerId, int level, Fake? target = null, Ctx? ctx = null)
        {
            var (t, def) = Tower(towerId, level);
            return AbilityEngine.Plan(t, def, new Vec2(0, 0), target ?? new Fake(), ctx ?? new Ctx());
        }

        // ── B-01: cấp N chạy ĐÚNG MỘT kỹ năng ───────────────────────────────

        /// <summary>
        /// El Cinco (batigol, 2026-07-22): mỗi cấp một đòn NỔ (đấm/đá/đạp) = Splash
        /// tại mục tiêu, bán kính tăng dần theo cấp (nổ càng to). Đây là đổi thiết kế
        /// từ xuyên-hàng/nuke sang AoE nổ vùng.
        /// </summary>
        [Fact]
        public void Batigol_moi_cap_deu_no_lan_rong_dan()
        {
            Assert.Equal(AttackShape.Splash, Plan("batigol", 1).Shape);
            Assert.Equal(AttackShape.Splash, Plan("batigol", 2).Shape);
            Assert.Equal(AttackShape.Splash, Plan("batigol", 3).Shape);

            Assert.Equal(0.8, Plan("batigol", 1).SplashRadius);
            Assert.Equal(1.1, Plan("batigol", 2).SplashRadius);
            Assert.Equal(1.4, Plan("batigol", 3).SplashRadius);
        }

        [Fact]
        public void Ky_nang_doi_theo_cap_dung_mot_cai()
        {
            Assert.Equal("cu_dam", Plan("batigol", 1).AbilityId);
            Assert.Equal("cu_da", Plan("batigol", 2).AbilityId);
            Assert.Equal("cu_dap", Plan("batigol", 3).AbilityId);
        }

        // ── 🔴 Luật 19: levels[] là nguồn chân lý của chỉ số ─────────────────

        /// <summary>
        /// Quả mìn vòng 10: `cu_cham_thien_tai` từng khai `splashRadiusBonus: 0.5`
        /// trong khi `levels[1].splashRadius` đã là 1.7 (= 1.2 + 0.5). Cộng cả hai
        /// ra 2.2 — D10S Lv2 mạnh hơn thiết kế 29%, không crash, không log.
        /// </summary>
        [Fact]
        public void D10S_lan_dung_bang_levels_KHONG_cong_them()
        {
            Assert.Equal(1.2, Plan("d10s", 1).SplashRadius);
            Assert.Equal(1.7, Plan("d10s", 2).SplashRadius);   // 🔴 KHÔNG phải 2.2
            Assert.Equal(1.2, Plan("d10s", 3).SplashRadius);   // B-01: mất cu_cham_thien_tai
        }

        /// <summary>B-01 tạo đánh đổi thật: Lv2 lan RỘNG HƠN Lv3.</summary>
        [Fact]
        public void D10S_Lv2_lan_rong_hon_Lv3()
        {
            Assert.True(Plan("d10s", 2).SplashRadius > Plan("d10s", 3).SplashRadius);
        }

        [Fact]
        public void Sat_thuong_co_ban_lay_dung_cap()
        {
            Assert.Equal(20, Plan("batigol", 1).Damage);
            Assert.Equal(30, Plan("batigol", 2, new Fake { Hp = 50 }).Damage);   // dame nền cấp 2
            Assert.Equal(45, Plan("batigol", 3).Damage);                          // dame nền cấp 3
        }

        /// <summary>
        /// `solo_run` bật theo THỜI GIAN (`durationSec: 4.0` → mọi phát trong 4s đều
        /// ×2). `cu_vo_le` là MỘT PHÁT (không khai duration). Hai cơ chế khác nhau,
        /// nên hai trường khác nhau — dùng chung thì hoặc cú vô lê kéo dài vô hạn,
        /// hoặc solo_run chỉ còn một phát.
        /// </summary>
        [Fact]
        public void Hai_co_che_cooldown_khong_lan_sang_nhau()
        {
            var (pulga, pulgaDef) = Tower("la_pulga", 1);
            pulga.AbilityCharged = true;              // trường của cu_vo_le
            pulga.AbilityActiveRemaining = 0;
            Assert.Equal(45, AbilityEngine.Plan(pulga, pulgaDef, new Vec2(0, 0), new Fake(), new Ctx()).Damage);

            var (bati, batiDef) = Tower("batigol", 3);
            bati.AbilityActiveRemaining = 4;          // trường của solo_run
            bati.AbilityCharged = false;
            Assert.Equal(45, AbilityEngine.Plan(bati, batiDef, new Vec2(0, 0), new Fake(), new Ctx()).Damage);
        }

        // ── Hệ số có điều kiện ──────────────────────────────────────────────

        /// <summary>
        /// `nhan_quan`: +25% TỈ LỆ với mức chậm, không phải bật/tắt.
        ///
        /// Chậm kịch trần (70) = buff đầy đủ; chậm một nửa trần = nửa buff; không
        /// chậm = không buff. Xem `AbilityEngine.SlowRatio`.
        /// </summary>
        [Fact]
        public void La_Pulga_Lv2_buff_ti_le_voi_muc_cham()
        {
            Assert.Equal(90, Plan("la_pulga", 2, ctx: new Ctx { Slow = 0 }).Damage);
            Assert.Equal(90 * 1.25, Plan("la_pulga", 2, ctx: new Ctx { Slow = 70 }).Damage, precision: 6);
            Assert.Equal(90 * 1.125, Plan("la_pulga", 2, ctx: new Ctx { Slow = 35 }).Damage, precision: 6);
        }

        /// <summary>
        /// 🔴 Bài kiểm CHỐNG HỒI QUY cho lỗi "vách kháng chậm".
        ///
        /// Trước 2026-08-21 buff kiểm nhị phân `slow > 0`, nên boss kháng 90% (chậm
        /// hiệu dụng 7%) hưởng buff Y HỆT quân thường không kháng (chậm 70%). Tức
        /// `slowResistPercent` là vách ở 100, không phải núm xoay — và cả cơ chế
        /// "boss cuối kháng chậm mạnh" lẫn thang độ khó giữa các map đều vô nghĩa.
        ///
        /// Nếu ai đó đổi lại thành `> 0` thì test này đỏ.
        /// </summary>
        [Fact]
        public void Khang_cham_lam_YEU_buff_chu_khong_phai_bat_tat()
        {
            // Cùng một nguồn chậm 70% (kịch trần), khác nhau ở mức KHÁNG của mục tiêu.
            // chậm hiệu dụng = 70 × (1 − kháng/100)
            double khongKhang = Plan("la_pulga", 2, ctx: new Ctx { Slow = 70.0 }).Damage;   // kháng 0
            double khang75    = Plan("la_pulga", 2, ctx: new Ctx { Slow = 17.5 }).Damage;   // kháng 75%
            double khang90    = Plan("la_pulga", 2, ctx: new Ctx { Slow = 7.0 }).Damage;    // kháng 90%

            Assert.Equal(90 * 1.25, khongKhang, precision: 6);      // buff đầy đủ
            Assert.True(khang75 < khongKhang, "kháng 75% mà buff vẫn bằng không kháng?");
            Assert.True(khang90 < khang75, "kháng 90% mà buff vẫn bằng kháng 75%?");
            Assert.Equal(90 * 1.0625, khang75, precision: 6);       // 25% × 0.25
            Assert.Equal(90 * 1.025,  khang90, precision: 6);       // 25% × 0.10
        }

        /// <summary>
        /// `ban_thang_the_ky` (D10S Lv3) buff MỌI tướng, không riêng chủ nhân.
        /// Và nó cần NGƯỜI KHÁC làm chậm — B-01 lấy mất khả năng tự slow của nó.
        /// Cũng TỈ LỆ với mức chậm, cùng luật với `nhan_quan`.
        /// </summary>
        [Fact]
        public void D10S_Lv3_buff_toan_doi_va_can_nguoi_khac_lam_cham()
        {
            var khongCham = new Ctx { AnyGlobalSlowedDamageBuff = true, Slow = 0 };
            var chamKichTran = new Ctx { AnyGlobalSlowedDamageBuff = true, Slow = 70 };
            var chamMotNua = new Ctx { AnyGlobalSlowedDamageBuff = true, Slow = 35 };

            // Batigol Lv1 hưởng buff của D10S Lv3 — nó là "mọi tướng"
            Assert.Equal(20, Plan("batigol", 1, ctx: khongCham).Damage);
            Assert.Equal(20 * 1.2, Plan("batigol", 1, ctx: chamKichTran).Damage, precision: 6);
            Assert.Equal(20 * 1.1, Plan("batigol", 1, ctx: chamMotNua).Damage, precision: 6);
        }

        /// <summary>Chậm vượt trần (cấu hình sai) không được cho buff quá 100%.</summary>
        [Fact]
        public void Cham_vuot_tran_bi_kep_lai()
        {
            Assert.Equal(90 * 1.25, Plan("la_pulga", 2, ctx: new Ctx { Slow = 200 }).Damage, precision: 6);
        }

        [Fact]
        public void Khong_co_D10S_Lv3_thi_khong_buff_du_muc_tieu_bi_cham()
        {
            var ctx = new Ctx { AnyGlobalSlowedDamageBuff = false, Slow = 50 };
            Assert.Equal(20, Plan("batigol", 1, ctx: ctx).Damage);
        }

        // ── every_nth_attack ────────────────────────────────────────────────

        /// <summary>`la_pulga.so_10`: n=4 → đòn 4, 8, 12 xuyên 2 con.</summary>
        [Theory]
        [InlineData(0, AttackShape.Single)]   // đòn thứ 1
        [InlineData(2, AttackShape.Single)]   // đòn thứ 3
        [InlineData(3, AttackShape.Line)]     // đòn thứ 4 ✓
        [InlineData(7, AttackShape.Line)]     // đòn thứ 8 ✓
        public void La_Pulga_Lv3_xuyen_moi_don_thu_4(int fired, AttackShape expected)
        {
            var (t, def) = Tower("la_pulga", 3);
            t.AttacksFired = fired;
            AttackPlan p = AbilityEngine.Plan(t, def, new Vec2(0, 0), new Fake(), new Ctx());
            Assert.Equal(expected, p.Shape);
            if (expected == AttackShape.Line) Assert.Equal(2, p.MaxExtraTargets);
        }

        // ── solo_run: chỉ áp khi đang bật ───────────────────────────────────

        [Fact]
        public void La_Pulga_Lv1_x2_chi_khi_solo_run_dang_bat()
        {
            var (t, def) = Tower("la_pulga", 1);

            t.AbilityActiveRemaining = 0;
            Assert.Equal(45, AbilityEngine.Plan(t, def, new Vec2(0, 0), new Fake(), new Ctx()).Damage);

            t.AbilityActiveRemaining = 4;
            Assert.Equal(90, AbilityEngine.Plan(t, def, new Vec2(0, 0), new Fake(), new Ctx()).Damage);
        }

        // ── Tướng 0 sát thương ──────────────────────────────────────────────

        [Theory]
        [InlineData("el_arbitro")]
        [InlineData("dibu")]
        public void Tuong_khong_sat_thuong_thi_plan_ra_0(string id)
        {
            for (int lv = 1; lv <= 3; lv++)
                Assert.Equal(0, Plan(id, lv).Damage);
        }

        /// <summary>
        /// Mọi kỹ năng trong config PHẢI được AbilityEngine cài. Thêm config mà quên
        /// thêm code = tướng im lặng không làm gì — không crash, không log, chỉ là
        /// một tướng vô dụng mà không ai biết vì sao.
        /// </summary>
        [Fact]
        public void Moi_ky_nang_trong_config_deu_duoc_cai()
        {
            foreach (TowerDef def in Cfg.Towers)
                for (int lv = 1; lv <= 3; lv++)
                {
                    var (t, _) = Tower(def.Id, lv);
                    // Ném ConfigException nếu gặp kỹ năng lạ — xem `default:` trong switch
                    AbilityEngine.Plan(t, def, new Vec2(0, 0), new Fake(), new Ctx());
                }
        }
    }
}
