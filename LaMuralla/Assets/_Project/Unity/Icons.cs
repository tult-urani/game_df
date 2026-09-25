using System.Collections.Generic;
using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Icon của HUD — nạp từ `Resources/Icons/`, nguồn **Kenney CC0**
    /// (xem CREDITS.txt cạnh đó). Cùng nguồn với âm thanh ở `Resources/Audio/`.
    ///
    /// Vì sao KHÔNG dùng emoji: font mặc định của IMGUI là LiberationSans, có
    /// Latin-1 nhưng KHÔNG có bảng emoji. Trên máy thật (iPhone 12, iOS 18.2,
    /// 2026-08-20) mọi emoji ra **ô vuông tofu**. Đã đo, không phải phỏng đoán.
    ///
    /// Vì sao KHÔNG vẽ tay: có bộ CC0 sẵn dùng được, chuyên nghiệp hơn và không
    /// tốn công bảo trì công thức hình. Bản đầu của file này rasterise icon bằng
    /// code — chạy được nhưng là việc thừa.
    ///
    /// 🔴 MỌI ICON PHẢI LÀ HÌNH TRẮNG trên nền trong suốt. `GUI.color` là phép
    /// NHÂN màu: icon đen thì tô màu gì cũng ra đen. Thêm icon mới, kiểm cái này
    /// trước tiên.
    /// </summary>
    public static class Icons
    {
        public static Texture2D Coin => Get("coin");
        public static Texture2D Goal => Get("goal");
        public static Texture2D Save => Get("save");
        public static Texture2D AudioOn => Get("audio_on");
        public static Texture2D AudioOff => Get("audio_off");
        public static Texture2D Speed => Get("speed");
        public static Texture2D Star => Get("star");
        public static Texture2D Plus => Get("plus");

        private static readonly Dictionary<string, Texture2D?> Cache = new();

        /// <summary>Thiếu file → cảnh báo ĐÚNG MỘT LẦN rồi trả về texture trắng 1×1,
        /// để HUD vẫn vẽ ra một ô màu chứ không ném giữa `OnGUI`.</summary>
        private static Texture2D Get(string name)
        {
            if (Cache.TryGetValue(name, out Texture2D? cached))
                return cached != null ? cached : Texture2D.whiteTexture;

            var tex = Resources.Load<Texture2D>($"Icons/{name}");
            if (tex == null)
                Debug.LogWarning($"[Icons] thiếu Resources/Icons/{name}.png — " +
                                 "chạy lại bước copy icon Kenney.");
            Cache[name] = tex;
            return tex != null ? tex : Texture2D.whiteTexture;
        }
    }
}
