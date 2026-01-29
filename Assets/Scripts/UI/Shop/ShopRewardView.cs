using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopRewardView: BaseView
    {
        [SerializeField] private GameObject packReward;
        [SerializeField] private GameObject noAdsReward;
        [SerializeField] private GameObject coinReward;
        [SerializeField] TextMeshProUGUI packCoinAmountText;
        [SerializeField] TextMeshProUGUI packJokerAmountText;
        [SerializeField] TextMeshProUGUI packHintAmountText;
        [SerializeField] TextMeshProUGUI packUndoAmountText;
        [SerializeField] TextMeshProUGUI coinOnlyAmountText;
        [SerializeField] Button okButton;
        
        public event Action OkClicked;

        private void Start()
        {
            okButton.onClick.AddListener(()=>OkClicked?.Invoke());
        }

        public override void Show()
        {
            base.Show();
            packReward.SetActive(false);
            noAdsReward.SetActive(false);
            coinReward.SetActive(false);
        }

        public void ShowNoAds()
        {
            noAdsReward.SetActive(true);
        }

        public void ShowPack(int dataCoinReward, int dataHintReward, int dataJokerReward, int dataUndoReward)
        {
            packReward.SetActive(true);
            packCoinAmountText.text = dataCoinReward.ToString();
            packHintAmountText.text = "x" + dataHintReward;
            packJokerAmountText.text = "x" + dataJokerReward;
            packUndoAmountText.text = "x" + dataUndoReward;
        }

        public void ShowCoin(int dataCoinReward)
        {
            coinReward.SetActive(true);
            coinOnlyAmountText.text = dataCoinReward.ToString();
        }
    }
}