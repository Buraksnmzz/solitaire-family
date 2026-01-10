using System.Collections;
using Loading;
using Services;
using UI.Signals;
using UI.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace Levels
{
    public class LevelMapLanguageSyncService : ILevelMapLanguageSyncService
    {
        private readonly IEventDispatcherService _eventDispatcherService;
        private readonly ILevelGeneratorService _levelGeneratorService;
        private readonly ISnapshotService _snapshotService;
        private readonly ILocalizationService _localizationService;
        private readonly ILevelMapCacheService _levelMapCacheService;
        private readonly IUIService _uiService;
        private Coroutine _runningCoroutine;
        private bool _isWaitingForLanguageChange;

        public LevelMapLanguageSyncService()
        {
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _levelGeneratorService = ServiceLocator.GetService<ILevelGeneratorService>();
            _snapshotService = ServiceLocator.GetService<ISnapshotService>();
            _localizationService = ServiceLocator.GetService<ILocalizationService>();
            _levelMapCacheService = ServiceLocator.GetService<ILevelMapCacheService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _eventDispatcherService.AddListener<LanguageChangeRequestedSignal>(OnLanguageChangeRequested);
        }

        private void OnLanguageChangeRequested(LanguageChangeRequestedSignal signal)
        {
            if (_localizationService.GetCurrentLanguage() == signal.Language)
            {
                return;
            }

            ShowWaiting();

            if (_runningCoroutine != null)
            {
                MonoHelper.Instance.StopCoroutine(_runningCoroutine);
                _runningCoroutine = null;
            }

            _runningCoroutine = MonoHelper.Instance.StartCoroutine(ReloadLevelsForLanguageRoutine(signal.Language));
        }

        private IEnumerator ReloadLevelsForLanguageRoutine(SystemLanguage language)
        {
            var configurationJson = BootCache.ConfigurationJson;
            if (string.IsNullOrWhiteSpace(configurationJson))
            {
                FailAndHideWaiting();
                yield break;
            }

            var levelDataUrl = LevelDataUrlResolver.ResolveLevelDataUrl(configurationJson, language, false, false);
            if (string.IsNullOrWhiteSpace(levelDataUrl))
            {
                FailAndHideWaiting();
                yield break;
            }

            using (var request = UnityWebRequest.Get(levelDataUrl))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    if (TryApplyCachedLevelsForLanguage(language))
                    {
                        yield break;
                    }

                    FailAndHideWaiting();
                    yield break;
                }

                var levelsJson = request.downloadHandler.text;
                if (string.IsNullOrWhiteSpace(levelsJson) || levelsJson == "{}")
                {
                    if (TryApplyCachedLevelsForLanguage(language))
                    {
                        yield break;
                    }

                    FailAndHideWaiting();
                    yield break;
                }

                try
                {
                    _levelMapCacheService.SaveLevelsJson(language, levelsJson);
                    ApplyLevelsAndSwitchLanguage(language, levelsJson);
                }
                catch
                {
                    FailAndHideWaiting();
                }
            }
        }

        private bool TryApplyCachedLevelsForLanguage(SystemLanguage language)
        {
            if (!_levelMapCacheService.TryGetLevelsJson(language, out var cachedLevelsJson))
            {
                return false;
            }

            try
            {
                ApplyLevelsAndSwitchLanguage(language, cachedLevelsJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ApplyLevelsAndSwitchLanguage(SystemLanguage language, string levelsJson)
        {
            BootCache.SetLevelsJson(levelsJson);
            _levelGeneratorService.ParseLevelsJson(levelsJson);
            _snapshotService.ClearSnapshot();
            _localizationService.SetLanguage(language);
        }

        private void ShowWaiting()
        {
            if (_isWaitingForLanguageChange)
            {
                return;
            }

            _isWaitingForLanguageChange = true;
            _eventDispatcherService.AddListener<LanguageChangedSignal>(OnLanguageChanged);
            _uiService.ShowPopup<WaitingPresenter>();
        }

        private void OnLanguageChanged(LanguageChangedSignal _)
        {
            HideWaiting();
        }

        private void FailAndHideWaiting()
        {
            HideWaiting();
        }

        private void HideWaiting()
        {
            if (!_isWaitingForLanguageChange)
            {
                return;
            }

            _isWaitingForLanguageChange = false;
            _eventDispatcherService.RemoveListener<LanguageChangedSignal>(OnLanguageChanged);

            if (_runningCoroutine != null)
            {
                _runningCoroutine = null;
            }

            _uiService.HidePopup<WaitingPresenter>();
        }
    }
}
