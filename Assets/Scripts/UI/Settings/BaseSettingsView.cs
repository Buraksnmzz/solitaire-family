using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public abstract class BaseSettingsView: BaseView
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Button hapticButton;
        public SettingsCard soundCard;
        public SettingsCard hapticCard;
        
        public event Action SoundToggled;
        public event Action HapticToggled;
        
        protected virtual void Start()
        {
            closeButton.onClick.AddListener(Hide);
            soundButton.onClick.AddListener(()=>SoundToggled?.Invoke());
            hapticButton.onClick.AddListener(()=>HapticToggled?.Invoke());
        }
    }
}