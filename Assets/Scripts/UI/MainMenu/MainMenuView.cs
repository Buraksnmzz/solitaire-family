
using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuView : BaseView
    {
        public Button continueButton;
        public Button settingsButton;
        public TextMeshProUGUI continueLevelText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private List<Sprite> backgroundSprites;
        [SerializeField] private Button coinButton;
        [SerializeField] private Button noAdsButton;
        
        public event Action ContinueButtonClicked;
        public event Action SettingsButtonClicked;
        public event Action CoinButtonClicked;
        public event Action NoAdsButtonClicked;
        protected override void Awake()
        {
            base.Awake();
            continueButton.onClick.AddListener(()=>ContinueButtonClicked?.Invoke());
            settingsButton.onClick.AddListener(()=>SettingsButtonClicked?.Invoke());
            coinButton.onClick.AddListener(()=>CoinButtonClicked?.Invoke());
            noAdsButton.onClick.AddListener(()=>NoAdsButtonClicked?.Invoke());
        }

        protected override void OnShown()
        {
            base.OnShown();
            continueButton.transform.DOScale(1, 0.4f).SetEase(Ease.OutBack);
        }

        public override void Show()
        {
            base.Show();
            continueButton.transform.localScale = Vector3.zero;
        }

        public void SetLevelText(int currentLevel)
        {
            continueLevelText.SetText("Level " + currentLevel);
        }

        public void SetCoinText(int totalCoins)
        {
            coinText.SetText(totalCoins.ToString());
        }

        public void SetBackgroundImageFromRemote(int backgroundImageId)
        {
            if (backgroundImageId >= backgroundSprites.Count || backgroundImageId < 0)
            {
                backgroundImage.sprite = backgroundSprites[0];
                return;
            }
            backgroundImage.sprite = backgroundSprites[backgroundImageId];
        }

        public void SetNoAdsButton(bool isTrue)
        {
            noAdsButton.gameObject.SetActive(isTrue);
        }
    }
    
}
