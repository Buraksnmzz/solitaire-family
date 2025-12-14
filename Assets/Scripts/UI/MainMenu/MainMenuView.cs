
using System;
using TMPro;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuView : BaseView
    {
        public Button levelButton;
        public Button continueButton;
        public TextMeshProUGUI continueLevelText;
        public TextMeshProUGUI levelText;
        
        public event Action LevelButtonClicked;
        public event Action ContinueButtonClicked;
        protected override void Awake()
        {
            base.Awake();
            levelButton.onClick.AddListener(()=>LevelButtonClicked?.Invoke());
            continueButton.onClick.AddListener(()=>ContinueButtonClicked?.Invoke());
        }

        public void SetLevelText(int currentLevel)
        {
            continueLevelText.SetText("Level " + (currentLevel+1));
            levelText.SetText("Level " + (currentLevel+1));
        }
    }
    
}
