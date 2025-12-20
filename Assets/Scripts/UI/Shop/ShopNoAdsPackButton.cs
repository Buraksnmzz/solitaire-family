using Collectible;
using IAP;
using TMPro;
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

            if (coinAmountText != null) coinAmountText.SetText(_catalogProduct.Coins.ToString());
            if (jokerAmountText != null) jokerAmountText.SetText(_catalogProduct.Jokers.ToString());
            if (hintAmountText != null) hintAmountText.SetText(_catalogProduct.Hints.ToString());
            if (undoAmountText != null) undoAmountText.SetText(_catalogProduct.Undos.ToString());
        }

        protected override void GiveReward(bool success)
        {
            if (success)
            {
                var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
                var settingsModel = _savedDataService.GetModel<SettingsModel>();
                collectibleModel.totalCoins += _catalogProduct.Coins;
                collectibleModel.totalHints += _catalogProduct.Hints;
                collectibleModel.totalUndo += _catalogProduct.Undos;
                collectibleModel.totalJokers += _catalogProduct.Jokers;
                settingsModel.IsNoAds = true;
                //todo: Rise event to remove banner ads and hide shopView buttons
                _savedDataService.SaveData(collectibleModel);
                _savedDataService.SaveData(settingsModel);
            }
        }
    }
}