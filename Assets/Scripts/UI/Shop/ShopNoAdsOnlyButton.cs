namespace UI.Shop
{
    public class ShopNoAdsOnlyButton: ShopPurchaseButton
    {
        protected override void GiveReward(bool success)
        {
            var settingsModel = SavedDataService.GetModel<SettingsModel>();
            settingsModel.IsNoAds = true;
            SavedDataService.SaveData(settingsModel);
            EventDispatcherService.Dispatch(new RewardGivenSignal(transform));
        }

        public override void SetRewardValue()
        {
            
        }
    }
}