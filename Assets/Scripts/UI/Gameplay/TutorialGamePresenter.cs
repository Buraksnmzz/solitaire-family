using System.Collections.Generic;
using Card;
using Configuration;
using Core.Scripts.Services;
using DG.Tweening;
using Gameplay;
using Levels;
using Services;
using Services.Drag;
using Services.Hint;
using UI.Signals;
using UI.Tutorial;
using Tutorial;
using UnityEngine;

namespace UI.Gameplay
{
    public class TutorialGamePresenter : BasePresenter<TutorialGameplayView>
    {
        ILevelGeneratorService _levelGeneratorService;
        IEventDispatcherService _eventDispatcherService;
        IUIService _uiService;
        IHintService _hintService;
        ITutorialMoveRestrictionService _tutorialMoveRestrictionService;
        IDragStateService _dragStateService;
        ISavedDataService _savedDataService;
        ISoundService _soundService;
        IHapticService _hapticService;
        int _currentLevelIndex;
        bool _isGameWon;
        Tween _hintLoopTween;
        bool _isDragging;
        TutorialConfig _tutorialConfig;
        int _currentStepIndex;
        int _currentStepMoveIndex;
        CardPresenter _currentPresenter;
        CardContainer _currentTargetContainer;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _levelGeneratorService = ServiceLocator.GetService<ILevelGeneratorService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _hintService = ServiceLocator.GetService<IHintService>();
            _tutorialMoveRestrictionService = ServiceLocator.GetService<ITutorialMoveRestrictionService>();
            _dragStateService = ServiceLocator.GetService<IDragStateService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            _hapticService = ServiceLocator.GetService<IHapticService>();
        }

        public override void ViewShown()
        {
            base.ViewShown();
            _isGameWon = false;
            _hintLoopTween?.Kill();
            _tutorialConfig = View.TutorialConfig;
            _currentStepIndex = 0;
            _currentStepMoveIndex = 0;
            UpdateInstructionText();
            var levelData = _levelGeneratorService.GetLevelData(0);
            View.SetupBoardWithoutShuffle(levelData, _currentLevelIndex);
            DOVirtual.DelayedCall(1, () =>
            {
                PrepareNextStep();
                _eventDispatcherService.AddListener<CardMovementStateChangedSignal>(OnCardMovementStateChanged);
            });
        }

        public override void ViewHidden()
        {
            base.ViewHidden();
            _eventDispatcherService.RemoveListener<CardMovementStateChangedSignal>(OnCardMovementStateChanged);
        }

        void PrepareNextStep()
        {
            if (_isGameWon) return;
            if (_tutorialMoveRestrictionService == null) return;

            _hintLoopTween?.Kill();
            _isDragging = false;
            var movement = GetCurrentTutorialMovement();
            if (movement == null)
            {
                if (View.Board != null && View.Board.IsGameWon())
                {
                    _isGameWon = true;
                    _uiService.ShowPopup<TutorialCompletedPresenter>();
                }
                return;
            }

            if (movement.Presenters == null || movement.Presenters.Count == 0)
                return;

            var presenter = movement.Presenters[0];
            if (presenter == null)
                return;

            _currentPresenter = presenter;
            _currentTargetContainer = movement.ToContainer;
            _tutorialMoveRestrictionService.SetCurrentMove(presenter, movement.ToContainer);
            var moveDuration = 1.5f;
            var fadeDuration = 0.6f;
            var shouldShowHand = movement.Presenters.Count > 1;
            _hintService.ShowHintForMovement(View.Board, movement, shouldShowHand, moveDuration, fadeDuration);

            var totalDelay = moveDuration + fadeDuration + 0.8f;
            ScheduleNextHint(totalDelay, moveDuration, fadeDuration, shouldShowHand);
        }

        void ScheduleNextHint(float totalDelay, float moveDuration, float fadeDuration, bool showHand)
        {
            _hintLoopTween = DOVirtual.DelayedCall(totalDelay, () =>
            {
                if (_isGameWon) return;
                if (_tutorialMoveRestrictionService == null) return;
                if (!_tutorialMoveRestrictionService.IsActive) return;
                if (_isDragging) return;
                if (!_dragStateService.CanStartDrag()) return;
                var movement = GetCurrentTutorialMovement();
                if (movement == null) return;
                _hintService.ShowHintForMovement(View.Board, movement, showHand, moveDuration, fadeDuration);
                ScheduleNextHint(totalDelay, moveDuration, fadeDuration, showHand);
            });
        }


        void OnCardMovementStateChanged(CardMovementStateChangedSignal signal)
        {
            if (signal.IsMoving)
            {
                _isDragging = true;

                _hintLoopTween?.Kill();
                _hintService?.ShowHintForMovement(View.Board, null);
                View.Board?.Dealer?.StopHintCue();

                return;
            }

            _isDragging = false;

            var isCorrectMove = false;
            if (_currentPresenter != null && _currentTargetContainer != null)
            {
                var container = _currentPresenter.GetContainer();
                if (container == _currentTargetContainer)
                    isCorrectMove = true;
            }

            if (View.Board != null && View.Board.IsGameWon())
            {
                _isGameWon = true;
                _hintLoopTween?.Kill();
                _tutorialMoveRestrictionService?.ClearCurrentMove();
                _currentPresenter = null;
                _currentTargetContainer = null;
                PlayerPrefs.SetInt(StringConstants.IsTutorialShown, 1);
                PlayerPrefs.Save();
                View.SetErrorImage(false);
                var levelProgressModel = _savedDataService.GetModel<LevelProgressModel>();
                levelProgressModel.CurrentLevelIndex++;
                _savedDataService.SaveData(levelProgressModel);

                DOVirtual.DelayedCall(0.8f, () =>
                {
                    _soundService.PlaySound(ClipName.GameWon);
                    _hapticService.HapticLow();
                    View.PlayConfetti();
                    DOVirtual.DelayedCall(0.5f, () => _uiService.ShowPopup<TutorialCompletedPresenter>());
                });
                return;
            }

            _hintLoopTween?.Kill();
            _tutorialMoveRestrictionService?.ClearCurrentMove();

            if (isCorrectMove)
            {
                AdvanceStepIfNeeded();
            }

            _currentPresenter = null;
            _currentTargetContainer = null;
            PrepareNextStep();
        }

        HintMovement GetCurrentTutorialMovement()
        {
            if (_tutorialConfig == null || _tutorialConfig.steps == null || _tutorialConfig.steps.Count == 0)
                return null;

            if (_currentStepIndex < 0 || _currentStepIndex >= _tutorialConfig.steps.Count)
                return null;

            var step = _tutorialConfig.steps[_currentStepIndex];
            if (step == null || step.movements == null || step.movements.Count == 0)
                return null;

            if (_currentStepMoveIndex < 0 || _currentStepMoveIndex >= step.movements.Count)
                return null;

            var config = step.movements[_currentStepMoveIndex];
            return BuildMovementFromConfig(config);
        }

        HintMovement BuildMovementFromConfig(TutorialMovementConfig config)
        {
            if (config == null)
                return null;

            var board = View.Board;
            if (board == null)
                return null;

            CardPresenter presenter = null;
            CardContainer fromContainer = null;

            bool Matches(CardPresenter p)
            {
                if (p == null) return false;
                var model = p.CardModel;
                if (model == null) return false;
                if (config.isCategory && model.Type != CardType.Category) return false;
                if (!config.isCategory && model.Type != CardType.Content) return false;
                if (!string.IsNullOrEmpty(config.cardName) && model.ContentName != config.cardName && model.CategoryName != config.cardName) return false;
                return true;
            }

            var dealer = board.Dealer;
            if (dealer != null)
            {
                var cards = dealer.GetAllCards();
                for (var i = 0; i < cards.Count; i++)
                {
                    if (!Matches(cards[i])) continue;
                    presenter = cards[i];
                    fromContainer = dealer;
                    break;
                }
            }

            if (presenter == null)
            {
                var openDealer = board.OpenDealer;
                if (openDealer != null)
                {
                    var cards = openDealer.GetAllCards();
                    for (var i = 0; i < cards.Count; i++)
                    {
                        if (!Matches(cards[i])) continue;
                        presenter = cards[i];
                        fromContainer = openDealer;
                        break;
                    }
                }
            }

            if (presenter == null)
            {
                foreach (var pileContainer in board.Piles)
                {
                    if (pileContainer == null) continue;
                    var cards = pileContainer.GetAllCards();
                    for (var i = 0; i < cards.Count; i++)
                    {
                        if (!Matches(cards[i])) continue;
                        presenter = cards[i];
                        fromContainer = pileContainer;
                        break;
                    }

                    if (presenter != null)
                        break;
                }
            }

            if (presenter == null)
            {
                foreach (var foundation in board.Foundations)
                {
                    if (foundation == null) continue;
                    var cards = foundation.GetAllCards();
                    for (var i = 0; i < cards.Count; i++)
                    {
                        if (!Matches(cards[i])) continue;
                        presenter = cards[i];
                        fromContainer = foundation;
                        break;
                    }

                    if (presenter != null)
                        break;
                }
            }

            if (presenter == null)
                return null;

            CardContainer toContainer = null;
            switch (config.toContainerType)
            {
                case TutorialContainerType.Dealer:
                    toContainer = board.Dealer;
                    break;
                case TutorialContainerType.OpenDealer:
                    toContainer = board.OpenDealer;
                    break;
                case TutorialContainerType.Foundation:
                    if (config.toContainerIndex >= 0 && config.toContainerIndex < board.Foundations.Count)
                        toContainer = board.Foundations[config.toContainerIndex];
                    break;
                case TutorialContainerType.Pile:
                    if (config.toContainerIndex >= 0 && config.toContainerIndex < board.Piles.Count)
                        toContainer = board.Piles[config.toContainerIndex];
                    break;
            }

            if (toContainer == null)
                return null;

            //var stack = new List<CardPresenter> { presenter };
            var pile = fromContainer as Pile;
            var stack = pile != null
                ? pile.GetCardsFrom(presenter)
                : new List<CardPresenter> { presenter };

            var movement = new HintMovement(fromContainer, toContainer, stack, true);
            return movement;
        }

        void AdvanceStepIfNeeded()
        {
            if (_tutorialConfig == null || _tutorialConfig.steps == null || _tutorialConfig.steps.Count == 0)
                return;

            if (_currentStepIndex < 0 || _currentStepIndex >= _tutorialConfig.steps.Count)
                return;

            var step = _tutorialConfig.steps[_currentStepIndex];
            if (step == null)
                return;

            var totalMovements = step.movements.Count;
            if (totalMovements <= 0)
                return;

            _currentStepMoveIndex++;
            if (_currentStepMoveIndex < totalMovements)
                return;

            _currentStepIndex++;
            _currentStepMoveIndex = 0;
            UpdateInstructionText();
        }

        void UpdateInstructionText()
        {
            if (_tutorialConfig == null || _tutorialConfig.steps == null || _tutorialConfig.steps.Count == 0)
                return;

            if (_currentStepIndex < 0 || _currentStepIndex >= _tutorialConfig.steps.Count)
                return;

            var step = _tutorialConfig.steps[_currentStepIndex];
            if (step == null)
                return;

            View.ShowInstructionMessage(step.instructionText);
        }
    }
}
