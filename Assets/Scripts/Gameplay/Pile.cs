using Card;
using System.Collections.Generic;
using UI.Signals;
using UnityEngine;

namespace Gameplay
{
    public class Pile : CardContainer
    {
        private const float DefaultGapRatio = 0.28f;
        private const float MinGapRatio = 0.08f;
        private const float RectOverlapEpsilon = 0.001f;

        private readonly List<float> _gapRatios = new();
        private RectTransform _bottomPanelRectTransform;

        public override Vector3 GetCardLocalPosition(int index)
        {
            EnsureGapRatiosSize();

            if (index <= 0)
            {
                return Vector3.zero;
            }

            var rectTransform = transform as RectTransform;
            if (rectTransform == null)
            {
                return Vector3.zero;
            }

            var width = rectTransform.rect.width;
            var totalY = 0f;
            for (var i = 0; i < index; i++)
            {
                var gapRatio = i < _gapRatios.Count ? _gapRatios[i] : DefaultGapRatio;
                totalY += -width * gapRatio;
            }

            return new Vector3(0f, totalY, 0f);
        }

        public void ConfigureDynamicSpacing(RectTransform bottomPanelRectTransform)
        {
            _bottomPanelRectTransform = bottomPanelRectTransform;
        }

        public override void AddCard(CardPresenter cardPresenter, float delay = 0, float moveDuration = 0.25f)
        {
            var previousTop = GetTopCardPresenter();
            CardPresenters.Add(cardPresenter);
            var index = CardPresenters.IndexOf(cardPresenter);
            EnsureGapRatiosSize();

            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);
            var targetLocalPosition = GetCardLocalPosition(index);

            cardPresenter.MoveToLocalPosition(targetLocalPosition, moveDuration, delay, DG.Tweening.Ease.OutQuad, () =>
            {
                EventDispatcherService.Dispatch(new CardMovementStateChangedSignal(false));
                TryCompressIfOverlapping();
            });

            if (cardPresenter.CardView != null)
            {
                cardPresenter.CardView.transform.SetAsLastSibling();
            }

            OnCardAdded(previousTop, cardPresenter);
        }

        public override CardPresenter RemoveCard(CardPresenter cardPresenter)
        {
            var removedPresenter = base.RemoveCard(cardPresenter);
            EnsureGapRatiosSize();
            TryExpandAfterRemove();
            return removedPresenter;
        }

        public override void RemoveCardsFrom(CardPresenter startPresenter)
        {
            base.RemoveCardsFrom(startPresenter);
            EnsureGapRatiosSize();
            TryExpandAfterRemove();
        }

        public override void ClearAllCards()
        {
            base.ClearAllCards();
            _gapRatios.Clear();
        }

        private void TryCompressIfOverlapping()
        {
            if (_bottomPanelRectTransform == null) return;
            if (CardPresenters.Count <= 1) return;

            EnsureGapRatiosSize();
            if (!IsBottomCardOverlapping()) return;

            for (var gapIndex = 0; gapIndex < _gapRatios.Count; gapIndex++)
            {
                if (!IsGapBetweenFaceDownCards(gapIndex))
                    continue;

                _gapRatios[gapIndex] = MinGapRatio;
                ApplyCurrentCardPositions();

                if (!IsBottomCardOverlapping())
                    return;
            }
        }

        private void TryExpandAfterRemove()
        {
            if (_bottomPanelRectTransform == null) return;
            if (_gapRatios.Count == 0) return;

            EnsureGapRatiosSize();

            for (var gapIndex = _gapRatios.Count - 1; gapIndex >= 0; gapIndex--)
            {
                if (!Mathf.Approximately(_gapRatios[gapIndex], MinGapRatio))
                    continue;

                _gapRatios[gapIndex] = DefaultGapRatio;
                ApplyCurrentCardPositions();

                if (IsBottomCardOverlapping())
                {
                    _gapRatios[gapIndex] = MinGapRatio;
                    ApplyCurrentCardPositions();
                    return;
                }
            }
        }

        private bool IsGapBetweenFaceDownCards(int gapIndex)
        {
            if (gapIndex < 0) return false;
            if (gapIndex + 1 >= CardPresenters.Count) return false;

            var upper = CardPresenters[gapIndex];
            var lower = CardPresenters[gapIndex + 1];
            if (upper == null || lower == null) return false;

            return !upper.IsFaceUp && !lower.IsFaceUp;
        }

        private void EnsureGapRatiosSize()
        {
            var required = Mathf.Max(0, CardPresenters.Count - 1);
            if (_gapRatios.Count > required)
            {
                _gapRatios.RemoveRange(required, _gapRatios.Count - required);
                return;
            }

            while (_gapRatios.Count < required)
            {
                _gapRatios.Add(DefaultGapRatio);
            }
        }

        private void ApplyCurrentCardPositions()
        {
            for (var i = 0; i < CardPresenters.Count; i++)
            {
                var presenter = CardPresenters[i];
                if (presenter == null) continue;
                presenter.SetLocalPosition(GetCardLocalPosition(i));
            }
        }

        private bool IsBottomCardOverlapping()
        {
            if (_bottomPanelRectTransform == null) return false;
            if (CardPresenters.Count == 0) return false;

            var bottomPresenter = CardPresenters[^1];
            if (bottomPresenter == null) return false;
            if (bottomPresenter.CardView == null) return false;

            var cardRectTransform = bottomPresenter.CardView.transform as RectTransform;
            if (cardRectTransform == null) return false;

            return RectsOverlapInclusive(GetWorldRect(cardRectTransform), GetWorldRect(_bottomPanelRectTransform));
        }

        private static Rect GetWorldRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            var xMin = corners[0].x;
            var xMax = corners[0].x;
            var yMin = corners[0].y;
            var yMax = corners[0].y;

            for (var i = 1; i < 4; i++)
            {
                var c = corners[i];
                if (c.x < xMin) xMin = c.x;
                if (c.x > xMax) xMax = c.x;
                if (c.y < yMin) yMin = c.y;
                if (c.y > yMax) yMax = c.y;
            }

            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private static bool RectsOverlapInclusive(Rect a, Rect b)
        {
            var aMinX = a.xMin - RectOverlapEpsilon;
            var aMaxX = a.xMax + RectOverlapEpsilon;
            var aMinY = a.yMin - RectOverlapEpsilon;
            var aMaxY = a.yMax + RectOverlapEpsilon;

            var bMinX = b.xMin - RectOverlapEpsilon;
            var bMaxX = b.xMax + RectOverlapEpsilon;
            var bMinY = b.yMin - RectOverlapEpsilon;
            var bMaxY = b.yMax + RectOverlapEpsilon;

            return aMinX <= bMaxX && aMaxX >= bMinX && aMinY <= bMaxY && aMaxY >= bMinY;
        }

        public bool CanDrag(CardPresenter presenter)
        {
            var index = CardPresenters.IndexOf(presenter);
            if (index == -1) return false;
            if (!presenter.IsFaceUp) return false;

            var jokerIndex = CardPresenters.FindIndex(x => x.CardModel.Type == CardType.Joker);
            if (jokerIndex == -1) return true;
            if (index < jokerIndex) return false;

            var hasCardAfterJoker = CardPresenters.Count > jokerIndex + 1;
            if (hasCardAfterJoker)
            {
                return index > jokerIndex;
            }

            return index == jokerIndex;
        }

        protected override void OnCardAdded(CardPresenter previousTop, CardPresenter newTop)
        {
            if (previousTop != null)
            {
                var previousModel = previousTop.CardModel;
                if (previousModel.Type == CardType.Joker)
                {
                    previousTop.ApplyViewState(CardViewState.JokerBelow);
                    return;
                }

                if (previousModel.Type == CardType.Category)
                {
                    previousTop.ApplyViewState(CardViewState.CategoryBelowWithTopText);
                }
                if (previousModel.CategoryType == Levels.CardCategoryType.Text)
                {
                    previousTop.ApplyViewState(CardViewState.ContentTextBelowWithUpInfo);
                }
                else
                {
                    previousTop.ApplyViewState(CardViewState.ContentImageBelowWithUpInfo);
                }
            }

            ApplyTopCardState(newTop);
        }

        protected override void OnTopCardChangedAfterRemove(CardPresenter newTop)
        {
            if (newTop == null) return;
            ApplyTopCardState(newTop);
        }

        void ApplyTopCardState(CardPresenter presenter)
        {
            var model = presenter.CardModel;

            if (model.Type == CardType.Joker)
            {
                presenter.ApplyViewState(CardViewState.JokerTop);
                return;
            }

            if (model.Type == CardType.Category)
            {
                presenter.ApplyViewState(CardViewState.CategoryTop);
                return;
            }

            if (model.CategoryType == Levels.CardCategoryType.Text)
            {
                presenter.ApplyViewState(CardViewState.ContentTextTopNoCategoryInfo);
            }
            else
            {
                presenter.ApplyViewState(CardViewState.ContentImageTopNoCategoryInfo);
            }
        }

    }
}