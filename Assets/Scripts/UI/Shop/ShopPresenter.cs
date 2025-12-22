using Collectible;
using UI.MainMenu;
using Unity.VisualScripting;

namespace UI.Shop
{
    public class ShopPresenter: BasePresenter<ShopView>
    {
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        IUIService _uiService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _eventDispatcherService.AddListener<RewardGivenSignal>(OnRewardGiven);
        }

        private void OnRewardGiven(RewardGivenSignal rewardGivenSignal)
        {
            View.totalCoins = _savedDataService.GetModel<CollectibleModel>().totalCoins;
            View.PlayCoinAnimation(null, rewardGivenSignal.ButtonTransform);
            View.SetNoAdsButtons(_savedDataService.GetModel<SettingsModel>().IsNoAds);
        }

        public override void ViewShown()
        {
            base.ViewShown();
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
            View.SetNoAdsButtons(_savedDataService.GetModel<SettingsModel>().IsNoAds);
        }
    }
}