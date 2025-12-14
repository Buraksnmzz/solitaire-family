using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class MainSettingsView : BaseSettingsView
    {
        [SerializeField] private Button aboutButton;
        
        public event Action AboutToggled;

        private void Start()
        {
            aboutButton.onClick.AddListener(()=>AboutToggled?.Invoke());
        }
    }
}
