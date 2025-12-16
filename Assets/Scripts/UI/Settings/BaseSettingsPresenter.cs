namespace UI.Settings
{
    public abstract class BaseSettingsPresenter<TView> : BasePresenter<TView> where TView : BaseSettingsView
    {
        private SettingsModel _settingsModel;
        private ISavedDataService _savedDataService;
        protected IUIService UIService;
        protected override void OnInitialize()
        {
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            UIService = ServiceLocator.GetService<IUIService>();
            base.OnInitialize();
            _settingsModel = _savedDataService.GetModel<SettingsModel>();
            View.SoundToggled += OnSoundToggle;
            View.HapticToggled += OnHapticToggle;
        }

        public override void ViewShown()
        {
            base.ViewShown();
            View.hapticCard.SetState(_settingsModel.IsHapticOn, false);
            View.soundCard.SetState(_settingsModel.IsSoundOn, false);
        }

        private void OnHapticToggle()
        {
            _settingsModel.IsHapticOn = !_settingsModel.IsHapticOn;
            View.hapticCard.SetState(_settingsModel.IsHapticOn);
            _savedDataService.SaveData(_settingsModel);
        }

        private void OnSoundToggle()
        {
            _settingsModel.IsSoundOn = !_settingsModel.IsSoundOn;
            View.soundCard.SetState(_settingsModel.IsSoundOn);
            _savedDataService.SaveData(_settingsModel);
        }
    }
}