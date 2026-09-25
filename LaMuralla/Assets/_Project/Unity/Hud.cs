using System.Collections.Generic;
using LaMuralla.Core.Config;
using LaMuralla.Core.Json;
using LaMuralla.Core.Match;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Tiền, máu cầu môn, wave, nút bắt đầu/skip/×2, và màn thắng-thua.
    ///
    /// Dùng IMGUI (`OnGUI`) chứ không uGUI: không cần prefab, không cần Canvas dựng
    /// tay, không cần font asset — mọi thứ trong code, đọc được trong diff. Đây là
    /// HUD TẠM cho việc kiểm luật trên máy thật; `docs/06` mô tả HUD thật.
    ///
    /// Cột "không làm" của UI (docs/05 §3): **"Thay đổi state trực tiếp"**. Lớp này
    /// chỉ ĐỌC `MatchController` và phát ý định (`StartNextWave`, `SkipRest`).
    /// </summary>
    internal sealed class Hud : MonoBehaviour
    {
        private MatchController _m = null!;

        // Nhãn "cửa nào ra quân" — dựng lại đúng một lần mỗi wave, xem `DrawLanePreview`.
        private readonly System.Text.StringBuilder _laneSb = new();
        private string? _lanePreviewText;
        private int _lanePreviewWave = -1;
        private PathDef _def = null!;
        private MatchView _view = null!;
        private GUIStyle? _big, _mid, _tiny, _btn;

        internal void Bind(MatchController m, PathDef def, MatchView view)
        {
            _m = m; _def = def; _view = view;
        }

        // Style DẪN XUẤT + GUIContent tái dùng. `EnsureStyles` đã chặn dựng lại
        // _big/_mid/_btn/_tiny, nhưng các biến thể tạo TRONG OnGUI thì không ai chặn —
        // đó mới là chỗ rác dồn. Xem chú thích ở IconText.
        private static GUIStyle? _btnLabel;
        private static GUIStyle? _btnShadow;
        private static GUIStyle? _btnSmall;
        private static GUIStyle? _tinyDim;
        private static readonly GUIContent _scratch = new(string.Empty);

        private void EnsureStyles()
        {
            if (_big != null) return;
            // Style dẫn xuất phải dựng lại khi style gốc đổi (đổi độ phân giải).
            _btnLabel = null; _btnSmall = null; _tinyDim = null; _btnShadow = null;
            LoadFonts();

            // Đậm = DÙNG FONT SemiBold, không bật `FontStyle.Bold`. Unity làm đậm một
            // font vốn không có nét đậm bằng cách NHOÈ chữ ra hai bên; ở cỡ 26px của
            // thẻ tướng nó cho ra chữ bẩn chứ không phải chữ đậm. Chỉ khi thiếu font
            // mới quay về faux-bold, vì lúc đó không còn lựa chọn nào khác.
            FontStyle weight = _fontBold != null ? FontStyle.Normal : FontStyle.Bold;

            // Cỡ chữ theo BỀ NGANG màn, không theo pixel cố định: 1080px và 1170px
            // cho ra chữ cùng cỡ vật lý. Gõ `fontSize = 40` thì trên máy khác nó to
            // nhỏ khác nhau.
            //
            // Inter đặc hơn LiberationSans ở cùng cỡ px, nên mọi cỡ hạ ~4%: giữ
            // nguyên số là chữ nở ra và dòng "out of range (1.3)" ở thẻ tướng tràn.
            float u = Screen.width / 1080f;
            _big = new GUIStyle(GUI.skin.label)
            {
                font = _fontBold, fontSize = (int)(62 * u), fontStyle = weight,
                alignment = TextAnchor.MiddleCenter,
            };
            _mid = new GUIStyle(GUI.skin.label)
            { font = _fontBold, fontSize = (int)(38 * u), fontStyle = weight };
            _btn = new GUIStyle(GUI.skin.label)
            {
                font = _fontBold, fontSize = (int)(33 * u), fontStyle = weight,
                alignment = TextAnchor.MiddleCenter, wordWrap = false,
            };
            _tiny = new GUIStyle(GUI.skin.label)
            { font = _fontBody, fontSize = (int)(26 * u), normal = { textColor = Color.white } };
        }

        /// <summary>
        /// Inter (SIL OFL 1.1) — xem `Resources/Fonts/CREDITS.txt`.
        ///
        /// Font mặc định của IMGUI là LiberationSans: thứ Unity đưa ra khi KHÔNG AI
        /// CHỌN GÌ. Nó không hỏng, nhưng chữ chiếm phần lớn diện tích HUD này — để
        /// nó ở mặc định là để phần lớn HUD ở mặc định.
        ///
        /// Thiếu file → rơi về font cũ, HUD vẫn đọc được, cảnh báo ĐÚNG MỘT LẦN.
        /// Cùng lối hỏng-mềm với `Icons` và `AudioService`.
        /// </summary>
        private static Font? _fontBold, _fontBody;
        private static bool _fontsTried;

        private static void LoadFonts()
        {
            if (_fontsTried) return;
            _fontsTried = true;
            _fontBold = Resources.Load<Font>("Fonts/Inter-SemiBold");
            _fontBody = Resources.Load<Font>("Fonts/Inter-Regular");
            if (_fontBold == null || _fontBody == null)
                Debug.LogWarning("[Hud] thiếu Resources/Fonts/Inter-*.ttf — rơi về font mặc " +
                                 "định của IMGUI. Kiểm `includeFontData: 1` trong .meta.");
        }

        /// <summary>
        /// Mép trên của thanh chọn tướng, theo pixel ĐO TỪ ĐÁY (hệ của Input System,
        /// ngược với hệ của GUI). MatchView hỏi để biết cú chạm rơi vào thanh hay
        /// vào sân — chạm thanh thì đừng bỏ chọn ô.
        /// </summary>
        /// 🔴 Thanh hiện ở CẢ Preparing LẪN Fighting. Bản đầu chỉ vẽ khi Preparing
        /// → bấm BẮT ĐẦU là thanh biến mất, không mua được tướng nữa. Nhưng mua tướng
        /// GIỮA LÚC wave đang chạy chính là trò chơi — `01` §8 **FM-03** còn chốt rõ
        /// "Bán tướng ngay khi wave đang chạy → Cho phép". Người chơi báo đúng lỗi này.
        internal float BarTopPixels =>
            _m != null && _m.Phase is MatchPhase.Preparing or MatchPhase.Fighting
                ? BarHeight + Screen.safeArea.y : 0;

        // 0.20 → 0.25: thẻ giờ có chân dung, ở 0.20 thì mặt tướng chỉ còn ~70px
        // và ba dòng chữ chồng lên nhau. Thanh chỉ hiện khi đã chọn ô nên phần màn
        // bị che là tạm thời.
        private float BarHeight => Screen.width * 0.25f;

        // 🔴 KHÔNG EMOJI TRONG BẤT KỲ CHUỖI NÀO VẼ RA MÀN HÌNH.
        // Font mặc định của IMGUI trên iOS là LiberationSans — có Latin-1 (× · À-ỹ)
        // nhưng KHÔNG có bảng emoji. Trên máy thật 2026-08-20: 🔇 ra ô vuông.
        // Icon nay lay tu `Icons` — vẽ bằng code, không phụ thuộc font nào.

        // ── Bảng màu thẻ tướng ──────────────────────────────────────────────
        private static readonly Color CardBg = new(0.10f, 0.11f, 0.13f, 0.96f);
        private static readonly Color CardOff = new(0.10f, 0.11f, 0.13f, 0.72f);
        private static readonly Color Coin = new(1f, 0.84f, 0.30f);
        private static readonly Color Warn = new(1f, 0.45f, 0.38f);
        private static readonly Color Dim = new(0.62f, 0.65f, 0.70f);

        /// <summary>
        /// Vẽ icon + chữ như MỘT khối. Icon cao bằng `fontSize` nên đổi cỡ chữ là
        /// icon tự theo, không bao giờ lệch nhau.
        /// </summary>
        private static void IconText(Rect r, Texture2D icon, string text, GUIStyle st,
                                     Color tint, float u, bool centre)
        {
            // 🔴 KHÔNG cấp phát trong OnGUI. Bản cũ gọi `new GUIContent(text)` VÀ
            // `new GUIStyle(st)` ở đây; IconText được vẽ 5–8 lần mỗi frame, mà OnGUI
            // chạy ít nhất 2 lần mỗi frame (Layout + Repaint) → ~1000 GUIStyle/giây.
            // GUIStyle là đối tượng nặng; rác đó dồn lại thành cú thu gom giật hình.
            //
            // Thay bằng: một GUIContent tái dùng, và MƯỢN-TRẢ trường `alignment` của
            // chính style gốc. IMGUI vẽ TỨC THÌ nên sửa rồi trả lại ngay là an toàn —
            // không ai đọc style đó giữa hai dòng này.
            _scratch.text = text;
            float ih = st.fontSize, gap = 7 * u;
            float tw = st.CalcSize(_scratch).x;
            float x = centre ? r.x + (r.width - (ih + gap + tw)) / 2f : r.x;

            GUI.color = tint;
            GUI.DrawTexture(new Rect(x, r.y + ih * 0.08f, ih, ih), icon, ScaleMode.ScaleToFit);
            GUI.color = Color.white;

            TextAnchor keep = st.alignment;
            st.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(x + ih + gap, r.y, tw + 6 * u, r.height), text, st);
            st.alignment = keep;
        }

        private static void Fill(Rect r, Color c)
        {
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        // ── Bút vẽ: bo góc, chuyển sắc, bóng đổ ─────────────────────────────
        //
        // 🔴 GÓC VUÔNG + MÀU PHẲNG LÀ THỨ LÀM HUD NÀY TRÔNG THÔ. Mọi hình ở đây
        // trước giờ là `Fill` = một chữ nhật đặc, cạnh sắc, một màu duy nhất. Đó là
        // hình dạng của "chưa ai thiết kế".
        //
        // `GUI.DrawTexture` có overload nhận `borderWidths` + `borderRadiuses`
        // (Unity 2019.1+, chạy trong player): bo góc và viền THẬT, khử răng cưa,
        // không cần texture 9-slice, không cần shader, không thêm một asset nào.
        // Nếu overload này im lặng không hoạt động trên một nền tảng nào đó thì hỏng
        // mềm — ra lại đúng hình chữ nhật như cũ, không mất nút.
        private static class Ui
        {
            /// <summary>Khối đặc bo góc. `rad` theo từng góc: (trái-trên, phải-trên,
            /// phải-dưới, trái-dưới) — dải màu nhận dạng ở đầu thẻ tướng chỉ bo hai
            /// góc trên, nếu bo cả bốn thì nó rời khỏi thân thẻ.</summary>
            internal static void Round(Rect r, Color c, Vector4 rad) =>
                GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                                true, 0f, c, Vector4.zero, rad);

            internal static void Round(Rect r, Color c, float rad) =>
                Round(r, c, rad * Vector4.one);

            internal static void Border(Rect r, Color c, float w, float rad) =>
                GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                                true, 0f, c, w * Vector4.one, rad * Vector4.one);

            /// <summary>Thân nút: sáng ở đỉnh, tối dần xuống đáy. Một mặt một màu
            /// không có hướng ánh sáng, nên mắt không đọc ra khối — nó đọc ra ô màu.</summary>
            internal static void Gradient(Rect r, Color c, float rad) =>
                GUI.DrawTexture(r, Ramp(), ScaleMode.StretchToFill,
                                true, 0f, c, Vector4.zero, rad * Vector4.one);

            /// <summary>Bóng đổ mềm = ba lớp đen rất nhạt nở dần. Một lớp đặc cho ra
            /// đường viền đen cứng, nhìn còn tệ hơn là không có bóng.</summary>
            internal static void Shadow(Rect r, float rad, float u)
            {
                for (int i = 3; i >= 1; i--)
                {
                    float g = i * 2.5f * u;
                    Round(new Rect(r.x - g, r.y - g + 5f * u, r.width + g * 2f, r.height + g * 2f),
                          new Color(0f, 0f, 0f, 0.10f), rad + g);
                }
            }

            // Dải sáng→tối 1×64, dựng một lần.
            //
            // 🔴 GIÁ TRỊ PHẢI ≤ 1. Texel là RGBA32 (kẹp 0..1) và `GUI.DrawTexture`
            // NHÂN nó với màu truyền vào. Đặt 1.15 cho "sáng hơn màu gốc" thì nó
            // lặng lẽ kẹp về 1.0 và nửa trên của dải thành phẳng lì — sai mà không
            // báo lỗi. Muốn sáng hơn thì nâng màu gốc lên, không nâng dải.
            private static Texture2D? _ramp;

            private static Texture2D Ramp()
            {
                if (_ramp != null) return _ramp;
                const int n = 64;
                var t = new Texture2D(1, n, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    // Không có cờ này thì texture chết theo mỗi lần nạp lại scene —
                    // và nút RESTART làm đúng việc đó.
                    hideFlags = HideFlags.HideAndDontSave,
                };
                for (int i = 0; i < n; i++)
                {
                    // i = 0 là ĐÁY texture. IMGUI vẽ hàng trên cùng của texture vào
                    // mép trên của rect, nên hàng n-1 là chỗ sáng nhất.
                    float f = i / (n - 1f);
                    float g = Mathf.Lerp(0.72f, 1f, f * f * (3f - 2f * f));   // smoothstep
                    t.SetPixel(0, i, new Color(g, g, g, 1f));
                }
                t.Apply();
                return _ramp = t;
            }
        }

        /// <summary>
        /// Bắt chạm cho MỘT nút: trả về "đang bị nhấn xuống", và `clicked` khi nhả
        /// tay ra TRONG lòng nút.
        ///
        /// 🔴 VÌ SAO BỎ `GUI.Button`. Nó không cho biết nút có đang bị nhấn không, và
        /// thứ lớp này dùng thay cho điều đó là CODE CHẾT TRÊN ĐIỆN THOẠI.
        ///
        /// Mọi nút ở đây từng đọc trạng thái bằng `r.Contains(Event.current.mousePosition)`
        /// rồi gọi nó là `hover`. Máy thật KHÔNG CÓ CON TRỎ: `mousePosition` chỉ có
        /// nghĩa trong đúng khung hình có sự kiện chạm, rồi đứng nguyên tại đó mãi.
        /// Hai hệ quả, cả hai đều nhìn thấy được: (a) bấm nút không có phản hồi nào,
        /// (b) một nút bất kỳ SÁNG VĨNH VIỄN ở chỗ ngón tay vừa rời đi. Đó là phần
        /// lớn cảm giác "thô", và không có kiểu vẽ nào chữa được nó.
        ///
        /// `hotControl` giải quyết luôn điều chú thích cũ lo — "bấm xuyên qua hai nút
        /// chồng nhau": nút nào bắt được cú nhấn thì GIỮ, nút khác không nhận nữa.
        /// Đó là cùng cơ chế `GUI.Button` vẫn dùng bên trong, chỉ là giờ ta đọc được.
        ///
        /// Nhả tay ra NGOÀI nút = huỷ, không tính là bấm. Đó là thói quen của cả iOS
        /// lẫn Android, và là đường thoát duy nhất khi người chơi nhận ra ngón tay
        /// mình đang đè lên QUIT.
        /// </summary>
        private static bool Interact(Rect r, int id, out bool clicked)
        {
            clicked = false;
            Event e = Event.current;
            switch (e.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (e.button == 0 && GUIUtility.hotControl == 0 && r.Contains(e.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        clicked = r.Contains(e.mousePosition);
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    // Nuốt drag khi đang giữ, nếu không thì kéo ngón tay trên nút sẽ
                    // rơi xuống `MatchView.HandleTouch` và chọn mất một ô phía sau.
                    if (GUIUtility.hotControl == id) e.Use();
                    break;
            }
            return GUIUtility.hotControl == id;
        }

        /// <summary>
        /// Mã điều khiển IMGUI, kèm một `hint` dẫn từ khoá của nút.
        ///
        /// IMGUI cấp mã theo THỨ TỰ VẼ; `hint` là thứ Unity dùng để khớp lại control
        /// khi thứ tự đó xê dịch. Nó làm mã bền hơn, KHÔNG phải bảo chứng — thuật
        /// toán bên trong `GetControlID` là chi tiết nội bộ của Unity và ở đây không
        /// kiểm chứng được.
        ///
        /// Nên chỗ dựa thật là hai thứ đo được: (a) tập nút của HUD này chỉ đổi lúc
        /// NHẢ TAY, không đổi giữa một cú nhấn; (b) `ReleaseStuckPress` gỡ kẹt cho
        /// đúng cái đường hiếm còn lại.
        /// </summary>
        private static int IdOf(string key) =>
            GUIUtility.GetControlID(key.GetHashCode(), FocusType.Passive);

        // Chân dung nạp một lần rồi giữ. `null` cũng được nhớ: chưa chạy
        // "Bake Tower Portraits" thì thẻ rơi về ô màu, không spam Resources.Load
        // mỗi khung hình.
        private readonly Dictionary<string, Texture2D?> _portrait = new();

        private Texture2D? Portrait(string id)
        {
            if (_portrait.TryGetValue(id, out Texture2D? t)) return t;
            t = Resources.Load<Texture2D>($"Art/portrait_{id}");
            _portrait[id] = t;
            return t;
        }

        // ── Nút ─────────────────────────────────────────────────────────────
        // `GUI.skin.button` mặc định của Unity là nút xám bo góc thời 2010 và
        // KHÔNG scale theo màn — trên 1080 dọc nó vừa nhạt vừa lạc lõng cạnh thẻ
        // tướng nền tối. Vẽ tay bằng đúng bộ hàm `Fill`/`Outline` của thẻ để cả
        // HUD nói cùng một ngôn ngữ hình.
        //
        // Hit-test do `Interact` lo — xem chú thích ở đó về lý do bỏ `GUI.Button`.
        private enum Btn { Primary, Upgrade, Danger, Neutral }

        // Màu THÂN nút. Dải màu nhân vào đây kéo mọi thứ tối xuống (0.72→1.0), nên
        // màu gốc phải sáng hơn bản phẳng cũ thì nút mới ra đúng sắc độ cũ.
        private static Color BtnBase(Btn k) => k switch
        {
            Btn.Primary => new Color(0.24f, 0.78f, 0.45f),   // xanh "đi thôi"
            Btn.Upgrade => new Color(0.28f, 0.62f, 1f),
            Btn.Danger => new Color(0.92f, 0.33f, 0.29f),
            _ => new Color(0.26f, 0.28f, 0.34f),
        };

        private static readonly Color BtnOff = new(0.23f, 0.24f, 0.28f);

        /// <summary>
        /// Nút khối: chân đế tối, thân chuyển sắc bo góc nổi trên nó, bóng đổ mềm.
        ///
        /// 🔴 CẢ HIỆU ỨNG NHẤN GÓI TRONG MỘT BIẾN `lip`. Bấm xuống thì THÂN trượt
        /// xuống đúng bề dày chân đế: chân đế lộ ra ở TRÊN thay vì ở dưới, tức nguồn
        /// sáng đảo chiều, và bóng đổ tắt. Mắt đọc ra "khối bị ấn lún" mà không cần
        /// một hình vẽ nào khác. Đổi màu khi bấm thì rẻ hơn nhưng nó nói "nút đổi
        /// màu", không nói "nút bị ấn".
        ///
        /// `enabled = false` thì xám, không bóng, và LUÔN trả về false — không bao
        /// giờ để nút nhìn bấm được mà bấm không ăn.
        /// </summary>
        private bool Button(Rect r, string label, Btn kind, float u, bool enabled = true,
                            Texture2D? icon = null)
        {
            bool held = Interact(r, IdOf(label), out bool clicked) && enabled;

            float rad = 20 * u, lip = 8 * u;
            Color b = enabled ? BtnBase(kind) : BtnOff;

            // Bóng CHỈ khi nút đang nổi. Bấm xuống mà bóng còn nguyên thì nút không
            // lún, nó chỉ đổi màu — và mắt bắt được sự sai lệch đó ngay.
            if (enabled && !held) Ui.Shadow(r, rad, u);

            Ui.Round(r, b * (enabled ? 0.48f : 0.78f), rad);            // chân đế

            var body = new Rect(r.x, r.y + (held ? lip : 0f), r.width, r.height - lip);
            Ui.Gradient(body, held ? b * 0.88f : b, rad);               // thân

            // Vệt sáng mép trên, thụt vào khỏi hai góc bo — kéo dài hết bề rộng thì
            // nó cắt ngang chỗ góc cong và lộ ra là một vạch thẳng dán lên.
            if (!held && enabled)
                Ui.Round(new Rect(body.x + rad * 0.7f, body.y + 2f * u,
                                  body.width - rad * 1.4f, 2f * u),
                         new Color(1f, 1f, 1f, 0.32f), 1f * u);

            Ui.Border(r, new Color(0f, 0f, 0f, 0.28f), 2f * u, rad);

            // Tái dùng thay vì `new GUIStyle(_btn!)` mỗi nút mỗi frame — xem chú
            // thích ở IconText. Chỉ đổi màu chữ nên mutate trực tiếp là đủ.
            _btnLabel ??= new GUIStyle(_btn!);
            _btnLabel.font = _btn!.font;
            _btnLabel.fontSize = _btn.fontSize;
            _btnLabel.alignment = _btn.alignment;
            _btnLabel.normal.textColor = enabled ? Color.white : new Color(0.56f, 0.58f, 0.63f);
            GUIStyle st = _btnLabel;

            if (icon == null)
            {
                // Bóng chữ 1.5px. Chữ trắng phẳng nằm trên thân chuyển sắc bị "trôi"
                // ở nửa dưới, chỗ nền sáng gần bằng nó.
                if (enabled)
                {
                    _btnShadow ??= new GUIStyle(_btn);
                    _btnShadow.font = _btn.font;
                    _btnShadow.fontSize = _btn.fontSize;
                    _btnShadow.alignment = _btn.alignment;
                    _btnShadow.normal.textColor = new Color(0f, 0f, 0f, 0.32f);
                    GUI.Label(new Rect(body.x, body.y + 1.5f * u, body.width, body.height),
                              label, _btnShadow);
                }
                GUI.Label(body, label, st);
            }
            else
            {
                // Cùng phép canh dọc với bản cũ — `IconText` vẽ từ MÉP TRÊN của rect
                // truyền vào, nên nó phải là (giữa thân − nửa cỡ chữ). Lệch một chút
                // ở đây là hai nút Lv/Sell lệch chữ so với mọi nút còn lại.
                IconText(new Rect(body.x, body.center.y - st.fontSize * 0.5f,
                                  body.width, st.fontSize * 1.4f),
                         icon, label, st, st.normal.textColor, u, centre: true);
            }

            // Bấm nút đang mờ vẫn phải kêu — cùng bài học với thẻ mua tướng: im lặng
            // thì người chơi kết luận nút hỏng, chứ không kết luận mình thiếu tiền.
            if (clicked && !enabled) AudioService.Play("denied", 0.6f);
            return clicked && enabled;
        }

        /// <summary>
        /// Nút phụ kiểu "chip": nền trong suốt, chỉ có viền mảnh và ký hiệu. Dùng cho
        /// thứ bấm thưa và không được tranh chú ý với START / mua tướng.
        ///
        /// 🔴 VÙNG CHẠM KHÔNG NHỎ THEO PHẦN VẼ. `touch` giữ nguyên 48pt của
        /// `path.json → ui`; phần vẽ thụt vào <see cref="ChipInset"/>. Bản cũ vẽ nút
        /// 150×110 và lấy luôn đó làm vùng chạm — 110px trên 1080-design chỉ là
        /// **40pt**, tức đã DƯỚI ngưỡng HIG từ trước. Thu nhỏ tiếp phần vẽ mà không
        /// tách hai thứ này ra là làm nó tệ thêm.
        /// </summary>
        private const float ChipInset = 0.19f;   // mỗi bên, theo bề rộng vùng chạm

        /// <summary>Phần VẼ của một chip, thụt vào từ vùng chạm. Tách ra vì glyph vẽ
        /// tay (nút tạm dừng) phải nằm đúng trong ô đó, và hai công thức inset rời
        /// nhau là cách chắc chắn để chúng lệch nhau sau lần sửa thứ ba.</summary>
        private static Rect ChipBody(Rect touch)
        {
            float pad = touch.width * ChipInset;
            return new Rect(touch.x + pad, touch.y + pad,
                            touch.width - pad * 2, touch.height - pad * 2);
        }

        /// <param name="key">Khoá ổn định cho `IdOf` — xem chú thích ở đó. Không
        /// lấy từ `label` được vì chip icon có label rỗng, và hai chip cùng khoá
        /// rỗng sẽ tranh nhau một `hotControl`.</param>
        private bool Chip(Rect touch, string key, string label, bool active, float u,
                          Texture2D? icon = null)
        {
            Rect r = ChipBody(touch);
            bool held = Interact(touch, IdOf(key), out bool clicked);

            Color accent = BtnBase(Btn.Upgrade);
            // Chip bo gần tròn: nó là nút PHỤ, và hình mềm hơn thì nó không tranh
            // chú ý với khối vuông vức của START.
            float rad = r.height * 0.30f;
            Color body = active ? new Color(accent.r, accent.g, accent.b, held ? 0.62f : 0.44f)
                                : new Color(0f, 0f, 0f, held ? 0.50f : 0.30f);
            Color edge = active ? new Color(accent.r, accent.g, accent.b, 0.95f)
                                : new Color(1f, 1f, 1f, held ? 0.55f : 0.24f);
            Color fg = active ? Color.white : new Color(0.88f, 0.90f, 0.94f);

            if (!held) Ui.Shadow(r, rad, u * 0.55f);
            Ui.Round(r, body, rad);
            Ui.Border(r, edge, 2f * u, rad);

            // Nội dung nhích xuống 2px khi giữ — chip quá nhỏ để dùng trò chân đế,
            // nhưng vẫn phải có gì đó động khi ngón tay chạm vào.
            float sink = held ? 2f * u : 0f;

            if (icon != null)
            {
                float ip = r.width * 0.20f;
                GUI.color = fg;
                GUI.DrawTexture(new Rect(r.x + ip, r.y + ip + sink,
                                         r.width - ip * 2, r.height - ip * 2),
                                icon, ScaleMode.ScaleToFit);
                GUI.color = Color.white;
            }
            else
            {
                _btnSmall ??= new GUIStyle(_btn!);
                _btnSmall.font = _btn!.font;
                _btnSmall.fontSize = (int)(30 * u);
                _btnSmall.alignment = _btn.alignment;
                _btnSmall.normal.textColor = fg;
                GUI.Label(new Rect(r.x, r.y + sink, r.width, r.height), label, _btnSmall);
            }

            return clicked;
        }

        /// <summary>
        /// Thanh chọn tướng ở ĐÁY màn hình.
        ///
        /// Trước đây là radial menu xoè quanh ô. Người chơi báo lỗi trên máy thật:
        /// ô gần mép màn (f02 x=-4.73, f05 x=+4.73) làm nút bị KHUẤT. `path_check.py`
        /// có luật chặn menu tràn mép TRÊN nhưng không có luật cho mép TRÁI/PHẢI —
        /// nên nó lọt.
        ///
        /// Đáy màn hình không bao giờ khuất, luôn trong tầm ngón cái, và có chỗ ghi
        /// GIÁ. Radial menu đẹp về ý tưởng nhưng thua ở chỗ nó phải vừa cả 12 vị trí
        /// khác nhau — đáy chỉ có một vị trí.
        ///
        /// Mỗi tướng là một THẺ: dải màu nhận dạng ở đầu, chân dung, tên, giá, chỉ số,
        /// và khi không mua được thì nói THẲNG thiếu gì. Trước đây thẻ chỉ là một ô
        /// màu đặc — người chơi phải học thuộc màu nào là tướng nào.
        /// </summary>
        /// <summary>
        /// Hai vạch dọc = "tạm dừng". Vẽ bằng `Fill` chứ không thêm PNG: bộ icon
        /// Kenney ở `Resources/Icons/` không có cái này, và hai hình chữ nhật thì
        /// không đáng một file asset + một dòng CREDITS. Cũng KHÔNG dùng ký tự
        /// "II" — cùng lý do với emoji ở đầu lớp: đừng đặt cược một nút vào font.
        /// </summary>
        private static void PauseGlyph(Rect body, Color fg)
        {
            float w = body.width * 0.15f;
            float h = body.height * 0.46f;
            float gap = body.width * 0.15f;
            float y = body.center.y - h / 2f;
            Fill(new Rect(body.center.x - gap / 2f - w, y, w, h), fg);
            Fill(new Rect(body.center.x + gap / 2f, y, w, h), fg);
        }

        /// <summary>
        /// Câu hỏi đang treo trong menu tạm dừng. CHƠI LẠI và THOÁT đều vứt bỏ cả
        /// ván đang chơi, và chúng nằm cạnh RESUME — thứ người chơi bấm nhiều nhất.
        /// Một bước xác nhận là giá rẻ; mất một run ở wave 18 thì không.
        ///
        /// Màn hình kết thúc KHÔNG hỏi lại: ở đó chẳng còn gì để mất.
        /// </summary>
        private enum Ask { None, Restart, Quit }
        private Ask _ask;

        private static readonly string[] KeeperTowerIds = { "dibu" };
        private static readonly string[] FieldTowerIds =
            { "batigol", "la_pulga", "d10s", "el_arbitro" };

        private void DrawTowerBar(float u)
        {
            string? slot = _view.SelectedSlot;

            // CHƯA chọn ô → KHÔNG vẽ gì. Cầu môn nằm ở đáy màn (đường kéo dài xuống
            // -9.13 ở vòng 21); thanh này che mất nó. Người chơi báo đúng. Thanh chỉ
            // bung ra khi thật sự chạm một ô để mua — lúc đó che cầu môn tạm thời là
            // chấp nhận được. Ô trống vẫn có vòng sáng trên sân để biết chạm vào đâu.
            if (slot == null) return;

            float h = BarHeight;
            float y = Screen.height - Screen.safeArea.y - h;

            Fill(new Rect(0, y, Screen.width, h), new Color(0.04f, 0.05f, 0.06f, 0.82f));
            Fill(new Rect(0, y, Screen.width, 2 * u), new Color(1f, 1f, 1f, 0.10f));

            TowerInstance? here = _m.Slots.At(slot);
            if (here != null) { DrawOwnedTower(slot, here, u, y, h); return; }

            bool keeper = !_m.Slots.SlotOf(slot).IsField;
            // Static: bản cũ cấp phát HAI mảng mới mỗi lượt OnGUI. Danh sách này
            // không đổi lúc chạy nên không có lý do dựng lại 120 lần mỗi giây.
            string[] ids = keeper ? KeeperTowerIds : FieldTowerIds;

            // Ô thủ môn chỉ mua được Dibu. Chia đều thì thẻ rộng cả màn mà chân
            // dung vẫn bé — canh giữa với bề rộng bằng một thẻ thường.
            float bw = Screen.width / Mathf.Max(ids.Length, 3);
            float x0 = ids.Length < 3 ? (Screen.width - bw * ids.Length) / 2f : 0f;
            for (int i = 0; i < ids.Length; i++)
            {
                TowerDef def = _m.Upgrades.TowerById(ids[i]);
                int cost = def.Levels[0].Cost;
                bool afford = _m.Economy.CanAfford(cost);
                bool placeable = _m.Slots.CanPlace(slot, def, out _);

                // 🔴 VỚI TỚI ĐƯỜNG KHÔNG? Đây là thứ sửa tận gốc ba lần người chơi
                // báo "tướng không gây dame". Cả ba lần đều là thang tầm: f04/f09
                // cách đường 1.30 nên CHỈ La Pulga (tầm Lv1 1.4) bắn được; Batigol
                // tầm 1.0 đặt vào đó là đứng im vĩnh viễn. (El Fideo từng giữ vai
                // này, đã gỡ 2026-08-20 — La Pulga thế chỗ, vẫn phủ 11/11 ô.)
                //
                // Thiết kế đúng — thang tầm là thứ làm việc chọn ô có ý nghĩa (vòng 5
                // dựng nó để giết nội dung chết). Nhưng vẽ vòng tầm thôi thì người
                // chơi vẫn phải tự đoán vòng trắng có chạm đường trắng không, SAU KHI
                // đã trả tiền. Giờ: không với tới = không bấm được.
                double dPath = _view.DistanceToPath(_m.Slots.SlotOf(slot).Position);
                bool reaches = def.Levels[0].Range >= dPath || def.Levels[0].Damage <= 0;
                bool ok = afford && placeable && reaches;

                var r = new Rect(x0 + i * bw + 6 * u, y + 10 * u, bw - 12 * u, h - 20 * u);
                bool held = Interact(r, IdOf("card:" + ids[i]), out bool tapped);
                Color tint = Draw.OfTower(ids[i]);

                float rad = 18 * u;
                if (ok && !held) Ui.Shadow(r, rad, u * 0.7f);
                // Giữ ngón tay = thẻ sáng lên và lún xuống 3px. Trước đây chỗ này
                // dùng `hover`, thứ không tồn tại trên điện thoại.
                Ui.Round(r, ok ? (held ? CardBg * 1.55f : CardBg) : CardOff, rad);
                // Dải nhận dạng CHỈ bo hai góc trên — bo cả bốn thì nó tách khỏi thân
                // thẻ và nhìn như một viên thuốc dán lên trên.
                Ui.Round(new Rect(r.x, r.y, r.width, 7 * u), ok ? tint : Dim * 0.6f,
                         new Vector4(rad, rad, 0f, 0f));
                Ui.Border(r, held && ok ? tint : new Color(1f, 1f, 1f, ok ? 0.10f : 0.05f),
                          2 * u, rad);
                float sink = held && ok ? 3f * u : 0f;

                // Chân dung: nhìn MẶT là biết tướng nào, không phải học thuộc màu.
                float ps = Mathf.Min(r.width * 0.66f, r.height * 0.46f);
                var pr = new Rect(r.center.x - ps / 2f, r.y + 14 * u + sink, ps, ps);
                Texture2D? face = Portrait(ids[i]);
                if (face != null)
                {
                    GUI.color = ok ? Color.white : new Color(1f, 1f, 1f, 0.32f);
                    GUI.DrawTexture(pr, face, ScaleMode.ScaleToFit);
                    GUI.color = Color.white;
                }
                else
                {
                    // Chưa chạy "Bake Tower Portraits" → ô màu như bản cũ, vẫn chơi được.
                    Ui.Round(pr, ok ? tint : Dim * 0.5f, 10 * u);
                }

                float ty = pr.yMax + 2 * u;
                GUI.Label(new Rect(r.x, ty, r.width, 40 * u), def.DisplayName,
                          Centred(_tiny!, ok ? Color.white : Dim, FontStyle.Bold));

                // Giá LUÔN hiện, kể cả khi không đủ tiền — người chơi cần biết phải
                // tiết kiệm bao nhiêu. Ẩn đi thì họ không biết tướng đó tồn tại.
                IconText(new Rect(r.x, ty + 34 * u, r.width, 40 * u), Icons.Coin, $"{cost}",
                         Centred(_tiny!, afford ? Coin : Warn, FontStyle.Bold),
                         afford ? Coin : Warn, u, centre: true);

                // Dòng cuối: mua được thì hiện CHỈ SỐ, không mua được thì hiện LÝ DO.
                // Vẽ cả hai là chúng đè lên nhau — và lúc không mua được thì lý do
                // quan trọng hơn chỉ số.
                string? why = !reaches ? $"out of range ({dPath:0.0})"
                            : !afford ? $"need {cost - _m.Economy.Balance}"
                            : !placeable ? "can't place here"
                            : null;
                GUI.Label(new Rect(r.x, ty + 68 * u, r.width, 36 * u),
                          why ?? $"{PowerOf(def, 1)} · rng {def.Levels[0].Range:0.0}",
                          Centred(_tiny!, why == null ? Dim : Warn,
                                  why == null ? FontStyle.Normal : FontStyle.Bold));

                if (tapped)
                {
                    // Bấm mà không mua được vẫn phải có phản hồi — im lặng thì người
                    // chơi tưởng nút hỏng chứ không tưởng mình thiếu tiền.
                    if (!ok) AudioService.Play("denied", 0.6f);
                    else
                    {
                        _m.Upgrades.TryBuy(slot, ids[i], out _);
                        AudioService.Play("buy");
                        _view.RefreshRange();
                    }
                }
                // 🔴 XEM TRƯỚC TẦM BẮN, GIỜ MỚI CHẠY TRÊN ĐIỆN THOẠI.
                //
                // Nó từng nối vào `hover` — trên máy thật thì không có rê chuột, nên
                // tính năng này chưa từng hoạt động ở nơi nó cần nhất: người chơi phải
                // đoán vòng tầm có chạm đường không SAU KHI đã trả tiền, đúng cái lỗi
                // ba lần bị báo ở trên.
                //
                // Nối vào GIỮ: đặt ngón tay lên thẻ là thấy vòng tầm, nhấc lên mới mua.
                // Không thích thì trượt ngón ra ngoài rồi nhả — `Interact` huỷ.
                if (held && _view.PreviewTowerId != ids[i])
                {
                    _view.PreviewTowerId = ids[i];
                    _view.RefreshRange();
                }
            }
        }

        private static GUIStyle? _centred;

        /// <summary>
        /// 🔴 TÁI DÙNG, không `new GUIStyle` mỗi lần gọi.
        ///
        /// Hàm này được gọi BA lần cho MỖI thẻ tướng, mà thanh mua có tới 4 thẻ →
        /// 12 GUIStyle mỗi lượt `OnGUI`, và `OnGUI` chạy ít nhất 2 lượt mỗi frame.
        /// GUIStyle là đối tượng nặng; đó là ~1400 cái mỗi giây ném cho GC.
        ///
        /// Tái dùng an toàn vì IMGUI vẽ TỨC THÌ: giá trị được đọc ngay trong lời gọi
        /// `GUI.Label`/`Button` kế tiếp, không ai giữ tham chiếu qua frame.
        /// </summary>
        private static GUIStyle Centred(GUIStyle from, Color c, FontStyle f)
        {
            // Xem `EnsureStyles`: đậm là ĐỔI FONT, không phải nhoè chữ ra.
            bool bold = f == FontStyle.Bold && _fontBold != null;
            _centred ??= new GUIStyle(from);
            _centred.fontSize = from.fontSize;
            _centred.wordWrap = from.wordWrap;
            _centred.alignment = TextAnchor.UpperCenter;
            _centred.normal.textColor = c;
            _centred.font = bold ? _fontBold : from.font;
            _centred.fontStyle = bold ? FontStyle.Normal : f;
            return _centred;
        }

        /// <summary>
        /// Sức mạnh của tướng, nói bằng ĐƠN VỊ CỦA CHÍNH NÓ.
        ///
        /// "dmg 0" là con số vô nghĩa với Dibu (giết bằng xác suất) và El Árbitro
        /// (khống chế). Hiện nó ra là nói dối bằng sự thật: đúng về mặt dữ liệu,
        /// sai về mặt ý nghĩa — người chơi đọc "dmg 0" rồi kết luận tướng hỏng.
        /// Người chơi đã báo đúng chuyện này.
        /// </summary>
        private static string PowerOf(TowerDef def, int level)
        {
            // SÁT THƯƠNG TRƯỚC. Bản đầu hỏi killChance/slow trước rồi mới tới damage,
            // và D10S hiện ra "chậm 50%" — giấu mất `dame 30` của nó. D10S là tướng
            // SÁT THƯƠNG có kèm làm chậm, không phải tướng khống chế. Chỉ tướng
            // damage = 0 (Dibu, El Árbitro) mới cần đơn vị khác.
            double d = def.Levels[level - 1].Damage;
            if (d > 0) return $"dmg {d:0}";

            AbilityDef? ab = def.AbilityAt(level);
            if (ab != null && ab.Params.TryGetValue("killChancePercent", out JsonValue? k))
                return $"{k.AsNumber():0}% save";
            if (ab != null && ab.Params.TryGetValue("slowPercent", out JsonValue? sl))
                return $"slow {sl.AsNumber():0}%";
            if (ab != null && ab.Params.ContainsKey("appliesCard"))
                return "cards";

            return "control";
        }

        // Cache nhãn của thanh hero — xem chú thích trong DrawOwnedTower.
        private string _ownedKeySlot = "", _ownedKeyTower = "";
        private int _ownedKeyLevel = -1, _ownedKeyRefund = -1;
        private string _ownedName = "", _ownedStats = "", _ownedUpLabel = "", _ownedSellLabel = "";

        private void DrawOwnedTower(string slot, TowerInstance t, float u, float y, float h)
        {
            TowerDef def = _m.Upgrades.TowerById(t.TowerId);

            // Hai nút bên phải chiếm chỗ trước; phần chữ ở giữa lấy đúng chỗ còn lại.
            float bw = Screen.width * 0.22f;
            float bh = h * 0.52f;
            float by = y + (h - bh) / 2f;

            // Chân dung bên trái để khớp với thẻ mua — cùng một tướng, cùng một mặt.
            float ps = h * 0.62f;
            var pr = new Rect(24 * u, y + (h - ps) / 2f, ps, ps);
            Texture2D? face = Portrait(t.TowerId);
            if (face != null) GUI.DrawTexture(pr, face, ScaleMode.ScaleToFit);
            else Ui.Round(pr, Draw.OfTower(t.TowerId), 10 * u);

            float tx = pr.xMax + 20 * u;
            // Dừng trước nút nâng cấp: rect chữ thò xuống dưới nút thì tên tướng dài
            // (El Árbitro) sẽ đè lên nút, và chữ đè nút là chỗ người ta bấm nhầm.
            float tw = Screen.width - bw * 2 - 50 * u - tx;

            // 🔴 Bốn chuỗi dưới đây từng được NỘI SUY LẠI mỗi lượt OnGUI (≥2 lượt mỗi
            // frame) dù nội dung chỉ đổi khi người chơi nâng cấp hoặc đổi ô. Cache theo
            // đúng ba thứ quyết định chúng: ô nào, tướng gì, cấp mấy — cộng số tiền
            // hoàn (đổi khi bán/nâng).
            int? up = _m.Upgrades.NextUpgradeCost(slot);
            int refund = _m.Upgrades.SellRefund(slot);
            if (_ownedKeySlot != slot || _ownedKeyTower != t.TowerId ||
                _ownedKeyLevel != t.Level || _ownedKeyRefund != refund)
            {
                _ownedKeySlot = slot; _ownedKeyTower = t.TowerId;
                _ownedKeyLevel = t.Level; _ownedKeyRefund = refund;
                _ownedName = $"{def.DisplayName}  Lv{t.Level}";
                _ownedStats = $"{PowerOf(def, t.Level)} · rng {def.Levels[t.Level - 1].Range:0.0}";
                _ownedUpLabel = up != null ? $"Lv{t.Level + 1}   {up}" : "";
                _ownedSellLabel = $"Sell  +{refund}";
            }

            GUI.Label(new Rect(tx, y + 14 * u, tw, 56 * u), _ownedName, _mid);
            GUI.Label(new Rect(tx, y + h * 0.50f, tw, 46 * u), _ownedStats,
                      _tinyDim ??= new GUIStyle(_tiny!) { normal = { textColor = Dim } });

            if (up != null)
            {
                if (Button(new Rect(Screen.width - bw * 2 - 30 * u, by, bw, bh),
                           _ownedUpLabel, Btn.Upgrade, u,
                           _m.Economy.CanAfford(up.Value), Icons.Plus))
                { _m.Upgrades.TryUpgrade(slot, out _); AudioService.Play("upgrade"); _view.RefreshRange(); }
            }
            if (Button(new Rect(Screen.width - bw - 16 * u, by, bw, bh),
                       _ownedSellLabel, Btn.Danger, u, true, Icons.Coin))
            { _m.Upgrades.Sell(slot); AudioService.Play("sell"); _view.Select(null); }
        }

        /// <summary>
        /// Máu cầu môn NGAY TRÊN khung thành — đây là điều kiện thua, nó phải ở chỗ
        /// mắt đang nhìn (quân sắp lọt), không phải ở góc màn hình.
        ///
        /// Nhãn id ô (f01, f02…) đã GỠ 2026-08-20 theo yêu cầu. Nó từng có lý do
        /// thật — người chơi cần gọi tên ô để mô tả lỗi ("ô 4 không bắn"). Nếu sau
        /// này lại cần thì bật lại từ `_view` chứ đừng dựng lại từ đầu.
        /// </summary>
        private void DrawGoalLabel()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 gp = cam.WorldToScreenPoint(_view.GoalLabelWorld);
            IconText(new Rect(gp.x - 100, Screen.height - gp.y - 24, 200, 48), Icons.Goal,
                     $"{_m.Goal.Current}/{_m.Goal.Max}", _mid!, Color.white,
                     Screen.width / 1080f, centre: true);
        }

        private void OnGUI()
        {
            if (_m == null) return;
            EnsureStyles();
            DrawGoalLabel();

            float u = Screen.width / 1080f;
            float pad = 24 * u;

            // Safe area: notch che 1.30 unit trên, home indicator 0.94 dưới (đo trên
            // iPhone 12 ở vòng 8). Vẽ đè lên đó thì chữ chui vào tai thỏ.
            Rect safe = Screen.safeArea;
            float top = Screen.height - safe.y - safe.height + pad;
            float bottom = safe.y + pad;

            IconText(new Rect(pad, top, 500 * u, 60 * u), Icons.Coin,
                     $"{_m.Economy.Balance}", _mid!, Coin, u, centre: false);
            IconText(new Rect(pad, top + 55 * u, 500 * u, 60 * u), Icons.Goal,
                     $"{_m.Goal.Current}/{_m.Goal.Max}", _mid!, Color.white, u, centre: false);
            // Chip tạm dừng chiếm góc phải trên, nên hai nhãn bên phải lùi vào ĐÚNG
            // bề rộng vùng chạm của nó. Gõ tay một hằng số ở đây là hẹn ngày chữ
            // "Wave 20/20" bò vào dưới nút — cùng cái bẫy `minTouchTargetPt` sinh ra
            // để tránh.
            float chip = (float)(_def.MinTouchTargetPt / _def.DesignWidthPt) * Screen.width;
            float rightX = Screen.width - pad - chip - 300 * u;
            GUI.Label(new Rect(rightX, top, 300 * u, 60 * u), $"Wave {_m.Wave}/20", _mid);
            // Số quái thủ môn (Dibu) đã cản. Cản là XÁC SUẤT — người chơi cần thấy
            // Dibu "ăn" được bao nhiêu để biết có đáng tiền không.
            IconText(new Rect(rightX, top + 55 * u, 300 * u, 60 * u), Icons.Save,
                     $"{_m.SavedCount}", _mid!, Color.white, u, centre: false);

            bool live = _m.Phase is MatchPhase.Preparing or MatchPhase.Fighting;
            bool paused = live && _view.Paused;

            // Nút tạm dừng: chỉ khi trận còn chạy VÀ chưa dừng. Lúc đã dừng, đường
            // về nằm ở nút RESUME trong menu — hai chỗ cùng làm một việc thì người
            // chơi phải đoán xem chúng có khác nhau không.
            if (live && !paused)
            {
                var pr = new Rect(Screen.width - pad - chip, top, chip, chip);
                bool hit = Chip(pr, "pause", "", false, u);
                PauseGlyph(ChipBody(pr), new Color(0.86f, 0.88f, 0.92f));
                if (hit)
                {
                    _ask = Ask.None;
                    _view.SetPaused(true);
                    AudioService.Play("tap", 0.6f);
                }
            }

            switch (_m.Phase)
            {
                case MatchPhase.Preparing:
                    // Đang dừng thì KHÔNG vẽ thanh mua tướng lẫn nút START. Nền mờ
                    // của overlay không chặn được chạm (nó chỉ là DrawTexture) — thứ
                    // chặn thật là việc mấy nút này không tồn tại trong khung hình đó.
                    if (paused) break;
                    DrawTowerBar(u);
                    DrawPrepare(u, bottom + BarHeight);
                    break;
                case MatchPhase.Fighting:
                    if (paused) break;
                    DrawTowerBar(u);                       // mua/nâng/bán GIỮA wave
                    DrawSpeed(u, bottom + BarHeight);
                    break;
                case MatchPhase.Won:
                case MatchPhase.Lost:
                    DrawEnd(u);
                    break;
            }

            // Vẽ CUỐI CÙNG để nó nằm trên mọi thứ đã vẽ ở trên (IMGUI vẽ theo thứ
            // tự gọi), và để nút của nó được hit-test trước.
            if (paused) DrawPauseMenu(u);

            ReleaseStuckPress();
        }

        /// <summary>
        /// 🔴 LƯỚI AN TOÀN: thả `hotControl` khi một cú nhả tay đi hết `OnGUI` mà
        /// KHÔNG nút nào nhận.
        ///
        /// Mã điều khiển IMGUI cấp theo thứ tự vẽ. Bình thường tập nút của HUD này
        /// chỉ đổi lúc NHẢ TAY (mua xong, `_ask` đổi, đổi phase) nên trong suốt một
        /// cú nhấn nó đứng yên. Nhưng có đường đổi giữa chừng: đang giữ nút thì wave
        /// dọn sạch → Fighting chuyển Preparing → hai chip ×2/loa bị thay bằng nút
        /// START. Mã trượt, `MouseUp` không khớp `hotControl` nào, và nó KẸT LẠI
        /// khác 0 — từ đó mọi nút im lặng vì `Interact` không cho ai bắt cú nhấn mới.
        /// Không nạp lại scene thì không thoát được.
        ///
        /// `Event.current.type` trả về `Used` sau khi ai đó gọi `e.Use()`. Nên còn
        /// thấy `MouseUp` ở đây nghĩa là đúng: không ai nhận.
        /// </summary>
        private static void ReleaseStuckPress()
        {
            if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl != 0)
                GUIUtility.hotControl = 0;
        }

        // ── Menu tạm dừng ───────────────────────────────────────────────────

        /// <summary>Nền mờ toàn màn + khung panel. Dùng chung cho menu tạm dừng và
        /// màn kết thúc để hai cái nói cùng một ngôn ngữ hình.</summary>
        /// <summary>
        /// Hệ số co để một panel cao `want` nằm lọt màn, chừa lề `margin`.
        ///
        /// 🔴 MỌI THỨ TRONG HUD NÀY ĐO THEO BỀ RỘNG MÀN (`u = Screen.width / 1080`).
        /// Đúng cho máy dọc — 1080 và 1170 cho ra chữ cùng cỡ vật lý. Nhưng ở Game
        /// view NGANG của Editor bề rộng nở ra trong khi chiều cao co lại: đo ở
        /// 1920×1080 thì panel 3 nút cao 1189px trên màn 1080px, tràn 55px cả trên
        /// lẫn dưới. Chữ tràn thì xấu; NÚT tràn thì bấm không tới, và nút đó là
        /// đường duy nhất ra khỏi trận.
        ///
        /// Co có thể kéo nút xuống dưới sàn 48pt. Chấp nhận: tỉ lệ làm nó xảy ra là
        /// màn ngang, tức Editor — không phải máy người chơi cầm.
        /// </summary>
        private static float FitScale(float want, float margin)
        {
            float room = Screen.height - margin * 2f;
            return want <= room ? 1f : room / want;
        }

        private static void Scrim(Rect panel, float u)
        {
            float rad = 28 * u;
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.74f));
            Ui.Shadow(panel, rad, u * 2.2f);
            Ui.Round(panel, new Color(0.09f, 0.10f, 0.12f, 0.97f), rad);
            // Vệt sáng mép trên: cùng thủ pháp với nút, để panel đọc ra là một tấm
            // có bề dày chứ không phải một lỗ thủng màu đen trên màn hình.
            Ui.Round(new Rect(panel.x + rad, panel.y + 2f * u, panel.width - rad * 2f, 2f * u),
                     new Color(1f, 1f, 1f, 0.10f), 1f * u);
            Ui.Border(panel, new Color(1f, 1f, 1f, 0.13f), 2f * u, rad);
        }

        /// <summary>Chiều cao nút của menu: 48pt của `path.json → ui` × 1.15. Gõ tay
        /// `118 * u` ở đây cho ra 42.6pt — DƯỚI sàn HIG, đúng cái bẫy `Chip` đã dính
        /// một lần và phải tách vùng chạm khỏi phần vẽ để thoát ra.</summary>
        private float MenuButtonHeight =>
            (float)(_def.MinTouchTargetPt / _def.DesignWidthPt) * Screen.width * 1.15f;

        private void DrawPauseMenu(float u)
        {
            float bw = Screen.width * 0.58f;
            float bh = MenuButtonHeight, gap = 20 * u, titleH = 84 * u, edge = 30 * u;

            // Panel cao theo SỐ NÚT đang hiện, không phải một tỉ lệ màn hình cố định:
            // lúc hỏi lại chỉ còn 2 nút, và một panel chừa sẵn chỗ cho 3 nút sẽ có
            // một khoảng trống không ai giải thích được.
            int rows = _ask == Ask.None ? 3 : 2;
            float ph = titleH + edge * 3 + bh * rows + gap * (rows - 1);

            // `ph` là tổ hợp tuyến tính thuần của bốn số dưới, nên nhân cả bốn với
            // `k` là nhân `ph` đúng bằng `k` — không phải tính lại. `u` đi theo để
            // chữ và gờ nút trong `Button` co cùng nhịp, chứ không thành nút bé mà
            // chữ vẫn to.
            float k = FitScale(ph, 24 * u);
            if (k < 1f) { bh *= k; gap *= k; titleH *= k; edge *= k; ph *= k; u *= k; }

            var panel = new Rect((Screen.width - bw) / 2f - edge,
                                 (Screen.height - ph) / 2f, bw + edge * 2, ph);
            Scrim(panel, u);

            float x = (Screen.width - bw) / 2f;
            float y = panel.y + edge;

            string title = _ask switch
            {
                Ask.Restart => "RESTART MATCH?",
                Ask.Quit => "LEAVE MATCH?",
                _ => "PAUSED",
            };
            GUI.Label(new Rect(panel.x, y, panel.width, titleH), title,
                      new GUIStyle(_big!) { fontSize = (int)((_ask == Ask.None ? 60 : 42) * u) });
            y += titleH + edge;

            if (_ask == Ask.None)
            {
                if (Button(new Rect(x, y, bw, bh), "RESUME", Btn.Primary, u))
                {
                    _view.SetPaused(false);
                    AudioService.Play("tap", 0.6f);
                }
                y += bh + gap;
                if (Button(new Rect(x, y, bw, bh), "RESTART", Btn.Upgrade, u))
                { _ask = Ask.Restart; AudioService.Play("tap", 0.6f); }
                y += bh + gap;
                // 🔵 "QUIT" → "HOME". Trước đây nút này gọi `Application.Quit()` —
                // thoát hẳn app. Trên điện thoại đó gần như không bao giờ là thứ người
                // chơi muốn: bỏ dở một trận nghĩa là muốn CHỌN MAP KHÁC, không phải
                // đóng game. Giờ nó về màn chọn map.
                if (Button(new Rect(x, y, bw, bh), "HOME", Btn.Danger, u))
                { _ask = Ask.Quit; AudioService.Play("tap", 0.6f); }
                return;
            }

            // Bước xác nhận. Nút huỷ để MÀU TRUNG TÍNH và nằm dưới: cái phá huỷ
            // không được là cái dễ bấm nhất.
            Ask ask = _ask;
            if (Button(new Rect(x, y, bw, bh), ask == Ask.Restart ? "RESTART" : "TO MAPS",
                       Btn.Danger, u))
            {
                _ask = Ask.None;
                AudioService.Play("tap", 0.6f);
                if (ask == Ask.Restart) _view.Restart(); else _view.BackToMenu();
            }
            y += bh + gap;
            if (Button(new Rect(x, y, bw, bh), "CANCEL", Btn.Neutral, u))
            { _ask = Ask.None; AudioService.Play("tap", 0.6f); }
        }

        private void DrawPrepare(float u, float bottom)
        {
            // 48pt vùng chạm tối thiểu (HIG của Apple) — cùng hằng số path.json dùng
            // cho ô đặt tướng. 48pt ≈ 132px trên 1080-design.
            // 48pt là SÀN của HIG, không phải cỡ nên dùng. 2.4× = 316px trên
            // 1080-design — to bằng một phần sáu màn hình, nhìn ra tấm biển. 1.6×
            // = 211px, vẫn gấp rưỡi ngưỡng chạm mà ra dáng nút.
            float h = (float)(_def.MinTouchTargetPt / _def.DesignWidthPt) * Screen.width * 1.6f;
            float w = Screen.width * 0.44f;

            string label = _m.Wave == 0 ? "START" : $"WAVE {_m.Wave + 1}";
            if (Button(new Rect(Screen.width / 2f - w / 2, Screen.height - bottom - h, w, h),
                       label, Btn.Primary, u))
            {
                _view.CloseMenu();
                if (_m.RestRemaining > 0) _m.SkipRest();
                else _m.StartNextWave();
            }

            if (_m.RestRemaining > 0)
                GUI.Label(new Rect(Screen.width / 2f - w / 2, Screen.height - bottom - h - 50 * u, w, 46 * u),
                          $"Skip: +{Round.HalfUp(_m.RestRemaining * 3)}", _mid);

            DrawLanePreview(u, bottom, h, w);
        }

        /// <summary>
        /// Báo trước wave sau ra CỬA NÀO — chỉ vẽ trên map nhiều tuyến.
        ///
        /// 🔴 Bắt buộc, không phải trang trí. `docs/maps/M03` §3: map cho quân ra
        /// luân phiên (wave lẻ tuyến trái, wave chẵn tuyến phải) mà không báo trước
        /// thì nửa số wave người chơi phải ĐOÁN nên xây bên nào — trò tung đồng xu,
        /// không phải bài toán bố trí. Vẽ trong lúc CHUẨN BỊ, tức là lúc còn kịp xây.
        /// </summary>
        private void DrawLanePreview(float u, float bottom, float h, float w)
        {
            if (_m.LaneCount < 2) return;

            // ⚠️ DỰNG CHUỖI MỘT LẦN MỖI WAVE, không phải mỗi khung hình. `OnGUI` chạy
            // 2 lần/khung; dựng StringBuilder ở đây là rác đều đặn — đúng lớp lỗi đã
            // tốn ba vòng chẩn đoán ở vòng 25 (xem STATE.md "SAI CHẨN ĐOÁN 3 VÒNG").
            if (_lanePreviewWave != _m.Wave + 1)
            {
                _lanePreviewWave = _m.Wave + 1;
                var lanes = _m.LanesOfWave(_lanePreviewWave);
                if (lanes.Count == 0) { _lanePreviewText = null; return; }

                _laneSb.Clear();
                _laneSb.Append("GATE: ");
                for (int i = 0; i < lanes.Count; i++)
                {
                    if (i > 0) _laneSb.Append("   ");
                    _laneSb.Append(lanes[i].Lane).Append(' ').Append(lanes[i].Count);
                }
                _lanePreviewText = _laneSb.ToString();
            }
            if (_lanePreviewText == null) return;

            GUI.Label(new Rect(Screen.width / 2f - w, Screen.height - bottom - h - 100 * u, w * 2, 46 * u),
                      _lanePreviewText, _mid);
        }

        /// <summary>
        /// Nút ×2. `05` §4.4: nó KHÔNG phải `Time.timeScale = 2`. Thời gian NGHỈ
        /// không đổi khi ×2 — chỉ thời gian TRẬN nhanh lên. Nên nó nhân vào tham số
        /// của `MatchController.Tick`, không nhân vào đồng hồ của Unity.
        /// </summary>
        private void DrawSpeed(float u, float bottom)
        {
            // Cạnh vùng chạm = ĐÚNG 48pt của `path.json → ui`, không phải số gõ tay.
            // Đổi `minTouchTargetPt` trong config là hai nút này tự đi theo.
            float s = (float)(_def.MinTouchTargetPt / _def.DesignWidthPt) * Screen.width;
            float x = 16 * u, ty = Screen.height - bottom - s;

            // Icon tua nhanh thay cho chữ "×1/×2": nhất quán với chip âm thanh bên
            // cạnh, và bỏ nốt ký tự ngoài ASCII cuối cùng còn vẽ ra màn hình.
            // Trạng thái đọc qua MÀU chip (sáng xanh = đang ×2), không qua chữ.
            bool on = _view.SpeedMultiplier > 1.5f;
            if (Chip(new Rect(x, ty, s, s), "speed", "", on, u, Icons.Speed))
                _view.SpeedMultiplier = on ? 1f : 2f;

            // Tắt tiếng ngay cạnh ×2. Game mobile chơi ở chỗ đông người — không có
            // nút này thì người ta tắt cả app chứ không tắt loa.
            //
            // 🔴 KHÔNG DÙNG EMOJI Ở ĐÂY. Bản đầu để 🔇/🔊 và trên máy thật nó ra Ô
            // VUÔNG: font mặc định của IMGUI trên iOS là LiberationSans, không có
            // bảng emoji. Chip này chỉ có mỗi icon nên hỏng font là mất trắng nút.
            // Chữ ASCII + gạch ngang thì font nào cũng vẽ được.
            if (Chip(new Rect(x + s, ty, s, s), "mute", "", AudioService.Muted, u,
                     AudioService.Muted ? Icons.AudioOff : Icons.AudioOn))
            {
                AudioService.ToggleMute();
                // Phát SAU khi bật lại để nghe thấy mình vừa bật; lúc tắt thì im.
                AudioService.Play("tap", 0.6f);
            }
        }

        /// <summary>
        /// Màn kết thúc. Trước vòng này nó là ngõ cụt: thắng hay thua đều ngồi nhìn
        /// một cái hộp không có nút nào, và đường duy nhất ra khỏi trận là tắt app.
        /// Thua mà không chơi lại được ngay là chỗ người chơi rời game và không quay
        /// lại — nên RESTART ở đây quan trọng hơn cả dãy sao.
        ///
        /// 🔴 CHIỀU CAO HỘP TÍNH TỪ NỘI DUNG, không phải `Screen.height * 0.3f`.
        /// Mọi thứ bên trong đo bằng `u` (= bề rộng màn), nên trên màn NGANG hộp co
        /// lại theo chiều cao trong khi chữ nở ra theo chiều rộng — bản cũ đã tràn
        /// sẵn ở tỉ lệ 16:9 của Game view, chỉ là chưa ai đếm. Thêm hai nút vào đó
        /// là biến tràn ngầm thành nút bấm không tới.
        /// </summary>
        private void DrawEnd(float u)
        {
            bool won = _m.Phase == MatchPhase.Won;
            float bh = MenuButtonHeight;
            float boxH = (won ? 320f : 250f) * u + bh;

            // Cùng phép kẹp với menu tạm dừng — xem `FitScale`. Ghi đè `u` để MỌI
            // toạ độ bên dưới (vốn đều là bội của `u`) co theo một nhịp; sửa từng
            // con số một là cách chắc chắn bỏ sót đúng một cái.
            float k = FitScale(boxH, 24 * u);
            if (k < 1f) { bh *= k; boxH *= k; u *= k; }

            float gap = 22 * u;
            var box = new Rect(Screen.width * 0.1f, (Screen.height - boxH) / 2f,
                               Screen.width * 0.8f, boxH);
            Scrim(box, u);

            GUI.Label(new Rect(box.x, box.y + 20 * u, box.width, 90 * u),
                      won ? "VICTORY" : "DEFEAT", _big);

            // Dãy sao VẼ BẰNG Ô VUÔNG, không dùng ★ — cùng lý do font ở đầu lớp này.
            // Đây là khoảnh khắc thưởng của cả trận; để nó ra ba ô vuông tofu thì
            // hỏng đúng chỗ đáng ra phải đã đời nhất.
            if (won)
            {
                float sz = 56 * u, gap2 = 20 * u;
                float sx = box.x + (box.width - (sz * 3 + gap2 * 2)) / 2f;
                float sy = box.y + 130 * u;
                for (int i = 0; i < 3; i++)
                {
                    var sr = new Rect(sx + i * (sz + gap2), sy, sz, sz);
                    GUI.color = i < (int)_m.Rating ? Coin : new Color(1f, 1f, 1f, 0.22f);
                    GUI.DrawTexture(sr, Icons.Star, ScaleMode.ScaleToFit);
                    GUI.color = Color.white;
                }
            }
            GUI.Label(new Rect(box.x, box.y + (won ? 210f : 140f) * u, box.width, 70 * u),
                      won ? $"Goal HP left {_m.Goal.Current}/{_m.Goal.Max}"
                          : $"Conceded on wave {_m.Wave}",
                      new GUIStyle(_big!) { fontSize = (int)(36 * u) });

            // Hai nút bằng nhau, neo vào MÉP DƯỚI hộp — nội dung phía trên khác nhau
            // giữa thắng và thua (thắng có dãy sao), neo vào mép trên là hai bố cục
            // phải chỉnh riêng.
            float half = (box.width - gap * 3) / 2f;
            float by = box.yMax - 26 * u - bh;
            // 🔵 KHÔNG có QUIT ở đây. Trên điện thoại "thoát game" bằng nút là thao
            // tác lạ; thứ người chơi thật sự muốn sau một trận là ĐI TIẾP hoặc CHỌN
            // MAP KHÁC. QUIT vẫn còn trong menu tạm dừng cho ai cần.
            //
            // Thắng và còn map phía sau → ba nút. Thua, hoặc đã là map cuối → hai nút,
            // vì NEXT lúc đó không có nghĩa (thua thì chưa mở được map sau).
            bool coNext = won && !string.IsNullOrEmpty(_view.NextMapId);
            int soNut = coNext ? 3 : 2;
            float bwEnd = (box.width - gap * (soNut + 1)) / soNut;
            float bx = box.x + gap;

            if (Button(new Rect(bx, by, bwEnd, bh), "RESTART", Btn.Neutral, u))
            { AudioService.Play("tap", 0.6f); _view.Restart(); }
            bx += bwEnd + gap;

            if (coNext)
            {
                if (Button(new Rect(bx, by, bwEnd, bh), "NEXT", Btn.Primary, u))
                { AudioService.Play("tap", 0.6f); _view.GoToMap(_view.NextMapId); }
                bx += bwEnd + gap;
            }

            if (Button(new Rect(bx, by, bwEnd, bh), "MAPS", coNext ? Btn.Neutral : Btn.Primary, u))
            { AudioService.Play("tap", 0.6f); _view.BackToMenu(); }
        }
    }
}
