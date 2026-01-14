using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/AdjustCore")]
    public class AdjustCoreFeatureMediator : FeatureMediatorCore
    {
        public override void Setup(FeatureRegistry registry)
        {
            var context = new AdjustCoreContext();
            var flow = new AdjustCoreFlow(context);

            registry.Register(context);
            registry.Register(flow);

            var config = registry.Resolve<ServicesConfigInitializer>().ConfigData;
            var rc = registry.Resolve<RemoteConfigContext>();

#if UNITY_ANDROID
            flow.SetAppToken(config.adjustAppTokenAndroid);
#else
flow.SetAppToken(config.adjustAppTokenIOS);
#endif
            flow.SetRemoteConfig(rc);

            //SignalBusService.Subscribe<ResolveConsentSignal>(sig =>
            //{
            //    flow.Initialize(sig.value);
            //    flow.SetUserConsent(sig.value);
            //});

            //SignalBusService.Subscribe<TrackLevelEndSignal>(sig =>
            //{
            //    flow.TrackLevelEvent(sig.level_);
            //});

            //SignalBusService.Subscribe<TrackRevenueSignal>(sig =>
            //{
            //    flow.TrackAdRevenue(sig.value);
            //});
            SignalBusService.Subscribe<ResolveConsentSignal>(sig =>
            {
                flow.Initialize(sig.value);
                flow.SetUserConsent(sig.value);
            }, replaySticky: true);

            SignalBusService.Subscribe<TrackLevelEndSignal>(sig =>
            {
                flow.TrackLevelEvent(sig.level_);
            });

            SignalBusService.Subscribe<TrackRevenueSignal>(sig =>
            {
                flow.TrackAdRevenue(sig.value);
            }, replaySticky: true);
        }
    }
}
