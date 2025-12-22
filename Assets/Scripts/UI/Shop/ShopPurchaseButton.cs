using System;
using Collectible;
using IAP;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopPurchaseButton : MonoBehaviour
    {
        public string productId;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private TextMeshProUGUI purchaseValueText;
        [SerializeField][CanBeNull] private TextMeshProUGUI coinRewardText;
        [SerializeField][CanBeNull] private TextMeshProUGUI discountText;
        [SerializeField][CanBeNull] private GameObject popularObject;
        [SerializeField][CanBeNull] private GameObject discountObject;

        private IIAPService _iapService;
        protected IEventDispatcherService EventDispatcherService;
        protected ISavedDataService SavedDataService;
        protected CatalogProduct CatalogProduct;

        private void Awake()
        {
            _iapService = ServiceLocator.GetService<IIAPService>();
            SavedDataService = ServiceLocator.GetService<ISavedDataService>();
            EventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            CatalogProduct = CatalogService.GetProduct(productId);
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveAllListeners();
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }
        }

        private void Start()
        {
            SetPurchaseValue();
            SetRewardValue();
        }

        public void SetPurchaseValue()
        {
            if (string.IsNullOrEmpty(productId)) return;

            var localized = _iapService.GetLocalizedPrice(productId);
            if (!string.IsNullOrEmpty(localized))
            {
                purchaseValueText.SetText(localized);
                return;
            }
            purchaseValueText.SetText("--");
        }

        public virtual void SetRewardValue()
        {
            if (string.IsNullOrEmpty(productId)) return;
            if(coinRewardText != null)
                coinRewardText.SetText(CatalogProduct.Coins.ToString());
        }

        private void OnPurchaseClicked()
        {
            _iapService.Purchase(productId, GiveReward);
        }

        protected virtual void GiveReward(bool success)
        {
            if (success)
            {
                var collectibleModel = SavedDataService.GetModel<CollectibleModel>();
                collectibleModel.totalCoins += CatalogProduct.Coins;
                SavedDataService.SaveData(collectibleModel);
                EventDispatcherService.Dispatch(new RewardGivenSignal(transform));
            }
        }
    }
}