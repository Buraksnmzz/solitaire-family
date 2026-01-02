using Collectible;
using Configuration;
using IAP;
using Levels;
using Services;
using UI.Gameplay;
using UI.Settings;
using UI.Shop;
using UI.Signals;

namespace UI.MainMenu
{
    public class MainMenuPresenter : BasePresenter<MainMenuView>
    {
        IUIService _uiService;
        ISnapshotService _snapshotService;
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        private IIAPService _iapService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            _snapshotService = ServiceLocator.GetService<ISnapshotService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _eventDispatcherService.AddListener<RewardGivenSignal>(OnRewardGiven);
            _eventDispatcherService.AddListener<CoinChangedSignal>(OnCoinChanged);
            _iapService = ServiceLocator.GetService<IIAPService>();
            View.SetBackgroundImageFromRemote(_savedDataService.GetModel<GameConfigModel>().backgroundImageId -1);
            View.ContinueButtonClicked += OnContinueButtonClicked;
            View.SettingsButtonClicked += OnSettingsButtonClicked;
            View.CoinButtonClicked += OnCoinButtonCLicked;
            View.NoAdsButtonClicked += OnNoAdsButtonClicked;
            if (!_savedDataService.GetModel<SettingsModel>().IsNoAds)
                YoogoLabManager.ShowBanner();
        }

        private void OnCoinChanged(CoinChangedSignal _)
        {
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
        }

        private void OnNoAdsButtonClicked()
        {
            _iapService.Purchase("noads_only", GiveReward);
        }
        
        private void GiveReward(bool success)
        {
            if (success)
            {
                var settingsModel = _savedDataService.GetModel<SettingsModel>();
                settingsModel.IsNoAds = true;
                _savedDataService.SaveData(settingsModel);
                View.SetNoAdsButton(false);
                _eventDispatcherService.Dispatch(new BannerVisibilityChangedSignal(false));
            }
        }

        private void OnRewardGiven(RewardGivenSignal _)
        {
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
        }

        private void OnCoinButtonCLicked()
        {
            _uiService.ShowPopup<ShopPresenter>();
        }

        private void OnSettingsButtonClicked()
        {
            _uiService.ShowPopup<MainSettingsPresenter>();
        }

        public override void ViewShown()
        {
            var currentLevel = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            base.ViewShown();
            View.SetLevelText(currentLevel);
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
            var settingsModel = _savedDataService.GetModel<SettingsModel>();
            View.SetNoAdsButton(!settingsModel.IsNoAds);
        }

        private void OnContinueButtonClicked()
        {
            _uiService.HidePopup<MainMenuPresenter>();
            _uiService.ShowPopup<GameplayPresenter>();
        }

        private void OnLevelButtonClicked()
        {
            _snapshotService.ClearSnapshot();
            _uiService.HidePopup<MainMenuPresenter>();
            _uiService.ShowPopup<GameplayPresenter>();
        }
    }
}