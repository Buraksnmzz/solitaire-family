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
        protected override void SetCardPosition(CardView card)
        {
            card.transform.localPosition = Vector3.zero;
        }

        public override void Setup(IPlacableRule placableRule)
        {
            base.Setup(placableRule);
            dealerButton.onClick.AddListener(OnDealerButtonClick);
        }

        private void OnDealerButtonClick()
        {
            var topCardModel = GetTopCardModel();
            if (topCardModel == null) return;

            var topCardView = RemoveCard(topCardModel);
            if (topCardView == null) return;

            openDealer.AddCard(topCardView, topCardModel);
            topCardView.SetRotation(true);
        }
    }
}