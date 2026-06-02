using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/RevenueManager")]
    public class RevenueManagerFeatureMediator : FeatureMediatorCore
    {
        public override void Setup(FeatureRegistry registry)
        {
            var context = new RevenueManagerContext();
            var flow = new RevenueManagerFlow(context);

            var remoteConfig = registry.Resolve<RemoteConfigContext>();
            var session = registry.Resolve<SessionManagerContext>();

            registry.Register(context);
            registry.Register(flow);

            flow.Setup(remoteConfig, session);

            var config = registry.Resolve<ServicesConfigInitializer>().ConfigData;

#if UNITY_ANDROID
            flow.SetRevenueAPIKey(config.revenueAPIKeyAndroid);
#else
            flow.SetRevenueAPIKey(config.revenueAPIKeyIOS);
#endif

            //SignalBusService.Subscribe<TrackRevenueSignal>(sig =>
            //{
            //    flow.TrackAdRevenue(sig.value);
            //});

            //SignalBusService.Subscribe<LogIAPEventSignal>(sig =>
            //{
            //    flow.TrackInAppPurchase(sig.transactionId, sig.price, sig.currency, sig.productId);
            //});
            SignalBusService.Subscribe<TrackRevenueSignal>(sig =>
            {
                flow.TrackAdRevenue(sig.value);
            }, replaySticky: true);

            SignalBusService.Subscribe<LogIAPEventSignal>(sig =>
            {
                flow.TrackInAppPurchase(sig.transactionId, sig.price, sig.currency, sig.productId);
            }, replaySticky: true);

        }
    }
}
