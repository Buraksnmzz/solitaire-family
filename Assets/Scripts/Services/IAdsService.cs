using System;

namespace Services
{
    public interface IAdsService: IService
    {
        void GetReward(Action<bool> callback);
        bool IsRewardedAvailable();
    }
}