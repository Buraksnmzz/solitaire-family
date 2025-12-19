using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopPurchaseButton: MonoBehaviour
    {
        public string productId;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseValueText;
        [SerializeField][CanBeNull] private TextMeshProUGUI discountText;
        [SerializeField][CanBeNull] private GameObject popularObject;
        [SerializeField][CanBeNull] private GameObject discountObject;

        public void SetPurchaseValue()
        {
            
        }
    }
}