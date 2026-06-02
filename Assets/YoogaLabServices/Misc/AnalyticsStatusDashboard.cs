using UnityEngine;
using TMPro;
using System.Text;

namespace ServicesPackage
{
    public class AnalyticsStatusDashboard : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private float refreshInterval = 2f;

        private StringBuilder _builder = new();
        private FeatureRegistry _registry;
        private int rewardCount;

        private void Start()
        {
            _registry = AppRegistry.Registry;

            if (statusText == null)
            {
                Debug.LogWarning("[AnalyticsStatusDashboard] No TMP_Text assigned.");
                enabled = false;
                return;
            }

            InvokeRepeating(nameof(UpdateStatus), 0f, refreshInterval);
        }

        private void UpdateStatus()
        {
            _builder.Clear();

            _builder.AppendLine("Yoogalab Services Status");
            _builder.AppendLine("--------------------------------");

            DrawFirebaseCore();
            DrawFirebaseAnalytics();
            DrawAdjust();
            DrawRemoteConfig();
            DrawConsent();
            DrawAds();
            DrawRevenue();
            DrawNotifications();

            DrawRewardedTest();

            _builder.AppendLine("--------------------------------");
            _builder.AppendLine($"Last Updated: {System.DateTime.Now:T}");

            statusText.text = _builder.ToString();
        }

        private void DrawFirebaseCore()
        {
            if (_registry.TryResolve(out FirebaseCoreContext core))
            {
                _builder.AppendLine($"Firebase Core: {(core.isInitialized ? "Initialized" : "Not Ready")}");
            }
            else
                _builder.AppendLine("Firebase Core: Missing");
        }

        private void DrawFirebaseAnalytics()
        {
            if (_registry.TryResolve(out FirebaseAnalyticsContext ctx))
            {
                bool enabled = ctx.analyticsEnabled;
                _builder.AppendLine($"Firebase Analytics: {(enabled ? "Active" : "Inactive")}");
            }
            else
                _builder.AppendLine("Firebase Analytics: Missing");
        }

        private void DrawAdjust()
        {
            if (_registry.TryResolve(out AdjustCoreContext ctx))
            {
                _builder.AppendLine($"Adjust: {(ctx.isInitialized ? "Initialized" : "Not Ready")}");
                _builder.AppendLine($"Adjust Token: {(string.IsNullOrEmpty(ctx.currentAdjustAppToken) ? "(none)" : ctx.currentAdjustAppToken)}");
            }
            else
                _builder.AppendLine("Adjust: Missing");
        }

        private void DrawRemoteConfig()
        {
            if (_registry.TryResolve(out RemoteConfigContext ctx))
            {
                _builder.AppendLine($"Remote Config: {(ctx.IsInitialized ? "Fetched" : "Pending")}");

                int shown = 0;
                foreach (var kv in ctx.configDataValues)
                {
                    _builder.AppendLine($" • {kv.Key} = {kv.Value}");
                    if (++shown >= 3) break;
                }
            }
            else
                _builder.AppendLine("Remote Config: Missing");
        }

        private void DrawConsent()
        {
            if (_registry.TryResolve(out ConsentContext ctx))
            {
                _builder.AppendLine($"Consent: {(ctx.IsConsentGiven ? "Granted" : "Denied / Not Set")}");
            }
            else
                _builder.AppendLine("Consent: Missing");
        }

        private void DrawAds()
        {
            if (_registry.TryResolve(out AdsSystemContext ctx))
            {
                _builder.AppendLine($"Ads System: {(ctx.isInitialized ? "Initialized" : "Not Ready")}");
                _builder.AppendLine($" • Banner ID: {ctx.bannerAdUnitId}");
                _builder.AppendLine($" • Interstitial ID: {ctx.interstitialAdUnitId}");
                _builder.AppendLine($" • Rewarded ID: {ctx.rewardedAdUnitId}");
            }
            else
                _builder.AppendLine("Ads System: Missing");
        }

        private void DrawRevenue()
        {
            if (_registry.TryResolve(out RevenueManagerContext ctx))
            {
                bool ready = !string.IsNullOrEmpty(ctx.currToken);
                _builder.AppendLine($"Revenue Manager: {(ready ? "Active" : "Inactive")}");
                _builder.AppendLine($" • Token: {ctx.currToken}");
            }
            else
                _builder.AppendLine("Revenue Manager: Missing");
        }

        private void DrawNotifications()
        {
            if (_registry.TryResolve(out NotificationsContext ctx))
            {
                _builder.AppendLine($"Notifications: {(ctx.permissionGranted ? "Granted" : "Not Granted / Unknown")}");
            }
            else
                _builder.AppendLine("Notifications: Missing");
        }
        private void DrawRewardedTest()
        {
            _builder.AppendLine($"Reward Test Count: {rewardCount}");
        }

        //Button
        public void ShowBanner() { YoogoLabManager.ShowBanner();  }
        public void HideBanner() { YoogoLabManager.HideBanner(); }
        public void ShowInterstitial() { YoogoLabManager.ShowInterstitial(); }

        public void ShowRewarded()
        {
            YoogoLabManager.PlayRewarded(OnRewardedFinished);
        }

        private void OnRewardedFinished(bool success)
        {
            if (!success)
            {
                Debug.Log("[Reward] User failed to complete reward.");
                return;
            }

            rewardCount++;

            Debug.Log($"[Reward] Reward completed! Total: {rewardCount}");
        }
        public void SendIapEvent()
        {
            SignalBusService.Fire(new LogIAPEventSignal {
                currency = "dol",
                price = 0,
                productId = "test",
                transactionId = "test",
            });
        }
         public void SetFirebaseLog()
        {
            SignalBusService.Fire(new LogFirebaseEventSignal
            {
                eventName = "ad_impression",
                parameters = new (string, object)[]
                             {
                                ("ad_platform", "AppLovin"),
                                ("ad_source", "unknown"),
                                ("ad_format",  "unknown"),
                                ("ad_unit_name",  "unknown"),
                                ("value", 0),
                                ("currency", "USD")
                             }
            });
        }

        public void DumpRemoteConfig()
        {
            if (!YoogoLabManager.IsRemoteConfigReady())
            {
                Debug.LogWarning("[RemoteConfig Dump] Remote Config NOT ready yet.");
                return;
            }

            var json = YoogoLabManager.GetRemoteConfig();

            if (string.IsNullOrEmpty(json) || json == "{}")
            {
                Debug.LogWarning("[RemoteConfig Dump] Remote Config is empty.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("---- REMOTE CONFIG DUMP ----");
            sb.AppendLine(json);
            sb.AppendLine("--------------------------------");

            Debug.Log(sb.ToString());
        }
    }
}
