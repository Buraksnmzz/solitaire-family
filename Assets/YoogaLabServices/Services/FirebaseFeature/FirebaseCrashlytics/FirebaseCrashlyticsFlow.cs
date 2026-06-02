using Firebase.Crashlytics;
using System;
using UnityEngine;

namespace ServicesPackage
{
    public class FirebaseCrashlyticsFlow
    {
        private readonly FirebaseCrashlyticsContext ctx;
        private FirebaseCoreContext coreCtx;

        public FirebaseCrashlyticsFlow(FirebaseCrashlyticsContext context)
        {
            ctx = context;
        }

        public void Initialize(FirebaseCoreContext coreContext)
        {
            coreCtx = coreContext;
            Application.logMessageReceived += HandleLog;
            AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        }

        public void Setup()
        {
            Crashlytics.IsCrashlyticsCollectionEnabled = false;
            ServicesLogger.Log("[Crashlytics] Initialized.");
        }

        #region Crashlytics
        public void LogNonFatalError(string message)
        {
            if (coreCtx.isInitialized)
            {
                Crashlytics.Log(message);
            }
            else
            {
                ServicesLogger.LogWarning("[FirebaseManager] Cannot log error. Firebase is not initialized.");
            }
        }

        public void LogException(Exception ex)
        {
            if (coreCtx.isInitialized)
            {
                Crashlytics.LogException(ex);
                ServicesLogger.LogError($"[FirebaseManager] Exception logged: {ex.Message}");
            }
            else
            {
                ServicesLogger.LogWarning("[FirebaseManager] Cannot log exception. Firebase is not initialized.");
            }
        }

        public void ForceCrash()
        {
            if (coreCtx.isInitialized)
            {
                Crashlytics.Log("Forcing crash for testing.");
                throw new Exception("Test Crash");
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!coreCtx.isInitialized) return;

            if (type == LogType.Exception || type == LogType.Error)
            {
                LogNonFatalError($"{logString}\n{stackTrace}");
            }
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex);
            }
        }
        #endregion
    }
}
