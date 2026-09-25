using UnityEngine;

namespace LaMuralla.Unity
{
    public enum RewardedAdsAudience
    {
        Unset = 0,
        General = 1,
        UnderAgeOfConsent = 2,
    }

    /// <summary>Cấu hình ad unit. App ID của SDK nằm trong GoogleMobileAdsSettings.</summary>
    public sealed class RewardedAdsSettings : ScriptableObject
    {
        public const string ResourcePath = "RewardedAdsSettings";
        public const string AndroidTestAdUnitId = "ca-app-pub-3940256099942544/5224354917";
        public const string IosTestAdUnitId = "ca-app-pub-3940256099942544/1712485313";

        [SerializeField] private bool useTestAds = true;
        [SerializeField] private RewardedAdsAudience audience = RewardedAdsAudience.Unset;
        [SerializeField] private string androidRewardedAdUnitId = "";
        [SerializeField] private string iosRewardedAdUnitId = "";

        public bool UseTestAds => useTestAds;
        public RewardedAdsAudience Audience => audience;
        public bool AudienceConfigured => audience != RewardedAdsAudience.Unset;
        // Unset dùng mặc định hạn chế nhất trong Development; release guard bắt
        // buộc chủ sản phẩm chốt audience trước khi xuất bản.
        public bool TagForUnderAgeOfConsent => audience != RewardedAdsAudience.General;
        public string AndroidRewardedAdUnitId => androidRewardedAdUnitId;
        public string IosRewardedAdUnitId => iosRewardedAdUnitId;

        public string AdUnitId
        {
            get
            {
#if UNITY_ANDROID
                return useTestAds ? AndroidTestAdUnitId : androidRewardedAdUnitId;
#else
                return useTestAds ? IosTestAdUnitId : iosRewardedAdUnitId;
#endif
            }
        }
    }
}
