using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Vẽ hình khối bằng code — không cần file art nào.
    ///
    /// Đây là ART TẠM. `docs/06` mô tả tạo hình World Cup, sắc đỏ Argentina, hoạt
    /// ảnh ăn mừng. Không có cái nào ở đây. Mục đích của lớp này là làm game CHƠI
    /// ĐƯỢC để kiểm luật và cân bằng trên máy thật; thay art sau không phải viết
    /// lại gameplay, chỉ đổi lớp vẽ này.
    /// </summary>
    public static class Draw
    {
        private static UnityEngine.Sprite? _square;
        private static UnityEngine.Sprite? _circle;
        private static Shader? _shader;

        /// <summary>Sprite 1×1 trắng. Nhuộm màu bằng `SpriteRenderer.color`.</summary>
        public static UnityEngine.Sprite Square()
        {
            if (_square != null) return _square;
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, Color.white);
            t.Apply();
            _square = UnityEngine.Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _square;
        }

        /// <summary>Hình tròn 64×64 khử răng cưa. Quân và tướng là hình tròn để phân
        /// biệt với ô (vòng) và HUD (chữ nhật).</summary>
        public static UnityEngine.Sprite Circle()
        {
            if (_circle != null) return _circle;
            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            float r = n / 2f - 1f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f));
                    float a = Mathf.Clamp01(r - d);          // 1px mềm mép
                    t.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            t.Apply();
            _circle = UnityEngine.Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            return _circle;
        }

        private static Material? _unlit;

        /// <summary>
        /// `Sprites/Default` có sẵn trong mọi bản build. Shader URP có thể
        /// bị strip nếu không material nào tham chiếu.
        ///
        /// 🔴 TRẢ VỀ MATERIAL DÙNG CHUNG, KHÔNG cấp phát mới mỗi lần gọi.
        ///
        /// Bản cũ `new Material(_shader)` mỗi lần gọi, và nó bị gọi theo SỰ KIỆN
        /// TRẬN ĐẤU chứ không phải một lần: mỗi cú đánh trúng (`Draw.Ring("hit")`),
        /// mỗi vụ nổ (`UnitRing("boomRing")`), mỗi phát `so_10`. Ở W20 với 47 quân
        /// và ~10 tướng, đó là hàng chục material MỖI GIÂY.
        ///
        /// Vì sao nó gây ĐỨNG HÌNH chứ không chỉ chậm: Material là đối tượng NATIVE.
        /// GC quản lý không thu nó — chỉ `Resources.UnloadUnusedAssets()` mới thu, mà
        /// hàm đó không tự chạy giữa trận. Nên chúng tích luỹ suốt ván, ăn dần bộ nhớ;
        /// tới lúc Unity/iOS buộc phải dọn thì cú dọn đó khoá luôn nhiều frame.
        ///
        /// Dùng chung được vì màu của LineRenderer đi qua `startColor`/`endColor`
        /// (vertex color, thuộc renderer) chứ KHÔNG qua material — đã kiểm: không
        /// chỗ nào trong project ghi `material.color`.
        ///
        /// ⚠️ Nơi gán PHẢI dùng `sharedMaterial`. Gán qua `.material` khiến Unity
        /// nhân bản material một lần nữa, tức bản cũ rò rỉ HAI cái mỗi lần chứ không
        /// phải một.
        /// </summary>
        public static Material UnlitMaterial(Color? c = null)
        {
            _shader ??= Shader.Find("Sprites/Default");

            // Ca có màu riêng: hiện KHÔNG caller nào dùng. Giữ nhánh này để chữ ký
            // không đổi, và cấp phát riêng vì màu nằm TRONG material.
            if (c.HasValue) return new Material(_shader) { color = c.Value };

            return _unlit ??= new Material(_shader) { name = "UnlitShared" };
        }

        public static GameObject Make(string name, UnityEngine.Sprite sprite, Color c, float size,
                                        int order, Transform? parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = c;
            sr.sortingOrder = order;
            go.transform.localScale = Vector3.one * size;
            return go;
        }

        public static LineRenderer Ring(string name, Vector3 centre, float radius, Color c,
                                        float width, int order, Transform? parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            var lr = go.AddComponent<LineRenderer>();
            const int seg = 40;
            lr.positionCount = seg;
            lr.loop = true;
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                lr.SetPosition(i, centre + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * radius);
            }
            lr.widthMultiplier = width;
            lr.sharedMaterial = UnlitMaterial();
            lr.startColor = lr.endColor = c;
            lr.sortingOrder = order;
            lr.useWorldSpace = true;
            return lr;
        }

        /// <summary>Vòng tròn bán kính 1 trong TOẠ ĐỘ CỤC BỘ (useWorldSpace=false) →
        /// scale transform để nở/thu (sóng aura). Khác `Ring` (toạ độ thế giới, cố định).</summary>
        public static GameObject UnitRing(string name, Color c, float width, int order)
        {
            var go = new GameObject(name);
            var lr = go.AddComponent<LineRenderer>();
            const int seg = 48;
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = seg;
            for (int i = 0; i < seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f));
            }
            lr.widthMultiplier = width;
            lr.sharedMaterial = UnlitMaterial();
            lr.startColor = lr.endColor = c;
            lr.sortingOrder = order;
            return go;
        }

        private static readonly System.Collections.Generic.Dictionary<int, UnityEngine.Sprite> _poly = new();

        /// <summary>
        /// Đa giác `sides` cạnh, 96×96, khử răng cưa. `sides <= 2` → hình tròn.
        ///
        /// 🔴 HÌNH DẠNG, KHÔNG PHẢI MÀU. `docs/06` cảnh báo bảng màu ĐÃ HẾT CHỖ:
        /// 3 sắc đỏ + 2 sắc vàng đã dùng, nên vai trò phải đọc được qua HÌNH. Sáu
        /// vòng tròn khác màu là thiết kế hỏng — người mù màu không chơi được, và
        /// người thường cũng phải học thuộc bảng màu.
        ///
        /// Mỗi tướng một đa giác riêng: nhìn silhouette là biết ai, không cần màu.
        /// </summary>
        public static UnityEngine.Sprite Polygon(int sides)
        {
            if (sides <= 2) return Circle();
            if (_poly.TryGetValue(sides, out UnityEngine.Sprite? cached)) return cached;

            const int n = 96;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var c = new Vector2(n / 2f, n / 2f);
            float r = n / 2f - 2f;
            float rot = Mathf.PI / 2f;   // đỉnh hướng lên

            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f) - c;
                    float ang = Mathf.Atan2(p.y, p.x) - rot;
                    // Bán kính của cạnh đa giác tại góc này (công thức apothem)
                    float seg = Mathf.PI * 2f / sides;
                    float a = Mathf.Repeat(ang, seg) - seg / 2f;
                    float edge = r * Mathf.Cos(seg / 2f) / Mathf.Cos(a);
                    float alpha = Mathf.Clamp01(edge - p.magnitude);   // 1px mềm mép
                    t.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
            t.Apply();
            UnityEngine.Sprite sp = UnityEngine.Sprite.Create(t, new Rect(0, 0, n, n),
                                                             new Vector2(0.5f, 0.5f), n);
            _poly[sides] = sp;
            return sp;
        }

        /// <summary>
        /// Hình của từng tướng — bản sắc hình học, đọc được không cần màu.
        /// Số cạnh chọn theo VAI TRÒ, không tuỳ tiện.
        /// </summary>
        public static UnityEngine.Sprite ShapeOf(string towerId) => towerId switch
        {
            "batigol" => Polygon(3),     // tam giác — mũi nhọn, xuyên hàng
            "la_pulga" => Polygon(4),    // kim cương — sát thủ đơn mục tiêu
            "d10s" => Polygon(6),        // lục giác — lan toả đều mọi hướng
            "el_arbitro" => Polygon(8),  // bát giác — biển báo, khống chế
            "dibu" => Circle(),          // tròn — găng tay thủ môn
            _ => Circle(),
        };

        // ── Bảng màu tạm ────────────────────────────────────────────────────
        // `docs/06` cảnh báo bảng màu ĐÃ HẾT CHỖ: 3 sắc đỏ + 2 sắc vàng, phải phân
        // biệt bằng HÌNH DẠNG chứ không phải màu. Ở đây tướng khác nhau bằng màu vì
        // chỉ có 6 loại và đây là art tạm — KHÔNG áp dụng được cho art thật.
        // Grass/Path đã gỡ 2026-08-20: mặt sân và con đường giờ do `Pitch` sinh,
        // không còn là một màu phẳng và một vệt trắng nữa.
        public static readonly Color SlotFree = new(0.45f, 0.75f, 1f, 0.55f);
        public static readonly Color SlotKeeper = new(1f, 0.85f, 0.2f, 0.7f);
        public static readonly Color Touch = new(1f, 1f, 1f, 0.10f);

        public static Color OfTower(string id) => id switch
        {
            "batigol" => new Color(0.95f, 0.35f, 0.25f),
            "la_pulga" => new Color(0.45f, 0.80f, 1.00f),
            "d10s" => new Color(0.60f, 0.45f, 0.95f),
            "el_arbitro" => new Color(0.15f, 0.15f, 0.18f),
            "dibu" => new Color(1.00f, 0.85f, 0.20f),
            _ => Color.magenta,   // magenta = "quên khai màu", nhìn là biết ngay
        };

        /// <summary>Quân cũng phân biệt bằng HÌNH: tambor (máu ×5.5) phải nhìn ra
        /// ngay là khác adepto, không chờ đọc thanh máu.</summary>
        public static UnityEngine.Sprite ShapeOfEnemy(string id, bool boss) => boss
            ? Polygon(6)                 // boss — to, lục giác
            : id switch
            {
                "adepto" => Circle(),    // tròn — lính thường
                "tifoso" => Polygon(4),  // vuông — cứng hơn
                "tambor" => Polygon(3),  // tam giác — tank, máu ×5.5
                _ => Circle(),
            };

        public static Color OfEnemy(string id, bool boss) => boss
            ? new Color(0.55f, 0.05f, 0.05f)
            : id switch
            {
                "adepto" => new Color(0.85f, 0.80f, 0.75f),
                "tifoso" => new Color(0.90f, 0.55f, 0.20f),
                "tambor" => new Color(0.60f, 0.35f, 0.15f),
                _ => Color.magenta,
            };
    }
}
