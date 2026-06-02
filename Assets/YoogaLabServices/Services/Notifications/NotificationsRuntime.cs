using UnityEngine;

namespace ServicesPackage
{
    public class NotificationsRuntime : MonoBehaviour
    {
        private NotificationsFlow flow;
        private FeatureRegistry featureRegistry;
        public void Initialize(NotificationsFlow flow, FeatureRegistry registry)
        {
            this.flow = flow;
            featureRegistry = registry;
        }
    }
}