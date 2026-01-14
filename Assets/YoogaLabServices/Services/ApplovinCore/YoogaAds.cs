using System;
using static YoogoLabManager;

namespace ServicesPackage
{
    public static class YoogaAds
    {
        private static AdsSystemFlow Flow => AppRegistry.Registry.Resolve<AdsSystemFlow>();

        public static void ShowInterstitial()
        {
            Flow.ShowInterstitial();
        }

        public static void ShowRewarded(Action<bool> callback)
        {
            Flow.ShowRewardedAd();
        }

        public static bool IsRewardedAvailable()
        {
            return Flow.IsRewardedAdReady();
        }

        public static void ShowBanner()
        {
            Flow.ShowBanner();
        }

        public static void HideBanner()
        {
            Flow.HideBanner();
        }
    }

}
