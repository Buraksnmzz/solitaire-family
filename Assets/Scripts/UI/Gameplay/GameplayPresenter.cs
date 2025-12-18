using Collectible;
using Configuration;
using DG.Tweening;
using Gameplay;
using Levels;
using Services;
using Services.Hint;
using UI.NoMoreMoves;
using UI.OutOfMoves;
using UI.Settings;
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
        private CollectibleModel _collectibleModel;

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
            _eventDispatcherService.AddListener<ContinueWithCoinAddMovesSignal>(OnContinueWithCoinAddMoves);
            _eventDispatcherService.AddListener<ContinueWithCoinAddJokerSignal>(OnContinueWithCoinAddJoker);
            _eventDispatcherService.AddListener<JokerClickedSignal>(OnJokerClickedFromNoMoreMoves);
            View.UndoButtonClicked += OnUndoClicked;
            View.HintButtonClicked += OnHintClicked;
            View.JokerButtonClicked += OnJokerClicked;
            View.ApplicationPaused += OnApplicationPaused;
            View.CoinButtonClicked += OnCoinButtonClicked;
            View.DegubNextButtonClicked += OnDebugNextButtonClicked;
            View.DegubRestartButtonClicked += OnDebugRestartButtonClicked;
            View.SettingsButtonClicked += OnSettingsButtonClicked;
            View.DegubCompleteButtonClicked += OnDebugCompleteButtonClicked;
        }

        private void OnDebugCompleteButtonClicked()
        {
            _isGameWon = true;
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            var earnedCoinsFromMoves = _movesCount * gameConfigModel.earnedCoinPerMoveLeft;
            if (earnedCoinsFromMoves < 0)
                earnedCoinsFromMoves = 0;

            var initialCoins = _collectibleModel.totalCoins;
            _collectibleModel.totalCoins += earnedCoinsFromMoves;
            _savedDataService.SaveData(_collectibleModel);

            var levelProgressModel = _savedDataService.GetModel<LevelProgressModel>();
            levelProgressModel.CurrentLevelIndex++;
            _savedDataService.SaveData(levelProgressModel);
            if (_snapshotService.HasSnapShot())
            {
                _snapshotService.ClearSnapshot();
            }

            View.PlayEarnedMovesCoinAnimation(_movesCount, gameConfigModel.earnedCoinPerMoveLeft, initialCoins, () =>
            {
                _uiService.ShowPopup<WinPresenter>();
            });
        }

        private void OnContinueWithCoinAddJoker(ContinueWithCoinAddJokerSignal _)
        {
            View.SetCoinText(_collectibleModel.totalCoins);
            HandleJoker(_collectibleModel.totalJokers);
        }

        private void OnJokerClickedFromNoMoreMoves(JokerClickedSignal _)
        {
            HandleJoker(_collectibleModel.totalJokers);
        }

        private void OnContinueWithCoinAddMoves(ContinueWithCoinAddMovesSignal _)
        {
            View.SetCoinText(_collectibleModel.totalCoins);
            _movesCount += 10;
            View.SetMovesCount(_movesCount);
        }

        private void OnSettingsButtonClicked()
        {
            _uiService.ShowPopup<GameSettingsPresenter>();
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
            _collectibleModel.totalCoins += 1000;
            View.SetCoinText(_collectibleModel.totalCoins);
            _savedDataService.SaveData(_collectibleModel);
        }

        private void OnDebugRestartButtonClicked()
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
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (_collectibleModel.totalJokers >= 1)
            {
                _collectibleModel.totalJokers--;
                HandleJoker(_collectibleModel.totalJokers);
            }
            else if (_collectibleModel.totalCoins >= gameConfigModel.jokerCost)
            {
                _collectibleModel.totalCoins -= gameConfigModel.jokerCost;
                View.SetCoinText(_collectibleModel.totalCoins);
                HandleJoker(_collectibleModel.totalJokers);
            }
        }

        private void HandleJoker(int totalJokers)
        {
            View.board.GenerateJokerCard();
            View.SetJokerAmount(totalJokers);
            //View.SetJokerButtonInteractable(false);
            _savedDataService.SaveData(_collectibleModel);
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
            if (!signal.IsMoving)
            {
                if (_isGameWon)
                    return;
                if (_hintService.GetPlayableMovements(View.Board).Count == 0)
                    _uiService.ShowPopup<NoMoreMovesPresenter>();
            }
        }

        private void OnUndoClicked()
        {
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (_collectibleModel.totalUndo >= 1)
            {
                _collectibleModel.totalUndo--;
                HandleUndo(_collectibleModel.totalUndo);
            }
            else if (_collectibleModel.totalCoins >= gameConfigModel.undoCost)
            {
                _collectibleModel.totalCoins -= gameConfigModel.undoCost;
                View.SetCoinText(_collectibleModel.totalCoins);
                HandleUndo(_collectibleModel.totalUndo);
            }
        }

        private void HandleUndo(int totalUndo)
        {
            _eventDispatcherService.Dispatch(new UndoClickedSignal());
            _movesCount++;
            View.SetMovesCount(_movesCount);
            View.SetUndoButtonInteractable(false);
            View.SetUndoAmount(totalUndo);
            _savedDataService.SaveData(_collectibleModel);
        }

        private void OnHintClicked()
        {
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (_collectibleModel.totalHints >= 1)
            {
                _collectibleModel.totalHints--;
                _hintService.ShowHint(View.Board);
                View.SetHintAmount(_collectibleModel.totalHints);
            }
            else if (_collectibleModel.totalCoins >= gameConfigModel.undoCost)
            {
                _collectibleModel.totalCoins -= gameConfigModel.hintCost;
                View.SetCoinText(_collectibleModel.totalCoins);
                _hintService.ShowHint(View.Board);
                View.SetHintAmount(_collectibleModel.totalHints);
            }
            _savedDataService.SaveData(_collectibleModel);
        }

        public override void ViewShown()
        {
            _isGameWon = false;
            _currentLevelIndex = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
            _totalColumnCount = _levelGeneratorService.GetLevelColumnCount(_currentLevelIndex);
            _categoryCardCount = _levelGeneratorService.GetLevelCategoryCardCount(_currentLevelIndex);
            _totalGoalCount = _configurationService.GetLevelGoal(_currentLevelIndex + 1, _totalColumnCount);
            _levelData = _levelGeneratorService.GetLevelData(_currentLevelIndex);
            _movesCount = _totalGoalCount;
            _collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            View.SetJokerAmount(_collectibleModel.totalJokers);
            View.SetHintAmount(_collectibleModel.totalHints);
            View.SetUndoAmount(_collectibleModel.totalUndo);
            View.SetCoinText(_collectibleModel.totalCoins);
            View.SetLevelText(_currentLevelIndex + 1);
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
                _uiService.ShowPopup<OutOfMovesPresenter>();

            if (!View.Board.IsGameWon())
                return;

            _isGameWon = true;
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            var earnedCoinsFromMoves = _movesCount * gameConfigModel.earnedCoinPerMoveLeft;
            if (earnedCoinsFromMoves < 0)
                earnedCoinsFromMoves = 0;

            var initialCoins = _collectibleModel.totalCoins;
            _collectibleModel.totalCoins += earnedCoinsFromMoves;
            _savedDataService.SaveData(_collectibleModel);

            var levelProgressModel = _savedDataService.GetModel<LevelProgressModel>();
            levelProgressModel.CurrentLevelIndex++;
            _savedDataService.SaveData(levelProgressModel);
            if (_snapshotService.HasSnapShot())
            {
                _snapshotService.ClearSnapshot();
            }

            View.PlayEarnedMovesCoinAnimation(_movesCount, gameConfigModel.earnedCoinPerMoveLeft, initialCoins, () =>
            {
                _uiService.ShowPopup<WinPresenter>();
            });
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


