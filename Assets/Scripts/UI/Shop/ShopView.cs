using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopView : BaseView
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private ShopNoAdsPackButton shopNoAdsPackButton;
        [SerializeField] private ShopPurchaseButton noAdsButton;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private Transform coinImage;

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
            IAP.CatalogService.LoadCatalog();
            var settings = ServiceLocator.GetService<ISavedDataService>().GetModel<SettingsModel>();
            if (settings != null && settings.IsNoAds)
            {
                if (shopNoAdsPackButton != null) shopNoAdsPackButton.gameObject.SetActive(false);
                if (noAdsButton != null) noAdsButton.gameObject.SetActive(false);
            }

            // purchaseCoinButton1?.SetPurchaseValue();
            // purchaseCoinButton2?.SetPurchaseValue();
            // purchaseCoinButton3?.SetPurchaseValue();
            // purchaseCoinButton4?.SetPurchaseValue();
            // purchaseCoinButton5?.SetPurchaseValue();
            // noAdsButton?.SetPurchaseValue();
            // shopNoAdsPackButton?.SetRewardValue();
            // shopNoAdsPackButton?.SetPurchaseValue();
        }

        public void SetCoinText(int coin)
        {
            coinText.text = coin.ToString();
        }
    }
}