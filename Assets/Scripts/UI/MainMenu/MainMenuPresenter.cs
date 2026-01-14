using Collectible;
using Configuration;
using IAP;
using Levels;
using Services;
using UI.Gameplay;
using UI.Settings;
using UI.Shop;
using UI.Signals;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuPresenter : BasePresenter<MainMenuView>
    {
        IUIService _uiService;
        ISnapshotService _snapshotService;
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        ILocalizationService _localizationService;
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
            _localizationService = ServiceLocator.GetService<ILocalizationService>();
            View.SetBackgroundImageFromRemote(_savedDataService.GetModel<GameConfigModel>().backgroundImageId - 1);
            View.ContinueButtonClicked += OnContinueButtonClicked;
            View.SettingsButtonClicked += OnSettingsButtonClicked;
            View.CoinButtonClicked += OnCoinButtonCLicked;
            View.NoAdsButtonClicked += OnNoAdsButtonClicked;
            //if (!_savedDataService.GetModel<SettingsModel>().IsNoAds)
            //    YoogoLabManager.ShowBanner();
        }

        private void OnCoinChanged(CoinChangedSignal _)
        {
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
        }

        private void OnNoAdsButtonClicked()
        {
            _iapService.Purchase("solitairefam_noads", GiveReward);
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
            base.ViewShown();

            _eventDispatcherService.AddListener<LanguageChangedSignal>(OnLanguageChanged);

            RefreshLevelText();
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
            var settingsModel = _savedDataService.GetModel<SettingsModel>();
            View.SetNoAdsButton(!settingsModel.IsNoAds);
        }

        public override void ViewHidden()
        {
            base.ViewHidden();
            _eventDispatcherService.RemoveListener<LanguageChangedSignal>(OnLanguageChanged);
        }

        private void OnLanguageChanged(LanguageChangedSignal signal)
        {
            RefreshLevelText();
        }

        private void RefreshLevelText()
        {
            var currentLevel = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            var levelText = _localizationService.GetLocalizedString(LocalizationStrings.LevelX, currentLevel);
            View.SetLevelText(levelText);
        }

        private void OnContinueButtonClicked()
        {
            _uiService.HidePopup<MainMenuPresenter>();
            _uiService.ShowPopup<GameplayPresenter>();
        }
    }
}