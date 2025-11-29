using System;
using UnityEngine.Serialization;

namespace Collectible
{
    [Serializable]
    public class CollectibleModel : IModel
    {
        public int totalCoins = 0;
    }
}
