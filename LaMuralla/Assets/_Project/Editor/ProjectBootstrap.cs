using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LaMuralla.EditorTools
{
    /// <summary>
    /// Cấu hình project cho mobile bằng API chính thức của Unity, KHÔNG sửa tay
    /// YAML trong ProjectSettings.asset.
    ///
    /// Lý do: file .asset là YAML đã serialize, tên trường trong đó là tên nội bộ
    /// và Unity được phép đổi giữa các bản. Sửa tay thì hỏng ngầm — project vẫn
    /// mở được, chỉ là setting không có tác dụng, và không ai biết cho tới lúc
    /// build ra máy thật.
    ///
    /// Chạy lại được nhiều lần (idempotent). Gọi từ CLI:
    ///   Unity -projectPath . -batchmode -quit \
    ///         -executeMethod LaMuralla.EditorTools.ProjectBootstrap.Apply
    /// </summary>
    public static class ProjectBootstrap
    {
        public static void Apply()
        {
            PlayerSettings.companyName = "TuanTu";
            PlayerSettings.productName = "Football Tower Defense";
            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.iOS, "com.tuantu.lamuralla");

            // Dọc, khoá cứng. docs/03: bố cục 1080×1920, ô đặt tướng và menu
            // radial đều tính theo chiều dọc. Cho xoay ngang là vỡ toàn bộ toạ độ.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // IL2CPP + ARM64: bắt buộc với iOS. Apple cấm JIT, nên Mono không
            // chạy được trên máy thật. Đây là thứ trả lời câu "C# có chạy trên
            // iOS không" — có, vì IL2CPP dịch IL sang C++ rồi biên dịch native.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1); // 1 = ARM64
            PlayerSettings.iOS.targetOSVersionString = "13.0";
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            // Lấy từ keychain: OU của cert Apple Development = Team ID.
            // Personal Team (miễn phí) — đủ để chạy trên máy của chính mình.
            PlayerSettings.iOS.appleDeveloperTeamID = "PJS4V57G3Q";

            // Linear: URP trông đúng màu ở Linear. iOS chạy Metal nên không có
            // vấn đề tương thích như GLES2 đời cũ.
            PlayerSettings.colorSpace = ColorSpace.Linear;

            AssetDatabase.SaveAssets();
            Debug.Log("[ProjectBootstrap] Đã áp cấu hình mobile.");
        }
    }
}
