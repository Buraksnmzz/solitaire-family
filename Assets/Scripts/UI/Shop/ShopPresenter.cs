using Collectible;
using Core.Scripts.Services;
using UI.MainMenu;
using Unity.VisualScripting;

namespace UI.Shop
{
    public class ShopPresenter : BasePresenter<ShopView>
    {
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        ISoundService _soundService;
        IHapticService _hapticService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            _eventDispatcherService.AddListener<RewardGivenSignal>(OnRewardGiven);
            _hapticService = ServiceLocator.GetService<IHapticService>();
            View.OnIconMoved += IconMoved;
        }

        private void IconMoved()
        {
            _soundService.PlaySound(ClipName.CoinIncrease);
        }

        private void OnRewardGiven(RewardGivenSignal rewardGivenSignal)
        {
            _soundService.PlaySound(ClipName.ShopPurchase);
            _hapticService.HapticLow();
            View.totalCoins = _savedDataService.GetModel<CollectibleModel>().totalCoins;
            View.PlayCoinAnimation(null, rewardGivenSignal.ButtonTransform);
            View.SetNoAdsButtons(_savedDataService.GetModel<SettingsModel>().IsNoAds);
        }

        public override void ViewShown()
        {
            base.ViewShown();
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
            View.SetNoAdsButtons(_savedDataService.GetModel<SettingsModel>().IsNoAds);
            View.AnimateOnShow(_savedDataService.GetModel<SettingsModel>().IsNoAds);
        }
    }
}