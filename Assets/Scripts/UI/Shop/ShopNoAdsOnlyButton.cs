using UI.Signals;

namespace UI.Shop
{
    public class ShopNoAdsOnlyButton: ShopPurchaseButton
    {
        protected override void GiveReward(bool success)
        {
            if (success)
            {
                var settingsModel = SavedDataService.GetModel<SettingsModel>();
                settingsModel.IsNoAds = true;
                SavedDataService.SaveData(settingsModel);
                EventDispatcherService.Dispatch(new RewardGivenSignal(transform));
                EventDispatcherService.Dispatch(new BannerVisibilityChangedSignal(false));
            }
        }

        public override void SetRewardValue()
        {
            
        }
    }
}