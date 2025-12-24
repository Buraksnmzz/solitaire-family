using System.Collections.Generic;
using System.Linq;
using Card;
using Core.Scripts.Services;
using DG.Tweening;
using Gameplay.PlacableRules;
using UI.Signals;
using UnityEngine;

namespace Gameplay
{
    public abstract class CardContainer : MonoBehaviour
    {
        protected List<CardPresenter> CardPresenters = new();
        private IPlacableRule _placableRule;
        private IPlacableErrorPersistenceService _placableErrorPersistence;
        protected readonly float MoveDuration = 0.25f;
        protected readonly float FlipDuration = 0.1f;
        protected IEventDispatcherService EventDispatcherService;
        protected ISoundService SoundService;
        protected IHapticService HapticService;

        public abstract Vector3 GetCardLocalPosition(int index);

        public virtual void Setup(IPlacableRule placableRule)
        {
            _placableRule = placableRule;
            EventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _placableErrorPersistence = ServiceLocator.GetService<IPlacableErrorPersistenceService>();
            SoundService = ServiceLocator.GetService<ISoundService>();
            HapticService = ServiceLocator.GetService<IHapticService>();
        }

        public int GetCardsCount() => CardPresenters.Count;
        public int GetCardsCountWithoutJoker() => CardPresenters.Count(p => p.CardModel == null || p.CardModel.Type != CardType.Joker);

        public CardPresenter GetTopCard() => CardPresenters.LastOrDefault();

        public CardPresenter GetTopCardPresenter() => CardPresenters.LastOrDefault();

        public bool CanPlaceCard(CardPresenter sourceCardPresenter)
        {
            if (!gameObject.activeInHierarchy) return false;
            var topCardModel = CardPresenters.LastOrDefault()?.CardModel;
            var isPlacable = _placableRule.IsPlaceable(topCardModel, sourceCardPresenter.CardModel);
            var placableError = _placableRule.ErrorMessage;
            if (!isPlacable && placableError != null)
            {
                var shouldShow = true;
                if (_placableErrorPersistence != null)
                {
                    shouldShow = _placableErrorPersistence.ShouldShow(placableError);
                }

                if (shouldShow)
                {
                    EventDispatcherService.Dispatch(new PlacableErrorSignal(placableError));
                    _placableErrorPersistence?.RecordShown(placableError);
                }
            }
            return isPlacable;
        }

        public bool CanPlaceCardSilently(CardPresenter sourceCardPresenter)
        {
            if (!gameObject.activeInHierarchy) return false;
            var topCardModel = CardPresenters.LastOrDefault()?.CardModel;
            return _placableRule.IsPlaceable(topCardModel, sourceCardPresenter.CardModel);
        }

        public virtual void AddCard(CardPresenter cardPresenter, float delay = 0, float moveDuration = 0.25f)
        {
            var previousTop = GetTopCardPresenter();
            CardPresenters.Add(cardPresenter);
            var index = CardPresenters.IndexOf(cardPresenter);
            cardPresenter.SetParent(transform, true);
            cardPresenter.SetContainer(this);
            var targetLocalPosition = GetCardLocalPosition(index);
            cardPresenter.MoveToLocalPosition(targetLocalPosition, moveDuration, delay, Ease.OutQuad, () => EventDispatcherService.Dispatch(new CardMovementStateChangedSignal(false)));

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

        public IReadOnlyList<CardPresenter> GetAllCards()
        {
            return CardPresenters.ToList();
        }

        public virtual void ClearAllCards()
        {
            foreach (var presenter in CardPresenters)
            {
                if (presenter == null) continue;
                if (presenter.CardView == null) continue;
                Destroy(presenter.CardView.gameObject);
            }

            CardPresenters.Clear();
        }

        protected virtual void OnCardAdded(CardPresenter previousTop, CardPresenter newTop)
        {
        }

        protected virtual void OnTopCardChangedAfterRemove(CardPresenter newTop)
        {
        }
    }
}