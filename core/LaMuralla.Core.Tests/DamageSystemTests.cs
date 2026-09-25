using System;
using System.Collections.Generic;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class DamageSystemTests
    {
        private static (DamageSystem, List<KillInfo>) Fresh()
        {
            var d = new DamageSystem();
            var kills = new List<KillInfo>();
            d.EnemyKilled += k => kills.Add(k);
            return (d, kills);
        }

        [Fact]
        public void Sat_thuong_khong_chi_mang_thi_tru_mau()
        {
            var (d, kills) = Fresh();
            d.Register(1, 100);

            DamageResult r = d.Apply(1, 30, "f01");

            Assert.False(r.Killed);
            Assert.Equal(30, r.Dealt);
            Assert.Equal(0, r.Overkill);
            Assert.Equal(70, d.HpOf(1));
            Assert.Empty(kills);
        }

        [Fact]
        public void Dung_bang_mau_con_lai_la_chet()
        {
            var (d, kills) = Fresh();
            d.Register(1, 100);

            DamageResult r = d.Apply(1, 100, "f01");

            Assert.True(r.Killed);
            Assert.Equal(0, r.Overkill);
            Assert.Single(kills);
            Assert.False(d.IsAlive(1));
        }

        /// <summary>`01` §8 **FM-07**: "Damage thừa bỏ đi". Không được trừ âm, không
        /// được mang phần thừa sang con khác.</summary>
        [Fact]
        public void Danh_thua_thi_vut_phan_thua()
        {
            var (d, kills) = Fresh();
            d.Register(1, 10);

            DamageResult r = d.Apply(1, 90, "f01");

            Assert.True(r.Killed);
            Assert.Equal(10, r.Dealt);      // chỉ trừ được 10 máu có thật
            Assert.Equal(80, r.Overkill);   // 80 vứt đi
            Assert.Equal(80, kills[0].Overkill);
        }

        /// <summary>
        /// 🔴 FM-07 — lý do tồn tại của cả lớp này.
        ///
        /// Hai tướng cùng bắn phát cuối vào con còn 10 máu. Nếu mỗi tướng tự trừ
        /// máu rồi tự kết luận "tôi giết nó" thì cả hai đều đúng, và ví được cộng
        /// HAI lần cho MỘT con. Kinh tế vỡ sau ~10 wave.
        ///
        /// Ở đây: đúng một event, và nó ghi công đúng tướng bắn TRƯỚC.
        /// </summary>
        [Fact]
        public void Hai_tuong_cung_ban_phat_cuoi_chi_mot_lan_ghi_cong()
        {
            var (d, kills) = Fresh();
            d.Register(1, 10);

            DamageResult bati = d.Apply(1, 90, "f01");   // Batigol chạm trước
            DamageResult pulga = d.Apply(1, 56, "f05");  // La Pulga chạm sau, con đã chết

            Assert.True(bati.Killed);
            Assert.False(pulga.Killed);
            Assert.True(pulga.TargetAlreadyDead);
            Assert.Equal(0, pulga.Dealt);

            Assert.Single(kills);
            Assert.Equal("f01", kills[0].KillerSlotId);   // tiền về đúng Batigol
        }

        /// <summary>Đạn tới muộn không phải lỗi — mục tiêu chết trong lúc đạn bay
        /// là chuyện thường. Không được ném, chỉ được lặng lẽ bỏ qua.</summary>
        [Fact]
        public void Ban_vao_con_da_chet_khong_nem_khong_phat_event()
        {
            var (d, kills) = Fresh();
            d.Register(1, 10);
            d.Apply(1, 10, "f01");

            DamageResult r = d.Apply(1, 999, "f02");

            Assert.True(r.TargetAlreadyDead);
            Assert.False(r.Killed);
            Assert.Single(kills);
        }

        [Fact]
        public void Ban_vao_con_chua_bao_gio_ton_tai_cung_bo_qua()
        {
            var (d, kills) = Fresh();
            Assert.True(d.Apply(999, 50, "f01").TargetAlreadyDead);
            Assert.Empty(kills);
        }

        /// <summary>Dù bắn bao nhiêu đòn thừa, event chỉ đúng một. Đây là bất biến
        /// mà FM-07 đòi hỏi, kiểm ở dạng mạnh nhất.</summary>
        [Fact]
        public void Muoi_tuong_cung_ban_van_dung_mot_event()
        {
            var (d, kills) = Fresh();
            d.Register(1, 5);

            for (int i = 0; i < 10; i++) d.Apply(1, 100, $"f{i:00}");

            Assert.Single(kills);
            Assert.Equal("f00", kills[0].KillerSlotId);
        }

        /// <summary>Người nghe event gọi ngược lại Apply (hiệu ứng dây chuyền) phải
        /// thấy con đã chết. Nếu xoá SAU khi phát event thì chỗ này phát vô hạn.</summary>
        [Fact]
        public void Nguoi_nghe_goi_nguoc_lai_khong_gay_phat_hai_lan()
        {
            var d = new DamageSystem();
            var kills = new List<KillInfo>();
            d.EnemyKilled += k =>
            {
                kills.Add(k);
                d.Apply(k.EnemyId, 100, "f99");   // thử đánh lại chính con vừa chết
            };
            d.Register(1, 10);

            d.Apply(1, 10, "f01");

            Assert.Single(kills);
        }

        [Fact]
        public void Sat_thuong_am_thi_nem()
        {
            var (d, _) = Fresh();
            d.Register(1, 100);
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Apply(1, -5, "f01"));
        }

        [Fact]
        public void Sat_thuong_bang_0_khong_giet_ai()
        {
            var (d, kills) = Fresh();
            d.Register(1, 100);
            DamageResult r = d.Apply(1, 0, "f01");
            Assert.False(r.Killed);
            Assert.Equal(100, d.HpOf(1));
            Assert.Empty(kills);
        }

        [Fact]
        public void Dang_ky_trung_id_thi_nem()
        {
            var (d, _) = Fresh();
            d.Register(1, 100);
            Assert.Throws<InvalidOperationException>(() => d.Register(1, 50));
        }

        [Fact]
        public void Dang_ky_mau_khong_duong_thi_nem()
        {
            var (d, _) = Fresh();
            Assert.Throws<ArgumentOutOfRangeException>(() => d.Register(1, 0));
        }

        /// <summary>
        /// Dibu `can_pha` khai `dropsBounty: false`. Quân lọt lưới cũng không rơi tiền.
        /// Dùng Apply(∞) cho hai ca này sẽ phát KillInfo → MatchController trả tiền →
        /// người chơi được thưởng vì để quân tới tận cầu môn.
        /// </summary>
        [Fact]
        public void Xoa_khong_qua_sat_thuong_thi_khong_ai_duoc_ghi_cong()
        {
            var (d, kills) = Fresh();
            d.Register(1, 100);

            Assert.True(d.RemoveWithoutKill(1));

            Assert.False(d.IsAlive(1));
            Assert.Empty(kills);              // 🔴 không event = không tiền
            Assert.False(d.RemoveWithoutKill(1));
        }

        [Fact]
        public void Dem_so_con_con_song()
        {
            var (d, _) = Fresh();
            d.Register(1, 10);
            d.Register(2, 10);
            d.Register(3, 10);
            Assert.Equal(3, d.AliveCount);

            d.Apply(1, 10, "f01");
            d.RemoveWithoutKill(2);

            Assert.Equal(1, d.AliveCount);
        }

        /// <summary>Nhiều con cùng lúc: sát thương lên con này không đụng con kia.
        /// Nghe hiển nhiên, nhưng đây là bug kinh điển khi lan/xuyên chia sẻ state.</summary>
        [Fact]
        public void Sat_thuong_khong_ro_ri_sang_con_khac()
        {
            var (d, kills) = Fresh();
            d.Register(1, 10);
            d.Register(2, 10);

            d.Apply(1, 500, "f01");            // thừa 490

            Assert.Equal(10, d.HpOf(2));       // con 2 nguyên vẹn
            Assert.Single(kills);
        }
    }
}
