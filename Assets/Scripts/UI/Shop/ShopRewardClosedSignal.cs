namespace UI.Shop
{
    public class ShopRewardClosedSignal : ISignal
    {
        public bool IsNoAdsOnly;

        public ShopRewardClosedSignal(bool isNoAdsOnly)
        {
            IsNoAdsOnly = isNoAdsOnly;
        }
    }
}