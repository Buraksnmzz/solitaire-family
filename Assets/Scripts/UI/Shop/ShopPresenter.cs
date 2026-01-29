using Collectible;
using Configuration;
using Core.Scripts.Services;
using Services;
using UI.MainMenu;
using UI.Signals;
using Unity.VisualScripting;
using UnityEngine;

namespace UI.Shop
{
    public class ShopPresenter : BasePresenter<ShopView>
    {
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        ISoundService _soundService;
        IHapticService _hapticService;
        IAdsService  _adsService;
        IUIService _uiService;
        private Transform _buttonTransform;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            _eventDispatcherService.AddListener<RewardGivenSignal>(OnRewardGiven);
            _eventDispatcherService.AddListener<ShopRewardClosedSignal>(OnShopRewardClosed);
            _hapticService = ServiceLocator.GetService<IHapticService>();
            _adsService = ServiceLocator.GetService<IAdsService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            View.OnIconMoved += IconMoved;
            View.RewardedVideoButtonClicked += OnRewardedVideoButtonClicked;
        }

        private void OnShopRewardClosed(ShopRewardClosedSignal shopRewardClosedSignal)
        {
            var isNoAds = _savedDataService.GetModel<SettingsModel>().IsNoAds;
            if (shopRewardClosedSignal.IsNoAdsOnly)
            {
                View.SetNoAdsButtons(true);
                return;
            }
            
            View.PlayCoinAnimation(() => View.SetNoAdsButtons(isNoAds), _buttonTransform);
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
            _buttonTransform = rewardGivenSignal.ButtonTransform;
            //var isNoAds = _savedDataService.GetModel<SettingsModel>().IsNoAds;
            //View.PlayCoinAnimation(() => View.SetNoAdsButtons(isNoAds), rewardGivenSignal.ButtonTransform);
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