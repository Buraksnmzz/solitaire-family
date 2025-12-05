using System.Collections.Generic;
using Card;
using UnityEngine;

namespace Gameplay
{
    public class OpenDealer : CardContainer
    {
        private readonly float _distanceMultiplier = 0.368f;

        public override Vector3 GetCardLocalPosition(int index)
        {
            var cardRectTransform = (RectTransform)_cardPresenters[0].CardView.transform;
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
            _cardPresenters.Add(cardPresenter);

            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);

            var startIndex = Mathf.Max(0, _cardPresenters.Count - 3);

            for (var i = startIndex; i < _cardPresenters.Count; i++)
            {
                var targetLocalPosition = GetCardLocalPosition(i - startIndex);
                _cardPresenters[i].MoveToLocalPosition(targetLocalPosition, _moveDuration);
            }

            if (cardPresenter.CardView != null)
            {
                cardPresenter.CardView.transform.SetAsLastSibling();
            }
        }

        public List<CardPresenter> GetAllCardPresenters()
        {
            return _cardPresenters;
        }
    }
}