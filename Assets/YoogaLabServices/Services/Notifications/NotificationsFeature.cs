using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/Notifications")]
    public class NotificationsFeature : FeatureMediatorCore
    {
        public override void Setup(FeatureRegistry registry)
        {
            var ctx = new NotificationsContext();
            var flow = new NotificationsFlow(ctx);

            registry.Register(ctx);
            registry.Register(flow);

            var go = new GameObject("NotificationsRuntime");
            Object.DontDestroyOnLoad(go);

            var runtime = go.AddComponent<NotificationsRuntime>();
            runtime.Initialize(flow,registry);
            flow.Initialize(registry.Resolve<SessionManagerContext>());

        }
    }
}