using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    /// <summary>
    /// `LanesOfWave` — dữ liệu để HUD báo trước wave sau ra CỬA NÀO.
    ///
    /// Vì sao có test riêng: đây là điều kiện bắt buộc của map luân phiên
    /// (`docs/maps/M03` §3). Nếu nó trả sai tuyến thì người chơi xây nhầm bên và
    /// map biến thành trò tung đồng xu — hỏng âm thầm, không có ngoại lệ nào bắn ra.
    /// </summary>
    public class LanePreviewTests
    {
        [Fact]
        public void Map_mot_tuyen_thi_LaneCount_bang_1()
        {
            var m = new MatchController(RealConfigValidateTests.Load("m00-la-muralla.json"), 7);
            Assert.Equal(1, m.LaneCount);
            Assert.Equal(new[] { "L1" }, m.LanesOfWave(1).Select(x => x.Lane));
        }

        [Fact]
        public void M03_luan_phien_hai_cua_va_boss_wave_danh_ca_hai()
        {
            var m = new MatchController(RealConfigValidateTests.Load("m03-dos-rios.json"), 7);
            Assert.Equal(2, m.LaneCount);
            Assert.Equal(new[] { "L1" }, m.LanesOfWave(1).Select(x => x.Lane));
            Assert.Equal(new[] { "L2" }, m.LanesOfWave(2).Select(x => x.Lane));
            Assert.Equal(new[] { "L1", "L2" }, m.LanesOfWave(10).Select(x => x.Lane));
            Assert.Equal(new[] { "L1", "L2" }, m.LanesOfWave(20).Select(x => x.Lane));
        }

        [Fact]
        public void Map_tam_va_map_cuoi_co_du_ba_cua_vao()
        {
            Assert.Equal(3, new MatchController(
                RealConfigValidateTests.Load("m07-tres-puertas.json"), 7).LaneCount);
            Assert.Equal(3, new MatchController(
                RealConfigValidateTests.Load("m10-la-muralla-final.json"), 7).LaneCount);
        }

        [Theory]
        [InlineData("m04-el-cruce.json")]
        [InlineData("m09-la-horquilla.json")]
        [InlineData("m10-la-muralla-final.json")]
        public void Map_hai_loi_ra_co_hai_dich_hinh_hoc_va_hai_o_thu_mon(string map)
        {
            GameConfig cfg = RealConfigValidateTests.Load(map);
            Assert.Equal(2, cfg.Path.Lanes.Select(x => x.GoalPoint).Distinct().Count());
            Assert.Equal(2, cfg.Path.Slots.Count(x => !x.IsField));
            Assert.Equal(2, cfg.Economy.GoalkeeperSlots);
        }

        [Fact]
        public void Map_cuoi_co_hai_boss_o_hai_cua_khac_nhau()
        {
            GameConfig cfg = RealConfigValidateTests.Load("m10-la-muralla-final.json");
            WaveDef final = cfg.Waves[19];
            Assert.Equal(2, final.Bosses.Count);
            Assert.Equal(2, final.Bosses.Select(x => x.Lane).Distinct().Count());
        }

        [Fact]
        public void Bon_map_cuoi_co_nhieu_quai_cap_hai_va_ba_hon_map_bay()
        {
            GameConfig mapBay = RealConfigValidateTests.Load("m06-el-caracol.json");
            int capHaiMoc = mapBay.Waves.Sum(w => w.CountOf("tifoso"));
            int capBaMoc = mapBay.Waves.Sum(w => w.CountOf("tambor"));

            foreach (string map in new[]
            {
                "m07-tres-puertas.json", "m08-el-mirador.json",
                "m09-la-horquilla.json", "m10-la-muralla-final.json",
            })
            {
                GameConfig cfg = RealConfigValidateTests.Load(map);
                Assert.True(cfg.Waves.Sum(w => w.CountOf("tifoso")) > capHaiMoc, map);
                Assert.True(cfg.Waves.Sum(w => w.CountOf("tambor")) > capBaMoc, map);
            }
        }

        [Fact]
        public void Boss_duoc_tinh_vao_so_con_cua_tuyen_no_di_ra()
        {
            GameConfig cfg = RealConfigValidateTests.Load("m03-dos-rios.json");
            var m = new MatchController(cfg, 7);
            WaveDef w10 = cfg.Waves[9];
            Assert.NotEmpty(w10.Bosses);

            string bossLane = w10.Bosses[0].Lane;
            int quan = w10.Spawns.Where(g => g.Lane == bossLane).Sum(g => g.Count);
            var bao = m.LanesOfWave(10).First(x => x.Lane == bossLane);
            Assert.Equal(quan + 1, bao.Count);
        }

        [Fact]
        public void Wave_khong_ton_tai_thi_tra_rong_chu_khong_no()
        {
            var m = new MatchController(RealConfigValidateTests.Load("m00-la-muralla.json"), 7);
            Assert.Empty(m.LanesOfWave(0));
            Assert.Empty(m.LanesOfWave(21));
        }
    }
}
