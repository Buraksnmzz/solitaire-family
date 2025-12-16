using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class GameSettingsView: BaseSettingsView
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        
        public event Action RestartButtonClicked;
        public event Action MainMenuButtonClicked;
        
        protected override void Start()
        {
            base.Start();
            restartButton.onClick.AddListener(()=>RestartButtonClicked?.Invoke());
            mainMenuButton.onClick.AddListener(()=>MainMenuButtonClicked?.Invoke());
        }
    }
}