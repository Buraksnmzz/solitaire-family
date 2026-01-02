using Collectible;
using IAP;
using TMPro;
using UI.Signals;
using UnityEngine;

namespace UI.Shop
{
    public class ShopNoAdsPackButton : ShopPurchaseButton
    {
        [SerializeField] TextMeshProUGUI coinAmountText;
        [SerializeField] TextMeshProUGUI jokerAmountText;
        [SerializeField] TextMeshProUGUI hintAmountText;
        [SerializeField] TextMeshProUGUI undoAmountText;

        public override void SetRewardValue()
        {
            if (string.IsNullOrEmpty(productId)) return;

            if (coinAmountText != null) coinAmountText.SetText(CatalogProduct.Coins.ToString());
            if (jokerAmountText != null) jokerAmountText.SetText(CatalogProduct.Jokers.ToString());
            if (hintAmountText != null) hintAmountText.SetText(CatalogProduct.Hints.ToString());
            if (undoAmountText != null) undoAmountText.SetText(CatalogProduct.Undos.ToString());
        }

        protected override void GiveReward(bool success)
        {
            if (success)
            {
                var collectibleModel = SavedDataService.GetModel<CollectibleModel>();
                var settingsModel = SavedDataService.GetModel<SettingsModel>();
                collectibleModel.totalCoins += CatalogProduct.Coins;
                collectibleModel.totalHints += CatalogProduct.Hints;
                collectibleModel.totalUndo += CatalogProduct.Undos;
                collectibleModel.totalJokers += CatalogProduct.Jokers;
                settingsModel.IsNoAds = true;
                SavedDataService.SaveData(collectibleModel);
                SavedDataService.SaveData(settingsModel);
                EventDispatcherService.Dispatch(new RewardGivenSignal(transform));
                EventDispatcherService.Dispatch(new BannerVisibilityChangedSignal(false));
            }
        }
    }
}