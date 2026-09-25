using System.IO;
using System.Linq;
using LaMuralla.Core.Config;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    public class RealConfigValidateTests
    {
        private readonly ITestOutputHelper _out;
        public RealConfigValidateTests(ITestOutputHelper o) => _out = o;

        internal static GameConfig Load() => Load(TestPaths.DefaultMap);

        /// <summary>Nạp một map bất kỳ trong `config/maps/`. Towers/enemies/waves/economy
        /// dùng chung; file map ghi đè `waves`/`acts`/`hpScaling`/`economy`/`bossOverrides`
        /// nếu nó khai — xem docs/08-MAPS-ARCHITECTURE.md §2.</summary>
        internal static GameConfig Load(string mapFile) => ConfigMapper.Map(
            File.ReadAllText(TestPaths.Config("towers.json")),
            File.ReadAllText(TestPaths.Config("enemies.json")),
            File.ReadAllText(TestPaths.Config("waves.json")),
            File.ReadAllText(TestPaths.Config("economy.json")),
            File.ReadAllText(TestPaths.Map(mapFile)));

        [Fact]
        public void Config_that_qua_het_16_luat()
        {
            ConfigValidator.Validate(Load());   // ném = fail
        }

        /// <summary>
        /// MỌI map phải qua validator, không chỉ map mặc định.
        ///
        /// Trước vòng 27 chỉ m00 được validate; 10 map còn lại chỉ được kiểm "JSON đọc
        /// được" (`RealConfigParseTests`). Nghĩa là các luật thật sự nói về map — luật
        /// 22 (mọi nhóm cùng tuyến phải khai cùng `delaySec`), luật tuyến/ô — không
        /// canh gì cho đúng 10 map mới. Một map khai sai `lane` sẽ nổ lúc CHƠI.
        /// </summary>
        [Fact]
        public void Moi_map_deu_qua_validator()
        {
            string dir = Path.Combine(TestPaths.Root, "config", "maps");
            string[] files = Directory.GetFiles(dir, "*.json");
            Assert.Equal(11, files.Length);
            foreach (string f in files)
                ConfigValidator.Validate(Load(Path.GetFileName(f)));
        }

        [Fact]
        public void Model_khop_voi_thuc_te_da_chot()
        {
            GameConfig c = Load();
            Assert.Equal(5, c.Towers.Count);        // 4 cầu thủ + 1 trọng tài (El Fideo gỡ 2026-08-20)
            Assert.Equal(3, c.Enemies.Count);
            Assert.Equal(20, c.Waves.Count);
            Assert.Equal(700, c.Economy.StartingCash);
            Assert.Equal(20, c.Economy.GoalHealth);
            Assert.Equal(15, c.Towers.Sum(t => t.Abilities.Count));  // 5 × 3
        }

        [Fact]
        public void HpScaling_don_dieu_tang_qua_ca_20_wave()
        {
            // Máu MỖI CON quái không bao giờ được giảm giữa hai wave.
            // Xem waves.json → hpScaling._monotone.
            GameConfig c = Load();
            for (int w = 2; w <= 20; w++)
                Assert.True(c.HpScaling.MultiplierAt(w) > c.HpScaling.MultiplierAt(w - 1),
                    $"W{w} máu ×{c.HpScaling.MultiplierAt(w):0.###} không lớn hơn W{w - 1}");
        }

        [Theory]
        [InlineData(1, 1.00)]
        [InlineData(4, 1.00)]
        [InlineData(5, 1.20)]
        [InlineData(9, 1.20)]
        [InlineData(10, 1.40)]
        [InlineData(14, 1.40)]
        [InlineData(15, 1.70)]
        [InlineData(19, 1.70)]
        [InlineData(20, 2.00)]
        public void Moc_mau_Hard_ap_dung_dung_wave(int wave, double expected)
        {
            GameConfig c = Load();
            Assert.Equal(expected, c.HpScaling.MilestoneMultiplierAt(wave), precision: 6);
        }

        [Fact]
        public void Moi_map_ke_thua_cung_bon_moc_mau_Hard()
        {
            string dir = Path.Combine(TestPaths.Root, "config", "maps");
            foreach (string f in Directory.GetFiles(dir, "*.json"))
            {
                GameConfig c = Load(Path.GetFileName(f));
                Assert.Equal(new[] { 5, 10, 15, 20 }, c.HpScaling.Milestones.Select(x => x.Wave));
                Assert.Equal(new[] { 1.20, 1.40, 1.70, 2.00 },
                             c.HpScaling.Milestones.Select(x => x.Multiplier));
            }
        }

        [Fact]
        public void Rewarded_recovery_dung_contract_da_chot()
        {
            EconomyDef economy = Load().Economy;
            Assert.Equal(5, economy.RewardedHealAmount);
            Assert.Equal(1, economy.RewardedHealUsesPerMatch);
            Assert.Equal(5, economy.RewardedContinueHealth);
            Assert.Equal(1, economy.RewardedContinueUsesPerMatch);
        }

        [Fact]
        public void Bang_may_khop_bang_python_khong_lech_lam_tron()
        {
            // gen_docs.py và game phải ra CÙNG con số. Đây là chỗ vòng 4 lệch 28 Peso.
            GameConfig c = Load();
            EnemyDef tambor = c.Enemies.Single(x => x.Id == "tambor");
            _out.WriteLine($"Tambor thưởng: W1={c.BountyOf(tambor, 1)} W20={c.BountyOf(tambor, 20)}");
            Assert.Equal(24, c.BountyOf(tambor, 1));          // ×1.0
            Assert.Equal(53, c.BountyOf(tambor, 20));         // 24 × 2.2 = 52.8 → half_up 53
        }

        [Fact]
        public void HalfUp_khong_phai_bankers_rounding()
        {
            // Math.Round(62.5) mặc định = 62. Đây là bẫy đã cắn một lần rồi.
            Assert.Equal(63, Round.HalfUp(62.5));
            Assert.Equal(64, Round.HalfUp(63.5));    // banker's cho 64 — trùng, nhưng...
            Assert.Equal(2, Round.HalfUp(1.5));      // banker's cho 2 — trùng
            Assert.Equal(1, Round.HalfUp(0.5));      // banker's cho 0 ← KHÁC
        }
    }
}
