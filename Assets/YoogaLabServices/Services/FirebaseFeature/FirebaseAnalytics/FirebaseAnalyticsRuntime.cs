using System;
using UnityEngine;
using Firebase.Analytics;

namespace ServicesPackage
{
    public sealed class FirebaseAnalyticsRuntime : MonoBehaviour
    {
        private FirebaseAnalyticsContext _ctx;

        public void Initialize(FirebaseAnalyticsFlow flow, FirebaseAnalyticsContext context)
        {
            _ctx = context;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!_ctx.analyticsEnabled)
                return;

            if (!pause)
            {
                ServicesLogger.Log("[FirebaseAnalyticsRuntime] App resumed from background");

                FirebaseAnalytics.LogEvent(
                    "app_pause",
                    new Parameter("source", "resume"),
                    new Parameter("time", DateTime.UtcNow.ToString("o"))
                );
            }
        }
    }
}