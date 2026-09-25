using System;
using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    public class MatchControllerTests
    {
        private readonly ITestOutputHelper _out;
        public MatchControllerTests(ITestOutputHelper o) => _out = o;

        private static readonly GameConfig Cfg = RealConfigValidateTests.Load();
        private static MatchController New() => new(Cfg);

        private const double Dt = 1.0 / 60;

        /// <summary>Chạy tới khi điều kiện đúng, hoặc hết `maxSec` (chống treo).</summary>
        private static void RunUntil(MatchController m, Func<bool> done, double maxSec = 400)
        {
            for (double t = 0; t < maxSec && !done(); t += Dt) m.Tick(Dt);
        }

        [Fact]
        public void Khoi_dau_dung_trang_thai()
        {
            MatchController m = New();
            Assert.Equal(MatchPhase.Preparing, m.Phase);
            Assert.Equal(0, m.Wave);
            Assert.Equal(Cfg.Economy.StartingCash, m.Economy.Balance);
            Assert.Equal(Cfg.Economy.GoalHealth, m.Goal.Current);
        }

        [Fact]
        public void Bat_dau_wave_1_thi_quan_ra_san()
        {
            MatchController m = New();
            m.StartNextWave();
            Assert.Equal(MatchPhase.Fighting, m.Phase);
            Assert.Equal(1, m.Wave);

            m.Tick(Dt);
            Assert.Single(m.Enemies);          // con đầu ra ở t=0
            m.Tick(0.7);
            Assert.Equal(2, m.Enemies.Count);  // spawnIntervalSec = 0.7
        }

        /// <summary>
        /// 🔴 Không có tướng nào → quân lọt lưới → trừ máu cầu môn.
        /// W1 = N adepto × leakDamage 1 = N máu. N suy từ waves.json.
        /// </summary>
        [Fact]
        public void Khong_co_tuong_thi_quan_lot_luoi_va_tru_mau()
        {
            MatchController m = New();
            m.StartNextWave();
            RunUntil(m, () => m.Phase != MatchPhase.Fighting);

            int n = Cfg.Waves.First(x => x.Wave == 1).CountOf("adepto");
            Assert.Equal(Cfg.Economy.GoalHealth - n, m.Goal.Current);
            Assert.False(m.Goal.CleanSheet);
        }

        /// <summary>`03` §6: quân lọt lưới "KHÔNG rơi tiền". Nếu dùng Apply(∞) thay
        /// RemoveWithoutKill thì người chơi được thưởng vì để quân vào lưới.</summary>
        [Fact]
        public void Quan_lot_luoi_KHONG_roi_tien()
        {
            MatchController m = New();
            int before = m.Economy.LifetimeEarned;
            m.StartNextWave();
            RunUntil(m, () => m.Phase != MatchPhase.Fighting);

            // Chỉ có thưởng clear wave, KHÔNG có tiền hạ quân nào
            Assert.Equal(before + EconomyService.WaveClearBonus(1), m.Economy.LifetimeEarned);
        }

        /// <summary>🔴 Luật #1 gặp luật #2: DamageSystem quyết ai giết →
        /// EconomyService cộng tiền. Đúng một lần cho mỗi con.</summary>
        [Fact]
        public void Ha_quan_thi_duoc_tien_dung_mot_lan()
        {
            MatchController m = New();
            m.Upgrades.TryBuy("f01", "batigol", out _);
            m.Upgrades.TryBuy("f05", "batigol", out _);
            m.Upgrades.TryBuy("f07", "batigol", out _);   // f11 xóa ở vòng 20 → dùng f07

            int before = m.Economy.LifetimeEarned;
            m.StartNextWave();
            RunUntil(m, () => m.Phase != MatchPhase.Fighting);

            EnemyDef adepto = Cfg.Enemies.First(e => e.Id == "adepto");
            int bounty = Cfg.BountyOf(adepto, 1);
            int earned = m.Economy.LifetimeEarned - before - EconomyService.WaveClearBonus(1);

            Assert.True(earned > 0, "không hạ được con nào");
            Assert.Equal(0, earned % bounty);                    // bội số nguyên của thưởng
            int n1 = Cfg.Waves.First(x => x.Wave == 1).CountOf("adepto");
            Assert.True(earned / bounty <= n1, "cộng tiền nhiều hơn số quân có thật");
        }

        [Fact]
        public void Clear_wave_thi_nghi_roi_tu_sang_wave_sau()
        {
            MatchController m = New();
            for (int i = 0; i < 11; i++) m.Upgrades.TryBuy($"f{i + 1:00}", "batigol", out _);

            m.StartNextWave();
            RunUntil(m, () => m.Phase == MatchPhase.Preparing);

            Assert.Equal(1, m.Wave);
            Assert.True(m.RestRemaining > 0);

            m.Tick(Cfg.RestBetweenWavesSec + 0.1);
            Assert.Equal(2, m.Wave);
            Assert.Equal(MatchPhase.Fighting, m.Phase);
        }

        [Fact]
        public void Skip_nghi_thi_duoc_thuong_theo_giay_con_lai()
        {
            MatchController m = New();
            for (int i = 0; i < 11; i++) m.Upgrades.TryBuy($"f{i + 1:00}", "batigol", out _);
            m.StartNextWave();
            RunUntil(m, () => m.Phase == MatchPhase.Preparing);

            double left = m.RestRemaining;
            int before = m.Economy.Balance;
            m.SkipRest();

            Assert.Equal(before + Round.HalfUp(left * Cfg.Economy.SkipBonusPerSecond), m.Economy.Balance);
            Assert.Equal(2, m.Wave);
        }

        /// <summary>Máu cầu môn về 0 → THUA ngay, không chờ hết wave.</summary>
        [Fact]
        public void Mau_cau_mon_ve_0_thi_thua()
        {
            MatchController m = New();
            m.StartNextWave();
            RunUntil(m, () => m.Phase == MatchPhase.Lost, maxSec: 3000);
            // 20 máu / 1 leak mỗi adepto → cần ~20 con lọt; W1-W3 đủ
            for (int w = 0; w < 4 && m.Phase != MatchPhase.Lost; w++)
            {
                if (m.Phase == MatchPhase.Preparing) m.Tick(Cfg.RestBetweenWavesSec + 0.1);
                RunUntil(m, () => m.Phase != MatchPhase.Fighting, maxSec: 200);
            }
            Assert.Equal(MatchPhase.Lost, m.Phase);
            Assert.Equal(0, m.Goal.Current);
            Assert.Equal(StarRating.None, m.Rating);
        }

        [Fact]
        public void Thua_roi_thi_Tick_khong_lam_gi_nua()
        {
            MatchController m = New();
            m.StartNextWave();
            for (int w = 0; w < 4 && m.Phase != MatchPhase.Lost; w++)
            {
                if (m.Phase == MatchPhase.Preparing) m.Tick(Cfg.RestBetweenWavesSec + 0.1);
                RunUntil(m, () => m.Phase != MatchPhase.Fighting, maxSec: 200);
            }
            int wave = m.Wave;
            m.Tick(100);
            Assert.Equal(wave, m.Wave);
            Assert.Equal(MatchPhase.Lost, m.Phase);
        }

        /// <summary>El Árbitro (vòng 22) — Lv2 vừa có AURA NỀN chậm MỌI con trong tầm
        /// (25%), vừa NÉM THẺ vàng cho 1 con → con trúng chậm MẠNH HƠN (45% vĩnh viễn).
        /// SlowStack lấy MAX nên con trúng = 45, con chưa trúng = 25.</summary>
        [Fact]
        public void Arbitro_Lv2_aura_nen_phu_tat_ca_va_the_lam_cham_manh_hon()
        {
            MatchController m = New();
            m.Upgrades.TryBuy("f02", "el_arbitro", out _);
            m.Upgrades.TryUpgrade("f02", out _);            // Lv2 = the_vang
            m.StartNextWave();

            Vec2 pos = m.Slots.SlotOf("f02").Position;
            double range = m.Upgrades.TowerById("el_arbitro").Levels[1].Range;   // Lv2
            double r2 = range * range;

            // Chờ tới khi có con bị GÁN THẺ vàng — chứng tỏ cơ chế thẻ chạy.
            RunUntil(m, () => m.Enemies.Any(e => e.Card == CardState.Yellow), maxSec: 60);

            // Bất biến AURA NỀN: MỌI con trong tầm đều bị chậm (không chỉ con trúng thẻ).
            foreach (Enemy e in m.Enemies)
                if (Vec2.SqrDistance(pos, e.Position) <= r2)
                    Assert.True(e.CurrentSpeed < e.BaseSpeed, "con trong tầm mà không bị chậm → aura nền hỏng");

            // Con trúng thẻ vàng chậm MẠNH HƠN nền: hệ số tốc độ ≤ 1 − 0.45 (+ dung sai kháng).
            Enemy carded = m.Enemies.First(e => e.Card == CardState.Yellow);
            Assert.True(carded.CurrentSpeed <= carded.BaseSpeed * 0.60,
                        "con trúng thẻ phải chậm ~45%, mạnh hơn aura nền 25%");

            // Thẻ VĨNH VIỄN: theo con ra khỏi tầm vẫn còn.
            RunUntil(m, () => carded.DistanceTravelled > 20, maxSec: 60);
            Assert.Equal(CardState.Yellow, carded.Card);
            Assert.True(carded.CurrentSpeed < carded.BaseSpeed, "thẻ vĩnh viễn mà hết chậm");
        }

        /// <summary>
        /// Aura `scope: in_range` — ra khỏi tầm là HẾT chậm ngay.
        ///
        /// ⚠️ Ô phải là ô Árbitro Lv1 VỚI TỚI ĐƯỢC. Bản đầu của test này dùng `f02`
        /// và đỏ: f02 cách đường 1.31, Árbitro Lv1 tầm 1.2 → không với tới, không
        /// con nào bị chậm. Đó không phải bug — đó là thang tầm ở path.json
        /// (`f02 1.31 → Árbitro ❌`) đang hoạt động đúng. `f07` cách đường 0.79.
        /// </summary>
        [Fact]
        public void Aura_het_tac_dung_khi_quan_ra_khoi_tam()
        {
            MatchController m = New();
            m.Upgrades.TryBuy("f07", "el_arbitro", out _);   // Lv1 = coi_chi_tay (aura 25%)
            m.StartNextWave();

            RunUntil(m, () => m.Enemies.Any(e => e.CurrentSpeed < e.BaseSpeed), maxSec: 60);
            Enemy e2 = m.Enemies.First(e => e.CurrentSpeed < e.BaseSpeed);
            Assert.Equal(1, e2.Slows.SourceCount);

            // ⚠️ ĐƯỜNG LÀ SERPENTINE — nó ngoặt lại gần chính ô đó. f07 nằm ở arc
            // 0.59 tức quãng đường ~24/40.65. Bản đầu của test kiểm ở quãng 20 và
            // đỏ: quân lúc ấy đang QUAY LẠI gần f07, chưa hề ra khỏi tầm.
            // "Đi xa hơn" trên đường thẳng ≠ "ra xa tháp" trên đường cong.
            RunUntil(m, () => e2.DistanceTravelled > 33, maxSec: 90);

            Assert.Equal(CardState.None, e2.Card);
            Assert.Equal(0, e2.Slows.SourceCount);                      // aura đã gỡ
            Assert.Equal(e2.BaseSpeed, e2.CurrentSpeed, precision: 6);
        }

        /// <summary>
        /// Dibu (vòng 18) giết bằng XÁC SUẤT 20/30/40%, không phải cooldown.
        /// Cản thành công → quân biến mất, CÓ rơi tiền như cú giết thường (user chốt
        /// 2026-07-22, đảo lại `dropsBounty: false` cũ).
        ///
        /// Hạt giống cố định để test tái hiện được — đây là cơ chế RNG đầu tiên
        /// trong game, và RNG không hạt giống thì "thua oan ở W14" không dựng lại được.
        /// </summary>
        [Fact]
        public void Dibu_can_bang_xac_suat_co_roi_tien()
        {
            var m = new MatchController(Cfg, seed: 12345);
            m.Upgrades.TryBuy("gk01", "dibu", out _);
            int before = m.Economy.LifetimeEarned;
            int saves = 0;
            m.Saved += (_, _) => saves++;

            m.StartNextWave();
            RunUntil(m, () => m.Phase != MatchPhase.Fighting);

            int n = Cfg.Waves.First(x => x.Wave == 1).CountOf("adepto");
            _out.WriteLine($"Dibu Lv1 (20%) cứu {saves}/{n} con · máu {m.Goal.Current}/20");
            Assert.True(saves > 0, "20% × nhiều lần thử mà không cứu nổi con nào?");
            // Mỗi cú cứu = 1 máu KHÔNG mất (adepto leakDamage 1)
            Assert.Equal(Cfg.Economy.GoalHealth - (n - saves), m.Goal.Current);
            // 🟢 Cản CÓ rơi tiền: thưởng clear wave + bounty cho mỗi con Dibu giết.
            EnemyDef adepto = Cfg.Enemies.First(e => e.Id == "adepto");
            int killBounty = Cfg.BountyOf(adepto, 1);
            Assert.Equal(before + EconomyService.WaveClearBonus(1) + saves * killBounty,
                         m.Economy.LifetimeEarned);
        }

        /// <summary>Cùng hạt giống → cùng kết quả. Nếu không thì bảng cân bằng và
        /// mọi báo cáo lỗi của người chơi đều vô nghĩa.</summary>
        [Fact]
        public void Cung_hat_giong_thi_cung_ket_qua()
        {
            int Run(int seed)
            {
                var m = new MatchController(Cfg, seed);
                m.Upgrades.TryBuy("gk01", "dibu", out _);
                m.StartNextWave();
                RunUntil(m, () => m.Phase != MatchPhase.Fighting);
                return m.Goal.Current;
            }
            Assert.Equal(Run(777), Run(777));
        }

        /// <summary>Cấp cao hơn = tỉ lệ cao hơn = cứu nhiều hơn. Kiểm trên nhiều
        /// hạt giống vì một ván lẻ không nói lên gì về xác suất.</summary>
        [Fact]
        public void Dibu_cap_cao_hon_cuu_nhieu_hon()
        {
            int Total(int level)
            {
                int saves = 0;
                for (int seed = 0; seed < 40; seed++)
                {
                    var m = new MatchController(Cfg, seed);
                    m.Upgrades.TryBuy("gk01", "dibu", out _);
                    for (int l = 1; l < level; l++) m.Upgrades.TryUpgrade("gk01", out _);
                    m.Saved += (_, _) => saves++;
                    m.StartNextWave();
                    RunUntil(m, () => m.Phase != MatchPhase.Fighting);
                }
                return saves;
            }
            int lv1 = Total(1), lv3 = Total(3);
            _out.WriteLine($"40 ván: Lv1 (20%) cứu {lv1} · Lv3 (40%) cứu {lv3}");
            Assert.True(lv3 > lv1, $"Lv3 40% cứu {lv3} mà Lv1 20% cứu {lv1}?");
        }

        /// <summary>Overkill được đếm — để đo `etaWasteFactor = 0.75` ở M1.</summary>
        [Fact]
        public void Dem_sat_thuong_thua()
        {
            MatchController m = New();
            m.Upgrades.TryBuy("f01", "batigol", out _);
            m.Upgrades.TryUpgrade("f01", out _);
            m.Upgrades.TryUpgrade("f01", out _);   // 225 dmg vs adepto máu ~30

            m.StartNextWave();
            RunUntil(m, () => m.Phase != MatchPhase.Fighting);

            Assert.True(m.TotalOverkill > 0, "Batigol Lv3 bắn adepto mà không thừa dame?");
            _out.WriteLine($"overkill W1 = {m.TotalOverkill:0}");
        }
        // 🔵 2026-08-21 — ĐÃ XOÁ khỏi đây: `AutoSpend` + bài kiểm 20 wave.
        //
        // `AutoSpend` GÕ CỨNG danh sách ô (`crowd = f01,f11,f07,...`). Với một map thì
        // tạm được; với nhiều map bố cục khác nhau thì mỗi map lại phải sửa tay một
        // fixture — đúng món nợ mà docs/09-MAPS-REVIEW.md R3 chỉ ra.
        //
        // Thay bằng `GreedyPlayer` (chọn ô TỪ HÌNH HỌC, đo chord) và `MapPlayableTests`
        // (một bài kiểm cho MỖI map). Đối chiếu: luật suy-từ-chord tái tạo ĐÚNG danh
        // sách gõ cứng cũ của m00 — 7 ô đám đông f01,f11,f07,f08,f05,f10,f02 và 4 ô tầm
        // xa f06,f03,f09,f04 — và ra đúng kết quả cũ (thắng, còn 11/20 máu, kiếm 11245).

        [Fact]
        public void Bat_dau_wave_khi_dang_danh_thi_nem()
        {
            MatchController m = New();
            m.StartNextWave();
            Assert.Throws<InvalidOperationException>(() => m.StartNextWave());
        }

        [Fact]
        public void Skip_khi_khong_nghi_thi_nem()
        {
            MatchController m = New();
            Assert.Throws<InvalidOperationException>(() => m.SkipRest());
        }
    }
}
