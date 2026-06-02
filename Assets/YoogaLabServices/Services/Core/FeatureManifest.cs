using System.Collections.Generic;
using UnityEngine;

namespace ServicesPackage
{
    [CreateAssetMenu(menuName = "Features/FeatureManifest")]
    public class FeatureManifest : ScriptableObject
    {
        public List<FeatureMediatorCore> features = new();
    }
}
