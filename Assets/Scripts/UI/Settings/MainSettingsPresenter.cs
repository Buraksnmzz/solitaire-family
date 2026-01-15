using IAP;
using Services;

namespace UI.Settings
{
    public class MainSettingsPresenter : BaseSettingsPresenter<MainSettingsView>
    {
        IIAPService _iapService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _iapService = ServiceLocator.GetService<IIAPService>();
            View.AboutToggled += OnAboutToggle;
            View.RestorePurchaseClicked += OnRestorePurchasesClicked;
            View.LanguageClicked += OnLanguageClicked;
        }

        private void OnLanguageClicked()
        {
            UIService.ShowPopup<LanguageSelectPresenter>();
        }

        public override void ViewShown()
        {
            base.ViewShown();
            bool shouldShow;
            _settingsModel = _savedDataService.GetModel<SettingsModel>();

#if UNITY_IOS
            shouldShow = true;
#else
                shouldShow = false;
#endif
            
            View.SetRestoreButtonVisibility(shouldShow);
        }

        private void OnAboutToggle()
        {
            UIService.ShowPopup<AboutPresenter>();
            View.Hide();
        }
        
        private void OnRestorePurchasesClicked()
        {
            if (_iapService.IsInitialized)
            {
                _iapService.RestorePurchasesIOS();
            }
        }
    }
}