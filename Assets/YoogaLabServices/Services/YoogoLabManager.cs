using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using ServicesPackage;

public class YoogoLabManager
{
    public static event Action<bool> onRewardedCompleted;
    public static event Action onRewardedReady;

    public static bool isSystemReady = false;
    private static Action _onReadyCallback;

    private static bool isWaitingForRemoteConfig = false;

    public static Func<bool> CheckHasNoAds;

    /// <summary>
    /// Used in bootstraper when the sdk has finished loading at least the remote config
    /// </summary>
    public static void MarkSystemReady()
    {
        if (isSystemReady)
            return;

        isSystemReady = true;

        ServicesLogger.Log("[YoogoLabManager] System is now ready.");
        _onReadyCallback?.Invoke();
        _onReadyCallback = null;
    }

    /// <summary>
    /// Called by the game to attempt game start.
    /// If the plugin is not ready, the callback is stored and invoked later.
    /// </summary>
    public static void TryStartGame(Action callback)
    {
        if (isSystemReady)
        {
            callback?.Invoke();
        }
        else
        {
            ServicesLogger.Log("[YoogoLabManager] System not ready, deferring start.");
            _onReadyCallback = callback;
        }
    }

    public static void SetFirebaseUserProperty(string property, string value)
    {
        //PluginFirebaseManager.SetUserProperty(property, value);
    }

    public static void LogFirebaseEvent(string eventName, params object[] parameters)
    {
        if (!AppRegistry.Registry.Resolve<FirebaseCoreContext>().isInitialized)
        {
            ServicesLogger.LogWarning("[YoogoLabManager] Firebase is not initialized!");
            return;
        }
        if (string.IsNullOrEmpty(eventName))
        {
            ServicesLogger.LogWarning("[YoogoLabManager] Event name is null or empty. Skipping log.");
            return;
        }

#if (UNITY_EDITOR && LOG)
    string _args = "";
    if (parameters != null)
    {
        for (int i = 0; i < parameters.Length; i += 2)
        {
            string key = parameters[i]?.ToString() ?? "null";
            string val = (i + 1 < parameters.Length && parameters[i + 1] != null) ? parameters[i + 1].ToString() : "null";
            _args += $" {key}={val}";
        }
    }
    PluginLogger.Log($"[YoogoLabManager] Firebase Event Logged: {eventName}{_args}");
    return;
#endif

        if (parameters == null || parameters.Length == 0)
        {
            SignalBusService.Fire(new LogFirebaseEventSignal
            {
                eventName = eventName,
                parameters = null
            });
            return;
        }


        if (parameters.Length % 2 != 0)
        {
            ServicesLogger.LogWarning($"[YoogoLabManager] Invalid parameter count for Firebase event '{eventName}'");
            SignalBusService.Fire(new LogFirebaseEventSignal
            {
                eventName = eventName,
                parameters = null
            });
            return;
        }

        var firebaseParams = new (string, object)[parameters.Length / 2];
        for (int i = 0, j = 0; i < parameters.Length; i += 2, j++)
        {
            string key = parameters[i]?.ToString() ?? $"param_{j}";
            object value = parameters[i + 1] ?? "null";
            firebaseParams[j] = (key, value);
        }

        SignalBusService.Fire(new LogFirebaseEventSignal
        {
            eventName = eventName,
            parameters = firebaseParams
        });
    }

    public static void ShowInterstitial()
    {
        YoogaAds.ShowInterstitial();
    }

    internal static void NotifyRewardedReady()
    {
        onRewardedReady?.Invoke();
    }

    public static void PlayRewarded(Action<bool> callback)
    {
        onRewardedCompleted = callback;
        YoogaAds.ShowRewarded(null);
    }

    public static void OnRewardedFinished(bool success)
    {
        ServicesLogger.Log($"[YoogoLabManager] Reward result: {success}");

        var handler = onRewardedCompleted;
        onRewardedCompleted = null; 

        handler?.Invoke(success);
    }

    /// <summary>
    /// Checks for rewarded ad availability, invokes the corresponding callback, and returns the availability status.
    /// </summary>
    /// <param name="onAvailable">Called if a rewarded ad is available.</param>
    /// <param name="onUnavailable">Called if no rewarded ad is available.</param>
    /// <returns>True if the ad is available; false otherwise.</returns>
    public static bool RewardedAvailable(Action onAvailable, Action onUnavailable)
    {
        bool available = YoogaAds.IsRewardedAvailable();

        if (available)
        {
            ServicesLogger.Log("[YoogoLabManager] Rewarded ad available.");
            onAvailable?.Invoke();
        }
        else
        {
            ServicesLogger.LogWarning("[YoogoLabManager] Rewarded ad NOT available.");
            onUnavailable?.Invoke();
        }

        return available;
    }

    public static void Vibrate(int ms)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject vibratorService = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

        if (vibratorService != null)
        {
            if (AndroidVersionIsAtLeast(26))
            {
                // API 26+: use VibrationEffect
                AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                AndroidJavaObject vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(
                    "createOneShot", ms, vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE")
                );
                vibratorService.Call("vibrate", vibrationEffect);
            }
            else
            {
                // Legacy vibration
                vibratorService.Call("vibrate", (long)ms);
            }
        }
    }
    catch (System.Exception e)
    {
        ServicesLogger.LogWarning("[YoogoLabManager] Vibration failed: " + e.Message);
    }
#elif UNITY_IOS && !UNITY_EDITOR
    Handheld.Vibrate();
#else
        ServicesLogger.Log("[YoogoLabManager] Vibration triggered (Editor or unsupported platform).");
#endif
    }

    private static bool AndroidVersionIsAtLeast(int version)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION");
    int sdkInt = versionClass.GetStatic<int>("SDK_INT");
    return sdkInt >= version;
#else
        return false;
#endif
    }

    public static void ShowNativeReview()
    {
        SignalBusService.Fire(new RequestReviewSignal(),true);
    }

    public static void IAP(Product product, bool isRestore = false)
    {
        if (isRestore)
        {
            Debug.Log("[YoogoLabManager] Skipping IAP tracking because this is a restore call.");
            return;
        }

        if (product == null)
        {
            ServicesLogger.LogError("[YoogoLabManager] Product is null! Cannot track IAP.");
            return;
        }

        Debug.Log($"[YoogoLabManager] Tracking IAP: {product.transactionID} - {product.definition.id} - " +

                  $"{product.metadata.localizedPrice} {product.metadata.isoCurrencyCode}");

        SignalBusService.Fire(new LogIAPEventSignal
        {
            transactionId = product.transactionID,
            price = (double)product.metadata.localizedPrice,
            currency = product.metadata.isoCurrencyCode,
            productId = product.definition.id
        });
    }

    public static void ShowBanner()
    {
        YoogaAds.ShowBanner();
#if UNITY_EDITOR
        SimulateEditorBanner();
#endif
    }

    public static void HideBanner()
    {
        YoogaAds.HideBanner();
#if UNITY_EDITOR
        if (_simulatedBannerCanvas != null)
        {
            _simulatedBannerCanvas.SetActive(false);
        }
#endif
    }

#if UNITY_EDITOR
    private static GameObject _simulatedBannerCanvas;

    /// <summary>
    /// Simulates a banner ad in the Unity Editor for layout and UI testing.
    /// Creates the banner once and toggles its visibility on Show/Hide calls.
    /// Intended only for use in the Editor as a development aid.
    /// </summary>
    private static void SimulateEditorBanner()
    {
        if (_simulatedBannerCanvas != null)
        {
            _simulatedBannerCanvas.SetActive(true);
            return;
        }
        Debug.Log("[SimulatedBanner] Create/enable banner sim");

        GameObject parentGO = GameObject.FindObjectOfType<ServicesInitializer>()?.gameObject;

        if (parentGO == null)
        {
            Debug.LogWarning("[SimulatedBanner] YoogoLabManager instance not found. Banner will be unparented.");
        }

        _simulatedBannerCanvas = new GameObject("SimulatedBannerCanvas");

        if (parentGO != null)
        {
            _simulatedBannerCanvas.transform.SetParent(parentGO.transform, false);
        }

        var canvas = _simulatedBannerCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        _simulatedBannerCanvas.AddComponent<CanvasScaler>();
        _simulatedBannerCanvas.AddComponent<GraphicRaycaster>();

        var bannerGO = new GameObject("SimulatedBanner");
        bannerGO.transform.SetParent(_simulatedBannerCanvas.transform, false);

        var image = bannerGO.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(1f, 0f, 1f, 0.5f); // Magenta

        var rectTransform = image.GetComponent<RectTransform>();

        float bannerHeight = Screen.height * GetBannerHeightPercent();
        float bottomInset = Mathf.Max(0, Screen.safeArea.y);

        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = new Vector2(0f, bottomInset);
        rectTransform.sizeDelta = new Vector2(0f, bannerHeight);
    }
#endif

    /// <summary>
    /// This was originaly used in the native Java probably for repositioning the screen elements to accomodate the banner
    /// if needed it can be used, if not adsmanager uses smart banners with fixed height.
    /// </summary>
    /// <returns></returns>
    public static float GetBannerHeightPercent()
    {
        return 0.08f;
    }

    public static bool IsRemoteConfigReady()
    {
        if (Time.realtimeSinceStartup < 1f)
            return false;

        if (!AppRegistry.Registry.TryResolve<RemoteConfigContext>(out var rc))
            return false;

        if (!rc.IsInitialized)
        {
            if (!isWaitingForRemoteConfig)
            {
                isWaitingForRemoteConfig = true;
                ServiceCoroutineRunner.StartSafe(WaitForRemoteConfig());
            }
            return false;
        }

        return true;
    }

    public static string GetRemoteConfig()
    {
        var rc = AppRegistry.Registry.Resolve<RemoteConfigContext>();

        if (!rc.IsInitialized)
            return "{}";

        return rc.cachedJsonConfig;
    }

    private static IEnumerator WaitForRemoteConfig()
    {
        float timeout = 20f;
        float timer = 0f;

        AppRegistry.Registry.TryResolve<RemoteConfigContext>(out var rc);

        while (rc != null && !rc.IsInitialized && timer < timeout)
        {
            yield return new WaitForSeconds(0.5f);
            timer += 0.5f;
        }

        isWaitingForRemoteConfig = false;

        if (rc == null || !rc.IsInitialized)
        {
            ServicesLogger.LogWarning("[YoogoLabManager] Remote Config fetch timed out after 20 seconds.");
        }
        else
        {
            ServicesLogger.Log("[YoogoLabManager] Remote Config ready after wait.");
        }
    }

    public static void LevelEnd(int id)
    {
        ServicesLogger.Log("Level End called");
        SignalBusService.Fire(new TrackLevelEndSignal { level_ = id}, true);
    }
}