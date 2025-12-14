using Collectible;
using Configuration;
using Levels;
using Services;
using UI.Gameplay;
using UI.Settings;

namespace UI.MainMenu
{
    public class MainMenuPresenter : BasePresenter<MainMenuView>
    {
        IUIService _uiService;
        ISnapshotService _snapshotService;
        ISavedDataService _savedDataService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            _snapshotService = ServiceLocator.GetService<ISnapshotService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            View.SetBackgroundImageFromRemote(_savedDataService.GetModel<GameConfigModel>().backgroundImageId -1);
            //View.LevelButtonClicked += OnLevelButtonClicked;
            View.ContinueButtonClicked += OnContinueButtonClicked;
            View.SettingsButtonClicked += OnSettingsButtonClicked;
        }

        private void OnSettingsButtonClicked()
        {
            _uiService.ShowPopup<MainSettingsPresenter>();
        }

        public override void ViewShown()
        {
            var currentLevel = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            base.ViewShown();
            //var hasSnapshot = _snapshotService.HasSnapShot();
            //View.continueButton.gameObject.SetActive(hasSnapshot);
            //View.levelButton.gameObject.SetActive(!hasSnapshot);
            View.SetLevelText(currentLevel);
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);

        }

        private void OnContinueButtonClicked()
        {
            // if (!_snapshotService.HasSnapShot())
            //     return;
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