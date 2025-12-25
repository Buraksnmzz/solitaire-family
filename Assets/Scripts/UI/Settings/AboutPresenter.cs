namespace UI.Settings
{
    public class AboutPresenter: BasePresenter<AboutView>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            View.PrivacySettingsClicked += OnPrivacySettingsClicked;
        }

        private void OnPrivacySettingsClicked()
        {
            
        }
    }
}