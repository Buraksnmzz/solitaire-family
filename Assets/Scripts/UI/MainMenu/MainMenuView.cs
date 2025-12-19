
using System;
using System.Collections.Generic;
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
        
        public event Action ContinueButtonClicked;
        public event Action SettingsButtonClicked;
        public event Action CoinButtonClicked;
        protected override void Awake()
        {
            base.Awake();
            continueButton.onClick.AddListener(()=>ContinueButtonClicked?.Invoke());
            settingsButton.onClick.AddListener(()=>SettingsButtonClicked?.Invoke());
            coinButton.onClick.AddListener(()=>CoinButtonClicked?.Invoke());
        }

        public void SetLevelText(int currentLevel)
        {
            continueLevelText.SetText("Level " + (currentLevel+1));
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
    }
    
}
