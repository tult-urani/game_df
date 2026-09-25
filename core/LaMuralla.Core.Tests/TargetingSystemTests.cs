using System;
using System.Collections.Generic;
using System.Linq;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class TargetingSystemTests
    {
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

        private static readonly Vec2 Tower = new(0, 0);

        [Fact]
        public void Khong_co_ai_trong_tam_thi_tra_null()
        {
            var far = new Fake { Id = 1, Position = new Vec2(5, 0) };
            Assert.Null(TargetingSystem.Select(new[] { far }, Tower, 2.0, TargetSelector.FirstInRange));
        }

        [Fact]
        public void Danh_sach_rong_tra_null()
        {
            Assert.Null(TargetingSystem.Select(Array.Empty<Fake>(), Tower, 2.0, TargetSelector.FirstInRange));
        }

        [Fact]
        public void Con_chet_khong_bao_gio_duoc_chon()
        {
            var dead = new Fake { Id = 1, Position = new Vec2(1, 0), DistanceTravelled = 99, IsAlive = false };
            var alive = new Fake { Id = 2, Position = new Vec2(1, 0), DistanceTravelled = 1 };
            Assert.Same(alive, TargetingSystem.Select(new[] { dead, alive }, Tower, 2.0, TargetSelector.FirstInRange));
        }

        /// <summary>Đúng ở mép tầm vẫn tính là trong tầm. Ranh giới dao cạo: nếu
        /// dùng `<` thay `<=` thì tướng không bắn con đứng đúng mép, và người chơi
        /// thấy game lỗi chứ không đọc ra "ngoài tầm 0.001".</summary>
        [Fact]
        public void Dung_o_mep_tam_van_tinh_la_trong_tam()
        {
            var edge = new Fake { Id = 1, Position = new Vec2(2.0, 0) };
            Assert.Same(edge, TargetingSystem.Select(new[] { edge }, Tower, 2.0, TargetSelector.FirstInRange));
        }

        [Fact]
        public void Chon_con_di_xa_nhat_doc_duong()
        {
            var near = new Fake { Id = 1, Position = new Vec2(1, 0), DistanceTravelled = 5 };
            var far = new Fake { Id = 2, Position = new Vec2(1, 0), DistanceTravelled = 20 };
            Assert.Same(far, TargetingSystem.Select(new[] { near, far }, Tower, 2.0, TargetSelector.FirstInRange));
        }

        /// <summary>
        /// 🔴 Luật docs/05 §4.5 mục 1: KHÔNG được dựa vào thứ tự trong list.
        ///
        /// Đảo list là phép thử thẳng vào tim luật đó. Nếu kết quả đổi khi đảo thứ
        /// tự, nghĩa là mục tiêu phụ thuộc thứ tự phần tử — và trong trận thật, list
        /// bị sắp xếp lại mỗi khi một con chết ở giữa.
        /// </summary>
        [Fact]
        public void Ket_qua_khong_doi_khi_dao_thu_tu_list()
        {
            var a = new Fake { Id = 1, Position = new Vec2(1, 0), DistanceTravelled = 10 };
            var b = new Fake { Id = 2, Position = new Vec2(1, 0), DistanceTravelled = 10 };
            var c = new Fake { Id = 3, Position = new Vec2(1, 0), DistanceTravelled = 10 };

            var xuoi = new[] { a, b, c };
            var nguoc = new[] { c, b, a };
            var lung_tung = new[] { b, c, a };

            Fake? r1 = TargetingSystem.Select(xuoi, Tower, 2.0, TargetSelector.FirstInRange);
            Fake? r2 = TargetingSystem.Select(nguoc, Tower, 2.0, TargetSelector.FirstInRange);
            Fake? r3 = TargetingSystem.Select(lung_tung, Tower, 2.0, TargetSelector.FirstInRange);

            Assert.Same(a, r1);   // trùng quãng đường → Id nhỏ nhất thắng
            Assert.Same(r1, r2);
            Assert.Same(r1, r3);
        }

        /// <summary>Thứ tự toàn phần: MỌI hoán vị phải cho cùng một đáp án. 5 con,
        /// 120 hoán vị — không có hoán vị nào được lọt.</summary>
        [Fact]
        public void Moi_hoan_vi_cho_cung_ket_qua()
        {
            var quan = new[]
            {
                new Fake { Id = 10, Position = new Vec2(1, 0), DistanceTravelled = 7 },
                new Fake { Id = 20, Position = new Vec2(1, 0), DistanceTravelled = 7 },   // trùng với #10
                new Fake { Id = 30, Position = new Vec2(0.5, 0), DistanceTravelled = 3 },
                new Fake { Id = 40, Position = new Vec2(1.5, 0), DistanceTravelled = 7 }, // trùng nữa
                new Fake { Id = 50, Position = new Vec2(0.2, 0), DistanceTravelled = 1 },
            };

            var ketQua = Permutations(quan)
                .Select(p => TargetingSystem.Select(p.ToArray(), Tower, 2.0, TargetSelector.FirstInRange))
                .Distinct()
                .ToList();

            Assert.Single(ketQua);
            Assert.Equal(10, ketQua[0]!.Id);   // xa nhất (7), và Id nhỏ nhất trong ba con trùng
        }

        private static IEnumerable<IEnumerable<T>> Permutations<T>(IReadOnlyList<T> src)
        {
            if (src.Count <= 1) { yield return src; yield break; }
            for (int i = 0; i < src.Count; i++)
            {
                T head = src[i];
                var rest = src.Where((_, k) => k != i).ToList();
                foreach (var tail in Permutations(rest))
                    yield return new[] { head }.Concat(tail);
            }
        }

        [Fact]
        public void Highest_absolute_hp_chon_con_nhieu_mau_nhat_du_o_xa_hon()
        {
            var beo = new Fake { Id = 1, Position = new Vec2(1.9, 0), DistanceTravelled = 1, Hp = 5000 };
            var gay = new Fake { Id = 2, Position = new Vec2(0.1, 0), DistanceTravelled = 30, Hp = 50 };
            Assert.Same(beo, TargetingSystem.Select(new[] { beo, gay }, Tower, 2.0,
                                                    TargetSelector.HighestAbsoluteHp));
        }

        /// <summary>Máu TUYỆT ĐỐI, không phải %. Con lính đầy máu (100/100) không
        /// được ưu tiên hơn boss đang thoi thóp (900/9000).</summary>
        [Fact]
        public void Highest_absolute_hp_khong_phai_phan_tram()
        {
            var linh_day_mau = new Fake { Id = 1, Position = new Vec2(1, 0), Hp = 100 };
            var boss_thoi_thop = new Fake { Id = 2, Position = new Vec2(1, 0), Hp = 900 };
            Assert.Same(boss_thoi_thop, TargetingSystem.Select(
                new[] { linh_day_mau, boss_thoi_thop }, Tower, 2.0, TargetSelector.HighestAbsoluteHp));
        }

        [Fact]
        public void Highest_absolute_hp_bang_nhau_thi_theo_quang_duong()
        {
            var gan = new Fake { Id = 1, Position = new Vec2(1, 0), DistanceTravelled = 2, Hp = 500 };
            var xa = new Fake { Id = 2, Position = new Vec2(1, 0), DistanceTravelled = 9, Hp = 500 };
            Assert.Same(xa, TargetingSystem.Select(new[] { gan, xa }, Tower, 2.0,
                                                   TargetSelector.HighestAbsoluteHp));
        }

        [Fact]
        public void The_vang_chi_nham_con_chua_co_the()
        {
            var sach = new Fake { Id = 1, Position = new Vec2(1, 0), DistanceTravelled = 1 };
            var da_vang = new Fake { Id = 2, Position = new Vec2(1, 0), DistanceTravelled = 50, Card = CardState.Yellow };
            var da_do = new Fake { Id = 3, Position = new Vec2(1, 0), DistanceTravelled = 90, Card = CardState.Red };

            Assert.Same(sach, TargetingSystem.Select(new[] { sach, da_vang, da_do }, Tower, 2.0,
                                                     TargetSelector.FirstInRangeWithoutCard));
        }

        /// <summary>
        /// B-01: `the_do` (Lv3) rút thẻ đỏ THẲNG, không cần vàng trước — vì Lv3
        /// không chạy `the_vang` (Lv2) nữa. Nó dùng chung `FirstInRangeWithoutCard`
        /// với thẻ vàng: mỗi con chỉ ăn đúng một thẻ trong đời.
        ///
        /// Thiết kế cũ (vàng → đỏ) chết cùng `FirstInRangeWithYellowCard`, và test
        /// cũ ở đây chết theo. Xem chú thích trong TargetingSystem.
        /// </summary>
        [Fact]
        public void The_do_rut_thang_len_con_chua_co_the()
        {
            var sach = new Fake { Id = 1, Position = new Vec2(1, 0), DistanceTravelled = 5 };
            var da_vang = new Fake { Id = 2, Position = new Vec2(1, 0), DistanceTravelled = 90, Card = CardState.Yellow };
            var da_do = new Fake { Id = 3, Position = new Vec2(1, 0), DistanceTravelled = 99, Card = CardState.Red };

            // Con đã có thẻ (vàng HOẶC đỏ) đều bị loại, dù đi xa hơn nhiều.
            Assert.Same(sach, TargetingSystem.Select(new[] { sach, da_vang, da_do }, Tower, 2.0,
                                                     TargetSelector.FirstInRangeWithoutCard));
        }

        /// <summary>Ai cũng có thẻ rồi → trả null, không được rút thẻ chồng lên
        /// con đã bị phạt.</summary>
        [Fact]
        public void Tra_null_khi_moi_con_da_co_the()
        {
            var da_vang = new Fake { Id = 1, Position = new Vec2(1, 0), Card = CardState.Yellow };
            var da_do = new Fake { Id = 2, Position = new Vec2(1, 0), Card = CardState.Red };
            Assert.Null(TargetingSystem.Select(new[] { da_vang, da_do }, Tower, 2.0,
                                               TargetSelector.FirstInRangeWithoutCard));
        }

        /// <summary>Tầm được đo từ vị trí TƯỚNG, không phải từ gốc toạ độ.</summary>
        [Fact]
        public void Tam_do_tu_vi_tri_tuong()
        {
            var tuong = new Vec2(-4.73, 3.69);                       // f02 thật
            var trong = new Fake { Id = 1, Position = new Vec2(-4.0, 3.69) };
            var ngoai = new Fake { Id = 2, Position = new Vec2(0, 0) };

            Assert.Same(trong, TargetingSystem.Select(new[] { trong, ngoai }, tuong, 1.4,
                                                      TargetSelector.FirstInRange));
        }
    }
}
