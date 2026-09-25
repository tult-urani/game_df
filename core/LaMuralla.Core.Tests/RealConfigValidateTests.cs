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
