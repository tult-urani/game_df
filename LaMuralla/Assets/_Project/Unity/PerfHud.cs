using System;
using System.Text;
using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Đo hiệu năng NGAY TRÊN MÁY và hiện lên màn hình.
    ///
    /// 🔴 VÌ SAO CẦN. Người chơi báo "giật nhẹ" ba lần; ba lần tôi đoán chỗ và sửa
    /// theo suy luận tĩnh. Đoán không sai — cả bảy chỗ đã sửa đều là defect thật —
    /// nhưng KHÔNG cái nào được chứng minh là nguyên nhân. Trên iPhone không mở
    /// Profiler dễ như trong Editor, nên câu hỏi "giật kiểu gì" không có số để trả lời.
    ///
    /// Lớp này biến "giật nhẹ" thành ba con số phân biệt được hai nguyên nhân khác hẳn nhau:
    ///
    ///   • **Khựng từng nhịp** → `worst` cao vọt so với `avg`, và `GC` nhảy số.
    ///     Nguyên nhân: thu gom rác, hoặc nạp tài nguyên đồng bộ giữa trận.
    ///   • **Chậm đều** → `avg` cao mà `worst` không vọt, `GC` đứng yên.
    ///     Nguyên nhân: quá tải vẽ (số object, số batch), không phải rác.
    ///
    /// ⚠️ CHÍNH NÓ KHÔNG ĐƯỢC CẤP PHÁT MỖI FRAME — nếu không thì nó đo chính rác của
    /// mình. Nên: đệm vòng dựng sẵn, `StringBuilder` dùng lại, và chỉ ghép chuỗi
    /// 4 lần mỗi giây thay vì mỗi frame.
    /// </summary>
    public sealed class PerfHud : MonoBehaviour
    {
        private const int Window = 120;          // ~2 giây ở 60fps
        private const float RefreshSec = 0.25f;

        private readonly float[] _ms = new float[Window];
        private int _n;
        private float _next;
        private int _gcAtStart;
        private long _memPrev;
        private float _memAt;
        private float _allocKbPerSec;

        private readonly StringBuilder _sb = new(256);
        private string _text = "";
        private GUIStyle? _style;

        /// <summary>Nguồn cung cấp số quân đang sống — để biết giật có đi cùng đông quân không.</summary>
        internal Func<int>? LiveEnemies;

        private void Start()
        {
            _gcAtStart = GC.CollectionCount(0);
            _memPrev = GC.GetTotalMemory(false);
            _memAt = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            _ms[_n % Window] = Time.unscaledDeltaTime * 1000f;
            _n++;

            float now = Time.realtimeSinceStartup;
            if (now - _memAt >= 1f)
            {
                long mem = GC.GetTotalMemory(false);
                // Chỉ đếm phần TĂNG. Lúc GC vừa chạy thì hiệu âm — bỏ qua, vì mục tiêu
                // là đo TỐC ĐỘ SINH RÁC, không phải mức chiếm dụng.
                if (mem > _memPrev) _allocKbPerSec = (mem - _memPrev) / 1024f / (now - _memAt);
                _memPrev = mem;
                _memAt = now;
            }
            if (now < _next) return;
            _next = now + RefreshSec;

            int count = Mathf.Min(_n, Window);
            float sum = 0f, worst = 0f;
            for (int i = 0; i < count; i++)
            {
                float v = _ms[i];
                sum += v;
                if (v > worst) worst = v;
            }
            float avg = count > 0 ? sum / count : 0f;

            _sb.Clear();
            _sb.Append("avg ").Append(avg.ToString("0.0")).Append("ms (")
               .Append((avg > 0 ? 1000f / avg : 0f).ToString("0")).Append(" fps)\n");
            _sb.Append("worst ").Append(worst.ToString("0.0")).Append("ms\n");
            _sb.Append("alloc ").Append(_allocKbPerSec.ToString("0")).Append(" KB/s\n");
            _sb.Append("GC ").Append(GC.CollectionCount(0) - _gcAtStart).Append('\n');
            // Hiện luôn trần khung hình: nếu `avg` bám sát 1000/target thì đang CHẠM
            // TRẦN, không phải quá tải — hai chuyện khác hẳn nhau.
            _sb.Append("target ").Append(Application.targetFrameRate).Append(" fps\n");
            if (LiveEnemies != null) _sb.Append("quân ").Append(LiveEnemies());
            _text = _sb.ToString();
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = (int)(26 * (Screen.width / 1080f)),
                    alignment = TextAnchor.UpperLeft,
                    normal = { textColor = new Color(1f, 1f, 0.35f) },
                };
            }

            float u = Screen.width / 1080f;
            var r = new Rect(16 * u, Screen.safeArea.y + 200 * u, 420 * u, 260 * u);
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(r.x + 10 * u, r.y + 6 * u, r.width - 20 * u, r.height), _text, _style);
        }
    }
}
