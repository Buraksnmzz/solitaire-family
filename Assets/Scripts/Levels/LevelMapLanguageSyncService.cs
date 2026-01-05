using System.Collections;
using Loading;
using Services;
using UI.Signals;
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
        private Coroutine _runningCoroutine;

        public LevelMapLanguageSyncService()
        {
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _levelGeneratorService = ServiceLocator.GetService<ILevelGeneratorService>();
            _snapshotService = ServiceLocator.GetService<ISnapshotService>();
            _localizationService = ServiceLocator.GetService<ILocalizationService>();
            _eventDispatcherService.AddListener<LanguageChangeRequestedSignal>(OnLanguageChangeRequested);
        }

        private void OnLanguageChangeRequested(LanguageChangeRequestedSignal signal)
        {
            if (_localizationService.GetCurrentLanguage() == signal.Language)
            {
                return;
            }

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
                yield break;
            }

            var levelDataUrl = LevelDataUrlResolver.ResolveLevelDataUrl(configurationJson, language, false, false);
            if (string.IsNullOrWhiteSpace(levelDataUrl))
            {
                yield break;
            }

            using (var request = UnityWebRequest.Get(levelDataUrl))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    yield break;
                }

                var levelsJson = request.downloadHandler.text;
                if (string.IsNullOrWhiteSpace(levelsJson) || levelsJson == "{}")
                {
                    yield break;
                }

                BootCache.SetLevelsJson(levelsJson);
                _levelGeneratorService.ParseLevelsJson(levelsJson);
                _snapshotService.ClearSnapshot();

                _localizationService.SetLanguage(language);
            }
        }
    }
}
