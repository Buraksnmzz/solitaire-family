using Collectible;
using System;
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
        private bool _isInitialized;
        private Action<bool> _purchaseCallback;

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
            }
            if (_isInitialized && m_StoreController != null)
            {
                var product = m_StoreController.products.WithID(productId);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log($"Initiating purchase: {productId}");
                    m_StoreController.InitiatePurchase(product);
                    return;
                }

                Debug.LogError($"Product not available for purchase: {productId}");
                _purchaseCallback?.Invoke(false);
                _purchaseCallback = null;

            }
            Debug.Log("IAPService: falling back to simulated purchase: " + productId);
            _purchaseCallback?.Invoke(false);
            _purchaseCallback = null;
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
            var product = args.purchasedProduct;
            var hasReceipt = !string.IsNullOrEmpty(product.receipt);
            if (hasReceipt)
            {
                Debug.Log("Purchase successful: " + args.purchasedProduct.definition.id);
                _purchaseCallback.Invoke(true);
                _purchaseCallback = null;
            }
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            Debug.LogWarning($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
        }

        // public void RestorePurchases()
        // {
        //     if (!m_IsInitialized)
        //     {
        //         Debug.LogWarning("IAP not initialized - cannot restore purchases");
        //         return;
        //     }
        //
        //     if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
        //     {
        //         var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
        //         apple.RestoreTransactions(result => Debug.Log("RestorePurchases result: " + result));
        //     }
        //     else
        //     {
        //         Debug.Log("RestorePurchases not supported on this platform");
        //     }
        // }
    }
}
