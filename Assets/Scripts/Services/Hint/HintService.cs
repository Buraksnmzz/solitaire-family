using System.Collections.Generic;
using System.Linq;
using Card;
using DG.Tweening;
using Gameplay;
using UnityEngine;

namespace Services.Hint
{
    public class HintService : IHintService
    {
        private Sequence _sequence;
        private readonly List<GameObject> _activeCopies = new();

        public List<HintMovement> GetPlayableMovements(Board board)
        {
            var movements = new List<HintMovement>();
            if (board == null) return movements;

            AddOpenDealerMovements(board, movements);
            AddPileMovements(board, movements);
            AddRevealMovement(board, movements);

            return movements;
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

            movement = movements.FirstOrDefault(x => x.FromPile && x.IsStandardMove && x.LeavesColumnEmpty);
            if (movement != null)
            {
                movement.Priority = HintPriority.EmptyColumn;
                return movement;
            }

            movement = movements.FirstOrDefault(x => x.ToFoundation && x.IsStandardMove);
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

            movement = movements.FirstOrDefault(x => x.IsReveal);
            if (movement != null)
            {
                movement.Priority = HintPriority.RevealDealer;
                return movement;
            }
            return movements.FirstOrDefault();

            return null;
        }

        public void ShowHint(Board board)
        {
            CleanupAnimation();
            var movement = GetBestMovement(board);
            if (movement == null) return;

            if (movement.IsReveal && movement.FromContainer is Dealer dealer)
            {
                dealer.PlayHintCue();
                return;
            }

            PlayAnimation(movement, board);
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
            if (dealer.GetCardsCount() == 0) return;

            var presenter = dealer.GetTopCardPresenter();
            if (presenter == null) return;

            var stack = new List<CardPresenter> { presenter };
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

        private void PlayAnimation(HintMovement movement, Board board)
        {
            CleanupAnimation();

            if (movement.Presenters == null || movement.Presenters.Count == 0) return;

            var parent = movement.Presenters[0].CardView != null
                ? movement.Presenters[0].CardView.transform.parent
                : null;

            var baseCount = movement.ToContainer != null ? movement.ToContainer.GetCardsCount() : 0;

            for (var i = 0; i < movement.Presenters.Count; i++)
            {
                var presenter = movement.Presenters[i];
                var view = presenter.CardView;
                if (view == null) continue;

                var copy = Object.Instantiate(view, parent);
                var copyRect = copy.transform as RectTransform;
                var viewRect = view.transform as RectTransform;
                if (copyRect == null || viewRect == null)
                {
                    Object.Destroy(copy.gameObject);
                    continue;
                }

                copyRect.position = viewRect.position;
                copyRect.rotation = viewRect.rotation;
                copy.transform.SetParent(board.BoardParent);

                var dragger = copy.GetComponent<CardDragger>();
                if (dragger != null) dragger.enabled = false;
                if (copy.GetComponent<CanvasGroup>() == null)
                    copy.gameObject.AddComponent<CanvasGroup>();

                var canvasGroup = copy.GetComponent<CanvasGroup>();
                canvasGroup.alpha = 1f;

                _activeCopies.Add(copy.gameObject);

                var targetIndex = baseCount + i;
                var targetPosition = movement.ToContainer != null
                    ? GetTargetWorldPosition(movement.ToContainer, targetIndex)
                    : copyRect.position;

                _sequence ??= DOTween.Sequence();
                _sequence.Append(copyRect.DOMove(targetPosition, 0.4f).SetEase(Ease.OutQuad));
                _sequence.Append(canvasGroup.DOFade(0f, 0.3f).SetDelay(0.3f));
            }

            if (_sequence == null) return;

            _sequence.OnComplete(() =>
            {
                CleanupAnimation();
            });
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
