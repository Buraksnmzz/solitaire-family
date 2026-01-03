using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class MainSettingsView : BaseSettingsView
    {
        [SerializeField] private Button aboutButton;
        [SerializeField] private Button restorePurchaseButton;
        
        public event Action AboutToggled;
        public event Action RestorePurchaseClicked;

        protected override void Start()
        {
            base.Start();
            aboutButton.onClick.AddListener(()=>AboutToggled?.Invoke());
            restorePurchaseButton.onClick.AddListener(()=>RestorePurchaseClicked?.Invoke());
        }

        public void SetRestoreButtonVisibility(bool shouldShow)
        {
            restorePurchaseButton.gameObject.SetActive(shouldShow);
        }
    }
}
