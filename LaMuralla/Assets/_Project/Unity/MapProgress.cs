using System.Collections.Generic;
using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Tiến độ người chơi: map nào đã thắng, được mấy sao.
    ///
    /// Lưu bằng `PlayerPrefs` — đủ cho một game một người chơi, không có tài khoản,
    /// không đồng bộ. Khoá có đánh số phiên bản (`v1`) để sau này đổi định dạng thì
    /// còn đường nhận ra dữ liệu cũ thay vì đọc bừa.
    ///
    /// Sao lấy từ `StarRating` của Core (máu cầu môn còn lại: 1–9 → 1 sao, 10–19 →
    /// 2 sao, đúng 20 → 3 sao). Lớp này KHÔNG tự tính sao — nó chỉ nhớ.
    /// </summary>
    public static class MapProgress
    {
        private const string KeyPrefix = "lamuralla.progress.v1.";

        /// <summary>Số sao cao nhất từng đạt ở map này. 0 = chưa thắng bao giờ.</summary>
        public static int StarsOf(string mapId) => PlayerPrefs.GetInt(KeyPrefix + mapId, 0);

        /// <summary>
        /// Ghi kết quả. CHỈ ghi đè khi tốt hơn — chơi lại tệ hơn không được làm mất
        /// thành tích cũ, nếu không thì người chơi sẽ sợ bấm "chơi lại".
        /// </summary>
        public static void Record(string mapId, int stars)
        {
            if (stars <= StarsOf(mapId)) return;
            PlayerPrefs.SetInt(KeyPrefix + mapId, stars);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Map có mở không. Map đầu tiên LUÔN mở; các map sau mở khi map liền trước
        /// đã thắng (≥1 sao).
        ///
        /// Cố ý dùng "≥1 sao" chứ không phải "3 sao": ép 3 sao thì người chơi kẹt
        /// cứng ở một map và không còn gì để làm — đó là cách nhanh nhất để họ bỏ game.
        /// </summary>
        public static bool IsUnlocked(IReadOnlyList<BakedConfig.MapEntry> ordered, int index)
        {
            if (index <= 0) return true;
            return StarsOf(ordered[index - 1].id) > 0;
        }

        /// <summary>Xoá sạch tiến độ. Dùng khi thử nghiệm.</summary>
        public static void ResetAll(IReadOnlyList<BakedConfig.MapEntry> maps)
        {
            foreach (BakedConfig.MapEntry m in maps) PlayerPrefs.DeleteKey(KeyPrefix + m.id);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Map người chơi vừa chọn ở menu, chở qua lần nạp lại scene.
    ///
    /// Vì sao đi qua `static` + nạp lại scene thay vì bật/tắt object trong một scene:
    /// một trận dựng ra hàng trăm GameObject (sân, ô, tướng, quân, thanh máu, hiệu
    /// ứng). Tự tay dọn hết để về menu là chỗ rò rỉ chắc chắn xảy ra. Nạp lại scene
    /// cho ta việc dọn dẹp MIỄN PHÍ và đúng tuyệt đối — `MatchView.Restart()` vốn đã
    /// dùng đúng cách này từ trước.
    /// </summary>
    public static class MapSession
    {
        /// <summary>`null` = hiện menu chọn map. Có giá trị = vào thẳng map đó.</summary>
        public static string? PickedMapId;
    }
}
