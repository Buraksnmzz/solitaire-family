namespace UI.Shop
{
    public class ShopNoAdsOnlyButton: ShopPurchaseButton
    {
        protected override void GiveReward(bool success)
        {
            var settingsModel = _savedDataService.GetModel<SettingsModel>();
            settingsModel.IsNoAds = true;
            _savedDataService.SaveData(settingsModel);
            //todo: Rise event to remove banner ads and hide shopView buttons
        }

        public override void SetRewardValue()
        {
            
        }
    }
}