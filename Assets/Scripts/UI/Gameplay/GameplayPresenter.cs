using Collectible;
using Configuration;
using Gameplay;
using Levels;
using Services;
using Services.Hint;
using UI.NoMoreMoves;
using UI.OutOfMoves;
using UI.Signals;
using UI.Win;

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
        private bool _isGameWon;

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
             
            _eventDispatcherService.AddListener<MoveCountRequestedSignal>(OnMoveCountRequested);
            _eventDispatcherService.AddListener<CardMovePerformedSignal>(OnCardMovePerformed);
            _eventDispatcherService.AddListener<PlacableErrorSignal>(OnPlacableError);
            _eventDispatcherService.AddListener<CardMovementStateChangedSignal>(OnCardMovementStateChanged);
            _eventDispatcherService.AddListener<RestartButtonClickSignal>(OnRestartButtonClick);
            _eventDispatcherService.AddListener<AddMovesClickedSignal>(OnAddMovesClicked);
            View.UndoButtonClicked += OnUndoClicked;
            View.HintButtonClicked += OnHintClicked;
            View.JokerButtonClicked += OnJokerClicked;
            View.ApplicationPaused += OnApplicationPaused;
            View.CoinButtonClicked += OnCoinButtonClicked;
            View.DegubNextButtonClicked += OnDebugNextButtonClicked;
            View.DegubRestartButtonClicked += OnDegubRestartButtonClicked;
        }

        private void OnAddMovesClicked(AddMovesClickedSignal _)
        {
            _movesCount += 10;
            View.SetMovesCount(_movesCount);
        }

        private void OnRestartButtonClick(RestartButtonClickSignal _)
        {
            if (_snapshotService.HasSnapShot())
            {
                _snapshotService.ClearSnapshot();
            }
            _uiService.ShowPopup<GameplayPresenter>();
        }

        private void OnCoinButtonClicked()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            collectibleModel.totalCoins += 1000;
            View.SetCoinText(collectibleModel.totalCoins);
            _savedDataService.SaveData(collectibleModel);
        }

        private void OnDegubRestartButtonClicked()
        {
            if (_snapshotService.HasSnapShot())
            {
                _snapshotService.ClearSnapshot();
            }
            _uiService.ShowPopup<GameplayPresenter>();
        }

        private void OnDebugNextButtonClicked()
        {
            if (_snapshotService.HasSnapShot())
            {
                _snapshotService.ClearSnapshot();
            }
            var levelProgress = _savedDataService.GetModel<LevelProgressModel>();
            levelProgress.CurrentLevelIndex++;
            _savedDataService.SaveData(levelProgress);
            _uiService.ShowPopup<GameplayPresenter>();
        }

        private void OnJokerClicked()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (collectibleModel.totalJokers >= 1)
            {
                collectibleModel.totalJokers--;
                HandleJoker(collectibleModel.totalJokers);
            }
            else if(collectibleModel.totalCoins >= gameConfigModel.jokerCost)
            {
                collectibleModel.totalCoins -= gameConfigModel.jokerCost;
                View.SetCoinText(collectibleModel.totalCoins);
                HandleJoker(collectibleModel.totalJokers);
            }
        }

        private void HandleJoker(int totalJokers)
        {
            View.board.GenerateJokerCard();
            View.SetJokerAmount(totalJokers);
            View.SetJokerButtonInteractable(false);
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
                if(_isGameWon)
                    return;
                if (_hintService.GetPlayableMovements(View.Board).Count == 0)
                    _uiService.ShowPopup<OutOfMovesPresenter>();
            }
        }

        private void OnUndoClicked()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (collectibleModel.totalUndo >= 1)
            {
                collectibleModel.totalUndo--;
                HandleUndo(collectibleModel.totalUndo);
            }
            else if(collectibleModel.totalCoins >= gameConfigModel.undoCost)
            {
                collectibleModel.totalCoins -= gameConfigModel.undoCost;
                View.SetCoinText(collectibleModel.totalCoins);
                HandleUndo(collectibleModel.totalUndo);
            }
        }

        private void HandleUndo(int totalUndo)
        {
            _eventDispatcherService.Dispatch(new UndoClickedSignal());
            _movesCount++;
            View.SetMovesCount(_movesCount);
            View.SetUndoButtonInteractable(false);
            View.SetUndoAmount(totalUndo);
        }

        private void OnHintClicked()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (collectibleModel.totalHints >= 1)
            {
                collectibleModel.totalHints--;
                _hintService.ShowHint(View.Board);
                View.SetHintAmount(collectibleModel.totalHints);
            }
            else if(collectibleModel.totalCoins >= gameConfigModel.undoCost)
            {
                collectibleModel.totalCoins -= gameConfigModel.hintCost;
                View.SetCoinText(collectibleModel.totalCoins);
                _hintService.ShowHint(View.Board);
                View.SetHintAmount(collectibleModel.totalHints);
            }
        }

        public override void ViewShown()
        {
            _isGameWon = false;
            _currentLevelIndex = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            _totalColumnCount = _levelGeneratorService.GetLevelColumnCount(_currentLevelIndex);
            _categoryCardCount = _levelGeneratorService.GetLevelCategoryCardCount(_currentLevelIndex);
            _totalGoalCount = _configurationService.GetLevelGoal(_currentLevelIndex, _totalColumnCount);
            _levelData = _levelGeneratorService.GetLevelData(_currentLevelIndex);
            _movesCount = _totalGoalCount;
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            View.SetJokerAmount(collectibleModel.totalJokers);
            View.SetHintAmount(collectibleModel.totalHints);
            View.SetUndoAmount(collectibleModel.totalUndo);
            View.SetCoinText(collectibleModel.totalCoins);
            View.SetLevelText(_currentLevelIndex+1);
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

            if(!View.Board.IsGameWon())
                return;
            
            _isGameWon = true;
            var levelProgressModel = _savedDataService.GetModel<LevelProgressModel>();
            levelProgressModel.CurrentLevelIndex++;
            _savedDataService.SaveData(levelProgressModel);
            if (_snapshotService.HasSnapShot())
            {
                _snapshotService.ClearSnapshot();
            }
            _uiService.ShowPopup<WinPresenter>();
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


