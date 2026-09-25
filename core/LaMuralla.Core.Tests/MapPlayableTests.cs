using System.Collections.Generic;
using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    /// <summary>
    /// 🔴 MỖI MAP MỘT BÀI KIỂM PROFILE HARD CHẶN BUILD AOE THAM LAM.
    ///
    /// Vì sao bắt buộc: `tools/balance_sim.py` KHÔNG dùng được để chốt số. Đo vòng 22 —
    /// sim báo headroom trung bình 2.09 ("quá dễ", 18/20 wave ngoài dải) trong khi engine
    /// thật thắng sát nút còn 11/20 máu. Ba hệ số η/σ/τ của sim là ước lượng chưa đo và
    /// `economy.json` tự cảnh báo chúng nhân nhau thì lệch tới 73%.
    ///
    /// Build này cố nâng D10S AoE lên Lv3 sớm, đúng đường thắng quá dễ mà người chơi
    /// báo cáo. Nó phải bị chặn, nhưng vẫn tiến đủ xa để đầu trận không thành vách.
    /// </summary>
    public class MapPlayableTests
    {
        private readonly ITestOutputHelper _o;
        public MapPlayableTests(ITestOutputHelper o) => _o = o;

        /// <summary>Trần quân sống cùng lúc. m00 đo được 27; map đa tuyến dễ đội lên
        /// vì nhiều dòng cùng chảy. Vượt trần = phải đo FPS trên máy thật trước khi ship.</summary>
        private const int PeakLiveCap = 40;

        /// <summary>
        /// Mốc tiến tối thiểu của bot AoE tham lam. Profile Hard phải chặn bot này
        /// trước khi thắng, nhưng không được làm nó chết ngay ở các wave mở đầu.
        /// Đây không phải chứng minh map thắng được; `balance_sim.py` lo build tối ưu.
        /// </summary>
        internal static readonly (string File, int MinWave)[] Thang =
        {
            ("m00-la-muralla.json",       5),
            ("m01-el-potrero.json",      10),
            ("m02-la-bombonera.json",    10),
            ("m03-dos-rios.json",         5),
            ("m04-el-cruce.json",         5),
            ("m05-la-confluencia.json",  15),
            ("m06-el-caracol.json",       8),
            ("m07-tres-puertas.json",     5),
            ("m08-el-mirador.json",       4),
            ("m09-la-horquilla.json",     8),
            ("m10-la-muralla-final.json", 4),
        };

        public static IEnumerable<object[]> MoiMap => Thang.Select(x => new object[] { x.File, x.MinWave });

        [Theory]
        [MemberData(nameof(MoiMap))]
        public void Build_AoE_tham_lam_bi_Hard_chan_nhung_khong_chet_qua_som(string map, int minWave)
        {
            GameConfig cfg = RealConfigValidateTests.Load(map);
            var gp = new GreedyPlayer(cfg);
            var r = GreedyPlayer.PlayFullMatch(cfg);

            _o.WriteLine($"{cfg.MapId} — {cfg.MapDisplayName}");
            _o.WriteLine($"  ô đám đông ({gp.CrowdSlots.Count}): {string.Join(",", gp.CrowdSlots)}");
            _o.WriteLine($"  ô tầm xa   ({gp.ReachSlots.Count}): {string.Join(",", gp.ReachSlots)}");
            _o.WriteLine($"  kết cục: {r.Phase} · wave {r.Wave} · máu {r.Goal}/{cfg.Economy.GoalHealth}"
                       + $" · kiếm cả trận {r.Earned} · đỉnh quân đồng thời {r.PeakLive}");

            Assert.Equal(MatchPhase.Lost, r.Phase);
            Assert.True(r.Wave >= minWave,
                $"{map}: bot chỉ tới W{r.Wave}, sàn tiến độ là W{minWave}");
            Assert.True(r.PeakLive <= PeakLiveCap,
                $"{map}: đỉnh {r.PeakLive} quân cùng lúc > trần {PeakLiveCap} — phải đo FPS trước khi ship");
        }

        /// <summary>
        /// Bốn mốc boss là hợp đồng chung; HP mỗi boss phải tăng qua từng mốc.
        /// </summary>
        [Fact]
        public void Mau_boss_tang_dan_o_ca_11_map()
        {
            foreach (var (file, _) in Thang)
            {
                GameConfig cfg = RealConfigValidateTests.Load(file);
                BossDef boss = cfg.Bosses.Single(x => x.Id == "o_capitao");
                Assert.Equal(new[] { 5, 10, 15, 20 }, boss.Appearances.Select(x => x.Wave));
                for (int i = 1; i < boss.Appearances.Count; i++)
                    Assert.True(boss.Appearances[i].Hp > boss.Appearances[i - 1].Hp,
                        $"{cfg.MapId}: HP boss W{boss.Appearances[i].Wave} không tăng");
            }
        }

        [Fact]
        public void Rewarded_recovery_cho_build_yeu_them_co_hoi_that()
        {
            GameConfig withoutCfg = RealConfigValidateTests.Load("m00-la-muralla.json");
            GameConfig withCfg = RealConfigValidateTests.Load("m00-la-muralla.json");
            var without = GreedyPlayer.PlayFullMatch(withoutCfg, seed: 7, useRewardedRecovery: false);
            var with = GreedyPlayer.PlayFullMatch(withCfg, seed: 7, useRewardedRecovery: true);

            Assert.True(with.Wave > without.Wave || with.Earned > without.Earned,
                $"reward không tăng cơ hội: không ad W{without.Wave}/{without.Earned}, " +
                $"có ad W{with.Wave}/{with.Earned}");
        }

        /// <summary>
        /// Trần chi tiêu theo map: tiền kiếm cả trận phải NHỎ HƠN giá của build đắt nhất
        /// có thể dựng. Vượt trần = cuối trận tiền hết ý nghĩa, người chơi mua được tất.
        ///
        /// Vòng 22 đã vỡ luật này 16% mà không gì chặn — nó chỉ sống trong một ghi chú
        /// `_marginNote`. Giờ là test.
        /// </summary>
        [Theory]
        [MemberData(nameof(MoiMap))]
        public void Tien_toi_da_ca_tran_khong_vuot_tran_chi_tieu(string map, int _)
        {
            GameConfig cfg = RealConfigValidateTests.Load(map);
            int lifetime = cfg.Economy.StartingCash;
            foreach (WaveDef wave in cfg.Waves)
            {
                foreach (SpawnGroup group in wave.Spawns)
                {
                    EnemyDef enemy = cfg.Enemies.Single(x => x.Id == group.Enemy);
                    lifetime += group.Count * cfg.BountyOf(enemy, wave.Wave);
                }
                foreach (BossSpawn spawn in wave.Bosses)
                {
                    BossDef boss = cfg.Bosses.Single(x => x.Id == spawn.Id);
                    lifetime += boss.Appearances.Single(x => x.Wave == wave.Wave).Bounty;
                }
                lifetime += EconomyService.WaveClearBonus(wave.Wave);
                if (wave.Wave < cfg.Waves.Count)
                    lifetime += Round.HalfUp(cfg.RestBetweenWavesSec * cfg.Economy.SkipBonusPerSecond);
            }

            int fieldSlots = cfg.Path.Slots.Count(s => s.IsField);
            int pulgaFull = 0, dibuFull = 0;
            foreach (TowerDef t in cfg.Towers)
            {
                int sum = t.Levels.Sum(l => l.Cost);
                if (t.Id == "la_pulga") pulgaFull = sum;
                if (t.Id == "dibu") dibuFull = sum;
            }
            int tran = fieldSlots * pulgaFull + dibuFull;

            _o.WriteLine($"{cfg.MapId}: tiền tối đa {lifetime} · trần {fieldSlots}×{pulgaFull}+{dibuFull} = {tran}"
                       + $" · biên {(tran - lifetime) * 100.0 / tran:0.0}%");
            Assert.True(lifetime < tran,
                $"{map}: tiền tối đa {lifetime} >= trần {tran} → cuối trận tiền vô nghĩa");
        }
    }
}
