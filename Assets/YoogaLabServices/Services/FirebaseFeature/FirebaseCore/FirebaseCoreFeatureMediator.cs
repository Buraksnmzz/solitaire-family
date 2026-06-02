
namespace ServicesPackage
{
    public class FirebaseCoreFeatureMediator
    {
        public void Setup(FeatureRegistry registry)
        {
            var context = new FirebaseCoreContext();
            var flow = new FirebasCoreFlow(context);

            registry.Register(context);
            registry.Register(flow);
        }
    }
}