using System;
using System.Collections;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using GoogleMobileAds.Ump.Api;
using LaMuralla.Core.Match;
using UnityEngine;

namespace LaMuralla.Unity
{
    /// <summary>
    /// Một rewarded ad preload sẵn cho toàn app. UMP chạy trước MobileAds; callback
    /// SDK được đưa về main thread trước khi chạm Unity hoặc state trận.
    /// </summary>
    public sealed class AdMobRewardedAds : MonoBehaviour, IRewardedAdGateway
    {
        private const float MaxRetryDelaySec = 60f;

        public static AdMobRewardedAds? Instance { get; private set; }

        private RewardedAdsSettings _settings = null!;
        private RewardedAd? _ad;
        private Coroutine? _reloadRoutine;
        private Action? _rewarded;
        private Action? _closed;
        private bool _sdkStarted;
        private bool _loading;
        private bool _showing;
        private bool _rewardDelivered;
        private bool _consentUpdated;
        private float _retryDelaySec = 2f;

        public bool IsReady => _ad != null && _ad.CanShowAd() && !_showing;
        public bool PrivacyOptionsRequired =>
            _consentUpdated &&
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        public event Action? PresentationClosed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("AdMobRewardedAds");
            DontDestroyOnLoad(go);
            go.AddComponent<AdMobRewardedAds>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _settings = Resources.Load<RewardedAdsSettings>(RewardedAdsSettings.ResourcePath);
            if (_settings == null)
            {
                Debug.LogError("[Ads] thiếu Resources/RewardedAdsSettings.asset; rewarded ads đã tắt.");
                enabled = false;
            }
        }

        private void Start()
        {
            if (!enabled) return;
            if (!_settings.AudienceConfigured)
                Debug.LogWarning("[Ads] audience chưa được chốt; Development dùng " +
                                 "TagForUnderAgeOfConsent=true. Release sẽ bị chặn.");
            var request = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = _settings.TagForUnderAgeOfConsent,
            };
            ConsentInformation.Update(request, updateError => RunOnMain(() =>
            {
                _consentUpdated = true;
                if (updateError != null)
                    Debug.LogWarning("[Ads] không cập nhật được consent: " + updateError.Message);

                ConsentForm.LoadAndShowConsentFormIfRequired(formError => RunOnMain(() =>
                {
                    if (formError != null)
                        Debug.LogWarning("[Ads] không hiển thị được consent form: " + formError.Message);
                    StartSdkWhenAllowed();
                }));
            }));
        }

        private void StartSdkWhenAllowed()
        {
            if (_sdkStarted || !ConsentInformation.CanRequestAds()) return;
            _sdkStarted = true;
            MobileAds.SetiOSAppPauseOnBackground(true);
            MobileAds.Initialize(status => RunOnMain(() =>
            {
                if (status == null)
                {
                    Debug.LogError("[Ads] MobileAds.Initialize trả về null; không tải quảng cáo.");
                    return;
                }
                LoadRewarded();
            }));
        }

        private void LoadRewarded()
        {
            if (!_sdkStarted || _loading || _showing || _ad != null) return;
            string adUnitId = _settings.AdUnitId;
            if (string.IsNullOrWhiteSpace(adUnitId))
            {
                Debug.LogError("[Ads] chưa cấu hình rewarded ad unit ID cho platform hiện tại.");
                return;
            }

            _loading = true;
            RewardedAd.Load(adUnitId, new AdRequest(), (ad, error) => RunOnMain(() =>
            {
                _loading = false;
                if (error != null || ad == null)
                {
                    Debug.LogWarning("[Ads] tải rewarded ad thất bại: " +
                                     (error?.ToString() ?? "ad=null"));
                    ScheduleReload();
                    return;
                }

                _retryDelaySec = 2f;
                _ad = ad;
                ad.OnAdFullScreenContentClosed += () => RunOnMain(FinishPresentation);
                ad.OnAdFullScreenContentFailed += errorInfo => RunOnMain(() =>
                {
                    Debug.LogWarning("[Ads] không mở được rewarded ad: " + errorInfo);
                    FinishPresentation();
                });
            }));
        }

        public void Show(Action rewarded, Action closedWithoutReward)
        {
            if (rewarded == null) throw new ArgumentNullException(nameof(rewarded));
            if (closedWithoutReward == null) throw new ArgumentNullException(nameof(closedWithoutReward));
            if (!IsReady) throw new InvalidOperationException("rewarded ad chưa sẵn sàng");

            _rewarded = rewarded;
            _closed = closedWithoutReward;
            _rewardDelivered = false;
            _showing = true;

            try
            {
                _ad!.Show(_ => RunOnMain(() =>
                {
                    if (!_showing || _rewardDelivered) return;
                    // Chỉ ghi nhận ở đây; áp thưởng khi full-screen đã đóng. Nếu
                    // continue đổi Lost→Fighting ngay trong callback này, trận sẽ
                    // chạy phía sau video trước sự kiện OnAdFullScreenContentClosed.
                    _rewardDelivered = true;
                }));
            }
            catch (Exception ex)
            {
                Debug.LogError("[Ads] RewardedAd.Show ném lỗi: " + ex.Message);
                FinishPresentation();
            }
        }

        public bool ShowPrivacyOptions()
        {
            if (!PrivacyOptionsRequired) return false;
            ConsentForm.ShowPrivacyOptionsForm(error => RunOnMain(() =>
            {
                if (error != null)
                    Debug.LogWarning("[Ads] không mở được privacy options: " + error.Message);
                StartSdkWhenAllowed();
            }));
            return true;
        }

        private void FinishPresentation()
        {
            if (!_showing) return;
            _showing = false;

            Action? rewarded = _rewarded;
            Action? closed = _closed;
            bool rewardDelivered = _rewardDelivered;
            _rewarded = null;
            _closed = null;
            _rewardDelivered = false;

            if (rewardDelivered) rewarded?.Invoke();
            else closed?.Invoke();
            PresentationClosed?.Invoke();

            DestroyCurrentAd();
            LoadRewarded();
        }

        private void ScheduleReload()
        {
            if (_reloadRoutine != null) return;
            _reloadRoutine = StartCoroutine(ReloadAfterDelay(_retryDelaySec));
            _retryDelaySec = Mathf.Min(_retryDelaySec * 2f, MaxRetryDelaySec);
        }

        private IEnumerator ReloadAfterDelay(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            _reloadRoutine = null;
            LoadRewarded();
        }

        private void DestroyCurrentAd()
        {
            if (_ad == null) return;
            _ad.Destroy();
            _ad = null;
        }

        private static void RunOnMain(Action action) =>
            MobileAdsEventExecutor.ExecuteInUpdate(action);

        private void OnDestroy()
        {
            DestroyCurrentAd();
            if (Instance == this) Instance = null;
        }
    }
}
