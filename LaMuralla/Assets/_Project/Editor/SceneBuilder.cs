using System.IO;
using System.Linq;
using LaMuralla.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LaMuralla.EditorTools
{
    /// <summary>
    /// Dựng scene Match từ code rồi ghi ra `.unity`.
    ///
    /// Vì sao không tạo scene bằng tay trong Editor: file .unity là YAML với GUID
    /// và fileID — người không đọc được, diff không xem được, và không ai kiểm được
    /// nó còn khớp code hay không. Dựng bằng script thì scene luôn dựng lại được,
    /// và cái dựng nó là thứ đọc được.
    /// </summary>
    public static class SceneBuilder
    {
        public const string MatchScenePath = "Assets/_Project/Scenes/Match.unity";

        [MenuItem("La Muralla/Build Match Scene")]
        public static void BuildMatchScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,
                                                      NewSceneMode.Single);

            var go = new GameObject("MatchView");
            go.AddComponent<MatchView>();

            Directory.CreateDirectory(Path.GetDirectoryName(MatchScenePath)!);
            if (!EditorSceneManager.SaveScene(scene, MatchScenePath))
            {
                Debug.LogError("[SceneBuilder] Không lưu được scene.");
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }

            // Match phải là scene ĐẦU TIÊN, nếu không build ra vẫn là SampleScene
            // trống và ta lại tưởng code hỏng.
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MatchScenePath, true),
            };

            Debug.Log($"[SceneBuilder] OK — {MatchScenePath}, và đã đặt làm scene đầu tiên " +
                      $"({EditorBuildSettings.scenes.Count(s => s.enabled)} scene bật).");
        }
    }
}
