using System;
using System.Collections.Generic;

namespace LaMuralla.Core.Match
{
    /// <summary>
    /// AI bị trúng một phát bắn. Hình học thuần — không trừ máu, không cộng tiền,
    /// không biết DamageSystem tồn tại.
    ///
    /// Tách khỏi DamageSystem vì hai câu hỏi khác nhau: "trúng ai" là hình học,
    /// "chết chưa, ai được ghi công" là luật kinh tế. Gộp lại thì không test được
    /// cái nào mà không dựng cái kia.
    /// </summary>
    public static class CombatResolver
    {
        /// <summary>
        /// Quân trúng đòn lan.
        ///
        /// 🔴 TÂM LAN LÀ VỊ TRÍ MỤC TIÊU CHÍNH, KHÔNG PHẢI ĐIỂM ĐẠN CHẠM —
        /// docs/05 §4.5 mục 2. Hai chỗ đó khác nhau vì đạn BÁM mục tiêu (homing,
        /// `05` §4.3): lúc đạn tới, con quân đã đi tiếp. Lấy điểm đạn chạm làm tâm
        /// thì vùng lan lệch về phía sau theo hướng quân chạy, và giả định
        /// "D10S chạm 3 mục tiêu" (`04` §8 mục 4) — thứ cả bảng cân bằng dựa vào —
        /// không còn đúng.
        /// </summary>
        /// <param name="candidates">Mọi quân còn trên sân.</param>
        /// <param name="centre">Vị trí MỤC TIÊU CHÍNH tại thời điểm chạm.</param>
        /// <param name="radius">`splashRadius` của cấp đang chạy.</param>
        public static List<T> Splash<T>(IReadOnlyList<T> candidates, Vec2 centre, double radius)
            where T : class, ITarget
        {
            var hit = new List<T>();
            SplashInto(candidates, centre, radius, hit);
            return hit;
        }

        /// <summary>Như <see cref="Splash{T}"/> nhưng ghi vào buffer có sẵn.
        ///
        /// Tồn tại vì bản cấp phát chạy MỖI PHÁT BẮN: đo hồi còn El Fideo (đã gỡ)
        /// là 2 phát/giây × 6 tướng = 12 List rác mỗi giây. GC gom giữa trận → giật
        /// hình. Người chơi báo hai lần trước khi tôi tìm ra hết các chỗ này.</summary>
        public static void SplashInto<T>(IReadOnlyList<T> candidates, Vec2 centre, double radius,
                                         List<T> hit)
            where T : class, ITarget
        {
            if (radius < 0) throw new ArgumentOutOfRangeException(nameof(radius), radius, "bán kính lan phải ≥ 0");
            hit.Clear();
            double r2 = radius * radius;
            for (int i = 0; i < candidates.Count; i++)
            {
                T c = candidates[i];
                if (!c.IsAlive) continue;
                if (Vec2.SqrDistance(centre, c.Position) <= r2) hit.Add(c);
            }
        }

        /// <summary>
        /// Quân trúng `batigol.sut_xuyen` — tia thẳng.
        ///
        /// docs/05 §4.5 mục 3: "Tia từ **tháp → mục tiêu tại thời điểm bắn**, dài
        /// bằng tầm, rộng `lineWidth: 0.5`. Trúng mọi quân giao với tia. Trên khúc
        /// cua có thể chỉ trúng 1 con — **đó là ý đồ**: Batigol thưởng cho việc đặt
        /// ở đoạn thẳng."
        ///
        /// Hướng CHỐT LÚC BẮN, không bám mục tiêu (`05` §4.3: "Trừ `sut_xuyen` của
        /// Batigol — nó bay theo đường thẳng chốt lúc bắn, và đó chính là cơ chế
        /// xuyên"). Nếu để nó homing thì tia cong theo mục tiêu và "xuyên hàng" mất
        /// nghĩa — nó sẽ luôn trúng cả hàng bất kể đường thẳng hay cong, và bản sắc
        /// của Batigol (thưởng cho vị trí tốt) biến mất.
        ///
        /// Quân coi là ĐIỂM, không có bán kính va chạm — `03` §6 nói va chạm giữa
        /// quân "không có, chồng lên nhau chỉ là vấn đề hình ảnh", nên quân không
        /// có kích thước vật lý trong luật.
        /// </summary>
        /// <param name="from">Vị trí tháp.</param>
        /// <param name="towards">Vị trí mục tiêu TẠI THỜI ĐIỂM BẮN.</param>
        /// <param name="length">Tầm của cấp đang chạy.</param>
        /// <param name="width">`lineWidth` — bề RỘNG cả tia, nên nửa rộng = width/2.</param>
        public static List<T> Line<T>(IReadOnlyList<T> candidates, Vec2 from, Vec2 towards,
                                      double length, double width)
            where T : class, ITarget
        {
            var hit = new List<T>();
            LineInto(candidates, from, towards, length, width, hit);
            return hit;
        }

        /// <summary>Như <see cref="Line{T}"/> nhưng ghi vào buffer có sẵn — xem
        /// <see cref="SplashInto{T}"/> về lý do.</summary>
        public static void LineInto<T>(IReadOnlyList<T> candidates, Vec2 from, Vec2 towards,
                                       double length, double width, List<T> hit)
            where T : class, ITarget
        {
            if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length), length, "tia phải dài > 0");
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), width, "tia phải rộng > 0");
            hit.Clear();

            Vec2 d = towards - from;
            double len = d.Length;
            // Mục tiêu đứng ĐÚNG trên tháp: không có hướng. Chia cho 0 ra NaN và
            // mọi phép so sánh sau đó thành false → tia không trúng ai, im lặng.
            if (len <= 1e-9) return;

            Vec2 dir = d * (1.0 / len);
            double halfW = width / 2.0;

            for (int i = 0; i < candidates.Count; i++)
            {
                T c = candidates[i];
                if (!c.IsAlive) continue;
                if (InRectangle(c.Position, from, dir, length, halfW)) hit.Add(c);
            }
        }

        /// <summary>
        /// Điểm có nằm trong HÌNH CHỮ NHẬT `length × width` xuất phát từ `from`
        /// theo `dir` không.
        ///
        /// 🔴 HÌNH CHỮ NHẬT, KHÔNG PHẢI VIÊN NANG. Cách viết hiển nhiên là đo khoảng
        /// cách từ điểm tới ĐOẠN thẳng rồi so với nửa bề rộng — nhưng phép đo đó kẹp
        /// ở hai đầu đoạn, tức nó mô tả một viên nang: chữ nhật CỘNG hai chỏm tròn
        /// bán kính `halfW`.
        ///
        /// Hai chỏm đó cho Batigol với thêm `halfW` = 0.25 unit ngoài tầm khai báo.
        /// Trên tầm Lv3 = 1.4 thì đó là **+18% tầm miễn phí**, không ai khai, không
        /// ai đo. Dự án này đã một lần suýt chết vì tầm trôi (chord siêu tuyến tính,
        /// phải thu tầm ×0.36 ở vòng 5) — không tặng thêm tầm qua một chi tiết hình
        /// học nữa.
        ///
        /// Test `Xuyen_dung_o_tam_khong_voi_toi_con_xa_hon` khoá điều này: quân ở
        /// 1.5 với tia dài 1.4 KHÔNG được trúng.
        /// </summary>
        private static bool InRectangle(Vec2 p, Vec2 from, Vec2 dir, double length, double halfWidth)
        {
            Vec2 rel = p - from;
            double along = rel.X * dir.X + rel.Y * dir.Y;      // chiếu lên hướng tia
            if (along < 0 || along > length) return false;      // sau lưng tháp, hoặc quá tầm

            double perp = Math.Abs(rel.X * -dir.Y + rel.Y * dir.X);   // chiếu lên pháp tuyến
            return perp <= halfWidth;
        }
    }
}
