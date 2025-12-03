using Card;
using UnityEngine;

namespace Gameplay
{
    public class DealerButton : MonoBehaviour
    {
        [SerializeField] Dealer dealer;
        [SerializeField] OpenDealer openDealer;

        public void OnDealerClicked()
        {
            if (dealer == null || openDealer == null) return;

            var topCardModel = dealer.GetTopCardModel();
            if (topCardModel == null) return;

            var topCardView = dealer.RemoveCard(topCardModel);
            if (topCardView == null) return;

            openDealer.AddCard(topCardView, topCardModel);
            topCardView.SetRotation(true);
        }
    }
}
