using Collectible;
using Configuration;
using Levels;
using Services;
using UI.Gameplay;
using UI.Settings;
using UI.Shop;

namespace UI.MainMenu
{
    public class MainMenuPresenter : BasePresenter<MainMenuView>
    {
        IUIService _uiService;
        ISnapshotService _snapshotService;
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            _snapshotService = ServiceLocator.GetService<ISnapshotService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _eventDispatcherService.AddListener<RewardGivenSignal>(OnRewardGiven);
            View.SetBackgroundImageFromRemote(_savedDataService.GetModel<GameConfigModel>().backgroundImageId -1);
            View.ContinueButtonClicked += OnContinueButtonClicked;
            View.SettingsButtonClicked += OnSettingsButtonClicked;
            View.CoinButtonClicked += OnCoinButtonCLicked;
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