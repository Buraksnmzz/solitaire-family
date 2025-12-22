using System.Collections.Generic;
using Card;
using DG.Tweening;
using UI.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Foundation : CardContainer
    {
        private readonly float _completeScaleUpDuration = 0.3f;
        private readonly float _completeScaleDownDuration = 0.4f;
        [SerializeField] private Image glowImage;
        [SerializeField] private ParticleSystem completeParticle;

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
                cardPresenter.MoveToLocalPosition(targetLocalPosition, MoveDuration, 0, Ease.OutQuad, () =>
                {
                    EventDispatcherService.Dispatch(new CardMovementStateChangedSignal(false));
                });
                var isCompleted = TryCollectCompletedPresenters(out var presentersToRemove);
                if (isCompleted)
                {
                    DOVirtual.DelayedCall(MoveDuration, () => CheckAndHandleCompletion(presentersToRemove));
                }
            }

            OnCardAdded(null, cardPresenter);
            UpdateLastCardContentCountText();
            TryUpdateCategoryAndTopContentStatesForStackCase();
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

        void TryUpdateCategoryAndTopContentStatesForStackCase()
        {
            if (CardPresenters.Count < 2)
                return;

            var lastIndex = CardPresenters.Count - 1;
            var lastPresenter = CardPresenters[lastIndex];
            var previousPresenter = CardPresenters[lastIndex - 1];
            var lastModel = lastPresenter.CardModel;
            if (lastModel.Type == CardType.Content) return;

            lastPresenter.CardView.transform.SetAsFirstSibling();

            CardPresenters.RemoveAt(lastIndex);
            CardPresenters.Insert(0, lastPresenter);

            lastPresenter.ApplyViewState(CardViewState.CategoryBelowWithCategoryInfo);
            previousPresenter.ApplyViewState(previousPresenter.CardModel.CategoryType == Levels.CardCategoryType.Text
                ? CardViewState.ContentTextTopWithCategoryInfo
                : CardViewState.ContentImageTopWithCategoryInfo);

            UpdateLastCardContentCountText();
        }

        private void CheckAndHandleCompletion(List<CardPresenter> presentersToRemove)
        {
            if (presentersToRemove == null || presentersToRemove.Count == 0) return;

            glowImage.transform.SetAsLastSibling();
            var glowSequence = DOTween.Sequence();

            glowSequence.Append(glowImage.DOFade(1f, _completeScaleUpDuration/2));
            glowSequence.Join(glowImage.transform.DOScale(Vector3.one * 1.14f, _completeScaleUpDuration));
            glowSequence.Append(glowImage.transform.DOScale(Vector3.one, _completeScaleUpDuration));
            glowSequence.Append(glowImage.DOFade(0f, _completeScaleDownDuration));

            DOVirtual.DelayedCall(_completeScaleUpDuration + _completeScaleDownDuration, () => completeParticle.Play());
            foreach (var presenter in presentersToRemove)
            {
                if (presenter.CardView == null) continue;

                var cardTransform = presenter.CardView.transform;
                var sequence = DOTween.Sequence();

                sequence.Append(cardTransform
                    .DOScale(Vector3.one * 1.14f, _completeScaleUpDuration)
                    .SetEase(Ease.OutQuad));

                sequence.Append(cardTransform
                    .DOScale(Vector3.one * 1f, _completeScaleUpDuration)
                    .SetEase(Ease.OutQuad));

                sequence.Append(cardTransform
                    .DOScale(Vector3.zero, _completeScaleDownDuration));

                sequence.OnComplete(() => Destroy(cardTransform.gameObject));
            }
        }

        private bool TryCollectCompletedPresenters(out List<CardPresenter> presentersToRemove)
        {
            presentersToRemove = null;

            if (CardPresenters.Count == 0) return false;

            var categoryCard = CardPresenters[0].CardModel;
            //if (categoryCard.Type != CardType.Category) return false;

            var contentTotal = categoryCard.ContentCount;
            if (contentTotal <= 0) return false;

            var currentCount = CardPresenters.Count - 1;
            if (currentCount < contentTotal) return false;

            presentersToRemove = new List<CardPresenter>(CardPresenters);
            CardPresenters.Clear();
            return true;
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