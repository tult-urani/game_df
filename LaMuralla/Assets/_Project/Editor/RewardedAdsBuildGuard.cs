using LaMuralla.Unity;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LaMuralla.EditorTools
{
    /// <summary>Không cho bản release lọt test ID hoặc thiếu production ID.</summary>
    public sealed class RewardedAdsBuildGuard : IPreprocessBuildWithReport
    {
        private const string MobileAdsSettingsPath =
            "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
        private const string TestAppIdPrefix = "ca-app-pub-3940256099942544~";

        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform is not (BuildTarget.iOS or BuildTarget.Android)) return;
            if ((report.summary.options & BuildOptions.Development) != 0) return;
            ValidateRelease(report.summary.platform);
        }

        internal static void ValidateRelease(BuildTarget target)
        {
            RewardedAdsSettings settings =
                Resources.Load<RewardedAdsSettings>(RewardedAdsSettings.ResourcePath);
            if (settings == null)
                throw new BuildFailedException("Thiếu RewardedAdsSettings.asset.");
            if (settings.UseTestAds)
                throw new BuildFailedException(
                    "Release bị chặn: RewardedAdsSettings.useTestAds vẫn bật. " +
                    "Nhập production App ID + rewarded ad unit ID rồi tắt test mode.");
            if (!settings.AudienceConfigured)
                throw new BuildFailedException(
                    "Release bị chặn: chưa chọn audience General/UnderAgeOfConsent cho UMP.");

            string adUnitId = target == BuildTarget.Android
                ? settings.AndroidRewardedAdUnitId
                : settings.IosRewardedAdUnitId;
            if (!LooksLikeAdUnitId(adUnitId) || IsGoogleTestAdUnit(adUnitId))
                throw new BuildFailedException(
                    $"Release {target} bị chặn: rewarded ad unit ID là test ID hoặc không hợp lệ.");

            Object appSettings = AssetDatabase.LoadMainAssetAtPath(MobileAdsSettingsPath);
            if (appSettings == null)
                throw new BuildFailedException("Thiếu GoogleMobileAdsSettings.asset.");

            var serialized = new SerializedObject(appSettings);
            string property = target == BuildTarget.Android ? "adMobAndroidAppId" : "adMobIOSAppId";
            string appId = serialized.FindProperty(property)?.stringValue ?? "";
            if (!LooksLikeAppId(appId) || appId.StartsWith(TestAppIdPrefix))
                throw new BuildFailedException(
                    $"Release {target} bị chặn: AdMob App ID còn là test ID hoặc không hợp lệ.");
        }

        private static bool LooksLikeAdUnitId(string id) =>
            !string.IsNullOrWhiteSpace(id) && id.StartsWith("ca-app-pub-") && id.Contains("/");

        private static bool IsGoogleTestAdUnit(string id) =>
            id.StartsWith("ca-app-pub-3940256099942544/");

        private static bool LooksLikeAppId(string id) =>
            !string.IsNullOrWhiteSpace(id) && id.StartsWith("ca-app-pub-") && id.Contains("~");
    }
}
