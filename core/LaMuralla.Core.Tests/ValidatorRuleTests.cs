using System.Collections.Generic;
using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Json;
using Xunit;

namespace LaMuralla.Core.Tests
{
    /// <summary>
    /// Test ÂM cho từng luật: bẻ config → luật phải nổ.
    ///
    /// "Config thật qua hết 16 luật" một mình KHÔNG chứng minh gì — một luật viết
    /// sai thành no-op cũng cho kết quả xanh y hệt. Mỗi luật phải được chứng minh
    /// là bắt được thứ nó sinh ra để bắt.
    /// </summary>
    public class ValidatorRuleTests
    {
        private static GameConfig Cfg() => RealConfigValidateTests.Load();

        /// <summary>Chạy validate, trả về thông báo lỗi. Fail nếu KHÔNG ném.</summary>
        private static string Expect(GameConfig c, int rule)
        {
            ConfigException ex = Assert.Throws<ConfigException>(() => ConfigValidator.Validate(c));
            Assert.Contains($"[{rule}]", ex.Message);
            return ex.Message;
        }

        private static TowerDef Tower(GameConfig c, string id) => c.Towers.Single(t => t.Id == id);

        private static AbilityDef Ability(GameConfig c, string tower, string ability) =>
            Tower(c, tower).Abilities.Single(a => a.Id == ability);

        private static void SetParam(AbilityDef a, string key, JsonValue v)
        {
            var p = new Dictionary<string, JsonValue>(a.Params.ToDictionary(x => x.Key, x => x.Value)) { [key] = v };
            a.Params = p;
        }

        private static void RemoveParam(AbilityDef a, string key)
        {
            var p = a.Params.ToDictionary(x => x.Key, x => x.Value);
            p.Remove(key);
            a.Params = p;
        }

        [Fact]
        public void R01_id_trung()
        {
            GameConfig c = Cfg();
            Tower(c, "batigol").Id = "la_pulga";
            Assert.Contains("la_pulga", Expect(c, 1));
        }

        [Fact]
        public void R02_khong_du_3_cap()
        {
            GameConfig c = Cfg();
            Tower(c, "batigol").Levels = Tower(c, "batigol").Levels.Take(2).ToList();
            Expect(c, 2);
        }

        [Fact]
        public void R03_pha_cong_thuc_gia()
        {
            GameConfig c = Cfg();
            Tower(c, "la_pulga").Levels[1].Cost = 999;   // phải là 0.8 × 300 = 240
            Expect(c, 3);
        }

        [Fact]
        public void R04_thieu_wave()
        {
            GameConfig c = Cfg();
            c.Waves = c.Waves.Take(19).ToList();
            Expect(c, 4);
        }

        /// <summary>Tuyến sai chính tả phải bị chặn từ lúc bake — không thì nó nổ
        /// giữa trận, trên máy người chơi.</summary>
        [Fact]
        public void R05_lane_khong_ton_tai()
        {
            GameConfig c = Cfg();
            c.Waves[0].Spawns = new[]
            { new SpawnGroup { Enemy = "adepto", Count = 3, Lane = "KHONG_CO_TUYEN_NAY" } };
            Expect(c, 5);
        }

        /// <summary>
        /// `delaySec` là của TUYẾN. Hai nhóm cùng tuyến khai hai độ trễ khác nhau thì
        /// `WaveSpawner` lấy của nhóm đầu và bỏ nhóm kia IM LẶNG — config nói một đằng,
        /// engine làm một nẻo.
        /// </summary>
        [Fact]
        public void R22_hai_nhom_cung_tuyen_lech_delaySec()
        {
            GameConfig c = Cfg();
            c.Waves[0].Spawns = new[]
            {
                new SpawnGroup { Enemy = "adepto", Count = 3, Lane = "L1", DelaySec = 0 },
                new SpawnGroup { Enemy = "tifoso", Count = 2, Lane = "L1", DelaySec = 5 },
            };
            Expect(c, 22);
        }

        /// <summary>Cùng tuyến, cùng delaySec → hợp lệ. Chiều ngược lại của R22.</summary>
        [Fact]
        public void R22_cung_delaySec_thi_qua()
        {
            GameConfig c = Cfg();
            c.Waves[0].Spawns = new[]
            {
                new SpawnGroup { Enemy = "adepto", Count = 3, Lane = "L1", DelaySec = 5 },
                new SpawnGroup { Enemy = "tifoso", Count = 2, Lane = "L1", DelaySec = 5 },
            };
            ConfigValidator.Validate(c);   // không ném = đạt
        }

        [Fact]
        public void R05_spawn_id_khong_ton_tai()
        {
            GameConfig c = Cfg();
            c.Waves[0].Spawns = new[]
            { new SpawnGroup { Enemy = "khong_co_con_nay", Count = 3, Lane = "L1" } };
            Expect(c, 5);
        }

        [Fact]
        public void R06_slow_cap_vuot_70()
        {
            GameConfig c = Cfg();
            c.Economy.SlowCapPercent = 71;
            Expect(c, 6);
        }

        [Fact]
        public void R07_hai_thu_mon()
        {
            GameConfig c = Cfg();
            Tower(c, "batigol").SlotType = "goalkeeper";
            Expect(c, 7);
        }

        [Fact]
        public void R08_khong_du_3_ky_nang()
        {
            GameConfig c = Cfg();
            Tower(c, "dibu").Abilities = Tower(c, "dibu").Abilities.Take(2).ToList();
            Expect(c, 8);
        }

        [Fact]
        public void R09_ky_nang_cu_tu_scale_theo_cap()
        {
            GameConfig c = Cfg();
            SetParam(Ability(c, "batigol", "cu_dam"), "damageByLevel", new JsonNumber(2));
            Assert.Contains("ByLevel", Expect(c, 9));
        }

        [Fact]
        public void R10_thuong_duoi_kip_mau_thi_do_kho_phang()
        {
            GameConfig c = Cfg();
            c.Acts.Single(a => a.Id == 3).BountyMultiplier = 99;
            Expect(c, 10);
        }

        [Fact]
        public void R11_boss_khong_co_appearance_cho_wave_do()
        {
            GameConfig c = Cfg();
            BossDef b = c.Bosses[0];
            b.Appearances = b.Appearances.Where(a => a.Wave != 10).ToList();
            Expect(c, 11);
        }

        [Fact]
        public void R12_khang_cham_ngoai_khoang()
        {
            GameConfig c = Cfg();
            c.Bosses[0].SlowResistPercent = 150;
            Expect(c, 12);
        }

        [Fact]
        public void R13_mot_boss_lot_la_thua_ngay()
        {
            GameConfig c = Cfg();
            c.Bosses[0].LeakDamage = c.Economy.GoalHealth;
            Expect(c, 13);
        }

        [Fact]
        public void R14_slow_vuot_cap()
        {
            GameConfig c = Cfg();
            SetParam(Ability(c, "el_arbitro", "the_do"), "slowPercent", new JsonNumber(95));
            Expect(c, 14);
        }

        [Fact]
        public void R15_thieu_scope()
        {
            GameConfig c = Cfg();
            RemoveParam(Ability(c, "el_arbitro", "the_vang"), "scope");
            Expect(c, 15);
        }

        [Fact]
        public void R15_scope_la_gia_tri_bia()
        {
            GameConfig c = Cfg();
            SetParam(Ability(c, "el_arbitro", "the_vang"), "scope", new JsonString("mai_mai_luon"));
            Expect(c, 15);
        }

        [Fact]
        public void R15_timed_ma_thieu_durationSec()
        {
            GameConfig c = Cfg();
            RemoveParam(Ability(c, "d10s", "ban_tay_cua_chua"), "durationSec");
            Assert.Contains("durationSec", Expect(c, 15));
        }

        [Fact]
        public void R15_scope_khong_dung_duration_ma_van_khai_duration()
        {
            GameConfig c = Cfg();
            SetParam(Ability(c, "el_arbitro", "the_vang"), "durationSec", new JsonNumber(5));
            Expect(c, 15);
        }

        [Fact]
        public void R16_khong_sat_thuong_ma_van_co_toc_danh()
        {
            // Dùng El Árbitro, KHÔNG phải Dibu: từ vòng 18 Dibu giết bằng
            // `killChancePercent` nên nó ĐƯỢC PHÉP có nhịp đánh với damage 0 —
            // luật 16 miễn cho đúng ca đó. Árbitro không có killChance nên vẫn bị canh.
            GameConfig c = Cfg();
            Tower(c, "el_arbitro").Levels[0].AttackRate = 1.5;
            Expect(c, 16);
        }

        /// <summary>Luật 16 KHÔNG được bắt nhầm tướng giết bằng xác suất.</summary>
        [Fact]
        public void R16_mien_cho_tuong_giet_bang_xac_suat()
        {
            ConfigValidator.Validate(Cfg());   // Dibu: damage 0 + nhịp 0.5 → hợp lệ
        }

        /// <summary>
        /// `Params` là read-only và `JsonNumber` không dựng được ngoài parser — cố ý,
        /// nên test này đi qua JSON THẬT thay vì chọc vào model. Chậm hơn vài
        /// mili-giây, đổi lại nó kiểm đúng đường mà config thật đi qua.
        /// </summary>
        [Theory]
        [InlineData(0)]      // tướng không bao giờ làm gì
        [InlineData(150)]    // nhầm đơn vị (0.2 vs 20)
        public void R20_ti_le_giet_vo_ly_thi_bat(double pct)
        {
            string towers = System.IO.File.ReadAllText(TestPaths.Config("towers.json"))
                .Replace("\"killChancePercent\": 20", $"\"killChancePercent\": {pct}");
            GameConfig c = ConfigMapper.Map(
                towers,
                System.IO.File.ReadAllText(TestPaths.Config("enemies.json")),
                System.IO.File.ReadAllText(TestPaths.Config("waves.json")),
                System.IO.File.ReadAllText(TestPaths.Config("economy.json")),
                System.IO.File.ReadAllText(TestPaths.Map(TestPaths.DefaultMap)));
            Expect(c, 20);
        }

        /// <summary>killChancePercent mà nhịp = 0 → không bao giờ tung xúc xắc.</summary>
        [Fact]
        public void R20_giet_bang_xac_suat_ma_khong_co_nhip_thi_bat()
        {
            GameConfig c = Cfg();
            Tower(c, "dibu").Levels[0].AttackRate = 0;
            Expect(c, 20);
        }

        [Fact]
        public void Gom_het_loi_roi_moi_nem_chu_khong_dung_o_loi_dau()
        {
            // Ném ở lỗi đầu tiên = sửa một cái phải chạy lại mới thấy cái kế.
            GameConfig c = Cfg();
            c.Economy.SlowCapPercent = 71;              // luật 6
            c.Bosses[0].SlowResistPercent = 150;        // luật 12
            ConfigException ex = Assert.Throws<ConfigException>(() => ConfigValidator.Validate(c));
            Assert.Contains("[6]", ex.Message);
            Assert.Contains("[12]", ex.Message);
            Assert.Contains("2 lỗi", ex.Message);
        }
    }
}
