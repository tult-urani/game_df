using System.Collections.Generic;

namespace LaMuralla.Core.Match
{
    /// <summary>Thẻ của trọng tài. Vĩnh viễn trên một con quân — thẻ đỏ không gỡ được.</summary>
    public enum CardState
    {
        None,
        Yellow,
        Red,
    }

    /// <summary>Bốn cách chọn mục tiêu có thật trong config. Xem towers.json →
    /// `targetSelector`, và docs/05 §4.5 mục 1.</summary>
    public enum TargetSelector
    {
        /// <summary>Đòn đánh thường của mọi tướng.</summary>
        FirstInRange,
        /// <summary>`la_pulga.solo_run` — máu TUYỆT ĐỐI cao nhất, không phải %.</summary>
        HighestAbsoluteHp,
        /// <summary>`el_arbitro.the_vang` (Lv2) VÀ `el_arbitro.the_do` (Lv3) — chỉ
        /// con CHƯA có thẻ. Cả hai dùng chung selector này: mỗi con chỉ ăn đúng một
        /// thẻ trong đời, vàng hay đỏ tuỳ cấp Árbitro đã rút.</summary>
        FirstInRangeWithoutCard,

        // ĐÃ XOÁ: `FirstInRangeWithYellowCard`.
        //
        // Nó phục vụ thiết kế cũ, nơi `the_do` (Lv3) nhắm con mà `the_vang` (Lv2)
        // đã rút thẻ vàng — tức chuỗi vàng → đỏ, chỉ chạy được khi kỹ năng cộng dồn.
        // B-01 (user chốt 2026-07-17) bỏ cộng dồn: Lv3 không chạy `the_vang` nữa,
        // nên không ai có thẻ vàng, nên selector này không bao giờ tìm được ai.
        // `the_do` giờ rút thẻ đỏ thẳng qua `FirstInRangeWithoutCard`.
        //
        // Xoá thay vì để lại: enum không ai dùng là cái bẫy — người sau đọc thấy
        // nó, tưởng chuỗi vàng→đỏ vẫn tồn tại, và xây tiếp lên một luật đã chết.
        // Luật 17 của validator chặn config gọi tên nó.
    }

    /// <summary>
    /// Thứ TargetingSystem cần biết về một con quân — không hơn.
    ///
    /// Là interface chứ không phải class cụ thể để test dựng được tình huống
    /// (hai con trùng quãng đường, con đã chết, con có thẻ vàng) mà không phải
    /// chạy cả trận đấu.
    /// </summary>
    public interface ITarget
    {
        /// <summary>Định danh ổn định, duy nhất trong một trận. Là mắt xích CUỐI
        /// của thứ tự toàn phần — xem chú thích ở <see cref="TargetingSystem"/>.</summary>
        int Id { get; }

        Vec2 Position { get; }

        /// <summary>Quãng đường đã đi dọc spline. Lớn hơn = gần cầu môn hơn.</summary>
        double DistanceTravelled { get; }

        double Hp { get; }

        /// <summary>Máu tối đa của con này Ở WAVE NÀY (đã nhân hpScaling). Cần cho
        /// `batigol.ban_nang_sat_thu` — "+30% lên quân còn máu > 80%" là % của máu
        /// TỐI ĐA, không phải con số tuyệt đối.</summary>
        double MaxHp { get; }

        bool IsAlive { get; }
        CardState Card { get; }
    }

    /// <summary>
    /// Chọn mục tiêu cho một tướng.
    ///
    /// 🔴 THỨ TỰ TOÀN PHẦN, KHÔNG DỰA VÀO THỨ TỰ TRONG LIST — docs/05 §4.5 mục 1.
    ///
    /// `03` §6 cho phép quân chồng nhau, nên "con đầu tiên trong tầm" là câu hỏi
    /// mơ hồ: đầu tiên theo cái gì? Nếu lấy theo thứ tự phần tử trong list thì mục
    /// tiêu đổi khi list bị sắp xếp lại — mà list bị sắp xếp lại mỗi khi một con
    /// chết ở giữa. Người chơi thấy tướng đột nhiên đổi mục tiêu không vì lý do gì,
    /// và bug đó không tái hiện được vì nó phụ thuộc thứ tự chết.
    ///
    /// Nên: sắp theo QUÃNG ĐƯỜNG ĐÃ ĐI giảm dần (gần cầu môn nhất = nguy hiểm nhất
    /// = bắn trước), rồi khi trùng thì theo `Id` tăng dần. `Id` là mắt xích cuối để
    /// quan hệ này là THỨ TỰ TOÀN PHẦN thật: hai con trùng quãng đường tới từng bit
    /// (hoàn toàn có thể — cùng loại, spawn cùng frame) vẫn có đúng một câu trả lời.
    ///
    /// Không cấp phát, không sắp xếp: quét một lượt giữ con tốt nhất. Hàm này chạy
    /// mỗi frame cho mỗi tướng.
    /// </summary>
    public static class TargetingSystem
    {
        /// <summary>
        /// Con quân nên bắn, hoặc `null` nếu không có ai hợp lệ trong tầm.
        /// </summary>
        /// <param name="candidates">Mọi quân còn trên sân. Con đã chết bị loại ở đây.</param>
        /// <param name="towerPos">Vị trí tướng.</param>
        /// <param name="range">Tầm, đơn vị sân.</param>
        public static T? Select<T>(IReadOnlyList<T> candidates, Vec2 towerPos, double range,
                                   TargetSelector selector)
            where T : class, ITarget
        {
            // So bình phương: tránh Sqrt trong vòng lặp chạy mỗi frame cho mỗi tướng.
            double rangeSqr = range * range;
            T? best = null;

            for (int i = 0; i < candidates.Count; i++)
            {
                T c = candidates[i];
                if (!c.IsAlive) continue;
                if (!Eligible(c, selector)) continue;
                if (Vec2.SqrDistance(towerPos, c.Position) > rangeSqr) continue;
                if (best == null || Better(c, best, selector)) best = c;
            }

            return best;
        }

        /// <summary>Bộ lọc riêng của từng selector. Tách khỏi <see cref="Better"/>
        /// vì "được phép bắn" và "bắn con nào trước" là hai câu hỏi khác nhau —
        /// gộp lại là chỗ bug thích trốn.</summary>
        private static bool Eligible(ITarget t, TargetSelector selector) => selector switch
        {
            TargetSelector.FirstInRangeWithoutCard => t.Card == CardState.None,
            _ => true,
        };

        /// <summary>`a` có tốt hơn `b` không. Phải là thứ tự toàn phần: với mọi cặp
        /// a ≠ b, đúng một trong Better(a,b) / Better(b,a) là true.</summary>
        private static bool Better(ITarget a, ITarget b, TargetSelector selector)
        {
            if (selector == TargetSelector.HighestAbsoluteHp)
            {
                // Máu TUYỆT ĐỐI, không phải %. La Pulga săn con to nhất — boss,
                // chứ không phải con lính đầy máu nhưng máu tối đa bé tí.
                if (a.Hp != b.Hp) return a.Hp > b.Hp;
            }

            // Gần cầu môn hơn = nguy hiểm hơn = bắn trước.
            if (a.DistanceTravelled != b.DistanceTravelled)
                return a.DistanceTravelled > b.DistanceTravelled;

            // Mắt xích cuối. Không có nó thì hai con trùng quãng đường sẽ được
            // quyết bởi thứ tự trong list — thứ mà docs/05 §4.5 cấm.
            return a.Id < b.Id;
        }
    }
}
