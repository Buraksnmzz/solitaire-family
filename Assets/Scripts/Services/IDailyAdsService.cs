public interface IDailyAdsService : IService
{
    int DailyLimit { get; }
    int Remaining { get; }
    bool CanUseAd();
    void UseAd();
}