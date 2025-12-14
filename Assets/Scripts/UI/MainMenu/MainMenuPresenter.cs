using Levels;
using Services;
using UI.Gameplay;

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
            View.LevelButtonClicked += OnLevelButtonClicked;
            View.ContinueButtonClicked += OnContinueButtonClicked;
        }

        public override void ViewShown()
        {
            var currentLevel = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            base.ViewShown();
            var hasSnapshot = _snapshotService.HasSnapShot();
            View.continueButton.gameObject.SetActive(hasSnapshot);
            View.levelButton.gameObject.SetActive(!hasSnapshot);
            View.SetLevelText(currentLevel);
           
        }

        private void OnContinueButtonClicked()
        {
            if (!_snapshotService.HasSnapShot())
                return;

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