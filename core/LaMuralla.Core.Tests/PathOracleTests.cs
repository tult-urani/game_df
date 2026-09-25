using System.Collections.Generic;
using System.IO;
using System.Linq;
using LaMuralla.Core.Json;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    /// <summary>
    /// Khoá spline C# vào ĐÚNG con số mà tools/path_check.py tính ra.
    ///
    /// Vì sao gắt thế: balance_sim.py tính chord từ polyline Python → ra toàn bộ
    /// bảng wave 20/20. Nếu spline trong game lấy mẫu khác đi một chút, game và
    /// bảng cân bằng đang nói về HAI đường khác nhau, và không ai biết.
    /// </summary>
    public class PathOracleTests
    {
        private readonly ITestOutputHelper _out;
        public PathOracleTests(ITestOutputHelper o) => _out = o;

        // Lấy từ: python3 -c "from path_check import *; ..." trên config/path.json thật
        private const double OracleLength = 42.28285336247038;   // VÒNG 21: đường kéo dài (spawn/goal ×1.17)
        private const int OracleSamples = 3992;

        internal static EnemyPath LoadPath()
        {
            JsonObject o = JsonParser.Parse(File.ReadAllText(TestPaths.Map(TestPaths.DefaultMap))).AsObject();
            List<Vec2> wp = o["lanes"].AsArray().Items[0].AsObject()["waypoints"].AsArray().Items
                .Select(v => new Vec2(v.AsObject()["x"].AsNumber(), v.AsObject()["y"].AsNumber()))
                .ToList();
            return new EnemyPath(wp);
        }

        [Fact]
        public void Do_dai_spline_khop_python_toi_muc_sai_so_dau_phay_dong()
        {
            EnemyPath p = LoadPath();
            _out.WriteLine($"C#     : {p.Length:R}");
            _out.WriteLine($"Python : {OracleLength:R}");
            _out.WriteLine($"lệch   : {p.Length - OracleLength:E3}");
            Assert.Equal(OracleLength, p.Length, precision: 9);
        }

        [Fact]
        public void So_mau_khop_python()
        {
            Assert.Equal(OracleSamples, LoadPath().SampleCount);
        }

        [Fact]
        public void Spline_cham_dung_spawn_va_cau_mon_chu_khong_di_ngang_qua()
        {
            JsonObject path = JsonParser.Parse(File.ReadAllText(TestPaths.Map(TestPaths.DefaultMap)))
                .AsObject()["lanes"].AsArray().Items[0].AsObject();
            JsonObject sp = path["spawnPoint"].AsObject();
            JsonObject gp = path["goalPoint"].AsObject();
            EnemyPath p = LoadPath();

            Assert.Equal(sp["x"].AsNumber(), p.Spawn.X, 9);
            Assert.Equal(sp["y"].AsNumber(), p.Spawn.Y, 9);
            Assert.Equal(gp["x"].AsNumber(), p.Goal.X, 9);
            Assert.Equal(gp["y"].AsNumber(), p.Goal.Y, 9);
        }

        [Fact]
        public void Do_dai_that_khop_lengthUnits_da_khai_trong_2_phan_tram()
        {
            // Cùng luật mà path_check.py ép. docs/04 dùng lengthUnits để tính WDB —
            // lệch nghĩa là mô hình cân bằng đang dùng số sai.
            double declared = JsonParser.Parse(File.ReadAllText(TestPaths.Map(TestPaths.DefaultMap)))
                .AsObject()["lanes"].AsArray().Items[0].AsObject()["lengthUnits"].AsNumber();
            double drift = System.Math.Abs(LoadPath().Length - declared) / declared;
            _out.WriteLine($"khai {declared} · thật {LoadPath().Length:0.###} · lệch {drift:P2}");
            Assert.True(drift <= 0.02, $"lệch {drift:P2} > 2%");
        }

        [Fact]
        public void PositionAt_kep_o_hai_dau_chu_khong_bay_ra_ngoai()
        {
            EnemyPath p = LoadPath();
            Assert.Equal(p.Spawn, p.PositionAt(-5));
            Assert.Equal(p.Goal, p.PositionAt(p.Length + 5));
        }

        [Fact]
        public void PositionAt_di_lien_mach_khong_nhay_coc()
        {
            // Quân đi mượt: hai bước liền nhau không được cách nhau quá bước đi.
            EnemyPath p = LoadPath();
            const double step = 0.01;
            Vec2 prev = p.PositionAt(0);
            for (double d = step; d <= p.Length; d += step)
            {
                Vec2 cur = p.PositionAt(d);
                Assert.True(Vec2.Distance(prev, cur) < step * 2,
                    $"nhảy cóc ở quãng {d:0.##}: {Vec2.Distance(prev, cur):0.####}");
                prev = cur;
            }
        }

        [Fact]
        public void PositionAt_khong_bao_gio_tra_ve_NaN()
        {
            // Waypoint nhân đôi ở đầu tạo đoạn dài 0 → chia 0 → NaN → quân biến mất.
            EnemyPath p = LoadPath();
            for (double d = 0; d <= p.Length; d += 0.05)
            {
                Vec2 v = p.PositionAt(d);
                Assert.False(double.IsNaN(v.X) || double.IsNaN(v.Y), $"NaN ở quãng {d}");
            }
        }
    }
}
