using UnityEngine;
using System.Runtime.InteropServices;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
namespace ServicesPackage
{
    public static class ServicesLogger
    {
        public static void Log(string message)
        {
            if (IsAllowed())
                Debug.Log($"[YoogaLabSDK] {message}");
        }

        public static void LogWarning(string message)
        {
            if (IsAllowed())
                Debug.LogWarning($"[YoogaLabSDK] {message}");
        }

        public static void LogError(string message)
        {
            if (IsAllowed())
                Debug.LogError($"[YoogaLabSDK] {message}");
        }

        private static bool IsAllowed()
        {
#if UNITY_IOS
    // iOS: native flag ONLY
    return NativeLogging.IsEnabled();
#else
            return ServicesInitializer.Instance != null && ServicesInitializer.Instance.canLog;
#endif
        }
    }

    public static class NativeLogging
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern bool Yooga_IsLoggingEnabledFromPlist();
#endif

        public static bool IsEnabled()
        {
#if UNITY_IOS
            try
            {
                return Yooga_IsLoggingEnabledFromPlist();
            }
            catch
            {
                // Fail-safe: if symbol missing, assume off
                return false;
            }
#else
            return true;
#endif
        }
    }

}

// to log on ios use a new scheme
//YOOGA_LOGGING 1 = enabled services ca
