using Levels;
using Services;
using UI.Gameplay;

namespace UI.GameModeSelection
{
    public class GameModeSelectionPresenter : BasePresenter<GameModeSelectionView>
    {
        IUIService _uiService;
        ISavedDataService _savedDataService;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            View.ClassicButtonClicked += OnClassicButtonClicked;
            View.MathButtonClicked += OnMathButtonClicked;
        }

        void OnClassicButtonClicked()
        {
            StartTutorial(GameMode.Classic);
        }

        void OnMathButtonClicked()
        {
            StartTutorial(GameMode.Math);
        }

        void StartTutorial(GameMode gameMode)
        {
            var gameModeSelectionModel = _savedDataService.GetModel<GameModeSelectionModel>();
            gameModeSelectionModel.SelectedGameMode = gameMode;
            _savedDataService.SaveData(gameModeSelectionModel);
            _uiService.HidePopup<GameModeSelectionPresenter>();
            _uiService.ShowPopup<TutorialGamePresenter>();
        }
    }
}