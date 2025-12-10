
using System;
using TMPro;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuView : BaseView
    {
        public Button levelButton;
        public Button continueButton;
        public TextMeshProUGUI levelText;
        
        public event Action LevelButtonClicked;
        public event Action ContinueButtonClicked;
        protected override void Awake()
        {
            base.Awake();
            levelButton.onClick.AddListener(()=>LevelButtonClicked?.Invoke());
            continueButton.onClick.AddListener(()=>ContinueButtonClicked?.Invoke());
        }
    }
    
}
