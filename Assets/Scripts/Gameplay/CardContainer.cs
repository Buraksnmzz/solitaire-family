using System.Collections.Generic;
using System.Linq;
using Card;
using Gameplay.PlacableRules;
using UnityEngine;

namespace Gameplay
{
    public abstract class CardContainer : MonoBehaviour
    {
        public List<CardModel> _cardModels = new();
        public List<CardView> _cardViews = new();
        protected IPlacableRule PlacableRule;

        protected abstract void SetCardPosition(CardView card);

        public virtual void Setup(IPlacableRule placableRule)
        {
            PlacableRule = placableRule;
        }

        public CardModel GetTopCardModel() => _cardModels.LastOrDefault();

        public CardView GetTopCardView() => _cardViews.LastOrDefault();

        public bool CanPlaceCard(CardModel sourceCardModel)
        {
            var topCardModel = _cardModels.LastOrDefault();
            return PlacableRule.IsPlaceable(topCardModel, sourceCardModel);
        }

        public void AddCard(CardView cardView, CardModel cardModel)
        {
            _cardModels.Add(cardModel);
            _cardViews.Add(cardView);
            cardView.transform.SetParent(transform);
            SetCardPosition(cardView);
            cardView.transform.SetAsLastSibling();
            cardModel.Container = this;
            switch (this)
            {
                case Dealer:
                    cardModel.ContainerType = CardContainerType.Dealer;
                    break;
                case OpenDealer:
                    cardModel.ContainerType = CardContainerType.OpenDealer;
                    break;
                case Pile:
                    cardModel.ContainerType = CardContainerType.Pile;
                    break;
                case Foundation:
                    cardModel.ContainerType = CardContainerType.Foundation;
                    break;
            }
        }

        public CardView RemoveCard(CardModel cardModel)
        {
            var index = _cardModels.IndexOf(cardModel);
            if (index == -1) return null;
            var cardView = _cardViews[index];
            _cardViews.RemoveAt(index);
            _cardModels.RemoveAt(index);
            cardView.transform.SetParent(null);
            return cardView;
        }
    }
}