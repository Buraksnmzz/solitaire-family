using System.Collections.Generic;
using System.Linq;
using Card;
using Gameplay.PlacableRules;
using UI.Signals;
using UnityEngine;

namespace Gameplay
{
    public abstract class CardContainer : MonoBehaviour
    {
        protected List<CardPresenter> CardPresenters = new();
        private IPlacableRule _placableRule;
        protected readonly float MoveDuration = 0.3f;
        protected readonly float FlipDuration = 0.1f;
        protected IEventDispatcherService EventDispatcherService;

        public abstract Vector3 GetCardLocalPosition(int index);

        public virtual void Setup(IPlacableRule placableRule)
        {
            _placableRule = placableRule;
            EventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
        }

        public int GetCardsCount() => CardPresenters.Count;

        public CardPresenter GetTopCard() => CardPresenters.LastOrDefault();

        public CardPresenter GetTopCardPresenter() => CardPresenters.LastOrDefault();

        public bool CanPlaceCard(CardPresenter sourceCardPresenter)
        {
            var topCardModel = CardPresenters.LastOrDefault()?.CardModel;
            var isPlacable = _placableRule.IsPlaceable(topCardModel, sourceCardPresenter.CardModel);
            var placableError = _placableRule.ErrorMessage;
            if(!isPlacable && placableError != null)
                EventDispatcherService.Dispatch(new PlacableErrorSignal(placableError));
            return isPlacable;
        }

        public virtual void AddCard(CardPresenter cardPresenter)
        {
            var previousTop = GetTopCardPresenter();
            CardPresenters.Add(cardPresenter);
            var index = CardPresenters.IndexOf(cardPresenter);
            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);
            var targetLocalPosition = GetCardLocalPosition(index);
            cardPresenter.MoveToLocalPosition(targetLocalPosition, MoveDuration);

            if (cardPresenter.CardView != null)
            {
                cardPresenter.CardView.transform.SetAsLastSibling();
            }

            OnCardAdded(previousTop, cardPresenter);
        }

        public virtual CardPresenter RemoveCard(CardPresenter cardPresenter)
        {
            var index = CardPresenters.IndexOf(cardPresenter);
            if (index == -1) return null;
            var presenter = CardPresenters[index];
            var wasTop = index == CardPresenters.Count - 1;
            CardPresenters.RemoveAt(index);

            if (wasTop)
            {
                var newTop = GetTopCardPresenter();
                if (newTop != null)
                    OnTopCardChangedAfterRemove(newTop);
            }

            return presenter;
        }

        public virtual List<CardPresenter> GetCardsFrom(CardPresenter startPresenter)
        {
            var index = CardPresenters.IndexOf(startPresenter);
            if (index == -1) return new List<CardPresenter>();
            return CardPresenters.Skip(index).ToList();
        }

        public virtual List<CardPresenter> GetCardsBefore(CardPresenter startPresenter)
        {
            var index = CardPresenters.IndexOf(startPresenter);
            if (index <= 0) return new List<CardPresenter>();
            return CardPresenters.Take(index).ToList();
        }

        public virtual void RemoveCardsFrom(CardPresenter startPresenter)
        {
            var index = CardPresenters.IndexOf(startPresenter);
            if (index == -1) return;
            var wasTopAffected = index <= CardPresenters.Count - 1;
            CardPresenters.RemoveRange(index, CardPresenters.Count - index);

            if (wasTopAffected)
            {
                var newTop = GetTopCardPresenter();
                OnTopCardChangedAfterRemove(newTop);
            }
        }

        public virtual void RevealTopCardIfNeeded()
        {
            var topCardPresenter = GetTopCardPresenter();
            if (topCardPresenter == null) return;
            if (topCardPresenter.IsFaceUp) return;
            topCardPresenter.SetFaceUp(true, FlipDuration * 2.5f);
        }

        protected virtual void OnCardAdded(CardPresenter previousTop, CardPresenter newTop)
        {
        }

        protected virtual void OnTopCardChangedAfterRemove(CardPresenter newTop)
        {
        }
    }
}