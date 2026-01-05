using Services;
using UnityEngine;
using UI.Signals;

namespace UI.Settings
{
    public class LanguageSelectPresenter : BasePresenter<LanguageSelectView>
    {
        ILocalizationService _localizationService;
        IEventDispatcherService _eventDispatcherService;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _localizationService = ServiceLocator.GetService<ILocalizationService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            View.LanguageButtonCLicked += OnLanguageClicked;
        }

        private void OnLanguageClicked(SystemLanguage language)
        {
            _eventDispatcherService.Dispatch(new LanguageChangeRequestedSignal(language));
        }

        public override void ViewShown()
        {
            base.ViewShown();
            _eventDispatcherService.AddListener<LanguageChangedSignal>(OnLanguageChanged);
            View.InitializeLanguageButtons(_localizationService.GetAvailableLanguages(),
                _localizationService.GetCurrentLanguage());
        }

        public override void ViewHidden()
        {
            base.ViewHidden();
            _eventDispatcherService.RemoveListener<LanguageChangedSignal>(OnLanguageChanged);
        }

        private void OnLanguageChanged(LanguageChangedSignal signal)
        {
            View.InitializeLanguageButtons(_localizationService.GetAvailableLanguages(),
                _localizationService.GetCurrentLanguage());
        }
    }
}