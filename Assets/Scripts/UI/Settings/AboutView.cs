using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class AboutView: BaseView
    {
        [SerializeField] private Button privacySettingsButton;
        [SerializeField] private Button privacyPolicyButton;
        [SerializeField] private Button termsOfServiceButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private LegalTextView legalTextView;

        public event Action PrivacySettingsClicked;
        
        private void Start()
        {
            privacySettingsButton.onClick.AddListener(()=>PrivacySettingsClicked?.Invoke());
            privacyPolicyButton.onClick.AddListener(OnPrivacyPolicyClicked);
            termsOfServiceButton.onClick.AddListener(OnTermsOfServiceClicked);
            closeButton.onClick.AddListener(Hide);
        }
        
        private void OnContactUsClicked()
        {
            Application.OpenURL("mailto:contact@yoogalab.com");
        }

        private void OnTermsOfServiceClicked()
        {
            var asset = Resources.Load<TextAsset>("Legal/TermsOfService");
            if (legalTextView != null)
            {
                legalTextView.Show("Terms of Service", asset);
            }
        }

        private void OnPrivacyPolicyClicked()
        {
            var asset = Resources.Load<TextAsset>("Legal/PrivacyPolicy");
            if (legalTextView != null)
            {
                legalTextView.Show("Privacy Policy", asset);
            }
        }
    }
}