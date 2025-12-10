using Configuration;
using Gameplay;
using Levels;
using Services;
using Services.Hint;
using UI.NoMoreMoves;
using UI.OutOfMoves;
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
        private IHintService _hintService;
        private ISnapshotService _snapshotService;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _levelGeneratorService = ServiceLocator.GetService<ILevelGeneratorService>();
            _configurationService = ServiceLocator.GetService<IConfigurationService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _undoService = ServiceLocator.GetService<IUndoService>();
            _hintService = ServiceLocator.GetService<IHintService>();
            _snapshotService = ServiceLocator.GetService<ISnapshotService>();
            _currentLevelIndex = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            _totalColumnCount = _levelGeneratorService.GetLevelColumnCount(_currentLevelIndex);
            _categoryCardCount = _levelGeneratorService.GetLevelCategoryCardCount(_currentLevelIndex);
            _totalGoalCount = _configurationService.GetLevelGoal(_currentLevelIndex, _totalColumnCount);
            _levelData = _levelGeneratorService.GetLevelData(_currentLevelIndex);
            _movesCount = _totalGoalCount;
            _eventDispatcherService.AddListener<MoveCountRequestedSignal>(OnMoveCountRequested);
            _eventDispatcherService.AddListener<CardMovePerformedSignal>(OnCardMovePerformed);
            _eventDispatcherService.AddListener<PlacableErrorSignal>(OnPlacableError);
            _eventDispatcherService.AddListener<CardMovementStateChangedSignal>(OnCardMovementStateChanged);
            View.UndoButtonClicked += OnUndoClicked;
            View.HintButtonClicked += OnHintClicked;
            View.ApplicationPaused += OnApplicationPaused;
        }

        private void OnApplicationPaused(bool isPaused)
        {
            if (!isPaused)
                return;

            SaveSnapshot();
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

        private void OnCardMovementStateChanged(CardMovementStateChangedSignal signal)
        {
            View.SetInputBlocked(signal.IsMoving);
            if (!signal.IsMoving)
            {
                if (_hintService.GetPlayableMovements(View.Board).Count == 0)
                    _uiService.ShowPopup<OutOfMovesPresenter>();
            }
        }

        private void OnUndoClicked()
        {
            _eventDispatcherService.Dispatch(new UndoClickedSignal());
            _movesCount++;
            View.SetMovesCount(_movesCount);
            View.SetUndoButtonInteractable(false);
        }

        private void OnHintClicked()
        {
            _hintService.ShowHint(View.Board);
        }

        public override void ViewShown()
        {
            base.ViewShown();
            if (_snapshotService.HasSnapShot())
            {
                var snapshot = _snapshotService.LoadSnapshot();
                if (TryResumeFromSnapshot(snapshot))
                    return;

                _snapshotService.ClearSnapshot();
            }

            StartNewLevel();
        }

        void OnMoveCountRequested(MoveCountRequestedSignal signal)
        {
            if (_movesCount <= 0) return;
            _movesCount--;
            View.SetMovesCount(_movesCount);
            if (_movesCount == 0)
                _uiService.ShowPopup<NoMoreMovesPresenter>();
        }

        void StartNewLevel()
        {
            _movesCount = _totalGoalCount;
            View.SetupBoard(_levelData, _currentLevelIndex);
            View.SetMovesCount(_movesCount);
            View.SetUndoButtonInteractable(false);
        }

        bool TryResumeFromSnapshot(SnapShotModel snapshot)
        {
            if (snapshot == null)
                return false;

            if (snapshot.Cards == null || snapshot.Cards.Count == 0)
                return false;

            if (snapshot.LevelIndex != _currentLevelIndex)
            {
                _currentLevelIndex = snapshot.LevelIndex;
                _totalColumnCount = _levelGeneratorService.GetLevelColumnCount(_currentLevelIndex);
                _categoryCardCount = _levelGeneratorService.GetLevelCategoryCardCount(_currentLevelIndex);
                _totalGoalCount = _configurationService.GetLevelGoal(_currentLevelIndex, _totalColumnCount);
                _levelData = _levelGeneratorService.GetLevelData(_currentLevelIndex);
            }

            _movesCount = snapshot.MovesCount;
            View.SetupBoard(_levelData, _currentLevelIndex, snapshot);
            View.SetMovesCount(_movesCount);
            View.SetUndoButtonInteractable(false);
            return true;
        }

        void SaveSnapshot()
        {
            if (View == null || View.Board == null)
                return;

            var snapshot = View.Board.CreateSnapshot(_movesCount, _currentLevelIndex);
            if (snapshot == null)
            {
                _snapshotService.ClearSnapshot();
                return;
            }

            _snapshotService.SaveSnapshot(snapshot);
        }
    }
}


