
using System;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuView : BaseView
    {
        public Button levelButton;
        
        public event Action LevelButtonClicked;
        protected override void Awake()
        {
            base.Awake();
            levelButton.onClick.AddListener(()=>LevelButtonClicked?.Invoke());
        }
    }
    
}
