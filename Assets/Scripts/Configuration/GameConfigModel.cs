using System;

namespace Configuration
{
    [Serializable]
    public class GameConfigModel : IModel
    {
        public int EarnedCoinAtLevelEnd = 50;
        public int EarnedCoinPerMoveLeft = 2;
        public int HintCost = 900;
        public int UndoCost = 800;
        public int JokerCost = 900;
        public int Layout = 1;
    }
}
