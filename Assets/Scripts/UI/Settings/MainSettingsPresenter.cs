namespace UI.Settings
{
    public class MainSettingsPresenter : BaseSettingsPresenter<MainSettingsView>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
            View.AboutToggled += OnAboutToggle;
        }

        private void OnAboutToggle()
        {
            UIService.ShowPopup<AboutPresenter>();
            View.Hide();
        }
    }
}