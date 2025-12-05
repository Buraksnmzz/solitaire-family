using System.Collections.Generic;
using System.Linq;
using Card;
using Gameplay.PlacableRules;
using UnityEngine;

namespace Gameplay
{
    public abstract class CardContainer : MonoBehaviour
    {
        public List<CardPresenter> _cardPresenters = new();
        protected IPlacableRule PlacableRule;
        protected readonly float _moveDuration = 0.3f;
        protected readonly float _flipDuration = 0.1f;

        public abstract Vector3 GetCardLocalPosition(int index);

        public virtual void Setup(IPlacableRule placableRule)
        {
            PlacableRule = placableRule;
        }

        public CardPresenter GetTopCard() => _cardPresenters.LastOrDefault();

        public CardPresenter GetTopCardPresenter() => _cardPresenters.LastOrDefault();

        public bool CanPlaceCard(CardPresenter sourceCardPresenter)
        {
            var topCardModel = _cardPresenters.LastOrDefault()?.CardModel;
            return PlacableRule.IsPlaceable(topCardModel, sourceCardPresenter.CardModel);
        }

        public virtual void AddCard(CardPresenter cardPresenter)
        {
            _cardPresenters.Add(cardPresenter);
            var index = _cardPresenters.IndexOf(cardPresenter);

            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);

            var targetLocalPosition = GetCardLocalPosition(index);
            cardPresenter.MoveToLocalPosition(targetLocalPosition, _moveDuration);

            if (cardPresenter.CardView != null)
            {
                cardPresenter.CardView.transform.SetAsLastSibling();
            }
        }

        public virtual CardPresenter RemoveCard(CardPresenter cardPresenter)
        {
            var index = _cardPresenters.IndexOf(cardPresenter);
            if (index == -1) return null;
            var presenter = _cardPresenters[index];
            _cardPresenters.RemoveAt(index);
            return presenter;
        }

        public virtual List<CardPresenter> GetCardsFrom(CardPresenter startPresenter)
        {
            var index = _cardPresenters.IndexOf(startPresenter);
            if (index == -1) return new List<CardPresenter>();
            return _cardPresenters.Skip(index).ToList();
        }

        public virtual void RemoveCardsFrom(CardPresenter startPresenter)
        {
            var index = _cardPresenters.IndexOf(startPresenter);
            if (index == -1) return;
            _cardPresenters.RemoveRange(index, _cardPresenters.Count - index);
        }

        public virtual void RevealTopCardIfNeeded()
        {
            var topCardPresenter = GetTopCardPresenter();
            if (topCardPresenter == null) return;
            if (topCardPresenter.IsFaceUp) return;
            topCardPresenter.SetFaceUp(true, _flipDuration * 2.5f);
        }
    }
}