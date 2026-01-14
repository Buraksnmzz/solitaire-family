using System;
using System.Collections;
using UnityEngine;

namespace ServicesPackage
{
    public class AdsSystemFlow
    {
        private readonly AdsSystemContext ctx;
        private AdsCallbacks callbacks;
        private SessionManagerContext sessionManagerContext;
        private RemoteConfigContext remoteConfigContext;
        private ConsentContext consentContext;
        public AdsSystemFlow(AdsSystemContext context) => ctx = context;

        public void Setup(ConsentContext consent, RemoteConfigContext remoteConfig, SessionManagerContext sessionManager)
        {
            consentContext = consent;
            remoteConfigContext = remoteConfig;
            sessionManagerContext = sessionManager;
            ResetSession();
        }

        public bool IsRewardedAdReady()
        {
            return MaxSdk.IsRewardedAdReady(ctx.rewardedAdUnitId);
        }

        public void SetAdIds(string banner, string interstitial, string rewarded)
        {
            ctx.bannerAdUnitId = banner;
            ctx.interstitialAdUnitId = interstitial;
            ctx.rewardedAdUnitId = rewarded;

            ServicesLogger.Log($"[AdsManager] SetAdIds called ? Platform: {Application.platform}, Banner: {banner}, " +
                $"Interstitial: {interstitial}, Rewarded: {rewarded}");
        }

        public void ResetSession()
        {
            ctx.hasShownInterstitialThisSession = false;
            ctx.lastInterstitialTime = 0f;
        }

        public void Initialize()
        {
            if (ctx.isInitialized) return;
            callbacks = new AdsCallbacks(ctx, this);

            try
            {
                ctx.sdkInitializedTime = Time.realtimeSinceStartup;
                ServicesLogger.Log("[AppLovinManager] Starting initialization...");

                bool consent = consentContext.GetStoredConsent();

                MaxSdk.SetHasUserConsent(consent);
                MaxSdk.SetDoNotSell(!consent);

                MaxSdk.InitializeSdk();

                MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
                {
                    ctx.isInitialized = true;

                    MaxSdk.LoadInterstitial(ctx.interstitialAdUnitId);

                    //if (PluginSDKManager.Instance.isSandboxMode)
                    //{
                    //    MaxSdk.SetVerboseLogging(true);
                    //}
                    //MaxSdk.SetExtraParameter("disable_all_logs", PluginSDKManager.Instance.isSandboxMode ? "false" : "true");

                    MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += callbacks.OnInterstitialAdRevenuePaidEvent;
                    MaxSdkCallbacks.Interstitial.OnAdClickedEvent += callbacks.OnInterstitialAdClickedEvent;
                    MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += callbacks.OnInterstitialAdHiddenEvent;
                    MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += (adUnitId, adInfo) =>
                    {
                        float timeSinceSdkInit = Time.realtimeSinceStartup - ctx.sdkInitializedTime;
                        ServicesLogger.Log($"[AppLovinManager] Interstitial is ready! AdUnit: {adUnitId} " +
                            $"| Loaded after {timeSinceSdkInit:F2} seconds");
                    };

                    ServiceCoroutineRunner.StartSafe(InitializeBannerAds());
                    ServiceCoroutineRunner.StartSafe(InitializeRewardedAds());
                    ServiceCoroutineRunner.StartSafe(LoadAdsWithDelay());

                    ServicesLogger.Log($"[AppLovinManager] AppLovin SDK Initialized Successfully! STATUS {ctx.isInitialized}");
                };
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[AppLovinManager] Initialization failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public IEnumerator InitializeBannerAds()
        {
            ServicesLogger.Log("[AppLovinManager] Initializing Banner Ads...");

            if (string.IsNullOrEmpty(ctx.bannerAdUnitId))
            {
                ServicesLogger.LogWarning("[AppLovinManager] Banner Ad Unit ID is missing!");
                yield break;
            }

            CreateMaxBannerAd();
        }

        private IEnumerator LoadAdsWithDelay()
        {
            if (sessionManagerContext.IsFirstSession)
            {
                int delay = remoteConfigContext.AdDelayFirstSession;
                ServicesLogger.Log($"[AppLovinManager] Delaying ads by {delay} seconds for the first session.");
                yield return new WaitForSeconds(delay);
            }
            else
            {
                ServicesLogger.Log($"[AppLovinManager] Initialize and load ads at once");
            }

            MaxSdk.LoadInterstitial(ctx.interstitialAdUnitId);
            MaxSdk.LoadRewardedAd(ctx.rewardedAdUnitId);
        }

        private IEnumerator InitializeRewardedAds()
        {
            ServicesLogger.Log("[AppLovinManager] Initializing Rewarded Ads...");

            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += callbacks.OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += callbacks.OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += callbacks.OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += callbacks.OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += callbacks.OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += callbacks.OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += callbacks.OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += callbacks.OnRewardedAdReceivedRewardEvent;

            MaxSdk.LoadRewardedAd(ctx.rewardedAdUnitId);

            yield break;
        }

        private void CreateMaxBannerAd()
        {
            var isFirstSession = sessionManagerContext.IsFirstSession;

            if (isFirstSession && !remoteConfigContext.BannerFirstSession)
            {
                ServicesLogger.Log("[AppLovinManager] Skipping banner creation ? first session and not allowed.");
                return;
            }

            Debug.Log($"[ApplovinManager] Banner FirstSession: {remoteConfigContext.BannerFirstSession}," +
                $" IsFirstSession: {isFirstSession}");
            try
            {
                ServicesLogger.Log("[AppLovinManager] Initialize Banners");

                MaxSdk.SetBannerExtraParameter(ctx.bannerAdUnitId, "adaptive_banner", "true");

                MaxSdk.CreateBanner(ctx.bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
                MaxSdk.SetBannerBackgroundColor(ctx.bannerAdUnitId, Color.white);

                MaxSdkCallbacks.Banner.OnAdLoadedEvent += callbacks.OnBannerAdLoadedEvent;
                MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += callbacks.OnBannerAdLoadFailedEvent;
                MaxSdkCallbacks.Banner.OnAdClickedEvent += callbacks.OnBannerAdClickedEvent;
                MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += callbacks.OnBannerAdRevenuePaidEvent;

                ServicesLogger.Log("[AppLovinManager] MAX Banner Ad Created and Loading...");
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[AppLovinManager] Error creating MAX banner ad: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void ShowBanner()
        {
            ServicesLogger.Log("[AppLovinManager] Try Show banner");
            ServiceCoroutineRunner.StartSafe(WaitAndShowBanner());
        }

        private IEnumerator WaitAndShowBanner(float timeout = 5f)
        {
            float elapsed = 0f;

            while (!ctx.isInitialized && elapsed < timeout)
            {
                ServicesLogger.Log($"[AppLovinManager] Waiting for SDK init... {elapsed:F2}/{timeout}");
                yield return new WaitForSeconds(0.2f);
                elapsed += 0.2f;
            }

            if (!ctx.isInitialized)
            {
                ServicesLogger.LogWarning("[AppLovinManager] Failed to show banner: SDK not initialized after timeout.");
                yield break;
            }

            if (remoteConfigContext.ShowBanner &&
                (!sessionManagerContext.IsFirstSession || remoteConfigContext.BannerFirstSession))
            {
                ServicesLogger.Log("[AppLovinManager] Showing banner after init!");
                MaxSdk.ShowBanner(ctx.bannerAdUnitId);
            }
            else
            {
                ServicesLogger.Log("[AppLovinManager] Banner not allowed by RemoteConfig.");
            }
        }

        public void HideBanner()
        {
            if (ctx.isInitialized) MaxSdk.HideBanner(ctx.bannerAdUnitId);
        }

        public void ShowInterstitial()
        {
            if (!ctx.isInitialized || !remoteConfigContext.ShowInterstitial) return;

            bool isFirstSession = sessionManagerContext.IsFirstSession;

            if (isFirstSession && !remoteConfigContext.InterstitialFirstSession)
            {
                Debug.Log("[AppLovinManager] Interstitial ads are disabled for the first session.");
                return;
            }

            float timeSinceStartup = Time.realtimeSinceStartup;
            int startISDelay = remoteConfigContext.StartIS;
            int frequencyCap = remoteConfigContext.FrequencyCapIS;

            bool startDelayPassed = timeSinceStartup >= startISDelay;
            bool cooldownPassed = timeSinceStartup - ctx.lastInterstitialTime >= frequencyCap;

            if (isFirstSession && !startDelayPassed)
            {
                Debug.Log($"[AppLovinManager] StartIS not passed yet. Elapsed: {timeSinceStartup}s / Required: {startISDelay}s");
                return;
            }

            if (!ctx.hasShownInterstitialThisSession || cooldownPassed)
            {
                if (MaxSdk.IsInterstitialReady(ctx.interstitialAdUnitId))
                {
                    ServicesLogger.Log("[AppLovinManager] IS Ready SHOW");
                    MaxSdk.ShowInterstitial(ctx.interstitialAdUnitId);
                }
                else
                {
                    ServicesLogger.Log("[AppLovinManager] IS not ready");

                }

                ctx.hasShownInterstitialThisSession = true;
                ctx.lastInterstitialTime = timeSinceStartup;
            }
            else
            {
                float timeRemaining = frequencyCap - (timeSinceStartup - ctx.lastInterstitialTime);
                Debug.Log($"[AppLovinManager] Cooldown active. Wait {timeRemaining:F1}s more.");
            }

            ServicesLogger.Log($"[AppLovinManager] IS Check => FirstSession: {isFirstSession}, " +
                $"StartDelayPassed: {startDelayPassed}, CooldownPassed: {cooldownPassed}," +
                $" ShownThisSession: {ctx.hasShownInterstitialThisSession}");
        }

        public void LoadRewardedAd() => MaxSdk.LoadRewardedAd(ctx.rewardedAdUnitId);

        public void ShowRewardedAd()
        {
            if (ctx.isInitialized && MaxSdk.IsRewardedAdReady(ctx.rewardedAdUnitId))
            {
                MaxSdk.ShowRewardedAd(ctx.rewardedAdUnitId);
            }
            else
            {
                LoadRewardedAd();
            }
        }
    }
}