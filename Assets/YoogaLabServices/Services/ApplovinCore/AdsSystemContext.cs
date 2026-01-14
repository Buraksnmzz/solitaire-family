using UnityEngine;

namespace ServicesPackage
{
    public class AdsSystemContext
    {
        public bool isInitialized = false;
        public bool IsInitialized => isInitialized;

        internal string bannerAdUnitId;
        internal string interstitialAdUnitId;
        internal string rewardedAdUnitId;

        internal int retryAttempt;
        internal float lastInterstitialTime;

        internal const string BannerSpacerName = "YoogoLab_BannerSpacer";

        public bool hasShownInterstitialThisSession { get; set; } = false;
        internal float sdkInitializedTime = 0f;
    }
}