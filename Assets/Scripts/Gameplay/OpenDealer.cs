using System.Collections.Generic;
using Card;
using DG.Tweening;
using UI.Signals;
using UnityEngine;

namespace Gameplay
{
    public class OpenDealer : CardContainer
    {
        private readonly float _distanceMultiplier = 0.368f;

        public override Vector3 GetCardLocalPosition(int index)
        {
            var cardRectTransform = (RectTransform)CardPresenters[0].CardView.transform;
            var cardWidth = cardRectTransform.rect.width;
            var cardHeight = cardRectTransform.rect.height;
            var offsetX = cardWidth * _distanceMultiplier;

            if (index == 0)
            {
                return new Vector3(-cardWidth * 0.5f, -cardHeight * 0.5f, 0f);
            }

            if (index == 1)
            {
                return new Vector3(-cardWidth * 0.5f - offsetX, -cardHeight * 0.5f, 0f);
            }

            if (index == 2)
            {
                return new Vector3(-cardWidth * 0.5f - offsetX * 2f, -cardHeight * 0.5f, 0f);
            }

            var baseX = -cardWidth * 0.5f;
            var baseY = -cardHeight * 0.5f;

            if (index >= 3)
            {
                return new Vector3(baseX - offsetX * 2f, baseY, 0f);
            }

            return Vector3.zero;
        }

        public override void AddCard(CardPresenter cardPresenter)
        {
            var previousTop = GetTopCardPresenter();

            CardPresenters.Add(cardPresenter);

            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);

            var startIndex = Mathf.Max(0, CardPresenters.Count - 3);

            for (var i = startIndex; i < CardPresenters.Count; i++)
            {
                var targetLocalPosition = GetCardLocalPosition(i - startIndex);
                CardPresenters[i].MoveToLocalPosition(targetLocalPosition, MoveDuration, 0, Ease.OutQuad, () => EventDispatcherService.Dispatch(new CardMovementStateChangedSignal(false)));
            }

            if (cardPresenter.CardView != null)
            {
                cardPresenter.CardView.transform.SetAsLastSibling();
            }

            OnCardAdded(previousTop, cardPresenter);
        }

        protected override void OnCardAdded(CardPresenter previousTop, CardPresenter newTop)
        {
            if (previousTop != null)
            {
                var previousModel = previousTop.CardModel;

                if (previousModel.Type == CardType.Category)
                {
                    previousTop.ApplyViewState(CardViewState.CategoryBelowNoCategoryInfo);
                }
                else if (previousModel.CategoryType == Levels.CardCategoryType.Text)
                {
                    previousTop.ApplyViewState(CardViewState.ContentTextBelowWithSideInfo);
                }
                else
                {
                    previousTop.ApplyViewState(CardViewState.ContentImageBelowWithSideInfo);
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

        public List<CardPresenter> GetAllCardPresenters()
        {
            return CardPresenters;
        }

        public override CardPresenter RemoveCard(CardPresenter cardPresenter)
        {
            var removed = base.RemoveCard(cardPresenter);
            if (removed == null) return null;

            // reposition visible window (mirror AddCard)
            if (CardPresenters.Count > 0)
            {
                var startIndex = Mathf.Max(0, CardPresenters.Count - 3);
                for (var i = startIndex; i < CardPresenters.Count; i++)
                {
                    var relativeIndex = i - startIndex;
                    var targetLocalPosition = GetCardLocalPosition(relativeIndex);
                    CardPresenters[i].MoveToLocalPosition(targetLocalPosition, MoveDuration, 0, Ease.OutQuad, () => EventDispatcherService.Dispatch(new CardMovementStateChangedSignal(false)));
                }

                var top = GetTopCardPresenter();
                if (top != null && top.CardView != null)
                    top.CardView.transform.SetAsLastSibling();
            }

            return removed;
        }

        public override void RemoveCardsFrom(CardPresenter startPresenter)
        {
            base.RemoveCardsFrom(startPresenter);

            if (CardPresenters.Count == 0) return;

            var startIndex = Mathf.Max(0, CardPresenters.Count - 3);
            for (var i = startIndex; i < CardPresenters.Count; i++)
            {
                var relativeIndex = i - startIndex;
                var targetLocalPosition = GetCardLocalPosition(relativeIndex);
                CardPresenters[i].MoveToLocalPosition(targetLocalPosition, MoveDuration, 0, Ease.OutQuad, () => EventDispatcherService.Dispatch(new CardMovementStateChangedSignal(false)));
            }

            var top = GetTopCardPresenter();
            if (top != null && top.CardView != null)
                top.CardView.transform.SetAsLastSibling();
        }
    }
}