using System.Linq;
using LaMuralla.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaMuralla.EditorTools
{
    /// <summary>
    /// Chặn cái bẫy "bấm Play ra màn hình trống".
    ///
    /// 🔴 VÌ SAO CẦN. Editor luôn chạy scene ĐANG MỞ, không chạy scene đầu tiên trong
    /// Build Settings — Build Settings chỉ áp cho bản build thật. Mà scene đang mở lại
    /// hay bị reset về "Untitled" trống: mỗi lần chạy Unity ở `-batchmode` (bake config,
    /// build iOS) thì lúc thoát nó ghi `Library/LastSceneManagerSetup.txt` thành RỖNG,
    /// nên lần mở Editor kế tiếp là scene trống.
    ///
    /// Hậu quả cũ: Play ra một khung xanh đứng im, không log, không lỗi — trông y hệt
    /// code hỏng. Đã mất thời gian truy hai lần vì đúng chuyện này.
    ///
    /// Nay: nếu scene không có `MatchView` thì báo rõ và MỞ SẴN scene đúng cho người
    /// dùng chỉ việc bấm Play lại.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeGuard
    {
        static PlayModeGuard() => EditorApplication.playModeStateChanged += OnChange;

        private static void OnChange(PlayModeStateChange s)
        {
            if (s != PlayModeStateChange.ExitingEditMode) return;
            if (Object.FindFirstObjectByType<MatchView>() != null) return;   // scene đúng rồi

            EditorApplication.isPlaying = false;

            Scene open = SceneManager.GetActiveScene();
            string ten = string.IsNullOrEmpty(open.path) ? "(scene chưa lưu)" : open.path;
            Debug.LogWarning(
                $"[PlayModeGuard] Scene đang mở `{ten}` KHÔNG có MatchView → Play sẽ ra màn hình trống.\n" +
                $"Editor chạy scene ĐANG MỞ, không chạy scene đầu Build Settings.\n" +
                $"Đang mở giúp `{SceneBuilder.MatchScenePath}` — bấm Play lại.");

            if (!System.IO.File.Exists(SceneBuilder.MatchScenePath))
            {
                Debug.LogError($"[PlayModeGuard] Không có `{SceneBuilder.MatchScenePath}`. " +
                               "Chạy `La Muralla → Build Match Scene` để dựng lại.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(SceneBuilder.MatchScenePath, OpenSceneMode.Single);
        }

        /// <summary>Mở thẳng scene trận — khỏi phải lục Project panel.</summary>
        [MenuItem("La Muralla/Open Match Scene %#m")]
        public static void OpenMatchScene()
        {
            if (!System.IO.File.Exists(SceneBuilder.MatchScenePath))
            {
                Debug.LogError($"[La Muralla] chưa có `{SceneBuilder.MatchScenePath}` — " +
                               "chạy `La Muralla → Build Match Scene`.");
                return;
            }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(SceneBuilder.MatchScenePath, OpenSceneMode.Single);
        }
    }
}
