using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/AdsSystem")]
    public class AdsSystemFeatureMediator : FeatureMediatorCore
    {
        public override void Setup(FeatureRegistry registry)
        {
            var context = new AdsSystemContext();
            var flow = new AdsSystemFlow(context);

            registry.Register(context);
            registry.Register(flow);

            var config = registry.Resolve<ServicesConfigInitializer>().ConfigData;
            var rc = registry.Resolve<RemoteConfigContext>();

#if UNITY_ANDROID
            flow.SetAdIds(config.adBannerKeyAndroid, config.adInterstitialKeyAndroid, config.adRewardedKeyAndroid);
#else
            flow.SetAdIds(config.adBannerKeyIOS, config.adInterstitialKeyIOS, config.adRewardedKeyIOS);
#endif

            flow.Setup(
                registry.Resolve<ConsentContext>(),
                registry.Resolve<RemoteConfigContext>(),
                registry.Resolve<SessionManagerContext>()
            );

            flow.Initialize();
        }
    }
}