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

            var reward = GetProductReward();

            if (coinAmountText != null) coinAmountText.SetText(reward.Coins.ToString());
            if (jokerAmountText != null) jokerAmountText.SetText(reward.Jokers.ToString());
            if (hintAmountText != null) hintAmountText.SetText(reward.Hints.ToString());
            if (undoAmountText != null) undoAmountText.SetText(reward.Undos.ToString());
        }

        protected override void GiveReward(bool success)
        {
            if (success)
            {
                var reward = GetProductReward();
                var collectibleModel = SavedDataService.GetModel<CollectibleModel>();
                var settingsModel = SavedDataService.GetModel<SettingsModel>();
                collectibleModel.totalCoins += reward.Coins;
                collectibleModel.totalHints += reward.Hints;
                collectibleModel.totalUndo += reward.Undos;
                collectibleModel.totalJokers += reward.Jokers;
                settingsModel.IsNoAds = true;
                SavedDataService.SaveData(collectibleModel);
                SavedDataService.SaveData(settingsModel);
                EventDispatcherService.Dispatch(new RewardGivenSignal(transform));
                EventDispatcherService.Dispatch(new BannerVisibilityChangedSignal(false));
            }
        }
    }
}