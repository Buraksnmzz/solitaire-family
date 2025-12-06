using System.Collections.Generic;
using Card;
using DG.Tweening;
using UI.Signals;
using UnityEngine;

namespace Gameplay
{
    public class Foundation : CardContainer
    {
        [SerializeField] private float completeScaleDuration = 0.25f;

        public override Vector3 GetCardLocalPosition(int index)
        {
            return Vector3.zero;
        }

        public override void AddCard(CardPresenter cardPresenter)
        {
            var index = CardPresenters.Count;
            CardPresenters.Add(cardPresenter);
            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);

            var targetLocalPosition = GetCardLocalPosition(index);

            if (cardPresenter.CardView != null)
            {
                cardPresenter.CardView.transform.SetAsLastSibling();
                cardPresenter.MoveToLocalPosition(targetLocalPosition, MoveDuration);
                ClearPresentersIfCompleted(out var presentersToRemove);
                DOVirtual.DelayedCall(MoveDuration, ()=>CheckAndHandleCompletion(presentersToRemove));
            }

            OnCardAdded(null, cardPresenter);

            UpdateLastCardContentCountText();
        }

        protected override void OnCardAdded(CardPresenter previousTop, CardPresenter newTop)
        {
            var model = newTop.CardModel;

            if (model.Type == CardType.Category)
            {
                newTop.ApplyViewState(CardViewState.CategoryTop);
                return;
            }

            if (model.Type == CardType.Content)
            {
                if (model.CategoryType == Levels.CardCategoryType.Text)
                {
                    newTop.ApplyViewState(CardViewState.ContentTextTopWithCategoryInfo);
                }
                else
                {
                    newTop.ApplyViewState(CardViewState.ContentImageTopWithCategoryInfo);
                }

                var categoryPresenter = FindCategoryPresenter();
                if (categoryPresenter != null)
                {
                    categoryPresenter.ApplyViewState(CardViewState.CategoryBelowWithCategoryInfo);
                }
            }
        }

        protected override void OnTopCardChangedAfterRemove(CardPresenter newTop)
        {
            var model = newTop.CardModel;

            if (model.Type == CardType.Category)
            {
                newTop.ApplyViewState(CardViewState.CategoryTop);
            }
        }

        CardPresenter FindCategoryPresenter()
        {
            foreach (var presenter in CardPresenters)
            {
                if (presenter.CardModel.Type == CardType.Category)
                {
                    return presenter;
                }
            }

            return null;
        }

        private void CheckAndHandleCompletion(List<CardPresenter> presentersToRemove)
        {
            //if (ClearPresentersIfCompleted(out var presentersToRemove)) return;

            foreach (var presenter in presentersToRemove)
            {
                if (presenter.CardView == null) continue;

                var cardTransform = presenter.CardView.transform;
                cardTransform.DOScale(Vector3.zero, completeScaleDuration)
                    .OnComplete(() => Destroy(cardTransform.gameObject));
            }
        }

        private bool ClearPresentersIfCompleted(out List<CardPresenter> presentersToRemove)
        {
            if (CardPresenters.Count == 0)
            {
                presentersToRemove = null;
                return true;
            }

            var categoryCard = CardPresenters[0].CardModel;
            if (categoryCard.Type != CardType.Category)
            {
                presentersToRemove = null;
                return true;
            }

            var contentTotal = categoryCard.ContentCount;
            if (contentTotal <= 0)
            {
                presentersToRemove = null;
                return true;
            }

            var currentCount = CardPresenters.Count - 1;
            if (currentCount < contentTotal)
            {
                presentersToRemove = null;
                return true;
            }

            presentersToRemove = new List<CardPresenter>(CardPresenters);
            CardPresenters.Clear();
            return false;
        }

        private void UpdateLastCardContentCountText()
        {
            if (CardPresenters.Count == 0) return;

            var categoryModel = CardPresenters[0].CardModel;
            if (categoryModel.Type != CardType.Category) return;

            var totalContentCount = categoryModel.ContentCount;
            if (totalContentCount <= 0) return;

            var currentContentCount = 0;
            for (var i = 1; i < CardPresenters.Count; i++)
            {
                if (CardPresenters[i].CardModel.Type == CardType.Content)
                {
                    currentContentCount++;
                }
            }

            var lastPresenter = CardPresenters[^1];
            lastPresenter.SetContentCount(currentContentCount, totalContentCount);
        }
    }
}