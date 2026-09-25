using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    /// <summary>
    /// path.json → PathDef, và hình học đọc ra phải khớp thứ tools/path_check.py đo.
    /// Đây là chỗ duy nhất buộc config, oracle Python, và code C# nói cùng một chuyện.
    /// </summary>
    public class PathConfigTests
    {
        private static PathDef Path => RealConfigValidateTests.Load().Path;

        [Fact]
        public void Doc_du_14_waypoint_va_12_o()
        {
            // VÒNG 21: f11 thêm lại (đảo vòng 20) → 11 ô sân + 1 ô thủ môn = 12 ô.
            PathDef p = Path;
            Assert.Equal(14, p.Waypoints.Count);
            Assert.Equal(12, p.Slots.Count);
            Assert.Equal(11, p.Slots.Count(s => s.IsField));
            Assert.Single(p.Slots, s => s.Type == "goalkeeper");
        }

        [Fact]
        public void Spawn_va_cau_mon_dung_cho_da_khai()
        {
            // VÒNG 21: đường kéo dài dùng khoảng trống trên/dưới (spawn/goal ×1.17).
            PathDef p = Path;
            Assert.Equal(new Vec2(0.0, 9.36), p.Waypoints[0]);
            Assert.Equal(new Vec2(0.0, -9.13), p.Waypoints[p.Waypoints.Count - 1]);
        }

        /// <summary>Spline dựng từ waypoint của config phải dài đúng bằng con số
        /// oracle Python. Cùng dữ liệu, cùng thuật toán → cùng kết quả, nếu không
        /// thì một trong hai đã trôi.</summary>
        [Fact]
        public void Spline_tu_config_khop_oracle_Python()
        {
            var path = new EnemyPath(Path.Waypoints.ToList());
            Assert.Equal(42.28285336247038, path.Length, precision: 9);   // VÒNG 21: đường kéo dài
        }

        /// <summary>Luật ±2% mà tools/path_check.py ép: spline thật vs con số khai
        /// báo mà docs/04 dùng để tính cân bằng.</summary>
        [Fact]
        public void Do_dai_that_lech_khong_qua_2_phan_tram_so_voi_khai_bao()
        {
            PathDef p = Path;
            var path = new EnemyPath(p.Waypoints.ToList());
            double drift = System.Math.Abs(path.Length - p.DeclaredLengthUnits) / p.DeclaredLengthUnits;
            Assert.True(drift <= 0.02, $"lệch {drift:P1} — mô hình cân bằng dùng {p.DeclaredLengthUnits}, spline thật {path.Length:0.00}");
        }

        /// <summary>
        /// 🔴 VÙNG CHẠM phải nằm trong khung, không phải TÂM ô.
        ///
        /// Bản đầu của test này chỉ kiểm tâm — nên nó xanh trong khi f02/f05 có
        /// vùng chạm tràn mép màn hình thật. Người dùng nhìn iPhone thấy ngay
        /// ("vòng xanh bị khuất") còn test thì không. Ô có tâm trong màn nhưng
        /// vùng chạm tràn mép = ngón tay không bấm hết được = ô hỏng.
        /// </summary>
        [Fact]
        public void Vung_cham_moi_o_nam_tron_trong_khung_hinh()
        {
            PathDef p = Path;
            double halfW = p.ViewportWidthUnits / 2, halfH = p.ViewportHeightUnits / 2;
            double r = p.MinTouchTargetUnits / 2;

            foreach (SlotDef s in p.Slots)
            {
                Assert.True(System.Math.Abs(s.Position.X) + r <= halfW,
                    $"{s.Id} x={s.Position.X:0.00}: vùng chạm tràn mép ngang " +
                    $"{System.Math.Abs(s.Position.X) + r - halfW:0.00} unit (|x| tối đa {halfW - r:0.00})");
                Assert.True(System.Math.Abs(s.Position.Y) + r <= halfH,
                    $"{s.Id} y={s.Position.Y:0.00}: vùng chạm tràn mép dọc");
            }
        }

        /// <summary>
        /// Cầu nối hình học ↔ ngón tay. 1080px ÷ 100 PPU = 10.8 unit ngang, render
        /// ra 393pt → 36.4 pt/unit. Vùng chạm 48pt của Apple = 1.32 unit.
        ///
        /// Con số này chưa từng được kiểm trên máy thật — test chỉ khoá nó lại để
        /// nếu ai đổi PPU hay viewport thì biết ngay là đã đổi cả kích thước ngón tay.
        /// </summary>
        [Fact]
        public void Ti_le_quy_doi_pt_dung_nhu_docs()
        {
            PathDef p = Path;
            Assert.Equal(36.4, p.PtPerUnit, precision: 1);
            Assert.Equal(1.32, p.MinTouchTargetUnits, precision: 2);
        }

        /// <summary>Camera orthographic size = nửa chiều cao viewport. Sai cái này
        /// thì mọi toạ độ đúng vẫn hiện ra sai chỗ.</summary>
        [Fact]
        public void Orthographic_size_bang_nua_chieu_cao()
        {
            PathDef p = Path;
            Assert.Equal(p.ViewportHeightUnits / 2, p.CameraOrthographicSize, precision: 6);
        }
    }
}
