using System;
using UnityEngine;

namespace ServicesPackage
{
    public class AdsCallbacks
    {
        private AdsSystemContext ctx;
        private AdsSystemFlow flow;

        public AdsCallbacks(AdsSystemContext context, AdsSystemFlow flow)
        {
            ctx = context;
            this.flow = flow;
        }

        internal void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            ServicesLogger.Log("[AppLovinManager] Banner Ad Loaded Successfully!");
        }

        internal void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            ServicesLogger.LogWarning("[AppLovinManager] Banner Ad Load Failed!");
        }

        internal void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        internal void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            try
            {
                // PluginSDKManager.Instance.revenueManager.TrackAdRevenue(adInfo);
                //SignalBusService.Fire(new TrackRevenueSignal { value = adInfo });
                SignalBusService.Fire(
                    new TrackRevenueSignal { value = adInfo },
                    sticky: true
                );

                ServicesLogger.Log(
                    $"[REVENUE SOURCE AD CALLBACKS][REVENUELOG] Fired MAX revenue → " +
                    $"network={adInfo.NetworkName}, " +
                    $"unit={adInfo.AdUnitIdentifier}, " +
                    $"revenue={adInfo.Revenue}, " +
                    $"time={Time.realtimeSinceStartup:F2}"
                );
                ServicesLogger.Log($"[AdsManagerController] Banner ad revenue tracked: {adInfo.Revenue} USD");
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[AdsManagerController] Error in OnBannerAdRevenuePaidEvent: {ex.Message}\n{ex.StackTrace}");
            }
        }
        internal void OnInterstitialAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            try
            {
                // PluginSDKManager.Instance.revenueManager.TrackAdRevenue(adInfo);
                //SignalBusService.Fire(new TrackRevenueSignal { value = adInfo });
                SignalBusService.Fire(
                    new TrackRevenueSignal { value = adInfo },
                    sticky: true
                );

                ServicesLogger.Log(
                    $"[REVENUE SOURCE AD CALLBACKS][REVENUELOG] Fired MAX revenue → " +
                    $"network={adInfo.NetworkName}, " +
                    $"unit={adInfo.AdUnitIdentifier}, " +
                    $"revenue={adInfo.Revenue}, " +
                    $"time={Time.realtimeSinceStartup:F2}"
                );

                ServicesLogger.Log($"[AdsManagerController] Interstitial ad revenue tracked: {adInfo.Revenue} USD");
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[AdsManagerController] Error in OnInterstitialAdRevenuePaidEvent: {ex.Message}\n{ex.StackTrace}");
            }
        }

        internal void OnInterstitialAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        internal void OnInterstitialAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            AudioSessionFixer.FixAudio();
            ctx.lastInterstitialTime = Time.realtimeSinceStartup;
            ctx.hasShownInterstitialThisSession = true;
            MaxSdk.LoadInterstitial(ctx.interstitialAdUnitId);
        }

        internal void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            try
            {
                ctx.retryAttempt = 0;
                YoogoLabManager.NotifyRewardedReady();
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[AppLovinManager] Error in OnRewardedAdLoadedEvent: {ex.Message}\n{ex.StackTrace}");
            }
        }

        internal void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            try
            {
                ctx.retryAttempt++;
                double retryDelay = Math.Pow(2, Math.Min(6, ctx.retryAttempt));
                ServiceCoroutineRunner.RunAfter((float)retryDelay, () => flow.LoadRewardedAd());

            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[AppLovinManager] Error in OnRewardedAdLoadFailedEvent: {ex.Message}\n{ex.StackTrace}");
            }
        }

        internal void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        internal void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            flow.LoadRewardedAd();
        }

        internal void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        internal void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            flow.LoadRewardedAd();
            AudioSessionFixer.FixAudio();
        }

        internal void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            ServicesLogger.Log("[AppLovinManager] Rewarded user: " + reward.Amount + " " + reward.Label);
            YoogoLabManager.OnRewardedFinished(true);
        }

        internal void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            try
            {
                // PluginSDKManager.Instance.revenueManager.TrackAdRevenue(adInfo);
                //SignalBusService.Fire(new TrackRevenueSignal { value = adInfo });
                SignalBusService.Fire(
                    new TrackRevenueSignal { value = adInfo },
                    sticky: true
                );

                ServicesLogger.Log(
                    $"[REVENUE SOURCE AD CALLBACKS][REVENUELOG] Fired MAX revenue → " +
                    $"network={adInfo.NetworkName}, " +
                    $"unit={adInfo.AdUnitIdentifier}, " +
                    $"revenue={adInfo.Revenue}, " +
                    $"time={Time.realtimeSinceStartup:F2}"
                );

                ServicesLogger.Log($"[AdsManagerController] Rewarded ad revenue tracked: {adInfo.Revenue} USD");
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[AdsManagerController] Error in OnRewardedAdRevenuePaidEvent: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

}
