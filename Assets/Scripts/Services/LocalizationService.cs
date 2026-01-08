using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UI.Signals;
using UI.Settings;

namespace Services
{
    public class LocaleInfo
    {
        public SystemLanguage Language { get; set; }
        public string DisplayName { get; set; }
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly ISavedDataService _savedDataService;
        private readonly SettingsModel _settingsModel;

        private AsyncOperationHandle<StringTable>? _stringTableLoadOperation;

        public LocalizationService()
        {
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _settingsModel = _savedDataService.LoadData<SettingsModel>();
            LocalizationSettings.InitializationOperation.Completed += OnLocalizationInitialized;
        }

        private void OnLocalizationInitialized(AsyncOperationHandle<LocalizationSettings> op)
        {
            SetLanguage(GetCurrentLanguage());
        }

        public List<LocaleInfo> GetAvailableLanguages()
        {
            return LocalizationSettings.AvailableLocales.Locales
                .Select(locale => new { locale, lang = GetSystemLanguageFromLocale(locale) })
                .Where(item => item.lang != SystemLanguage.Unknown)
                .Select(item => new LocaleInfo
                {
                    Language = item.lang,
                    DisplayName = GetDisplayName(item.locale),
                })
                .ToList();
        }

        private string GetDisplayName(Locale locale)
        {
            try
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(locale.Identifier.CultureInfo.NativeName);
            }
            catch
            {
                return locale.LocaleName;
            }
        }

        public void SetLanguage(SystemLanguage language)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(language);

            if (locale != null && LocalizationSettings.SelectedLocale != locale)
            {
                LocalizationSettings.SelectedLocale = locale;
            }
            else if (locale == null)
            {
                Debug.LogError($"Locale not found for language: {language}.");
                TryHideWaitingPopup();
                return;
            }

            DispatchLanguageChangedWhenReady(language);
        }

        private void DispatchLanguageChangedWhenReady(SystemLanguage language)
        {
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                void OnInitialized(AsyncOperationHandle<LocalizationSettings> _)
                {
                    LocalizationSettings.InitializationOperation.Completed -= OnInitialized;
                    DispatchLanguageChangedWhenReady(language);
                }

                LocalizationSettings.InitializationOperation.Completed += OnInitialized;
                return;
            }

            try
            {
                if (_stringTableLoadOperation.HasValue && !_stringTableLoadOperation.Value.IsDone)
                {
                    _stringTableLoadOperation.Value.Completed += _ => DispatchLanguageChanged(language);
                    return;
                }

                var tableLoadOperation = LocalizationSettings.StringDatabase.GetTableAsync(LocalizationStrings.LocalizationTable);
                _stringTableLoadOperation = tableLoadOperation;
                tableLoadOperation.Completed += _ => DispatchLanguageChanged(language);
            }
            catch
            {
                DispatchLanguageChanged(language);
            }
        }

        private void TryHideWaitingPopup()
        {
            try
            {
                ServiceLocator.GetService<IUIService>().HidePopup<WaitingPresenter>();
            }
            catch
            {
            }
        }

        private void DispatchLanguageChanged(SystemLanguage language)
        {
            _settingsModel.CurrentLanguage = language;
            _savedDataService.SaveData(_settingsModel);
            var eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            eventDispatcherService.Dispatch(new LanguageChangedSignal(language));
        }

        public SystemLanguage GetCurrentLanguage()
        {
            if (_settingsModel != null && _settingsModel.CurrentLanguage != SystemLanguage.Unknown)
            {
                return _settingsModel.CurrentLanguage;
            }

            var deviceLanguage = Application.systemLanguage;
            var availableLanguages = GetAvailableLanguages().Select(l => l.Language).ToList();

            return availableLanguages.Contains(deviceLanguage) ? deviceLanguage : SystemLanguage.English;
        }

        public string GetLocalizedString(string key)
        {
            if (!LocalizationSettings.InitializationOperation.IsDone) return key;

            try
            {
                var table = LocalizationSettings.StringDatabase.GetTable(LocalizationStrings.LocalizationTable);
                var entry = table?.GetEntry(key);
                return entry?.GetLocalizedString() ?? key;
            }
            catch
            {
                return key;
            }
        }

        public string GetLocalizedString(string key, params object[] args)
        {
            if (!LocalizationSettings.InitializationOperation.IsDone) return key;

            try
            {
                var table = LocalizationSettings.StringDatabase.GetTable(LocalizationStrings.LocalizationTable);
                var entry = table?.GetEntry(key);
                return entry?.GetLocalizedString(args) ?? key;
            }
            catch
            {
                return key;
            }
        }

        private SystemLanguage GetSystemLanguageFromLocale(Locale locale)
        {
            var cultureInfo = locale.Identifier.CultureInfo;
            if (cultureInfo != null && Enum.TryParse(cultureInfo.EnglishName, out SystemLanguage language))
            {
                return language;
            }
            return SystemLanguage.Unknown;
        }

        public void Dispose()
        {
            LocalizationSettings.InitializationOperation.Completed -= OnLocalizationInitialized;
        }
    }
}