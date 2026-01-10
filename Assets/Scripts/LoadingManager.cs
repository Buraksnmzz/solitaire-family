using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Loading;
using Levels;
using Services;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private TextAsset localConfigurationJson;
    [SerializeField] private TextAsset localLevelsJson;

    private static bool _bootServicesInstalled;
    private SystemLanguage _startupLanguage;

    private void Awake()
    {
        InstallBootServices();
    }

    private void Start()
    {
        _startupLanguage = ResolveStartupLanguage();
        StartCoroutine(InitializeGame());
    }

    private void InstallBootServices()
    {
        if (_bootServicesInstalled)
        {
            return;
        }

        ServiceLocator.Register<ISavedDataService>(new SavedDataService());
        ServiceLocator.Register<ILocalizationService>(new LocalizationService());
        ServiceLocator.Register<ILevelMapCacheService>(new LevelMapCacheService());
        _bootServicesInstalled = true;
    }

    private static SystemLanguage ResolveStartupLanguage()
    {
        var savedDataService = ServiceLocator.GetService<ISavedDataService>();
        var settingsModel = savedDataService.GetModel<SettingsModel>();
        if (settingsModel.CurrentLanguage != SystemLanguage.Unknown)
        {
            return settingsModel.CurrentLanguage;
        }

        var deviceLanguage = Application.systemLanguage;
        return deviceLanguage != SystemLanguage.Unknown ? deviceLanguage : SystemLanguage.English;
    }

    private IEnumerator InitializeGame()
    {
        float timeoutTimer = 0f;
        const float maxWaitTime = 5f;

        YoogoLabManager.TryStartGame(() =>
        {
            StartCoroutine(WaitForRemoteConfig());
        });

        IEnumerator WaitForRemoteConfig()
        {
            while (!YoogoLabManager.IsRemoteConfigReady() && timeoutTimer < maxWaitTime)
            {
                timeoutTimer += Time.deltaTime;
                yield return null;
            }

            var localConfiguration = GetLocalRawJson();
            var remoteConfiguration = YoogoLabManager.IsRemoteConfigReady() ? YoogoLabManager.GetRemoteConfig() : string.Empty;
            var configurationJson = BuildConfigurationJson(localConfiguration, remoteConfiguration);

            InitializeConfiguration(configurationJson);
            yield return InitializeLevels(configurationJson);

            SceneManager.LoadScene(gameSceneName);
        }
        yield break;
    }

    private static string BuildConfigurationJson(string localConfigurationJsonValue, string remoteConfigRawJson)
    {
        if (string.IsNullOrWhiteSpace(localConfigurationJsonValue))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(remoteConfigRawJson) || remoteConfigRawJson == "{}")
        {
            return localConfigurationJsonValue;
        }

        if (RemoteConfigConfigurationMerger.TryBuildConfigurationJson(localConfigurationJsonValue, remoteConfigRawJson, out var merged))
        {
            return merged;
        }

        return localConfigurationJsonValue;
    }

    private void InitializeConfiguration(string rawJson)
    {
        BootCache.SetConfigurationJson(rawJson);
    }

    private IEnumerator InitializeLevels(string rawJson)
    {
        var levelsJson = string.Empty;
        var configurationJson = string.IsNullOrEmpty(rawJson) ? GetLocalRawJson() : rawJson;
        var levelDataUrl = LevelDataUrlResolver.ResolveLevelDataUrl(configurationJson, _startupLanguage, false, false);
        var cacheService = ServiceLocator.GetService<ILevelMapCacheService>();
        if (string.IsNullOrEmpty(levelDataUrl))
        {
            levelsJson = cacheService.TryGetLevelsJson(_startupLanguage, out var cachedLevelsJson)
                ? cachedLevelsJson
                : GetLocalLevelJson();
        }
        else
        {
            yield return FetchLevelJson(levelDataUrl, value => levelsJson = value);
            if (string.IsNullOrEmpty(levelsJson) || levelsJson == "{}")
            {
                levelsJson = cacheService.TryGetLevelsJson(_startupLanguage, out var cachedLevelsJson)
                    ? cachedLevelsJson
                    : GetLocalLevelJson();
            }
            else
            {
                cacheService.SaveLevelsJson(_startupLanguage, levelsJson);
            }
        }
        BootCache.SetLevelsJson(levelsJson);
    }

    private IEnumerator FetchLevelJson(string levelDataUrl, System.Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(levelDataUrl))
        {
            onComplete?.Invoke(string.Empty);
            yield break;
        }

        using (var request = UnityWebRequest.Get(levelDataUrl))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            var result = request.result == UnityWebRequest.Result.Success
                ? request.downloadHandler.text
                : string.Empty;

            onComplete?.Invoke(result);
        }
    }

    private string GetLocalRawJson()
    {
        return localConfigurationJson != null ? localConfigurationJson.text : string.Empty;
    }

    private string GetLocalLevelJson()
    {
        return localLevelsJson != null ? localLevelsJson.text : string.Empty;
    }
}
