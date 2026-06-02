using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/FirebaseCrashlytics")]
    public class FirebaseCrashlyticsFeature : FeatureMediatorCore
    {
        public override void Setup(FeatureRegistry registry)
        {
            var context = new FirebaseCrashlyticsContext();
            var flow = new FirebaseCrashlyticsFlow(context);

            registry.Register(context);
            registry.Register(flow);
            flow.Setup();
            flow.Initialize(registry.Resolve<FirebaseCoreContext>());
        }
    }
}
