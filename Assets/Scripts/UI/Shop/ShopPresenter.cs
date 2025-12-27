using Collectible;
using Configuration;
using Core.Scripts.Services;
using Services;
using UI.MainMenu;
using UI.Signals;
using Unity.VisualScripting;

namespace UI.Shop
{
    public class ShopPresenter : BasePresenter<ShopView>
    {
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        ISoundService _soundService;
        IHapticService _hapticService;
        IAdsService  _adsService;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            _eventDispatcherService.AddListener<RewardGivenSignal>(OnRewardGiven);
            _hapticService = ServiceLocator.GetService<IHapticService>();
            _adsService = ServiceLocator.GetService<IAdsService>();
            View.OnIconMoved += IconMoved;
            View.RewardedVideoButtonClicked += OnRewardedVideoButtonClicked;
        }

        private void OnRewardedVideoButtonClicked()
        {
            _adsService.GetReward(CallbackReward);
            
        }
        
        private void CallbackReward(bool success)
        {
            if (success)
            {
                var configModel = _savedDataService.GetModel<GameConfigModel>();
                var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
                _soundService.PlaySound(ClipName.ShopPurchase);
                _hapticService.HapticLow();
                collectibleModel.totalCoins += configModel.rewardedVideoCoinAmount;
                _savedDataService.SaveData(collectibleModel);
                View.totalCoins = collectibleModel.totalCoins;
                _eventDispatcherService.Dispatch(new CoinChangedSignal());
                View.PlayCoinAnimation(null, View.rewardedVideoButton.transform);
            }
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
            var isNoAds = _savedDataService.GetModel<SettingsModel>().IsNoAds;
            View.PlayCoinAnimation(() => View.SetNoAdsButtons(isNoAds), rewardGivenSignal.ButtonTransform);
        }

        public override void ViewShown()
        {
            base.ViewShown();
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
            View.SetNoAdsButtons(_savedDataService.GetModel<SettingsModel>().IsNoAds);
            View.AnimateOnShow(_savedDataService.GetModel<SettingsModel>().IsNoAds);
            View.SetRewardedVideoCoinAmount(_savedDataService.GetModel<GameConfigModel>().rewardedVideoCoinAmount);
        }
    }
}