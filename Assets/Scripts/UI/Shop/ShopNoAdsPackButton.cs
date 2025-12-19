using TMPro;
using UnityEngine;

namespace UI.Shop
{
    public class ShopNoAdsPackButton: ShopPurchaseButton
    {
        [SerializeField] TextMeshProUGUI coinAmountText;
        [SerializeField] TextMeshProUGUI jokerAmountText;
        [SerializeField] TextMeshProUGUI hintAmountText;
        [SerializeField] TextMeshProUGUI undoAmountText;

        public void SetRewardValues()
        {
            
        } 
    }
}