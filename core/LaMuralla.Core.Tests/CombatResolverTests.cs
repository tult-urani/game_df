using System;
using System.Linq;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    public class CombatResolverTests
    {
        private readonly ITestOutputHelper _out;
        public CombatResolverTests(ITestOutputHelper o) => _out = o;

        private sealed class Fake : ITarget
        {
            public int Id { get; set; }
            public Vec2 Position { get; set; }
            public double DistanceTravelled { get; set; }
            public double Hp { get; set; } = 100;
            public double MaxHp { get; set; } = 100;
            public bool IsAlive { get; set; } = true;
            public CardState Card { get; set; } = CardState.None;
            public override string ToString() => $"#{Id}";
        }

        private static Fake At(int id, double x, double y) => new() { Id = id, Position = new Vec2(x, y) };

        // ── Lan ─────────────────────────────────────────────────────────────

        [Fact]
        public void Lan_trung_moi_con_trong_ban_kinh()
        {
            var a = At(1, 0, 0); var b = At(2, 1.0, 0); var c = At(3, 5, 0);
            var hit = CombatResolver.Splash(new[] { a, b, c }, new Vec2(0, 0), 1.2);

            Assert.Equal(2, hit.Count);
            Assert.DoesNotContain(c, hit);
        }

        [Fact]
        public void Lan_dung_o_mep_van_trung()
        {
            var e = At(1, 1.2, 0);
            Assert.Single(CombatResolver.Splash(new[] { e }, new Vec2(0, 0), 1.2));
        }

        [Fact]
        public void Lan_bo_qua_con_da_chet()
        {
            var dead = At(1, 0, 0); dead.IsAlive = false;
            Assert.Empty(CombatResolver.Splash(new[] { dead }, new Vec2(0, 0), 2.0));
        }

        /// <summary>
        /// 🔴 docs/05 §4.5 mục 2: tâm lan là VỊ TRÍ MỤC TIÊU, không phải điểm đạn
        /// chạm. Đạn bám mục tiêu (`05` §4.3) nên lúc chạm, quân đã đi tiếp — hai
        /// điểm đó khác nhau. Lấy điểm đạn chạm thì vùng lan lệch về phía sau, và
        /// giả định "D10S chạm 3 mục tiêu" (`04` §8 mục 4) sai.
        ///
        /// Test dựng đúng tình huống: đạn chạm ở (0,0) nhưng mục tiêu đã tới (1,0).
        /// Lấy tâm đúng → trúng 3 con quanh mục tiêu. Lấy sai → chỉ trúng 1.
        /// </summary>
        [Fact]
        public void Tam_lan_la_vi_tri_muc_tieu_khong_phai_diem_dan_cham()
        {
            var diemDanCham = new Vec2(0, 0);
            var viTriMucTieu = new Vec2(1.0, 0);

            var mucTieu = At(1, 1.0, 0);
            var banA = At(2, 1.5, 0);      // quanh MỤC TIÊU
            var banB = At(3, 1.9, 0);      // quanh MỤC TIÊU
            var xa = At(4, -0.9, 0);       // quanh ĐIỂM ĐẠN CHẠM
            var all = new[] { mucTieu, banA, banB, xa };

            var dung = CombatResolver.Splash(all, viTriMucTieu, 1.0);
            var sai = CombatResolver.Splash(all, diemDanCham, 1.0);

            Assert.Equal(3, dung.Count);                  // mục tiêu + 2 bạn
            Assert.DoesNotContain(xa, dung);

            Assert.Equal(2, sai.Count);                   // lệch: bắt nhầm `xa`
            Assert.Contains(xa, sai);
            _out.WriteLine($"tâm đúng: {dung.Count} con · tâm sai: {sai.Count} con");
        }

        [Fact]
        public void Ban_kinh_am_thi_nem()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => CombatResolver.Splash(new[] { At(1, 0, 0) }, new Vec2(0, 0), -1));
        }

        // ── Xuyên hàng ──────────────────────────────────────────────────────

        /// <summary>docs/05 §4.5 mục 3: "Trúng mọi quân giao với tia."</summary>
        [Fact]
        public void Xuyen_trung_ca_hang_tren_duong_thang()
        {
            var q = Enumerable.Range(1, 4).Select(i => At(i, i * 0.5, 0)).ToArray();
            var hit = CombatResolver.Line(q, new Vec2(0, 0), new Vec2(1, 0), length: 3.0, width: 0.5);
            Assert.Equal(4, hit.Count);
        }

        /// <summary>
        /// 🔴 "Trên khúc cua có thể chỉ trúng 1 con — ĐÓ LÀ Ý ĐỒ: Batigol thưởng cho
        /// việc đặt ở đoạn thẳng" (`05` §4.5 mục 3).
        ///
        /// Đây KHÔNG phải bug cần sửa. Nếu tia bám theo mục tiêu thì nó sẽ luôn trúng
        /// cả hàng bất kể đường cong, và bản sắc của Batigol biến mất.
        /// </summary>
        [Fact]
        public void Xuyen_tren_khuc_cua_chi_trung_it_va_do_la_y_do()
        {
            var mucTieu = At(1, 1.0, 0);
            var congA = At(2, 1.8, 0.9);    // đã rẽ khỏi tia
            var congB = At(3, 2.4, 1.8);
            var hit = CombatResolver.Line(new[] { mucTieu, congA, congB },
                                          new Vec2(0, 0), mucTieu.Position, 3.0, 0.5);

            Assert.Single(hit);
            Assert.Same(mucTieu, hit[0]);
        }

        [Fact]
        public void Xuyen_khong_trung_con_ngoai_be_rong_tia()
        {
            var trong = At(1, 1, 0.24);     // nửa rộng = 0.25
            var ngoai = At(2, 1, 0.26);
            var hit = CombatResolver.Line(new[] { trong, ngoai }, new Vec2(0, 0), new Vec2(1, 0), 3.0, 0.5);
            Assert.Single(hit);
            Assert.Same(trong, hit[0]);
        }

        /// <summary>Tia là ĐOẠN dài bằng tầm, không phải đường thẳng vô hạn — quân
        /// quá tầm không trúng dù nằm đúng trên phương.</summary>
        [Fact]
        public void Xuyen_dung_o_tam_khong_voi_toi_con_xa_hon()
        {
            var trong = At(1, 1.4, 0);
            var ngoai = At(2, 1.5, 0);
            var hit = CombatResolver.Line(new[] { trong, ngoai }, new Vec2(0, 0), new Vec2(1, 0),
                                          length: 1.4, width: 0.5);   // Batigol Lv3 tầm 1.4
            Assert.Single(hit);
            Assert.Same(trong, hit[0]);
        }

        /// <summary>Quân đứng SAU LƯNG tháp không trúng — tia đi một chiều.</summary>
        [Fact]
        public void Xuyen_khong_trung_con_sau_lung_thap()
        {
            var sau = At(1, -1, 0);
            Assert.Empty(CombatResolver.Line(new[] { sau }, new Vec2(0, 0), new Vec2(1, 0), 3.0, 0.5));
        }

        /// <summary>Mục tiêu đứng ĐÚNG trên tháp: không có hướng. Chia cho 0 ra NaN,
        /// mọi so sánh sau thành false → tia im lặng không trúng ai.</summary>
        [Fact]
        public void Muc_tieu_trung_vi_tri_thap_khong_gay_NaN()
        {
            var tren = At(1, 0, 0);
            var khac = At(2, 1, 0);
            var hit = CombatResolver.Line(new[] { tren, khac }, new Vec2(0, 0), new Vec2(0, 0), 3.0, 0.5);
            Assert.Empty(hit);   // không ném, không NaN
        }

        [Fact]
        public void Xuyen_bo_qua_con_da_chet()
        {
            var dead = At(1, 1, 0); dead.IsAlive = false;
            Assert.Empty(CombatResolver.Line(new[] { dead }, new Vec2(0, 0), new Vec2(1, 0), 3.0, 0.5));
        }

        [Fact]
        public void Tia_vo_ly_thi_nem()
        {
            var q = new[] { At(1, 1, 0) };
            Assert.Throws<ArgumentOutOfRangeException>(() => CombatResolver.Line(q, new Vec2(0, 0), new Vec2(1, 0), 0, 0.5));
            Assert.Throws<ArgumentOutOfRangeException>(() => CombatResolver.Line(q, new Vec2(0, 0), new Vec2(1, 0), 3, 0));
        }

        // ── Tia là HÌNH CHỮ NHẬT, không phải viên nang ──────────────────────

        /// <summary>
        /// 🔴 Cách viết hiển nhiên — đo khoảng cách tới ĐOẠN thẳng rồi so nửa bề
        /// rộng — mô tả một VIÊN NANG: chữ nhật cộng hai chỏm tròn bán kính 0.25.
        /// Chỏm đó cho Batigol thêm 0.25 unit ngoài tầm khai báo, tức +18% trên
        /// tầm Lv3 = 1.4. Không ai khai, không ai đo.
        ///
        /// Quét quanh mũi tia: mọi điểm có `x > length` phải TRƯỢT, kể cả khi nó
        /// nằm trong 0.25 unit của điểm cuối.
        /// </summary>
        [Theory]
        [InlineData(1.39, 0.00, true)]    // trong tia
        [InlineData(1.40, 0.00, true)]    // đúng mũi tia
        [InlineData(1.41, 0.00, false)]   // quá 0.01 → viên nang sẽ trúng, chữ nhật thì không
        [InlineData(1.60, 0.00, false)]   // quá 0.2  → viên nang vẫn trúng
        [InlineData(1.30, 0.24, true)]    // trong bề rộng
        [InlineData(1.30, 0.26, false)]   // ngoài bề rộng
        [InlineData(-0.10, 0.00, false)]  // sau lưng tháp
        public void Tia_la_chu_nhat_khong_phai_vien_nang(double x, double y, bool shouldHit)
        {
            var e = At(1, x, y);
            var hit = CombatResolver.Line(new[] { e }, new Vec2(0, 0), new Vec2(1, 0),
                                          length: 1.4, width: 0.5);   // Batigol Lv3
            Assert.Equal(shouldHit, hit.Count == 1);
        }

        /// <summary>Tia chéo cũng phải là chữ nhật — không được để phép chiếu sai
        /// dấu làm nó thành hình khác khi xoay.</summary>
        [Fact]
        public void Tia_cheo_van_dung_hinh_hoc()
        {
            var from = new Vec2(0, 0);
            var towards = new Vec2(1, 1);              // hướng 45°
            double L = 1.4;

            // Đúng trên phương, trong tầm: 1.0 unit dọc tia
            var trong = At(1, 1.0 / Math.Sqrt(2), 1.0 / Math.Sqrt(2));
            // Đúng trên phương, quá tầm: 1.5 unit dọc tia
            var ngoai = At(2, 1.5 / Math.Sqrt(2), 1.5 / Math.Sqrt(2));

            var hit = CombatResolver.Line(new[] { trong, ngoai }, from, towards, L, 0.5);
            Assert.Single(hit);
            Assert.Same(trong, hit[0]);
        }
    }
}
