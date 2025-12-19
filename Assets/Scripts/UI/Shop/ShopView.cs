using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopView: BaseView
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private ShopNoAdsPackButton shopNoAdsPackButton;
        [SerializeField] private ShopPurchaseButton noAdsButton;
        [SerializeField] private ShopPurchaseButton purchaseCoinButton1;
        [SerializeField] private ShopPurchaseButton purchaseCoinButton2;
        [SerializeField] private ShopPurchaseButton purchaseCoinButton3;
        [SerializeField] private ShopPurchaseButton purchaseCoinButton4;
        [SerializeField] private ShopPurchaseButton purchaseCoinButton5;

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
        }
    }
}