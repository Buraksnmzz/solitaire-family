using UnityEngine;

namespace ServicesPackage
{
    public abstract class FeatureMediatorCore : ScriptableObject
    {
        public abstract void Setup(FeatureRegistry registry);
        public virtual void Shutdown() { }
    }
}
