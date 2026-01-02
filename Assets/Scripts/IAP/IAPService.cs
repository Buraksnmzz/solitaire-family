using Collectible;
using System;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace IAP
{
    public class IAPService : IIAPService, IStoreListener
    {
        private readonly ICollectibelService _collectibleService;
        private readonly ISavedDataService _savedDataService;

        private IStoreController m_StoreController;
        private IExtensionProvider m_StoreExtensionProvider;
#if UNITY_IOS
        private static IAppleExtensions appleExtensions;
#endif
        private bool _isInitialized;
        private Action<bool> _purchaseCallback;
        private HashSet<string> _processedTransactions = new HashSet<string>();
        private string _pendingPurchaseProductId;

        private const string NoAdsProductID = "noads_only";
        private const string NoAdsPackProductID = "noads_pack";

        public IAPService()
        {
            _collectibleService = ServiceLocator.GetService<ICollectibelService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            CatalogService.LoadCatalog();
            InitializePurchasing();
        }

        public void Purchase(string productId, Action<bool> onComplete)
        {
            _purchaseCallback = onComplete;
            if (!_isInitialized)
            {
                Debug.LogError("IAP not initialized");
                _purchaseCallback?.Invoke(false);
                _purchaseCallback = null;
                _pendingPurchaseProductId = null;
                return;
            }
            if (_isInitialized && m_StoreController != null)
            {
                var product = m_StoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log($"Initiating purchase: {productId}");
                    _pendingPurchaseProductId = productId;
                    m_StoreController.InitiatePurchase(product);
                    return;
                }

                Debug.LogError($"Product not available for purchase: {productId}");
                _purchaseCallback?.Invoke(false);
                _purchaseCallback = null;
                _pendingPurchaseProductId = null;
                return;
            }
            Debug.Log("IAPService: falling back to simulated purchase: " + productId);
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
            _pendingPurchaseProductId = null;
        }

        public string GetLocalizedPrice(string productId)
        {
            if (string.IsNullOrEmpty(productId)) return string.Empty;

            if (_isInitialized && m_StoreController != null)
            {
                var prod = m_StoreController.products.WithID(productId);
                if (prod != null && prod.metadata != null && !string.IsNullOrEmpty(prod.metadata.localizedPriceString))
                    return prod.metadata.localizedPriceString;
            }

            return string.Empty;
        }

        private async void InitializePurchasing()
        {
            if (_isInitialized) return;

            //var options = new InitializationOptions().SetEnvironmentName("production");
            var options = new InitializationOptions().SetEnvironmentName("test");
            await UnityServices.InitializeAsync(options);
            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);

            foreach (var kv in ProductIds.ProductTypeMap)
            {
                builder.AddProduct(kv.Key, kv.Value);
            }
            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP initialized");
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
#if UNITY_IOS
            appleExtensions = extensions.GetExtension<IAppleExtensions>();
#endif
            _isInitialized = true;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogWarning("IAP initialization failed: " + error);
            _isInitialized = false;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message = null)
        {
            Debug.LogWarning("IAP initialization failed: " + error);
            _isInitialized = false;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var settingsModel = _savedDataService.GetModel<SettingsModel>();
            var product = args.purchasedProduct;
            string transactionId = product.transactionID;
            if (transactionId != null && _processedTransactions.Contains(transactionId))
            {
                return PurchaseProcessingResult.Complete;
            }

            var hasReceipt = product.hasReceipt;
            var productId = product.definition.id;
            var isUserInitiatedPurchase = _purchaseCallback != null && !string.IsNullOrEmpty(_pendingPurchaseProductId) && _pendingPurchaseProductId == productId;

            if (hasReceipt)
            {
                if (isUserInitiatedPurchase)
                {
                    if (productId != NoAdsProductID && productId != NoAdsPackProductID)
                        YoogoLabManager.IAP(product);
                    else if (!settingsModel.IsNoAds)
                        YoogoLabManager.IAP(product);
                }

                if (productId == NoAdsProductID || productId == NoAdsPackProductID)
                    settingsModel.IsNoAds = true;
                if (transactionId != null) _processedTransactions.Add(transactionId);
                Debug.Log("Purchase successful: " + args.purchasedProduct.definition.id);
                _purchaseCallback?.Invoke(true);
                _purchaseCallback = null;
                _pendingPurchaseProductId = null;
            }
            else
            {
                _purchaseCallback?.Invoke(false);
                _purchaseCallback = null;
                _pendingPurchaseProductId = null;
            }
            _savedDataService.SaveData(settingsModel);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
            _pendingPurchaseProductId = null;
        }

        private void RestorePurchases()
        {
            Debug.Log("[IAP RESTORE] RestorePurchases() called");

            var product1 = m_StoreController.products.WithID(NoAdsProductID);
            var product2 = m_StoreController.products.WithID(NoAdsPackProductID);
            if (product1 == null && product2 == null)
            {
                Debug.LogError("[IAP RESTORE] ERROR: Product not found in catalog");
                return;
            }

            var settingsModel = _savedDataService.GetModel<SettingsModel>();
            var hasReceiptFlag = (product1 != null && product1.hasReceipt) || (product2 != null && product2.hasReceipt);

            if (hasReceiptFlag)
            {
                Debug.Log("[IAP RESTORE] VALID purchase detected → Granting entitlement");
                settingsModel.IsNoAds = true;
            }
            else
            {
                Debug.Log("[IAP RESTORE] NO valid purchase → entitlement stays = " + settingsModel.IsNoAds);
            }

            _savedDataService.SaveData(settingsModel);

            Debug.Log($"[IAP RESTORE] Final entitlement state = {settingsModel.IsNoAds}");
        }

        public void RestorePurchasesIOS()
        {
#if UNITY_IOS
            if (appleExtensions == null)
            {
                Debug.LogWarning("RestorePurchases called before IAP was initialized.");
                return;
            }

            appleExtensions.RestoreTransactions((success, error) =>
            {
                Debug.Log($"RestorePurchases completed. Success: {success} | Error: {error}");

                if (m_StoreController != null)
                {
                    RestorePurchases();
                }
            });
#endif
        }
    }
}
