
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuView : BaseView
    {
        //public Button levelButton;
        public Button continueButton;
        public Button settingsButton;
        public TextMeshProUGUI continueLevelText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private List<Sprite> backgroundSprites;
        //public TextMeshProUGUI levelText;
        
        //public event Action LevelButtonClicked;
        public event Action ContinueButtonClicked;
        public event Action SettingsButtonClicked;
        protected override void Awake()
        {
            base.Awake();
            //levelButton.onClick.AddListener(()=>LevelButtonClicked?.Invoke());
            continueButton.onClick.AddListener(()=>ContinueButtonClicked?.Invoke());
            settingsButton.onClick.AddListener(()=>SettingsButtonClicked?.Invoke());
        }

        public void SetLevelText(int currentLevel)
        {
            continueLevelText.SetText("Level " + (currentLevel+1));
            //levelText.SetText("Level " + (currentLevel+1));
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
