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
        public int[] rateUsTriggerLevels;
        public int dailyAdsWatchAmount = 9;
        public int rewardedVideoCoinAmount = 500;
        public int extraGivenMovesCount = 500;
        public int noAdsPackCoinReward = 9000;
        public int noAdsPackJokerReward = 5;
        public int noAdsPackHintReward = 5;
        public int noAdsPackUndoReward = 5;
        public int shopCoinReward1 = 1500;
        public int shopCoinReward2 = 7000;
        public int shopCoinReward3 = 25000;
        public int shopCoinReward4 = 50000;
        public int shopCoinReward5 = 100000;
    }
}
