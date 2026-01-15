using System;

namespace Services
{
    public class AdsService: IAdsService
    {
        private readonly ICollectibelService _collectibelService;
        private readonly int _rewardedCoin = 25;
        private readonly int _rewardedHint = 1;
    
        public AdsService()
        {
            _collectibelService = ServiceLocator.GetService<ICollectibelService>();
        }

        public bool IsRewardedAvailable()
        {
            return YoogoLabManager.RewardedAvailable(null, null);
        }
        
        public void GetReward(Action<bool> callback)
        {
            YoogoLabManager.RewardedAvailable(
                onAvailable: () =>
                {
                    YoogoLabManager.PlayRewarded(success =>
                    {
                        if (success)
                        {
                            callback?.Invoke(true);
                            return;
                        }
                        callback?.Invoke(false);
                    });
                },
                onUnavailable: () =>
                {
                    callback?.Invoke(false);
                });
        }
    }
}