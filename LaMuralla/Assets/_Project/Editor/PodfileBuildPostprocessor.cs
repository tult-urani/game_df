using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace LaMuralla.EditorTools
{
    /// <summary>
    /// EDM4U sinh Podfile ở order 40 và chạy pod install ở order 50. Chèn giữa hai
    /// bước để dùng CDN thay repo Specs Git nặng hàng GB, đồng thời ép UTF-8 cho
    /// CocoaPods 1.17 (nó lỗi UnicodeNormalize khi shell kế thừa LC_ALL=C).
    /// </summary>
    public static class PodfileBuildPostprocessor
    {
        private const string GitSpecs = "source 'https://github.com/CocoaPods/Specs'";
        private const string CdnSpecs = "source 'https://cdn.cocoapods.org/'";
        private static string _previousLang = "";
        private static string _previousLcAll = "";
        private static bool _hadLang;
        private static bool _hadLcAll;

        [PostProcessBuild(45)]
        public static void PreparePodfile(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;

            string podfile = Path.Combine(buildPath, "Podfile");
            if (!File.Exists(podfile))
                throw new FileNotFoundException("EDM4U không sinh Podfile cho iOS build.", podfile);

            string text = File.ReadAllText(podfile);
            text = text.Replace(GitSpecs + "\r\n", "", StringComparison.Ordinal)
                       .Replace(GitSpecs + "\n", "", StringComparison.Ordinal);
            if (!text.Contains(CdnSpecs, StringComparison.Ordinal))
                text = CdnSpecs + Environment.NewLine + text;
            File.WriteAllText(podfile, text);

            string previousLang = Environment.GetEnvironmentVariable("LANG");
            string previousLcAll = Environment.GetEnvironmentVariable("LC_ALL");
            _hadLang = previousLang != null;
            _hadLcAll = previousLcAll != null;
            _previousLang = previousLang ?? "";
            _previousLcAll = previousLcAll ?? "";
            Environment.SetEnvironmentVariable("LANG", "en_US.UTF-8");
            Environment.SetEnvironmentVariable("LC_ALL", "en_US.UTF-8");
        }

        /// <summary>Không để locale phục vụ CocoaPods rò sang phần còn lại của Editor.</summary>
        [PostProcessBuild(55)]
        public static void RestoreLocale(BuildTarget target, string buildPath)
        {
            if (target != BuildTarget.iOS) return;
            Environment.SetEnvironmentVariable("LANG", _hadLang ? _previousLang : null);
            Environment.SetEnvironmentVariable("LC_ALL", _hadLcAll ? _previousLcAll : null);
        }
    }
}
