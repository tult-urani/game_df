using System.Collections.Generic;
using System.Linq;
using LaMuralla.Core.Config;
using LaMuralla.Core.Match;
using Xunit;
using Xunit.Abstractions;

namespace LaMuralla.Core.Tests
{
    /// <summary>
    /// 🔴 MỖI MAP MỘT BÀI KIỂM "CÓ CHƠI ĐƯỢC KHÔNG".
    ///
    /// Vì sao bắt buộc: `tools/balance_sim.py` KHÔNG dùng được để chốt số. Đo vòng 22 —
    /// sim báo headroom trung bình 2.09 ("quá dễ", 18/20 wave ngoài dải) trong khi engine
    /// thật thắng sát nút còn 11/20 máu. Ba hệ số η/σ/τ của sim là ước lượng chưa đo và
    /// `economy.json` tự cảnh báo chúng nhân nhau thì lệch tới 73%.
    ///
    /// Một map không có bài kiểm này là một map KHÔNG AI BIẾT có chơi được không.
    /// Thêm map mới thì thêm một dòng vào `Thang` — `GreedyPlayer` tự chọn ô từ hình học.
    /// </summary>
    public class MapPlayableTests
    {
        private readonly ITestOutputHelper _o;
        public MapPlayableTests(ITestOutputHelper o) => _o = o;

        /// <summary>Trần quân sống cùng lúc. m00 đo được 27; map đa tuyến dễ đội lên
        /// vì nhiều dòng cùng chảy. Vượt trần = phải đo FPS trên máy thật trước khi ship.</summary>
        private const int PeakLiveCap = 40;

        /// <summary>
        /// Thang 11 map theo đúng thứ tự chơi, kèm MỨC SÀN máu cầu môn còn lại.
        ///
        /// Máu còn lại là thước đo THÔ (mỗi boss lọt = 5 máu) nhưng nó là thước duy
        /// nhất đo được bằng engine thật. Sàn giảm dần theo thứ tự map = phát biểu
        /// "map sau khó hơn map trước", và `Thang_do_kho_giam_dan` ép nó bằng SỐ ĐO
        /// chứ không chỉ bằng sàn — sàn một mình không chặn được một map dễ bất ngờ.
        ///
        /// ⚠️ `GreedyPlayer` khai thác hình học KÉM HƠN người chơi thật, nên đây là
        /// SÀN chứ không phải trần. Người chơi thấy dễ hơn số này là bình thường.
        /// </summary>
        internal static readonly (string File, int SanMau)[] Thang =
        {
            ("m00-la-muralla.json",      11),
            ("m01-el-potrero.json",      10),
            ("m02-la-bombonera.json",     9),
            ("m03-dos-rios.json",         8),
            ("m04-el-cruce.json",         8),
            ("m05-la-confluencia.json",   6),
            ("m06-el-caracol.json",       6),
            ("m07-tres-puertas.json",     3),
            ("m08-el-mirador.json",       3),
            ("m09-la-horquilla.json",     3),
            ("m10-la-muralla-final.json", 3),
        };

        public static IEnumerable<object[]> MoiMap => Thang.Select(x => new object[] { x.File, x.SanMau });

        [Theory]
        [MemberData(nameof(MoiMap))]
        public void Nguoi_choi_biet_tieu_tien_thi_di_het_20_wave(string map, int sanMau)
        {
            GameConfig cfg = RealConfigValidateTests.Load(map);
            var gp = new GreedyPlayer(cfg);
            var r = GreedyPlayer.PlayFullMatch(cfg);

            _o.WriteLine($"{cfg.MapId} — {cfg.MapDisplayName}");
            _o.WriteLine($"  ô đám đông ({gp.CrowdSlots.Count}): {string.Join(",", gp.CrowdSlots)}");
            _o.WriteLine($"  ô tầm xa   ({gp.ReachSlots.Count}): {string.Join(",", gp.ReachSlots)}");
            _o.WriteLine($"  kết cục: {r.Phase} · wave {r.Wave} · máu {r.Goal}/{cfg.Economy.GoalHealth}"
                       + $" · kiếm cả trận {r.Earned} · đỉnh quân đồng thời {r.PeakLive}");

            Assert.Equal(MatchPhase.Won, r.Phase);
            Assert.Equal(20, r.Wave);
            Assert.True(r.Goal >= sanMau,
                $"{map}: còn {r.Goal}/{cfg.Economy.GoalHealth} máu, sàn là {sanMau}");
            Assert.True(r.PeakLive <= PeakLiveCap,
                $"{map}: đỉnh {r.PeakLive} quân cùng lúc > trần {PeakLiveCap} — phải đo FPS trước khi ship");
        }

        /// <summary>
        /// Thắng được thôi CHƯA ĐỦ — 11 map phải khó DẦN theo thứ tự mở khoá.
        ///
        /// Vòng 26 đã bắt được đúng lỗi này: 4 map đầu độ khó GIẢM dần vì hình học
        /// "thú vị" (zigzag/túi/xoáy ốc) làm đường tự áp sát chính nó → một tướng
        /// đánh nhiều lượt → map càng đẹp càng dễ. Sàn từng map không phát hiện ra;
        /// chỉ so SỐ ĐO giữa các map liền kề mới phát hiện.
        /// </summary>
        [Fact]
        public void Thang_do_kho_giam_dan()
        {
            var đo = new List<(string Map, int Goal)>();
            foreach (var (file, _) in Thang)
            {
                GameConfig cfg = RealConfigValidateTests.Load(file);
                var r = GreedyPlayer.PlayFullMatch(cfg);
                đo.Add((cfg.MapId, r.Goal));
                _o.WriteLine($"  {cfg.MapId} → {r.Phase} W{r.Wave} máu {r.Goal}/20");
            }

            for (int i = 1; i < đo.Count; i++)
                Assert.True(đo[i].Goal <= đo[i - 1].Goal,
                    $"{đo[i].Map} còn {đo[i].Goal} máu > {đo[i - 1].Map} còn {đo[i - 1].Goal}"
                    + " → map sau DỄ HƠN map trước, thang độ khó bị ngược");
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
        public void Tien_ca_tran_khong_vuot_tran_chi_tieu(string map, int _)
        {
            GameConfig cfg = RealConfigValidateTests.Load(map);
            var r = GreedyPlayer.PlayFullMatch(cfg);

            int fieldSlots = cfg.Path.Slots.Count(s => s.IsField);
            int pulgaFull = 0, dibuFull = 0;
            foreach (TowerDef t in cfg.Towers)
            {
                int sum = t.Levels.Sum(l => l.Cost);
                if (t.Id == "la_pulga") pulgaFull = sum;
                if (t.Id == "dibu") dibuFull = sum;
            }
            int tran = fieldSlots * pulgaFull + dibuFull;

            _o.WriteLine($"{cfg.MapId}: kiếm {r.Earned} · trần {fieldSlots}×{pulgaFull}+{dibuFull} = {tran}"
                       + $" · biên {(tran - r.Earned) * 100.0 / tran:0.0}%");
            Assert.True(r.Earned < tran,
                $"{map}: kiếm cả trận {r.Earned} >= trần {tran} → cuối trận tiền vô nghĩa");
        }
    }
}
