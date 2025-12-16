using System;
using UnityEngine.Serialization;

namespace Configuration
{
    [Serializable]
    public class GameConfigModel : IModel
    {
        public int earnedCoinAtLevelEnd = 50;
        public int earnedCoinPerMoveLeft = 2;
        public int hintCost = 900;
        public int undoCost = 800;
        public int jokerCost = 900;
        public int extraMovesCost = 1500;
        public int layout = 1;
        public int backgroundImageId = 1;
    }
}
