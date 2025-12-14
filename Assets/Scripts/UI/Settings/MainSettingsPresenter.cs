namespace UI.Settings
{
    public class MainSettingsPresenter : BaseSettingsPresenter
    {
        private IUIService _uiService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            ((MainSettingsView)View).AboutToggled += OnAboutToggle;
        }

        private void OnAboutToggle()
        {
            _uiService.ShowPopup<AboutPresenter>();
        }
    }
}