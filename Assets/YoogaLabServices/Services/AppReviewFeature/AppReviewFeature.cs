using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/AppReview")]
    public class AppReviewFeature : FeatureMediatorCore
    {
        public override void Setup(FeatureRegistry registry)
        {
            var context = new AppReviewContext();
            var flow = new AppReviewFlow(context);

            registry.Register(context);
            registry.Register(flow);

            flow.Initialize();
        }
    }
}
