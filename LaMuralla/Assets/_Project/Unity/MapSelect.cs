using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Màn campaign đầu game. Mười một map được trình bày như một hành trình
    /// ziczac tiến dần về sân vận động, thay cho danh sách thẻ dọc cũ.
    ///
    /// Luật mở khoá vẫn thuộc về <see cref="MapProgress"/>. Lớp này chỉ chụp một
    /// snapshot tiến độ khi scene mở, dựng layout và bắt chạm. Texture/style được
    /// tạo đúng một lần; OnGUI không dựng GUIStyle hay Texture2D mỗi frame.
    /// </summary>
    public sealed class MapSelect : MonoBehaviour
    {
        internal BakedConfig? Baked;

        /// <summary>Mở sẵn mọi map, chỉ dành cho thử nghiệm.</summary>
        internal bool UnlockAll;

        private readonly List<BakedConfig.MapEntry> _ordered = new();

        private Texture2D? _background;
        private Texture2D? _disc;
        private Font? _regularFont;
        private Font? _semiboldFont;

        private GUIStyle? _title;
        private GUIStyle? _subtitle;
        private GUIStyle? _nodeNumber;
        private GUIStyle? _mapNameLeft;
        private GUIStyle? _mapNameRight;
        private GUIStyle? _metaLeft;
        private GUIStyle? _metaRight;
        private GUIStyle? _footer;

        private int[] _stars = Array.Empty<int>();
        private bool[] _unlocked = Array.Empty<bool>();
        private string[] _numbers = Array.Empty<string>();
        private int _currentIndex = -1;
        private string _progressText = "";
        private string _currentText = "";

        private float _u;
        private int _styledWidth = -1;

        private static readonly Color Gold = new(0.96f, 0.72f, 0.24f);
        private static readonly Color GoldSoft = new(0.96f, 0.78f, 0.38f);
        private static readonly Color Mint = new(0.35f, 0.88f, 0.59f);
        private static readonly Color DeepGreen = new(0.035f, 0.13f, 0.09f);
        private static readonly Color Locked = new(0.24f, 0.29f, 0.27f);

        private void Start()
        {
            if (Baked == null)
            {
                enabled = false;
                return;
            }

            _ordered.AddRange(Baked.maps);
            _ordered.Sort((a, b) => a.order.CompareTo(b.order));

            _background = Resources.Load<Texture2D>("Art/home_campaign_bg");
            _regularFont = Resources.Load<Font>("Fonts/Inter-Regular");
            _semiboldFont = Resources.Load<Font>("Fonts/Inter-SemiBold");
            _disc = CreateDiscTexture(128);

            int count = _ordered.Count;
            _stars = new int[count];
            _unlocked = new bool[count];
            _numbers = new string[count];

            int totalStars = 0;
            for (int i = 0; i < count; i++)
            {
                _stars[i] = MapProgress.StarsOf(_ordered[i].id);
                _unlocked[i] = UnlockAll || MapProgress.IsUnlocked(_ordered, i);
                _numbers[i] = (i + 1).ToString("00");
                totalStars += _stars[i];

                if (_currentIndex < 0 && _unlocked[i] && _stars[i] == 0)
                    _currentIndex = i;
            }

            // Hoàn thành cả campaign: map cuối vẫn là điểm đang được nhấn mạnh để
            // người chơi có lối chơi lại rõ ràng.
            if (_currentIndex < 0 && count > 0)
                _currentIndex = count - 1;

            _progressText = $"{totalStars}/{count * 3} STARS";
            _currentText = _currentIndex >= 0
                ? $"CURRENT  ·  {_ordered[_currentIndex].displayName.ToUpperInvariant()}"
                : "CAMPAIGN COMPLETE";

            if (_background == null)
                Debug.LogWarning("[MapSelect] thiếu Resources/Art/home_campaign_bg.png — dùng nền màu dự phòng.");
        }

        private void OnDestroy()
        {
            if (_disc != null) Destroy(_disc);
        }

        private static Texture2D CreateDiscTexture(int size)
        {
            var pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float coverage = Mathf.Clamp01(radius + 1f - Mathf.Sqrt(dx * dx + dy * dy));
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(coverage * 255f));
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "MapSelectDisc",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private void EnsureStyles()
        {
            if (_title != null && _styledWidth == Screen.width) return;

            _styledWidth = Screen.width;
            _u = Screen.width / 1080f;

            _title = Style(68, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, _semiboldFont);
            _subtitle = Style(25, FontStyle.Normal, TextAnchor.MiddleCenter,
                              new Color(0.83f, 0.86f, 0.82f), _regularFont);
            _nodeNumber = Style(39, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, _semiboldFont);
            _mapNameLeft = Style(30, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, _semiboldFont);
            _mapNameRight = Style(30, FontStyle.Bold, TextAnchor.MiddleRight, Color.white, _semiboldFont);
            _metaLeft = Style(20, FontStyle.Bold, TextAnchor.MiddleLeft, GoldSoft, _semiboldFont);
            _metaRight = Style(20, FontStyle.Bold, TextAnchor.MiddleRight, GoldSoft, _semiboldFont);
            _footer = Style(22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, _semiboldFont);
        }

        private GUIStyle Style(int px, FontStyle fontStyle, TextAnchor alignment, Color color, Font? font)
        {
            return new GUIStyle(GUI.skin.label)
            {
                font = font,
                fontSize = Mathf.Max(1, Mathf.RoundToInt(px * _u)),
                fontStyle = fontStyle,
                alignment = alignment,
                clipping = TextClipping.Clip,
                normal = { textColor = color },
            };
        }

        private void OnGUI()
        {
            if (Baked == null || _ordered.Count == 0 || _disc == null) return;
            EnsureStyles();

            float u = _u;
            Rect screen = new(0f, 0f, Screen.width, Screen.height);

            if (_background != null)
                GUI.DrawTexture(screen, _background, ScaleMode.ScaleAndCrop, false);
            else
                Fill(screen, DeepGreen);

            // Lớp màu giữ chữ/node đọc rõ trên cả vùng đèn sân vận động lẫn cỏ.
            Fill(screen, new Color(0.015f, 0.055f, 0.04f, 0.28f));

            Rect safe = Screen.safeArea;
            float safeTop = Screen.height - safe.yMax;
            float safeBottom = safe.y;
            float headerTop = safeTop + 18f * u;

            Fill(new Rect(0f, 0f, Screen.width, headerTop + 184f * u),
                 new Color(0.01f, 0.035f, 0.027f, 0.68f));
            Fill(new Rect(0f, headerTop + 182f * u, Screen.width, 2f * u),
                 new Color(Gold.r, Gold.g, Gold.b, 0.45f));

            GUI.Label(new Rect(0f, headerTop + 12f * u, Screen.width, 78f * u), "LA MURALLA", _title);
            GUI.Label(new Rect(0f, headerTop + 82f * u, Screen.width, 38f * u),
                      "ROAD TO GLORY  ·  CAMPAIGN", _subtitle);

            Rect progress = new(Screen.width * 0.5f - 108f * u, headerTop + 127f * u,
                                 216f * u, 43f * u);
            DrawCapsule(progress, new Color(0.02f, 0.08f, 0.055f, 0.82f));
            GUI.Label(progress, _progressText, _footer);

            float journeyTop = headerTop + 235f * u;
            float journeyBottom = Screen.height - safeBottom - 225f * u;
            if (journeyBottom <= journeyTop)
                journeyBottom = journeyTop + 1f;

            // Đường đi nằm dưới node. Đoạn đã vượt qua sáng vàng, đoạn chưa tới
            // giữ màu trắng xám để người chơi đọc tiến độ chỉ bằng một cái liếc.
            for (int i = 0; i < _ordered.Count - 1; i++)
            {
                Vector2 from = NodeCenter(i, journeyTop, journeyBottom);
                Vector2 to = NodeCenter(i + 1, journeyTop, journeyBottom);
                DrawLine(from, to, 18f * u, new Color(0f, 0f, 0f, 0.42f));
                bool cleared = _stars[i] > 0;
                DrawLine(from, to, 9f * u,
                         cleared ? new Color(Gold.r, Gold.g, Gold.b, 0.92f)
                                 : new Color(0.58f, 0.64f, 0.61f, 0.42f));
            }

            for (int i = 0; i < _ordered.Count; i++)
                DrawNode(i, journeyTop, journeyBottom);

            Rect footer = new(Screen.width * 0.5f - 310f * u,
                               Screen.height - safeBottom - 92f * u,
                               620f * u, 58f * u);
            DrawCapsule(footer, new Color(0.01f, 0.045f, 0.032f, 0.88f));
            GUI.Label(footer, _currentText, _footer);

        }

        private Vector2 NodeCenter(int index, float top, float bottom)
        {
            float t = _ordered.Count <= 1 ? 0f : index / (float)(_ordered.Count - 1);
            float y = Mathf.Lerp(bottom, top, t);

            // Điểm đầu và đích nằm giữa; các checkpoint còn lại đổi bên liên tục.
            float xFraction;
            if (index == 0 || index == _ordered.Count - 1)
                xFraction = 0.5f;
            else
                xFraction = index % 2 == 0 ? 0.27f : 0.73f;

            return new Vector2(Screen.width * xFraction, y);
        }

        private void DrawNode(int index, float top, float bottom)
        {
            float u = _u;
            Vector2 center = NodeCenter(index, top, bottom);
            bool open = _unlocked[index];
            bool complete = _stars[index] > 0;
            bool current = index == _currentIndex;
            float diameter = (current ? 128f : 108f) * u;

            if (current)
            {
                float pulse = (Mathf.Sin(Time.unscaledTime * 3.2f) + 1f) * 0.5f;
                float ring = diameter + Mathf.Lerp(24f, 52f, pulse) * u;
                DrawDisc(center, ring, new Color(Mint.r, Mint.g, Mint.b, Mathf.Lerp(0.22f, 0.04f, pulse)));
            }

            DrawDisc(center + new Vector2(0f, 8f * u), diameter + 16f * u,
                     new Color(0f, 0f, 0f, 0.48f));

            Color rim = !open ? Locked : complete ? Gold : current ? Mint : new Color(0.66f, 0.78f, 0.71f);
            Color core = !open ? new Color(0.07f, 0.095f, 0.085f)
                               : complete ? new Color(0.055f, 0.22f, 0.135f)
                                          : new Color(0.08f, 0.31f, 0.19f);
            DrawDisc(center, diameter + 14f * u, rim);
            DrawDisc(center, diameter, core);
            DrawDisc(center, diameter - 18f * u, new Color(1f, 1f, 1f, open ? 0.07f : 0.025f));

            Rect nodeRect = new(center.x - diameter * 0.5f, center.y - diameter * 0.5f,
                                 diameter, diameter);
            Color oldContent = GUI.contentColor;
            GUI.contentColor = open ? Color.white : new Color(1f, 1f, 1f, 0.28f);
            GUI.Label(nodeRect, _numbers[index], _nodeNumber);
            GUI.contentColor = oldContent;

            if (!open)
                DrawLock(new Vector2(center.x + 39f * u, center.y - 39f * u), 31f * u);

            DrawMapLabel(index, center, diameter, complete, current, open);

            if (open)
            {
                int id = GUIUtility.GetControlID(FocusType.Passive);
                bool clicked = Tapped(new Rect(center.x - 70f * u, center.y - 70f * u,
                                               140f * u, 140f * u), id, out bool pressed);
                if (pressed)
                    DrawDisc(center, diameter - 12f * u, new Color(1f, 1f, 1f, 0.13f));

                if (clicked)
                {
                    AudioService.Play("tap", 0.6f);
                    MapSession.PickedMapId = _ordered[index].id;
                    Reload();
                }
            }
        }

        private void DrawMapLabel(int index, Vector2 center, float diameter,
                                  bool complete, bool current, bool open)
        {
            float u = _u;
            float width = 292f * u;
            bool placeRight = center.x <= Screen.width * 0.5f;
            float x = placeRight
                ? center.x + diameter * 0.5f + 19f * u
                : center.x - diameter * 0.5f - width - 19f * u;
            Rect panel = new(x, center.y - 43f * u, width, 86f * u);
            DrawCapsule(panel, new Color(0.005f, 0.035f, 0.024f, open ? 0.78f : 0.56f));

            GUIStyle nameStyle = placeRight ? _mapNameLeft! : _mapNameRight!;
            GUIStyle metaStyle = placeRight ? _metaLeft! : _metaRight!;
            float pad = 17f * u;
            Rect nameRect = new(panel.x + pad, panel.y + 7f * u,
                                panel.width - pad * 2f, 40f * u);
            Rect metaRect = new(panel.x + pad, panel.y + 44f * u,
                                panel.width - pad * 2f, 30f * u);

            Color oldContent = GUI.contentColor;
            GUI.contentColor = open ? Color.white : new Color(1f, 1f, 1f, 0.38f);
            GUI.Label(nameRect, _ordered[index].displayName, nameStyle);

            if (complete)
            {
                DrawStars(metaRect, _stars[index], placeRight);
            }
            else
            {
                GUI.contentColor = current ? Mint : open ? GoldSoft : new Color(1f, 1f, 1f, 0.30f);
                GUI.Label(metaRect, current ? "CURRENT" : open ? "AVAILABLE" : "LOCKED", metaStyle);
            }

            GUI.contentColor = oldContent;
        }

        private void DrawStars(Rect row, int stars, bool alignLeft)
        {
            float u = _u;
            float size = 23f * u;
            float gap = 7f * u;
            float total = size * 3f + gap * 2f;
            float x = alignLeft ? row.x : row.xMax - total;
            float y = row.y + (row.height - size) * 0.5f;

            for (int i = 0; i < 3; i++)
            {
                GUI.color = i < stars ? Gold : new Color(1f, 1f, 1f, 0.18f);
                GUI.DrawTexture(new Rect(x + i * (size + gap), y, size, size),
                                Icons.Star, ScaleMode.ScaleToFit, true);
            }
            GUI.color = Color.white;
        }

        private void DrawLock(Vector2 center, float size)
        {
            float u = _u;
            Color color = new(0.72f, 0.76f, 0.73f, 0.80f);
            float bodyW = size;
            float bodyH = size * 0.68f;
            Fill(new Rect(center.x - bodyW * 0.5f, center.y - bodyH * 0.05f,
                          bodyW, bodyH), color);
            DrawLine(new Vector2(center.x - size * 0.30f, center.y),
                     new Vector2(center.x - size * 0.30f, center.y - size * 0.38f),
                     5f * u, color);
            DrawLine(new Vector2(center.x + size * 0.30f, center.y),
                     new Vector2(center.x + size * 0.30f, center.y - size * 0.38f),
                     5f * u, color);
            DrawLine(new Vector2(center.x - size * 0.30f, center.y - size * 0.38f),
                     new Vector2(center.x + size * 0.30f, center.y - size * 0.38f),
                     5f * u, color);
        }

        private void DrawDisc(Vector2 center, float diameter, Color color)
        {
            if (_disc == null) return;
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x - diameter * 0.5f, center.y - diameter * 0.5f,
                                     diameter, diameter), _disc, ScaleMode.StretchToFill, true);
            GUI.color = Color.white;
        }

        private void DrawCapsule(Rect rect, Color color)
        {
            float radius = rect.height * 0.5f;
            Fill(new Rect(rect.x + radius, rect.y, Mathf.Max(0f, rect.width - rect.height), rect.height), color);
            DrawDisc(new Vector2(rect.x + radius, rect.center.y), rect.height, color);
            DrawDisc(new Vector2(rect.xMax - radius, rect.center.y), rect.height, color);
        }

        private static void DrawLine(Vector2 from, Vector2 to, float width, Color color)
        {
            Vector2 delta = to - from;
            float length = delta.magnitude;
            if (length <= 0.01f) return;

            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, from);
            Fill(new Rect(from.x, from.y - width * 0.5f, length, width), color);
            GUI.matrix = oldMatrix;
        }

        private static void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private static bool Tapped(Rect rect, int id, out bool pressed)
        {
            bool clicked = false;
            Event e = Event.current;
            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && GUIUtility.hotControl == 0 && rect.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        clicked = rect.Contains(e.mousePosition);
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id) e.Use();
                    break;
            }
            pressed = GUIUtility.hotControl == id;
            return clicked;
        }

        internal static void Reload()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }
    }
}
