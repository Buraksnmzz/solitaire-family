using System;
using UnityEngine.Serialization;

namespace Collectible
{
    [Serializable]
    public class CollectibleModel : IModel
    {
        public int totalCoins = 0;
        public int totalUndo = 0;
        public int totalHints = 0;
        public int totalJokers = 0;
    }
}
