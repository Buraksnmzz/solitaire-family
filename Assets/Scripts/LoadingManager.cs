using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using Loading;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private TextAsset localConfigurationJson;
    [SerializeField] private TextAsset localLevelsJson;

    private void Start()
    {
        StartCoroutine(InitializeGame());
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

            var rawJson = YoogoLabManager.IsRemoteConfigReady()
                ? YoogoLabManager.GetRemoteConfig()
                : GetLocalRawJson();

            if (string.IsNullOrEmpty(rawJson) || rawJson == "{}")
            {
                rawJson = GetLocalRawJson();
            }

            InitializeConfiguration(rawJson);
            yield return InitializeLevels(rawJson);

            SceneManager.LoadScene(gameSceneName);
        }
        yield break;
    }

    private void InitializeConfiguration(string rawJson)
    {
        BootCache.SetConfigurationJson(rawJson);
    }

    private IEnumerator InitializeLevels(string rawJson)
    {
        var levelsJson = string.Empty;
        var levelDataUrl = ExtractLevelDataUrl(rawJson);
        if (string.IsNullOrEmpty(levelDataUrl))
        {
            levelsJson = GetLocalLevelJson();
        }
        else
        {
            yield return FetchLevelJson(levelDataUrl, value => levelsJson = value);
            if (string.IsNullOrEmpty(levelsJson) || levelsJson == "{}")
            {
                levelsJson = GetLocalLevelJson();
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

    private string ExtractLevelDataUrl(string rawJson)
    {
        var configurationJson = GetLocalRawJson();
        if (rawJson != null)
            configurationJson = rawJson;

        var url = string.Empty;
        if (!string.IsNullOrEmpty(configurationJson))
        {
            var root = JsonUtility.FromJson<GameConfigurationRoot>(configurationJson);
            url = root != null ? root.levelDataUrl : string.Empty;
        }
        return url;
    }

    private string GetLocalRawJson()
    {
        return localConfigurationJson != null ? localConfigurationJson.text : string.Empty;
    }

    private string GetLocalLevelJson()
    {
        return localLevelsJson != null ? localLevelsJson.text : string.Empty;
    }

    [System.Serializable]
    private class GameConfigurationRoot
    {
        public string levelDataUrl;
    }

}
