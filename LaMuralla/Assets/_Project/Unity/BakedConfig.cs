using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Phương tiện chở `config/*.json` vào bản build.
    ///
    /// Vì sao cần: `config/` sống ở gốc repo, NGOÀI `Assets/` — cố ý, vì nó là
    /// nguồn chân lý dùng chung cho cả tools/*.py lẫn game. Unity chỉ đóng gói
    /// thứ nằm dưới `Assets/`. Nếu copy sang thì có hai bản, và hai bản thì lệch.
    ///
    /// Vì sao chở JSON THÔ chứ không mirror thành trường ScriptableObject:
    /// mirror 60+ trường của GameConfig ra ScriptableObject là tạo bản sao thứ hai
    /// của lược đồ — đổi config/ thì phải nhớ đổi cả mirror, và không gì nhắc.
    /// Chở chuỗi thô thì lược đồ chỉ có một, nằm ở ConfigMapper (đã có test).
    ///
    /// Cái giá: parse lúc chạy. Chấp nhận được — JsonParser là của ta, đã test,
    /// không reflection nên IL2CPP không đụng gì. Đổi lại, ConfigBaker validate
    /// ngay lúc bake: config sai thì KHÔNG build được, thay vì crash trên tay
    /// người chơi.
    /// </summary>
    [CreateAssetMenu(fileName = "BakedConfig", menuName = "La Muralla/Baked Config")]
    public sealed class BakedConfig : ScriptableObject
    {
        [TextArea(3, 10)] public string towersJson = "";
        [TextArea(3, 10)] public string enemiesJson = "";
        [TextArea(3, 10)] public string wavesJson = "";
        [TextArea(3, 10)] public string economyJson = "";
        /// <summary>Map mặc định — giữ để code cũ không vỡ. Bằng `maps[0].json`.</summary>
        [TextArea(3, 10)] public string pathJson = "";

        /// <summary>
        /// MỌI map trong `config/maps/`. Unity không serialize được Dictionary nên
        /// đây là mảng cặp (id, json) — tra bằng <see cref="MapJson"/>.
        /// </summary>
        [System.Serializable]
        public sealed class MapEntry
        {
            public string id = "";
            public string displayName = "";
            public int order;
            [TextArea(3, 10)] public string json = "";
        }

        public MapEntry[] maps = System.Array.Empty<MapEntry>();

        /// <summary>JSON của map theo id. Không có id đó → trả `null`.</summary>
        public string? MapJson(string id)
        {
            foreach (MapEntry m in maps)
                if (m.id == id) return m.json;
            return null;
        }

        /// <summary>Lúc bake, để biết asset này sinh từ đâu và khi nào.</summary>
        public string bakedAtUtc = "";

        /// <summary>Đường dẫn Resources để runtime nạp. Khớp thư mục thật.</summary>
        public const string ResourcePath = "BakedConfig";
    }
}
