using System.Collections.Generic;
using Card;
using DG.Tweening;
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
            var index = _cardPresenters.Count;

            _cardPresenters.Add(cardPresenter);
            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);

            var targetLocalPosition = GetCardLocalPosition(index);

            if (cardPresenter.CardView != null)
            {
                cardPresenter.CardView.transform.SetAsLastSibling();
                cardPresenter.MoveToLocalPosition(targetLocalPosition, _moveDuration,
                    0f, Ease.OutQuad);
                DOVirtual.DelayedCall(_moveDuration, CheckAndHandleCompletion);
            }
            else
            {
                cardPresenter.MoveToLocalPosition(targetLocalPosition, 0f);
                CheckAndHandleCompletion();
            }
        }

        private void CheckAndHandleCompletion()
        {
            if (_cardPresenters.Count == 0) return;

            var categoryCard = _cardPresenters[0].CardModel;
            if (categoryCard.Type != CardType.Category) return;

            var contentTotal = categoryCard.ContentCount;
            if (contentTotal <= 0) return;

            var currentCount = _cardPresenters.Count - 1;
            if (currentCount < contentTotal) return;

            var presentersToRemove = new List<CardPresenter>(_cardPresenters);
            _cardPresenters.Clear();

            foreach (var presenter in presentersToRemove)
            {
                if (presenter.CardView == null) continue;

                var transform = presenter.CardView.transform;
                transform.DOScale(Vector3.zero, completeScaleDuration)
                    .OnComplete(() => Destroy(transform.gameObject));
            }
        }
    }
}