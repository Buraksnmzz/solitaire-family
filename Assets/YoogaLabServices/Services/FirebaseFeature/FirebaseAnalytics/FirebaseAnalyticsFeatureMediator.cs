using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/FirebaseAnalytics")]
    public class FirebaseAnalyticsFeatureMediator : FeatureMediatorCore
    {
        public override void Setup(FeatureRegistry registry)
        {
            var context = new FirebaseAnalyticsContext();
            var flow = new FirebaseAnalyticsFlow(context);

            registry.Register(context);
            registry.Register(flow);

            flow.Setup();
            flow.Initialize(registry.Resolve<FirebaseCoreContext>(),registry);

            SignalBusService.Subscribe<ResolveConsentSignal>(sig =>
            {
                flow.LogConsentEvent(sig.value);
                flow.EnableAnalyticsAndCrashlytics(sig.value);
            });

            SignalBusService.Subscribe<SetFirebaseUserPropertiesSignal>(sig =>
            {
                flow.SetUserProperty(sig.key, sig.value);
            });

            SignalBusService.Subscribe<LogFirebaseEventSignal>(sig =>
            {
                flow.LogEvent(sig.eventName, sig.parameters);
            });

            SignalBusService.Subscribe<LogIAPEventSignal>(sig =>
            {
                flow.LogIAPEvent(sig.transactionId, sig.price, sig.currency, sig.productId);
            });
        }
    }
}