using Configuration;
using Gameplay;
using Levels;
using Services;
using UI.NoMoreMoves;
using UI.Signals;

namespace UI.Gameplay
{
    public class GameplayPresenter : BasePresenter<GameplayView>
    {
        private int _totalGoalCount;
        private int _currentLevelIndex;
        private int _totalColumnCount;
        private int _categoryCardCount;
        ILevelGeneratorService _levelGeneratorService;
        IConfigurationService _configurationService;
        ISavedDataService _savedDataService;
        IEventDispatcherService _eventDispatcherService;
        LevelData _levelData;
        int _movesCount;
        private IUIService _uiService;
        private IUndoService _undoService;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _levelGeneratorService = ServiceLocator.GetService<ILevelGeneratorService>();
            _configurationService = ServiceLocator.GetService<IConfigurationService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _undoService = ServiceLocator.GetService<IUndoService>();
            _currentLevelIndex = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            _totalColumnCount = _levelGeneratorService.GetLevelColumnCount(_currentLevelIndex);
            _categoryCardCount = _levelGeneratorService.GetLevelCategoryCardCount(_currentLevelIndex);
            _totalGoalCount = _configurationService.GetLevelGoal(_currentLevelIndex, _totalColumnCount);
            _levelData = _levelGeneratorService.GetLevelData(_currentLevelIndex);
            _movesCount = _totalGoalCount;
            _eventDispatcherService.AddListener<MoveCountRequestedSignal>(OnMoveCountRequested);
            _eventDispatcherService.AddListener<CardMovePerformedSignal>(OnCardMovePerformed);
            _eventDispatcherService.AddListener<PlacableErrorSignal>(OnPlacableError);
            View.UndoButtonClicked += OnUndoClicked;
        }

        private void OnPlacableError(PlacableErrorSignal placableError)
        {
            var errorMessage = placableError.PlacableErrorMessage;
            View.ShowErrorMessage(errorMessage);
        }

        private void OnCardMovePerformed(CardMovePerformedSignal _)
        {
            View.SetUndoButtonInteractable(_undoService.UndoAvailable);
        }

        private void OnUndoClicked()
        {
            _eventDispatcherService.Dispatch(new UndoClickedSignal());
            _movesCount++;
            View.SetMovesCount(_movesCount);
            View.SetUndoButtonInteractable(false);
        }

        public override void ViewShown()
        {
            base.ViewShown();
            View.SetupBoard(_levelData, _currentLevelIndex);
            View.SetMovesCount(_movesCount);
        }

        void OnMoveCountRequested(MoveCountRequestedSignal signal)
        {
            if (_movesCount <= 0) return;
            _movesCount--;
            View.SetMovesCount(_movesCount);
            if (_movesCount == 0)
                _uiService.ShowPopup<NoMoreMovesPresenter>();
        }
    }
}


