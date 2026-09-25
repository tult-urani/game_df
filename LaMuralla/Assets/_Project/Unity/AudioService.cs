using System.Collections.Generic;
using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Lớp phát âm thanh của trận. Giống `Draw`/`MatchView`: KHÔNG chứa luật chơi —
    /// nó chỉ nghe event từ `MatchController` rồi phát tiếng.
    ///
    /// Ba thứ lớp này tồn tại để giải quyết, vì `AudioSource.PlayOneShot` trần không
    /// làm được:
    ///   1. **Nhiều tiếng cùng lúc.** Một `AudioSource` bị cắt tiếng cũ khi có tiếng
    ///      mới. 5 tướng bắn cùng nhịp = nghe thấy 1. Nên có pool `VoiceCount` nguồn.
    ///   2. **Chống xếp chồng.** El Cinco nhịp 0.6 mà ở ×2 là 0.3s; cộng 5 ô nữa thì
    ///      cùng một file nổ chồng lên nhau thành tiếng rè. `MinGapSec` chặn việc
    ///      phát lại CÙNG một key quá dày.
    ///   3. **Đỡ nhàm.** Cùng một file lặp y hệt nghe rất máy móc → lệch cao độ
    ///      ngẫu nhiên ±`PitchJitter`.
    ///
    /// Clip nằm ở `Resources/Audio/<key>.ogg` (Kenney, CC0 — xem CREDITS.txt cạnh đó).
    /// Thiếu file thì cảnh báo ĐÚNG MỘT LẦN rồi câm luôn key đó, không spam log.
    /// </summary>
    internal sealed class AudioService : MonoBehaviour
    {
        internal static AudioService? Instance { get; private set; }

        private const int VoiceCount = 8;
        private const float MinGapSec = 0.05f;
        private const float PitchJitter = 0.06f;
        private const string MuteKey = "lamuralla.audio.muted";

        private readonly List<AudioSource> _voices = new();
        private readonly Dictionary<string, AudioClip?> _clips = new();
        private readonly Dictionary<string, float> _lastAt = new();
        private int _next;

        /// <summary>Tắt tiếng. Nhớ qua các phiên chơi bằng PlayerPrefs.</summary>
        internal static bool Muted { get; private set; }

        private void Awake()
        {
            Instance = this;
            Muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            for (int i = 0; i < VoiceCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                // 2D thuần: game nhìn từ trên xuống, nguồn tiếng cách camera vài unit
                // mà bật 3D thì tiếng ô rìa bị nhỏ đi vô cớ.
                src.spatialBlend = 0f;
                _voices.Add(src);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        internal static void ToggleMute()
        {
            Muted = !Muted;
            PlayerPrefs.SetInt(MuteKey, Muted ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>Phát `Resources/Audio/{key}.ogg`. An toàn khi gọi trước Awake
        /// (chưa có Instance) hoặc khi thiếu file — im lặng, không ném.</summary>
        internal static void Play(string key, float volume = 1f)
        {
            if (Muted || Instance == null) return;
            Instance.PlayInternal(key, volume);
        }

        private void PlayInternal(string key, float volume)
        {
            // Dùng unscaledTime: `Time.timeScale` không bị đụng tới ở đây, nhưng tốc
            // độ trận ×2 nhân vào deltaTime chứ không vào timeScale — nhịp chặn phải
            // đo bằng thời gian THẬT thì mới đúng ý "đừng phát lại quá dày".
            float now = Time.unscaledTime;
            if (_lastAt.TryGetValue(key, out float last) && now - last < MinGapSec) return;

            AudioClip? clip = Resolve(key);
            if (clip == null) return;

            _lastAt[key] = now;
            AudioSource src = _voices[_next];
            _next = (_next + 1) % _voices.Count;
            src.pitch = 1f + Random.Range(-PitchJitter, PitchJitter);
            src.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        private AudioClip? Resolve(string key)
        {
            if (_clips.TryGetValue(key, out AudioClip? cached)) return cached;

            var clip = Resources.Load<AudioClip>($"Audio/{key}");
            if (clip == null)
                Debug.LogWarning($"[AudioService] thiếu Resources/Audio/{key}.ogg — câm key này.");
            // Nhớ cả giá trị null: đó chính là cách "cảnh báo một lần rồi thôi".
            _clips[key] = clip;
            return clip;
        }
    }
}
