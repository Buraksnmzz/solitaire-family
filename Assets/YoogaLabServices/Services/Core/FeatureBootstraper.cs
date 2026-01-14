using UnityEngine;

namespace ServicesPackage
{
    public class FeatureBootstraper : MonoBehaviour
    {
        public FeatureManifest manifest;
        public Canvas ui_root;

        private FeatureRegistry _registry;

        private void Awake()
        {
            _registry = new FeatureRegistry();
            _registry.Register(ui_root);

            foreach (var feature in manifest.features)
                feature.Setup(_registry);
        }
    }
}
