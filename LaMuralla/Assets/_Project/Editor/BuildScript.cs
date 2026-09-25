using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LaMuralla.EditorTools
{
    /// <summary>
    /// Sinh Xcode project từ Unity. Đây là mắt xích Unity → Xcode trong chuỗi:
    ///
    ///     Unity  →  Xcode project  →  xcodebuild  →  iPhone
    ///
    /// Gọi từ CLI:
    ///   Unity -projectPath . -batchmode -quit \
    ///         -executeMethod LaMuralla.EditorTools.BuildScript.BuildIOS
    ///
    /// Thoát với mã khác 0 khi build hỏng — nếu không thì CLI báo thành công
    /// trong khi chẳng có gì sinh ra, và ta tin vào một lời nói dối.
    /// </summary>
    public static class BuildScript
    {
        private const string OutputDir = "../ios-build";

        public static void BuildIOS()
        {
            string[] scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogError("[BuildScript] Không có scene nào trong Build Settings — " +
                               "build ra sẽ là màn hình trống. Dừng.");
                EditorApplication.Exit(2);
                return;
            }

            Debug.Log($"[BuildScript] Build {scenes.Length} scene: {string.Join(", ", scenes)}");

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputDir,
                target = BuildTarget.iOS,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary s = report.summary;

            if (s.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildScript] OK — {s.totalSize / 1048576} MB, {s.totalTime.TotalSeconds:0}s → {OutputDir}");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"[BuildScript] HỎNG — {s.result}, {s.totalErrors} lỗi");
                EditorApplication.Exit(1);
            }
        }
    }
}
