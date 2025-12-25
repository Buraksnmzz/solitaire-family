using System.Collections.Generic;
using System.Linq;
using Card;
using DG.Tweening;
using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Services.Hint
{
    public class HintService : IHintService
    {
        private Sequence _sequence;
        private readonly List<GameObject> _activeCopies = new();
        private Image _handImage;

        public List<HintMovement> GetPlayableMovements(Board board)
        {
            var movements = new List<HintMovement>();
            if (board == null) return movements;

            AddOpenDealerMovements(board, movements);
            AddPileMovements(board, movements);
            AddRevealMovement(board, movements);
            movements.RemoveAll(m => m != null
                                      && m.FromPile
                                      && m.LeavesColumnEmpty
                                      && m.ToContainer is Pile targetPile
                                      && targetPile.GetCardsCount() == 0);

            movements.RemoveAll(IsRedundantSubStackPileMove);

            return movements;
        }

        private bool IsRedundantSubStackPileMove(HintMovement movement)
        {
            if (movement == null) return false;
            if (!movement.FromPile) return false;
            if (!movement.IsStandardMove) return false;

            var fromPile = movement.FromContainer as Pile;
            if (fromPile == null) return false;

            var toPile = movement.ToContainer as Pile;
            if (toPile == null) return false;

            var startPresenter = movement.Presenters?.FirstOrDefault();
            if (startPresenter == null) return false;
            if (!startPresenter.IsFaceUp) return false;

            var startModel = startPresenter.CardModel;
            if (startModel == null) return false;
            if (startModel.Type != CardType.Content) return false;
            if (string.IsNullOrEmpty(startModel.CategoryName)) return false;

            var cards = fromPile.GetAllCards();
            var startIndex = GetIndex(cards, startPresenter);
            if (startIndex <= 0) return false;

            for (var i = startIndex - 1; i >= 0; i--)
            {
                var abovePresenter = cards[i];
                if (abovePresenter == null) break;
                if (!abovePresenter.IsFaceUp) break;

                var aboveModel = abovePresenter.CardModel;
                if (aboveModel == null) break;
                if (aboveModel.Type != CardType.Content) break;
                if (aboveModel.CategoryName != startModel.CategoryName) break;

                if (!fromPile.CanDrag(abovePresenter))
                {
                    continue;
                }

                return movement.ToContainer.CanPlaceCardSilently(abovePresenter);
            }

            return false;
        }

        private int GetIndex(IReadOnlyList<CardPresenter> cards, CardPresenter presenter)
        {
            if (cards == null || presenter == null) return -1;

            for (var i = 0; i < cards.Count; i++)
            {
                if (cards[i] == presenter) return i;
            }

            return -1;
        }

        public HintMovement GetBestMovement(Board board)
        {
            var movements = GetPlayableMovements(board);
            if (movements.Count == 0) return null;

            var visiblePercentages = CalculateVisiblePercentages(board);

            var movement = movements.FirstOrDefault(x => x.ToFoundation && x.IsStandardMove && x.CompletesFoundation);
            if (movement != null)
            {
                movement.Priority = HintPriority.CompleteFoundation;
                return movement;
            }

            movement = movements.FirstOrDefault(x => x.FromPile && x.IsStandardMove && x.LeavesColumnEmpty && !(x.ToContainer is Pile targetPile && targetPile.GetCardsCount() == 0));
            if (movement != null)
            {
                movement.Priority = HintPriority.EmptyColumn;
                return movement;
            }

            movement = movements
                .Where(x => x.ToFoundation && x.IsStandardMove)
                .OrderByDescending(x => x.Presenters?.Count ?? 0)
                .FirstOrDefault();
            if (movement != null)
            {
                movement.Priority = HintPriority.FoundationStandard;
                return movement;
            }

            var categoryFoundation = movements
                .Where(x => x.ToFoundation && x.IsCategoryMove)
                .OrderByDescending(x => GetVisiblePercentage(visiblePercentages, x.CategoryName))
                .FirstOrDefault();
            if (categoryFoundation != null)
            {
                categoryFoundation.Priority = HintPriority.FoundationCategory;
                return categoryFoundation;
            }

            movement = movements.FirstOrDefault(x => x.IsCategoryMove && x.TargetsFullGroup);
            if (movement != null)
            {
                movement.Priority = HintPriority.CategoryOnFullGroup;
                return movement;
            }

            var mergeMovement = GetMergeGroupsMovement(board);
            if (mergeMovement != null)
            {
                mergeMovement.Priority = HintPriority.MergeGroups;
                return mergeMovement;
            }

            movement = movements.FirstOrDefault(x => x.FromPile && !x.ToFoundation && x.IsStandardMove);
            if (movement != null)
            {
                movement.Priority = HintPriority.ColumnStandard;
                return movement;
            }

            movement = movements.FirstOrDefault(x => x.FromOpenDealer && x.ToFoundation && x.IsStandardMove);
            if (movement != null)
            {
                movement.Priority = HintPriority.FromOpenDealer;
                return movement;
            }

            movement = movements.FirstOrDefault(x => x.FromOpenDealer);
            if (movement != null)
            {
                movement.Priority = HintPriority.FromOpenDealer;
                return movement;
            }

            movement = movements.FirstOrDefault(x => x.IsReveal);
            if (movement != null)
            {
                movement.Priority = HintPriority.RevealDealer;
                return movement;
            }
            return movements.FirstOrDefault();
        }

        public void ShowHint(Board board, bool showHand = false, Image handImage = null, float moveDuration = 0.8f, float fadeDuration = 0.35f)
        {
            var movement = GetBestMovement(board);
            if (movement == null) return;
            ShowHintForMovement(board, movement, showHand, moveDuration, fadeDuration);
        }

        public void ShowHintForMovement(Board board, HintMovement movement, bool showHand = false, float moveDuration = 0.8f, float fadeDuration = 0.35f)
        {
            CleanupAnimation();

            if (movement == null)
                return;

            if (movement.IsReveal && movement.FromContainer is Dealer dealer)
            {
                dealer.PlayHintCue();
                return;
            }

            PlayAnimation(movement, board, showHand, moveDuration, fadeDuration);
        }

        private void AddOpenDealerMovements(Board board, List<HintMovement> movements)
        {
            var openDealer = board.OpenDealer;
            if (openDealer == null) return;

            var presenter = openDealer.GetTopCardPresenter();
            if (presenter == null) return;

            var stack = new List<CardPresenter> { presenter };

            foreach (var foundation in board.Foundations)
            {
                if (foundation == null) continue;
                if (!foundation.CanPlaceCardSilently(presenter)) continue;
                movements.Add(CreateMovement(openDealer, foundation, stack));
            }

            foreach (var pileContainer in board.Piles)
            {
                if (pileContainer == null) continue;
                if (pileContainer == openDealer) continue;
                if (!pileContainer.CanPlaceCardSilently(presenter)) continue;
                movements.Add(CreateMovement(openDealer, pileContainer, stack));
            }
        }

        private void AddPileMovements(Board board, List<HintMovement> movements)
        {
            foreach (var pileContainer in board.Piles)
            {
                if (!pileContainer.gameObject.activeSelf)
                    return;
                var pile = pileContainer as Pile;
                if (pile == null) continue;

                var cards = pile.GetAllCards();
                for (var i = 0; i < cards.Count; i++)
                {
                    var presenter = cards[i];
                    if (!pile.CanDrag(presenter)) continue;

                    var stack = pile.GetCardsFrom(presenter);
                    if (stack.Count == 0) continue;

                    foreach (var foundation in board.Foundations)
                    {
                        if (foundation == null) continue;
                        if (!foundation.CanPlaceCardSilently(presenter)) continue;
                        movements.Add(CreateMovement(pile, foundation, stack));
                    }

                    foreach (var targetPileContainer in board.Piles)
                    {
                        if (targetPileContainer == null) continue;
                        if (targetPileContainer == pile) continue;
                        if (!targetPileContainer.CanPlaceCardSilently(presenter)) continue;
                        movements.Add(CreateMovement(pile, targetPileContainer, stack));
                    }
                }
            }
        }

        private void AddRevealMovement(Board board, List<HintMovement> movements)
        {
            var dealer = board.Dealer;
            var openDealer = board.OpenDealer;
            if (dealer == null || openDealer == null) return;
            // If neither dealer nor openDealer have any card that can be placed anywhere,
            // then showing a dealer hint (reveal cue) is pointless — don't add reveal movement.
            // We consider all cards in dealer and openDealer as potential candidates (treat dealer
            // cards as if they were face-up) and check silently whether any of them can be placed.

            var dealerCards = dealer.GetAllCards();
            var openCards = openDealer.GetAllCards();

            // If there are no cards anywhere, nothing to do
            if ((dealerCards == null || dealerCards.Count == 0) && (openCards == null || openCards.Count == 0))
                return;

            bool anyPlacable = false;

            // Helper to check a presenter against all possible targets
            bool CheckPresenterPlacable(CardPresenter presenter)
            {
                if (presenter == null) return false;

                // Check foundations
                foreach (var foundation in board.Foundations)
                {
                    if (foundation == null) continue;
                    if (foundation.CanPlaceCardSilently(presenter)) return true;
                }

                // Check piles (exclude openDealer container itself)
                foreach (var pileContainer in board.Piles)
                {
                    if (pileContainer == null) continue;
                    if (pileContainer == openDealer) continue;
                    if (pileContainer.CanPlaceCardSilently(presenter)) return true;
                }

                return false;
            }

            // Check dealer cards first (treating them as if they could become face-up)
            if (dealerCards != null)
            {
                foreach (var p in dealerCards)
                {
                    if (CheckPresenterPlacable(p))
                    {
                        anyPlacable = true;
                        break;
                    }
                }
            }

            // If none found yet, check openDealer cards
            if (!anyPlacable && openCards != null)
            {
                foreach (var p in openCards)
                {
                    if (CheckPresenterPlacable(p))
                    {
                        anyPlacable = true;
                        break;
                    }
                }
            }

            if (!anyPlacable)
            {
                // No card in dealer or openDealer can be placed anywhere — no reveal hint needed
                return;
            }

            // At least one card could be placed after dealing — add a reveal movement so the
            // dealer hint cue will be shown. If dealer currently has cards, show cue for dealer;
            // otherwise, use an openDealer presenter as the representative stack.
            CardPresenter presenterToUse = null;
            if (dealer.GetCardsCount() > 0)
            {
                presenterToUse = dealer.GetTopCardPresenter();
            }
            else
            {
                presenterToUse = openDealer.GetTopCardPresenter();
            }

            if (presenterToUse == null) return;

            var stack = new List<CardPresenter> { presenterToUse };
            movements.Add(CreateRevealMovement(dealer, openDealer, stack));
        }

        private HintMovement CreateMovement(CardContainer from, CardContainer to, List<CardPresenter> stack)
        {
            var presenters = new List<CardPresenter>(stack);
            var movement = new HintMovement(from, to, presenters, false)
            {
                ToFoundation = to is Foundation,
                FromPile = from is Pile,
                FromOpenDealer = from is OpenDealer,
                IsStandardMove = presenters[0].CardModel.Type == CardType.Content,
                IsCategoryMove = presenters[0].CardModel.Type == CardType.Category,
                LeavesColumnEmpty = from is Pile pile && pile.GetCardsCount() == presenters.Count,
                CompletesFoundation = to is Foundation && WillCompleteFoundation(to, presenters),
                CategoryName = presenters[0].CardModel.CategoryName,
                CategoryTotal = presenters[0].CardModel.ContentCount,
                TargetsFullGroup = to is Pile targetPile && IsFullGroup(targetPile, presenters[0].CardModel)
            };
            return movement;
        }

        private HintMovement CreateRevealMovement(CardContainer from, CardContainer to, List<CardPresenter> stack)
        {
            var presenters = new List<CardPresenter>(stack);
            var movement = new HintMovement(from, to, presenters, true)
            {
                IsStandardMove = presenters[0].CardModel.Type == CardType.Content,
                IsCategoryMove = presenters[0].CardModel.Type == CardType.Category,
                CategoryName = presenters[0].CardModel.CategoryName,
                CategoryTotal = presenters[0].CardModel.ContentCount
            };
            return movement;
        }

        private bool WillCompleteFoundation(CardContainer foundation, List<CardPresenter> stack)
        {
            var foundationCards = foundation.GetAllCards();
            var existingCategory = foundationCards.FirstOrDefault()?.CardModel;
            var stackCategory = stack.FirstOrDefault(x => x.CardModel.Type == CardType.Category)?.CardModel;
            var categoryModel = existingCategory ?? stackCategory;
            if (categoryModel == null) return false;

            var totalContent = categoryModel.ContentCount;
            if (totalContent <= 0) return false;

            var currentContent = foundationCards.Count(x => x.CardModel.Type == CardType.Content && x.CardModel.CategoryName == categoryModel.CategoryName);
            var incomingContent = stack.Count(x => x.CardModel.Type == CardType.Content && x.CardModel.CategoryName == categoryModel.CategoryName);
            var foundationMatches = foundationCards.All(x => x.CardModel.Type != CardType.Category || x.CardModel.CategoryName == categoryModel.CategoryName);
            var stackMatches = stack.All(x => x.CardModel.Type != CardType.Category || x.CardModel.CategoryName == categoryModel.CategoryName);
            if (!foundationMatches || !stackMatches) return false;

            return currentContent + incomingContent >= totalContent;
        }

        private bool IsFullGroup(Pile pile, CardModel model)
        {
            if (pile == null || model == null) return false;
            if (string.IsNullOrEmpty(model.CategoryName)) return false;
            if (model.ContentCount <= 0) return false;

            var cards = pile.GetAllCards();
            if (cards.Count == 0) return false;

            var sameCategory = cards.All(x => x.CardModel.CategoryName == model.CategoryName && x.CardModel.Type == CardType.Content);
            if (!sameCategory) return false;

            var contentCount = cards.Count(x => x.CardModel.Type == CardType.Content);
            return contentCount >= model.ContentCount;
        }

        private Dictionary<string, int> CalculateVisiblePercentages(Board board)
        {
            var totals = GetCategoryTotals(board);
            var visibleCounts = new Dictionary<string, int>();

            void AddVisible(IEnumerable<CardPresenter> presenters)
            {
                foreach (var presenter in presenters)
                {
                    var model = presenter.CardModel;
                    if (model.Type != CardType.Content) continue;
                    if (!presenter.IsFaceUp) continue;

                    if (!visibleCounts.ContainsKey(model.CategoryName))
                    {
                        visibleCounts[model.CategoryName] = 0;
                    }

                    visibleCounts[model.CategoryName] += 1;
                }
            }

            foreach (var pileContainer in board.Piles)
            {
                var pile = pileContainer as CardContainer;
                if (pile == null) continue;
                AddVisible(pile.GetAllCards());
            }

            foreach (var foundation in board.Foundations)
            {
                if (foundation == null) continue;
                AddVisible(foundation.GetAllCards());
            }

            var openDealer = board.OpenDealer;
            if (openDealer != null)
            {
                AddVisible(openDealer.GetAllCards());
            }

            var percentages = new Dictionary<string, int>();
            foreach (var total in totals)
            {
                var visible = visibleCounts.ContainsKey(total.Key) ? visibleCounts[total.Key] : 0;
                if (total.Value <= 0) continue;
                var percent = Mathf.RoundToInt((float)visible / total.Value * 100f);
                percentages[total.Key] = percent;
            }

            return percentages;
        }

        private Dictionary<string, int> GetCategoryTotals(Board board)
        {
            var totals = new Dictionary<string, int>();
            foreach (var model in board.CardModels)
            {
                if (model.Type != CardType.Content) continue;
                if (totals.ContainsKey(model.CategoryName)) continue;
                totals[model.CategoryName] = model.ContentCount;
            }

            return totals;
        }

        private int GetVisiblePercentage(Dictionary<string, int> percentages, string category)
        {
            if (string.IsNullOrEmpty(category)) return 0;
            return percentages.TryGetValue(category, out var value) ? value : 0;
        }

        private HintMovement GetMergeGroupsMovement(Board board)
        {
            var stacksByCategory = new Dictionary<string, List<StackInfo>>();

            foreach (var pileContainer in board.Piles)
            {
                var pile = pileContainer as Pile;
                if (pile == null) continue;
                var cards = pile.GetAllCards();
                foreach (var presenter in cards)
                {
                    if (!pile.CanDrag(presenter)) continue;
                    var stack = pile.GetCardsFrom(presenter);
                    if (stack.Count == 0) continue;
                    var model = stack[0].CardModel;
                    if (model.Type != CardType.Content) continue;
                    if (string.IsNullOrEmpty(model.CategoryName)) continue;
                    var sameCategory = stack.All(x => x.CardModel.Type == CardType.Content && x.CardModel.CategoryName == model.CategoryName);
                    if (!sameCategory) continue;

                    if (!stacksByCategory.ContainsKey(model.CategoryName))
                    {
                        stacksByCategory[model.CategoryName] = new List<StackInfo>();
                    }

                    stacksByCategory[model.CategoryName].Add(new StackInfo
                    {
                        Pile = pile,
                        Stack = stack
                    });
                    break;
                }
            }

            foreach (var stackGroup in stacksByCategory)
            {
                var stacks = stackGroup.Value;
                if (stacks.Count <= 2) continue;
                var ordered = stacks.OrderBy(x => x.Stack.Count).ToList();
                var smallest = ordered.First();
                var largest = ordered.Last();
                if (smallest.Pile == largest.Pile) continue;

                var presenter = smallest.Stack[0];
                if (!largest.Pile.CanPlaceCardSilently(presenter)) continue;

                return CreateMovement(smallest.Pile, largest.Pile, smallest.Stack);
            }

            return null;
        }

        private void PlayAnimation(HintMovement movement, Board board, bool showHand, float moveDuration, float fadeDuration)
        {
            
            CleanupAnimation();
            
            if (movement.Presenters == null || movement.Presenters.Count == 0) return;

            var parent = movement.Presenters[0].CardView != null
                ? movement.Presenters[0].CardView.transform.parent
                : null;

            var baseCount = movement.ToContainer != null ? movement.ToContainer.GetCardsCount() : 0;

            var targetPositions = new List<Vector3>();
            for (var i = 0; i < movement.Presenters.Count; i++)
            {
                var targetIndex = baseCount + i;
                var targetPosition = movement.ToContainer != null
                    ? GetTargetWorldPosition(movement.ToContainer, targetIndex)
                    : movement.Presenters[i].CardView != null
                        ? movement.Presenters[i].CardView.transform.position
                        : Vector3.zero;
                targetPositions.Add(targetPosition);
            }

            var copyData = new List<(RectTransform rect, CanvasGroup group)>();
            Vector3? firstBasePosition = null;
            var sourceOffsets = new List<Vector3>();

            for (var i = 0; i < movement.Presenters.Count; i++)
            {
                var presenter = movement.Presenters[i];
                var view = presenter.CardView;
                if (view == null) continue;

                var copy = Object.Instantiate(view, parent);
                copy.GetComponent<CardView>().SetRaycastTarget(false);
                var copyRect = copy.transform as RectTransform;
                var viewRect = view.transform as RectTransform;
                if (copyRect == null || viewRect == null)
                {
                    Object.Destroy(copy.gameObject);
                    continue;
                }

                if (showHand && i == movement.Presenters.Count-1)
                {
                    copy.SetHandVisible(true);
                }

                if (firstBasePosition == null)
                {
                    firstBasePosition = viewRect.position;
                }

                var sourceOffset = viewRect.position - firstBasePosition.Value;
                sourceOffsets.Add(sourceOffset);

                copyRect.position = firstBasePosition.Value + sourceOffset;
                copyRect.rotation = viewRect.rotation;
                copy.transform.SetParent(board.BoardParent);

                var dragger = copy.GetComponent<CardDragger>();
                if (dragger != null) dragger.enabled = false;
                var canvasGroup = copy.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = copy.gameObject.AddComponent<CanvasGroup>();
                }

                canvasGroup.alpha = 1f;

                _activeCopies.Add(copy.gameObject);
                copyData.Add((copyRect, canvasGroup));
            }

            if (copyData.Count == 0) return;

            _sequence = DOTween.Sequence();

            for (var i = 0; i < copyData.Count; i++)
            {
                var rootTarget = i < targetPositions.Count ? targetPositions[0] : copyData[i].rect.position;
                var offset = i < sourceOffsets.Count ? sourceOffsets[i] : Vector3.zero;
                var moveTarget = rootTarget + offset;
                _sequence.Join(copyData[i].rect.DOMove(moveTarget, moveDuration).SetEase(Ease.OutQuad));
            }

            var fadeSequence = DOTween.Sequence();
            for (var i = 0; i < copyData.Count; i++)
            {
                fadeSequence.Join(copyData[i].group.DOFade(0f, fadeDuration).SetEase(Ease.Linear));
            }

            _sequence.Append(fadeSequence);
            _sequence.OnComplete(CleanupAnimation);
        }

        private Vector3 GetTargetWorldPosition(CardContainer container, int targetIndex)
        {
            var rect = container.transform as RectTransform;
            if (rect == null) return container.transform.position;

            var predictedCount = targetIndex + 1;
            var visualIndex = targetIndex;

            var openDealer = container as OpenDealer;
            if (openDealer != null)
            {
                var startIndex = Mathf.Max(0, predictedCount - 3);
                visualIndex = predictedCount - 1 - startIndex;
            }

            var local = container.GetCardLocalPosition(visualIndex);
            return rect.TransformPoint(local);
        }

        private struct StackInfo
        {
            public Pile Pile;
            public List<CardPresenter> Stack;
        }

        private void CleanupAnimation()
        {
            if (_handImage != null)
            {
                _handImage.transform.SetParent(null);
                _handImage.gameObject.SetActive(false);
            }
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }

            if (_activeCopies.Count == 0) return;

            for (var i = 0; i < _activeCopies.Count; i++)
            {
                var copy = _activeCopies[i];
                if (copy != null)
                {
                    Object.Destroy(copy);
                }
            }

            _activeCopies.Clear();
        }
    }
}
