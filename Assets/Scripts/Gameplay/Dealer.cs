using System.Collections.Generic;
using Card;
using Gameplay.PlacableRules;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Dealer : CardContainer
    {
        [SerializeField] private Button dealerButton;
        [SerializeField] private OpenDealer openDealer;
        private List<CardModel> _cardModels;

        public void SetupDeck(List<CardModel> cardModels, List<CardPresenter> cardPresenters, List<CardView> cardViews)
        {
            _cardModels = cardModels;
            _cardPresenters = cardPresenters;

            for (var i = 0; i < _cardPresenters.Count && i < cardViews.Count; i++)
            {
                var presenter = _cardPresenters[i];
                var view = cardViews[i];
                presenter.Initialize(_cardModels[i], view);
                presenter.SetParent(transform, true);
                presenter.SetContainer(this);
            }
        }

        public void ShuffleDeck()
        {
            var count = _cardPresenters.Count;
            if (count == 0) return;

            for (var i = count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                if (i == j) continue;

                (_cardPresenters[i], _cardPresenters[j]) = (_cardPresenters[j], _cardPresenters[i]);
                if (_cardModels != null && _cardModels.Count == count)
                {
                    (_cardModels[i], _cardModels[j]) = (_cardModels[j], _cardModels[i]);
                }
            }

            for (var index = 0; index < _cardPresenters.Count; index++)
            {
                var presenter = _cardPresenters[index];
                if (presenter.CardView == null) continue;
                presenter.CardView.transform.SetSiblingIndex(index);
            }
        }

        public void DealInitialCards(List<CardContainer> piles, int foundationCount)
        {
            if (piles == null || piles.Count == 0) return;

            var pileCounts = GetInitialPileCounts(foundationCount);
            if (pileCounts == null) return;

            for (var pileIndex = 0; pileIndex < pileCounts.Length && pileIndex < piles.Count; pileIndex++)
            {
                var targetPile = piles[pileIndex];
                var cardsToDeal = pileCounts[pileIndex];

                for (var i = 0; i < cardsToDeal; i++)
                {
                    var topCardPresenter = GetTopCardPresenter();
                    if (topCardPresenter == null) return;

                    RemoveCard(topCardPresenter);
                    targetPile.AddCard(topCardPresenter);
                }
            }

            for (var i = 0; i < piles.Count; i++)
            {
                var topCard = piles[i].GetTopCardPresenter();
                if (topCard == null) continue;
                topCard.SetFaceUp(true, _flipDuration * 2f);
            }
        }

        private int[] GetInitialPileCounts(int foundationCount)
        {
            switch (foundationCount)
            {
                case 3:
                    return new[] { 3, 4, 5 };
                case 4:
                    return new[] { 3, 4, 5, 6 };
                case 5:
                    return new[] { 5, 6, 7, 8, 9 };
                default:
                    return null;
            }
        }

        public override Vector3 GetCardLocalPosition(int index)
        {
            return Vector3.zero;
        }

        public override void Setup(IPlacableRule placableRule)
        {
            base.Setup(placableRule);
            dealerButton.onClick.AddListener(OnDealerButtonClick);
        }

        private void OnDealerButtonClick()
        {
            var topCardPresenter = GetTopCard();
            if (topCardPresenter == null)
            {
                MoveAllCardsFromOpenDealerToDealer();
                return;
            }

            var removedPresenter = RemoveCard(topCardPresenter);
            if (removedPresenter == null) return;

            removedPresenter.SetParent(openDealer.transform, true);
            openDealer.AddCard(removedPresenter);
            removedPresenter.SetFaceUp(true, _flipDuration);
        }

        private void MoveAllCardsFromOpenDealerToDealer()
        {
            if (openDealer == null) return;

            var openDealerCards = openDealer.GetAllCardPresenters();
            if (openDealerCards == null || openDealerCards.Count == 0) return;

            for (var i = openDealerCards.Count - 1; i >= 0; i--)
            {
                var presenter = openDealerCards[i];
                if (presenter == null) continue;

                var removedPresenter = openDealer.RemoveCard(presenter);
                if (removedPresenter == null) continue;

                removedPresenter.SetFaceUp(false, _flipDuration);
                removedPresenter.SetParent(transform, true);
                AddCard(removedPresenter);
            }
        }
    }
}