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
    [SerializeField] private BootData bootData;

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

                string rawJson = bootData != null && YoogoLabManager.IsRemoteConfigReady()
                    ? YoogoLabManager.GetRemoteConfig()
                    : GetLocalRawJson();
                
                if(string.IsNullOrEmpty(rawJson) || rawJson == "{}")
                    rawJson = GetLocalRawJson();

                InitializeConfiguration(rawJson);
                yield return InitializeLevels(rawJson);

                SceneManager.LoadScene(gameSceneName);
            }
            yield break;
        }

        private void InitializeConfiguration(string rawJson)
        {
            if (bootData == null)
            {
                return;
            }
            bootData.configurationJson = rawJson;
        }

        private IEnumerator InitializeLevels(string rawJson)
        {
            if (bootData == null)
            {
                yield break;
            }

            var levelsJson = string.Empty;
            var levelDataUrl = ExtractLevelDataUrl(rawJson);
            if (string.IsNullOrEmpty(levelDataUrl))
            {
                levelsJson = GetLocalLevelJson();
            }
            else
            {
                levelsJson = GetLevelJsonFromUrl(levelDataUrl);
                if(string.IsNullOrEmpty(levelsJson) ||  levelsJson == "{}")
                    levelsJson = GetLocalLevelJson();
            }
            bootData.levelsJson = levelsJson;
        }

        private string GetLevelJsonFromUrl(string levelDataUrl)
        {
            if (string.IsNullOrEmpty(levelDataUrl))
            {
                return string.Empty;
            }

            string result = string.Empty;
            bool isCompleted = false;

            IEnumerator RequestCoroutine()
            {
                using (var request = UnityWebRequest.Get(levelDataUrl))
                {
                    yield return request.SendWebRequest();

                    result = request.result == UnityWebRequest.Result.Success
                        ? request.downloadHandler.text
                        : string.Empty;
                }

                isCompleted = true;
            }

            StartCoroutine(RequestCoroutine());

            const float maxWaitTime = 5f;
            float timer = 0f;

            while (!isCompleted && timer < maxWaitTime)
            {
                timer += Time.deltaTime;
            }

            return result;
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
