using System.Collections.Generic;
using LaMuralla.Core.Match;
using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Sân cỏ, con đường và bệ đứng — SINH BẰNG CODE, không file art nào.
    ///
    /// Vì sao không dùng sprite pack tải về (đã thử và loại, 2026-08-20):
    ///   • Đường đi là spline tự do 14 waypoint, góc rẽ đo từ `config/path.json` là
    ///     70°/77°/9.6°/-48°/-86°/-26°/37°/98°/26°/-42°/-99°/8.5°. Mọi pack tile
    ///     tower-defense đều là ô VUÔNG trên lưới 90° — không khớp một khúc nào.
    ///   • Chỉnh waypoint trong `path.json` thì đường ở đây tự vẽ lại. Dùng ảnh cắt
    ///     sẵn thì mỗi lần chỉnh lại phải cắt lại.
    ///   • `docs/06 §5`: asset ngoài luôn phải soi bản quyền. Sinh bằng code thì
    ///     rủi ro đó bằng 0.
    ///
    /// Cùng triết lý với <see cref="Draw"/>, khác ở chỗ đây là NỀN chứ không phải
    /// quân cờ: mọi thứ trong này phải MỜ và ÍT tương phản, kẻo nuốt mất tướng và
    /// quân. Sân đẹp mà không đọc được trận là sân hỏng.
    /// </summary>
    public static class Pitch
    {
        // Nền cỏ đậm hơn `Draw.Grass` một chút để vạch trắng và quân nổi lên.
        private static readonly Color GrassDark = new(0.07f, 0.26f, 0.11f);
        private static readonly Color GrassLite = new(0.09f, 0.32f, 0.14f);
        private static readonly Color Chalk = new(1f, 1f, 1f, 0.34f);
        // Ngoài đường biên: khán đài tối, để mắt tự khoanh vùng chơi.
        public static readonly Color Stands = new(0.05f, 0.11f, 0.08f);

        private const int PixelsPerUnit = 48;   // đủ nét cho vạch vôi, đủ nhẹ để sinh nhanh
        private const int StripeCount = 12;     // số dải cỏ cắt ngang

        /// <summary>
        /// Mặt sân: dải cỏ cắt ngang + vạch vôi (biên, giữa sân, vòng tròn giữa,
        /// hai vòng cấm, hai khu 5m50, chấm phạt đền). Trả về GameObject đã đặt
        /// đúng tâm/kích thước, nằm dưới mọi thứ khác.
        /// </summary>
        public static GameObject Field(Vector3 centre, float width, float height, int order)
        {
            int w = Mathf.RoundToInt(width * PixelsPerUnit);
            int h = Mathf.RoundToInt(height * PixelsPerUnit);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];

            // ── Dải cỏ ──────────────────────────────────────────────────────
            for (int y = 0; y < h; y++)
            {
                bool lite = (y * StripeCount / h) % 2 == 0;
                Color band = lite ? GrassLite : GrassDark;
                for (int x = 0; x < w; x++) px[y * w + x] = band;
            }

            // ── Vạch vôi ────────────────────────────────────────────────────
            // Biên sân thụt vào 4% mỗi phía: mép màn hình không bao giờ là vạch biên
            // ở đời thật, và thụt vào thì camera co giãn kiểu gì vạch cũng còn thấy.
            int mx = Mathf.RoundToInt(w * 0.055f), my = Mathf.RoundToInt(h * 0.030f);
            int lw = Mathf.Max(2, Mathf.RoundToInt(PixelsPerUnit * 0.06f));

            Rect(px, w, h, mx, my, w - mx, h - my, lw);                       // biên
            HLine(px, w, h, mx, w - mx, h / 2, lw);                           // vạch giữa sân
            Circle(px, w, h, w / 2, h / 2, Mathf.RoundToInt(1.35f * PixelsPerUnit), lw);
            Dot(px, w, h, w / 2, h / 2, Mathf.Max(2, lw));

            // Vòng cấm + khu 5m50 ở hai đầu. Tỉ lệ lấy theo sân thật thu nhỏ, không
            // phải số đẹp: 16m5/68m ≈ 0.24 chiều cao khung, rộng 40m/68m ≈ 0.59.
            int boxW = Mathf.RoundToInt(w * 0.59f), boxH = Mathf.RoundToInt(h * 0.115f);
            int smallW = Mathf.RoundToInt(w * 0.27f), smallH = Mathf.RoundToInt(h * 0.045f);
            foreach (int sign in new[] { 1, -1 })
            {
                int edge = sign > 0 ? my : h - my;
                int far = sign > 0 ? my + boxH : h - my - boxH;
                Rect(px, w, h, (w - boxW) / 2, Mathf.Min(edge, far), (w + boxW) / 2, Mathf.Max(edge, far), lw);
                int sfar = sign > 0 ? my + smallH : h - my - smallH;
                Rect(px, w, h, (w - smallW) / 2, Mathf.Min(edge, sfar), (w + smallW) / 2, Mathf.Max(edge, sfar), lw);
            }

            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), PixelsPerUnit);

            var go = new GameObject("Pitch");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            go.transform.position = new Vector3(centre.x, centre.y, 1f);   // sau mọi thứ
            return go;
        }

        /// <summary>
        /// Con đường: dải lưới bám theo spline, texture lát dọc theo chiều đi.
        ///
        /// Khác `LineRenderer` cũ ở ba điểm quan trọng: bề mặt có VÂN (nhìn ra là
        /// lối mòn chứ không phải vệt sáng), mép MỜ dần nên không có viền cứng, và
        /// có vạch đứt chạy giữa nên đọc được HƯỚNG quân sẽ đi.
        /// </summary>
        public static GameObject Road(EnemyPath path, float width, int order)
        {
            const int samples = 260;
            // Cửa sổ lan bề rộng bị bóp sang hai bên khúc cua. ±3 mẫu là số NHỎ NHẤT
            // đo được còn khử hết chỗ tự cắt trên đường hiện tại (±2 cũng đủ, lấy 3
            // cho có biên khi ai đó chỉnh waypoint).
            const int curveWindow = 3;
            const float curveSafety = 0.85f;

            float half = width * 0.5f;
            double len = path.Length;

            var centre = new Vector3[samples];
            var normal = new Vector2[samples];
            var radius = new float[samples];

            // ── Lượt 1: tâm, pháp tuyến, bán kính cong tại từng mẫu ─────────
            for (int i = 0; i < samples; i++)
            {
                double d = len * i / (samples - 1.0);
                double e = len / (samples * 2.0);
                Vec2 p = path.PositionAt(d);
                Vec2 a = path.PositionAt(System.Math.Max(0, d - e));
                Vec2 b = path.PositionAt(System.Math.Min(len, d + e));

                var t = new Vector2((float)(b.X - a.X), (float)(b.Y - a.Y));
                if (t.sqrMagnitude < 1e-8f) t = Vector2.up;
                t.Normalize();

                centre[i] = new Vector3((float)p.X, (float)p.Y, 0f);
                normal[i] = new Vector2(-t.y, t.x);
                radius[i] = Curvature(a, p, b);
            }

            // ── Lượt 2: bề rộng, kẹp theo khúc cua GẮT NHẤT quanh mẫu ───────
            // Đo trên đường hiện tại: bán kính cong nhỏ nhất 0.383 unit (quanh
            // waypoint (4.0, -6.55), khúc rẽ -99°) < nửa bề rộng 0.475 → hai mép
            // trong vượt qua nhau, dải lưới thắt nút hình nơ.
            //
            // Kẹp theo bán kính TẠI CHỖ thôi thì chưa đủ: đỉnh cong chỉ rơi vào ĐÚNG
            // một mẫu, hai mẫu kề bên đo ra bán kính lớn hơn nên vẫn phình ra và vẫn
            // gấp. Phải lấy min trên cả cửa sổ thì chỗ bóp mới trải đều qua khúc cua.
            // Đo lại sau khi sửa: 0 đoạn gấp ngược, chỉ 7/260 mẫu bị bóp, chỗ hẹp
            // nhất còn 0.76 unit — vẫn rộng hơn cổ động viên (0.7).
            var halfWidth = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float tightest = float.MaxValue;
                int lo = Mathf.Max(0, i - curveWindow), hi = Mathf.Min(samples - 1, i + curveWindow);
                for (int j = lo; j <= hi; j++) tightest = Mathf.Min(tightest, radius[j]);
                halfWidth[i] = Mathf.Min(half, curveSafety * tightest);
            }

            // ── Lượt 3: dựng lưới ───────────────────────────────────────────
            var verts = new Vector3[samples * 2];
            var uvs = new Vector2[samples * 2];
            var tris = new int[(samples - 1) * 6];

            for (int i = 0; i < samples; i++)
            {
                var n3 = new Vector3(normal[i].x, normal[i].y, 0f) * halfWidth[i];
                verts[i * 2] = centre[i] + n3;
                verts[i * 2 + 1] = centre[i] - n3;

                // v lát theo QUÃNG ĐƯỜNG thật → vân không bị kéo giãn ở khúc cong.
                float v = (float)(len * i / (samples - 1.0) / (width * 1.4));
                uvs[i * 2] = new Vector2(0f, v);
                uvs[i * 2 + 1] = new Vector2(1f, v);

                if (i == samples - 1) continue;
                int o = i * 6, k = i * 2;
                tris[o] = k; tris[o + 1] = k + 2; tris[o + 2] = k + 1;
                tris[o + 3] = k + 1; tris[o + 4] = k + 2; tris[o + 5] = k + 3;
            }

            var mesh = new Mesh { name = "RoadRibbon" };
            mesh.SetVertices(new List<Vector3>(verts));
            mesh.SetUVs(0, new List<Vector2>(uvs));
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();

            var go = new GameObject("Road");
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Draw.UnlitMaterial();
            mr.sharedMaterial.mainTexture = RoadTexture();
            mr.sortingOrder = order;
            go.transform.position = new Vector3(0f, 0f, 0.5f);   // trên sân, dưới ô
            return go;
        }

        /// <summary>Bán kính đường tròn đi qua 3 điểm (bán kính cong tại điểm giữa).
        /// Ba điểm gần thẳng hàng → diện tích ~0 → trả về vô cùng, tức không bóp.</summary>
        private static float Curvature(Vec2 a, Vec2 p, Vec2 b)
        {
            double abx = p.X - a.X, aby = p.Y - a.Y;
            double acx = b.X - a.X, acy = b.Y - a.Y;
            double area2 = System.Math.Abs(abx * acy - acx * aby);
            if (area2 < 1e-9) return float.MaxValue;
            double la = Vec2.Distance(a, p), lb = Vec2.Distance(p, b), lc = Vec2.Distance(a, b);
            return (float)(la * lb * lc / (2.0 * area2));
        }

        private static Texture2D? _roadTex;

        /// <summary>Vân đường lát vô hạn theo chiều v: đất mòn + vạch đứt giữa +
        /// mép mờ dần. Sinh một lần, dùng cho mọi trận.</summary>
        private static Texture2D RoadTexture()
        {
            if (_roadTex != null) return _roadTex;

            const int n = 64;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
            var px = new Color[n * n];
            var dirt = new Color(0.42f, 0.35f, 0.24f);

            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n;
                    // Mép mờ: alpha 0 ở hai rìa, đầy ở giữa. Không có bước này thì
                    // dải lưới có viền cứng, lộ ngay là hình chữ nhật ghép.
                    float edge = Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f - u) / 0.16f);

                    // Vân đất: nhiễu giả hai tần số, đủ để không phẳng lì.
                    float g = Mathf.PerlinNoise(x * 0.16f, y * 0.16f) * 0.6f
                            + Mathf.PerlinNoise(x * 0.47f, y * 0.47f) * 0.4f;
                    Color c = dirt * (0.82f + 0.30f * g);

                    // Vạch đứt giữa đường: nửa trên của mỗi ô texture, rộng 10%.
                    bool dashBand = Mathf.Abs(u - 0.5f) < 0.05f && y < n * 0.45f;
                    if (dashBand) c = Color.Lerp(c, new Color(0.95f, 0.93f, 0.80f), 0.55f);

                    c.a = edge * 0.92f;
                    px[y * n + x] = c;
                }

            t.SetPixels(px);
            t.Apply();
            _roadTex = t;
            return t;
        }

        private static Sprite? _pedestal;

        /// <summary>
        /// Bệ đứng của tướng: đĩa tròn có vành, tối dần ra rìa, kèm bóng đổ mềm.
        /// KHÔNG thay vòng chạm 48pt và vòng chọn ô — hai thứ đó là chức năng
        /// (`docs/06 §4b`), bệ chỉ là nền dưới chân.
        /// </summary>
        public static Sprite Pedestal()
        {
            if (_pedestal != null) return _pedestal;

            const int n = 128;
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var c0 = new Vector2(n / 2f, n / 2f);
            float rOuter = n / 2f - 2f;
            float rRim = rOuter * 0.86f;
            var face = new Color(0.24f, 0.22f, 0.20f);
            var rim = new Color(0.55f, 0.52f, 0.46f);

            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c0);
                    Color c;
                    float a;
                    if (d <= rRim)
                    {
                        // Mặt bệ: sáng hơn ở nửa trên cho ra cảm giác ánh sáng từ trên.
                        float lift = Mathf.InverseLerp(n * 0.15f, n * 0.85f, y) * 0.18f;
                        c = face * (0.9f + lift);
                        a = 0.78f;
                    }
                    else
                    {
                        c = rim;
                        a = 0.85f * Mathf.Clamp01(rOuter - d);   // 1px mềm mép ngoài
                    }
                    c.a = a;
                    t.SetPixel(x, y, c);
                }

            t.Apply();
            _pedestal = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
            return _pedestal;
        }

        // ── Bút vẽ vạch vôi trên mảng pixel ─────────────────────────────────

        private static void Blend(Color[] px, int w, int h, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            int i = y * w + x;
            px[i] = Color.Lerp(px[i], new Color(c.r, c.g, c.b), c.a);
        }

        private static void HLine(Color[] px, int w, int h, int x0, int x1, int y, int t)
        {
            for (int dy = 0; dy < t; dy++)
                for (int x = x0; x <= x1; x++) Blend(px, w, h, x, y + dy - t / 2, Chalk);
        }

        private static void VLine(Color[] px, int w, int h, int y0, int y1, int x, int t)
        {
            for (int dx = 0; dx < t; dx++)
                for (int y = y0; y <= y1; y++) Blend(px, w, h, x + dx - t / 2, y, Chalk);
        }

        private static void Rect(Color[] px, int w, int h, int x0, int y0, int x1, int y1, int t)
        {
            HLine(px, w, h, x0, x1, y0, t);
            HLine(px, w, h, x0, x1, y1, t);
            VLine(px, w, h, y0, y1, x0, t);
            VLine(px, w, h, y0, y1, x1, t);
        }

        private static void Circle(Color[] px, int w, int h, int cx, int cy, int r, int t)
        {
            int steps = Mathf.Max(64, r * 6);
            for (int i = 0; i < steps; i++)
            {
                float a = i / (float)steps * Mathf.PI * 2f;
                for (int k = 0; k < t; k++)
                {
                    float rr = r + k - t / 2f;
                    Blend(px, w, h, cx + Mathf.RoundToInt(Mathf.Cos(a) * rr),
                                    cy + Mathf.RoundToInt(Mathf.Sin(a) * rr), Chalk);
                }
            }
        }

        private static void Dot(Color[] px, int w, int h, int cx, int cy, int r)
        {
            for (int y = -r; y <= r; y++)
                for (int x = -r; x <= r; x++)
                    if (x * x + y * y <= r * r) Blend(px, w, h, cx + x, cy + y, Chalk);
        }
    }
}
