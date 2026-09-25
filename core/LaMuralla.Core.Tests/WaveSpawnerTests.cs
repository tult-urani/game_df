using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    public class WaveSpawnerTests
    {
        private readonly ITestOutputHelper _out;
        public WaveSpawnerTests(ITestOutputHelper o) => _out = o;

        private static readonly GameConfig Cfg = RealConfigValidateTests.Load();
        private static WaveSpawner New() => new(Cfg);

        [Fact]
        public void W1_dung_so_con_va_dung_khoang_cach()
        {
            var s = New().Schedule(1).ToList();

            // Số con SUY TỪ config, không gõ cứng: vòng 22 đổi W1 6 → 8 và bản cũ
            // gõ "6" ở ba chỗ nên vỡ cả ba. Cái test này canh KHOẢNG CÁCH spawn,
            // không canh kích thước wave — luật 4 của validator lo phần đó.
            int n = Cfg.Waves.First(x => x.Wave == 1).CountOf("adepto");

            Assert.Equal(n, s.Count);
            Assert.All(s, x => Assert.Equal("adepto", x.EnemyId));
            Assert.Equal(0, s[0].TimeSec);
            Assert.Equal(0.7, s[1].TimeSec, precision: 6);          // spawnIntervalSec
            Assert.Equal((n - 1) * 0.7, s[^1].TimeSec, precision: 6);
        }

        /// <summary>Máu phải khớp `hpScaling.MultiplierAt(wave)` với half_up —
        /// KHÔNG phải banker's rounding.</summary>
        [Fact]
        public void Mau_da_nhan_he_so_wave()
        {
            EnemyDef adepto = Cfg.Enemies.First(e => e.Id == "adepto");
            var s = New().Schedule(7).ToList();
            Assert.All(s.Where(x => x.EnemyId == "adepto"),
                       x => Assert.Equal(Cfg.HpOf(adepto, 7), x.Hp));
        }

        /// <summary>
        /// 🔴 `waves.json._bossSpawnNote`: "Boss spawn SAU khi toàn bộ spawns của
        /// wave ra hết. Đây là giả định nền của mô hình hoả lực tập trung (docs/04
        /// §3.3) — đổi cái này là đổi cả mô hình."
        /// </summary>
        [Fact]
        public void Boss_ra_SAU_moi_con_thuong()
        {
            foreach (int wave in new[] { 10, 20 })
            {
                var s = New().Schedule(wave).ToList();
                ScheduledSpawn boss = s.Single(x => x.IsBoss);

                Assert.Equal(s.Count - 1, s.IndexOf(boss));           // đứng cuối list
                Assert.All(s.Where(x => !x.IsBoss),
                           x => Assert.True(x.TimeSec < boss.TimeSec,
                               $"W{wave}: {x.EnemyId} ra lúc {x.TimeSec} ≥ boss {boss.TimeSec}"));
            }
        }

        /// <summary>Máu boss lấy THẲNG từ appearances, KHÔNG nhân hpScaling —
        /// nhân nữa là nhân đôi độ khó mà không ai định.</summary>
        [Fact]
        public void Mau_boss_lay_thang_tu_appearances()
        {
            BossDef b = Cfg.Bosses.First(x => x.Id == "o_capitao");
            Assert.Equal(b.Appearances.First(a => a.Wave == 10).Hp,
                         New().Schedule(10).Single(x => x.IsBoss).Hp);
            Assert.Equal(b.Appearances.First(a => a.Wave == 20).Hp,
                         New().Schedule(20).Single(x => x.IsBoss).Hp);
        }

        [Fact]
        public void Khong_wave_nao_co_2_boss()
        {
            for (int w = 1; w <= 20; w++)
                Assert.True(New().Schedule(w).Count(x => x.IsBoss) <= 1);
        }

        [Fact]
        public void Loai_khai_0_con_thi_khong_ra_san()
        {
            // W1–W2 khai `tifoso: 0`, `tambor: 0`
            Assert.DoesNotContain(New().Schedule(1), x => x.EnemyId == "tifoso");
            Assert.DoesNotContain(New().Schedule(2), x => x.EnemyId == "tambor");
        }

        /// <summary>
        /// Quân XEN KẼ, không gom theo loại. Mô hình cân bằng (`04`) coi wave là một
        /// DÒNG ĐỀU — gom 7 tambor (máu ×2) vào cuối wave thì mô hình nói về một
        /// trận khác.
        ///
        /// Kiểm bằng "chuỗi dài nhất cùng một loại": gom theo loại cho chuỗi = số
        /// lượng loại đông nhất; xen kẽ cho chuỗi ngắn.
        /// </summary>
        [Fact]
        public void Quan_xen_ke_chu_khong_gom_theo_loai()
        {
            var s = New().Schedule(20).Where(x => !x.IsBoss).Select(x => x.EnemyId).ToList();

            int longest = 1, cur = 1;
            for (int i = 1; i < s.Count; i++)
            {
                cur = s[i] == s[i - 1] ? cur + 1 : 1;
                if (cur > longest) longest = cur;
            }

            _out.WriteLine($"W20 thứ tự: {string.Join(" ", s.Select(x => x[0]))}");
            _out.WriteLine($"chuỗi dài nhất cùng loại: {longest}");

            // W20 = {adepto 14, tifoso 16, tambor 7}. Gom theo loại → chuỗi 16.
            Assert.True(longest <= 3, $"chuỗi {longest} con cùng loại liên tiếp — đang gom, không xen kẽ");
        }

        /// <summary>Lịch phải TẤT ĐỊNH: gọi hai lần ra y hệt. Nếu phụ thuộc thứ tự
        /// khoá Dictionary hay random thì cùng một wave khó dễ khác nhau mỗi lần
        /// chơi, và bảng cân bằng không tái hiện được.</summary>
        [Fact]
        public void Lich_tat_dinh()
        {
            for (int w = 1; w <= 20; w++)
            {
                var a = New().Schedule(w).Select(x => x.EnemyId).ToList();
                var b = New().Schedule(w).Select(x => x.EnemyId).ToList();
                Assert.Equal(a, b);
            }
        }

        [Fact]
        public void Du_20_wave_va_so_con_khop_config()
        {
            for (int w = 1; w <= 20; w++)
            {
                WaveDef def = Cfg.Waves.First(x => x.Wave == w);
                int expected = def.TotalEnemies + (def.Boss != null ? 1 : 0);
                Assert.Equal(expected, New().CountAt(w));
            }
        }

        [Fact]
        public void Wave_khong_ton_tai_thi_nem()
        {
            Assert.Throws<ConfigException>(() => New().Schedule(21));
            Assert.Throws<ConfigException>(() => New().Schedule(0));
        }

        /// <summary>In bảng để đọc bằng mắt — số con và độ dài spawn phải tăng dần
        /// một cách hợp lý, trừ hai chỗ nghỉ có chủ đích ở W8 và W15.</summary>
        [Fact]
        public void In_bang_lich_spawn()
        {
            _out.WriteLine("wave |  con | spawn kéo dài | boss");
            for (int w = 1; w <= 20; w++)
            {
                var s = New().Schedule(w);
                _out.WriteLine($"  W{w,-3}| {s.Count,4} | {New().SpawnDurationOf(w),10:0.0}s | " +
                               (s.Any(x => x.IsBoss) ? "BOSS" : ""));
            }
        }
    }
}
