using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
#if UNIYT_IOS
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;

namespace ServicesPackage
{
    public class FirebaseAnalyticsFlow
    {
        private readonly FirebaseAnalyticsContext ctx;
        private FeatureRegistry registry;
        private FirebaseCoreContext core;

        public FirebaseAnalyticsFlow(FirebaseAnalyticsContext context)
        {
            ctx = context;
        }

        public void Initialize(FirebaseCoreContext context, FeatureRegistry featureRegistry)
        {
            core = context;
            registry = featureRegistry;
        }

        public void Setup()
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        }

        private void DebugConsent(string source, string message)
        {
            ServicesLogger.Log($"[CMP DEBUG] [{source}] {message}");

            SignalBusService.Fire(
                new ConsentDebugSignal
                {
                    source = source,
                    message = message
                },
                true
            );
        }

        // public void EnableAnalyticsAndCrashlytics(bool consentGiven)
        // {
        //     if (!core.isInitialized)
        //     {
        //         ServicesLogger.LogWarning("[FirebaseManager] Firebase not initialized yet!");
        //         return;
        //     }

        //     bool isGdprRegion = registry.Resolve<RegionContext>().IsGDPRCountry();

        //     bool enableAnalytics = isGdprRegion ? consentGiven : true;

        //     Crashlytics.IsCrashlyticsCollectionEnabled = enableAnalytics;
        //     ctx.analyticsEnabled = enableAnalytics;

        //     var consentMap = new Dictionary<ConsentType, ConsentStatus>
        //     {
        //         { ConsentType.AdStorage, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied },
        //         { ConsentType.AnalyticsStorage, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied },
        //         { ConsentType.AdUserData, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied },
        //         { ConsentType.AdPersonalization, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied }
        //     };

        //     FirebaseAnalytics.SetConsent(consentMap);

        //     if (enableAnalytics)
        //     {
        //         FirebaseAnalytics.LogEvent(
        //             FirebaseAnalytics.EventScreenView,
        //             new Parameter(FirebaseAnalytics.ParameterScreenName, "Bootstraper"),
        //             new Parameter(FirebaseAnalytics.ParameterScreenClass, "Bootstrapper")
        //         );
        //         FirebaseAnalytics.LogEvent(
        //             "ack_event",
        //             new Parameter("source", "startup")
        //         );
        //         LogFirebaseAppInstanceId();
        //     }

        //     string npaValue = isGdprRegion ? (consentGiven ? "0" : "1") : "0";
        //     SetUserProperty("npa", npaValue);
        //     SetUserProperty("gdpr_region", isGdprRegion ? "true" : "false");
        //     SetUserProperty("analytics_enabled", enableAnalytics ? "true" : "false");

        //     DebugConsent(

        //         "ANALYTICS",

        //         $"GDPR={isGdprRegion} | " +
        //         $"ConsentGiven={consentGiven} | " +
        //         $"AnalyticsEnabled={enableAnalytics} | " +
        //         $"Crashlytics={Crashlytics.IsCrashlyticsCollectionEnabled} | " +
        //         $"NPA={npaValue}"
        //     );

        //     ServicesLogger.Log($"[FirebaseManager] Analytics Enabled: {enableAnalytics}, NPA set to {npaValue}, GDPR: {isGdprRegion}");
        // }

        public void EnableAnalyticsAndCrashlytics(bool consentGiven)
        {
            if (!core.isInitialized)
            {
                ServicesLogger.LogWarning("[FirebaseManager] Firebase not initialized yet!");
                return;
            }

            bool isGdprRegion = registry.Resolve<RegionContext>().IsGDPRCountry();
            bool attAuthorized = true;

#if UNITY_IOS
            attAuthorized =
                ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED;
#endif

            bool personalizedAllowed = consentGiven && attAuthorized;

            bool enableAnalytics = true;
            Crashlytics.IsCrashlyticsCollectionEnabled = enableAnalytics;
            ctx.analyticsEnabled = enableAnalytics;

            var consentMap = new Dictionary<ConsentType, ConsentStatus>
            {
                { ConsentType.AdStorage, ConsentStatus.Granted },
                { ConsentType.AnalyticsStorage, ConsentStatus.Granted },
                { ConsentType.AdUserData, personalizedAllowed ? ConsentStatus.Granted : ConsentStatus.Denied },
                { ConsentType.AdPersonalization, personalizedAllowed ? ConsentStatus.Granted : ConsentStatus.Denied }
            };

            FirebaseAnalytics.SetConsent(consentMap);

            FirebaseAnalytics.LogEvent(
                FirebaseAnalytics.EventScreenView,
                new Parameter(FirebaseAnalytics.ParameterScreenName, "Bootstraper"),
                new Parameter(FirebaseAnalytics.ParameterScreenClass, "Bootstrapper")
            );

            FirebaseAnalytics.LogEvent(
                "ack_event",
                new Parameter("source", "startup")
            );

            LogFirebaseAppInstanceId();

            string npaValue = personalizedAllowed ? "0" : "1";

            SetUserProperty("npa", npaValue);
            SetUserProperty("gdpr_region", isGdprRegion ? "true" : "false");
            SetUserProperty("analytics_enabled", enableAnalytics ? "true" : "false");
            SetUserProperty("att_authorized", attAuthorized ? "true" : "false");
            SetUserProperty("personalized_allowed", personalizedAllowed ? "true" : "false");

            DebugConsent(
                "ANALYTICS",
                $"GDPR={isGdprRegion} | " +
                $"ConsentGiven={consentGiven} | " +
                $"ATT={attAuthorized} | " +
                $"PersonalizedAllowed={personalizedAllowed} | " +
                $"AnalyticsEnabled={enableAnalytics} | " +
                $"Crashlytics={Crashlytics.IsCrashlyticsCollectionEnabled} | " +
                $"NPA={npaValue}"
            );

            ServicesLogger.Log(
                $"[FirebaseManager] Analytics Enabled: {enableAnalytics}, " +
                $"ConsentGiven: {consentGiven}, " +
                $"ATT: {attAuthorized}, " +
                $"PersonalizedAllowed: {personalizedAllowed}, " +
                $"NPA: {npaValue}, " +
                $"GDPR: {isGdprRegion}"
            );
        }

        public void SetUserProperty(string property, string value)
        {
            if (core.isInitialized)
            {
                FirebaseAnalytics.SetUserProperty(property, value);
            }
            else
            {
                ServicesLogger.LogWarning("[FirebaseManager] Cannot set user property. Firebase is not initialized.");
            }
        }

        public async void LogFirebaseAppInstanceId()
        {
            if (!core.isInitialized)
            {
                ServicesLogger.LogWarning("[FirebaseManager] Cannot log App Instance ID. Firebase is not initialized.");
                return;
            }
            try
            {
                var instanceId = await FirebaseAnalytics.GetAnalyticsInstanceIdAsync();
                ServicesLogger.Log($"[FirebaseManager] App Instance ID: {instanceId}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FirebaseManager] Failed to get App Instance ID: {ex.Message}");
            }
        }

        public void LogEvent(string eventName, params (string, object)[] parameters)
        {

            ServicesLogger.Log(
                $"[FIREBASE ANALYTICS][REVENUELOG] Event received → {eventName} " +
                $"time={Time.realtimeSinceStartup:F2} " +
                $"params={parameters.Length}"
            );

            if (!core.isInitialized)
            {
                ServicesLogger.LogWarning("[FirebaseManager] Firebase is not initialized!");
                return;
            }

#if UNITY_IOS
            if (FirebaseApp.DefaultInstance == null)
            {
                ServicesLogger.LogWarning($"[FirebaseManager] [iOS] FirebaseApp.DefaultInstance is null. Skipping event: {eventName}");
                return;
            }
#endif
            try
            {
                List<Parameter> firebaseParameters = new List<Parameter>();
                foreach (var (key, value) in parameters)
                {
                    if (string.IsNullOrEmpty(key) || value == null)
                    {
                        ServicesLogger.LogWarning($"[FirebaseManager] Skipping invalid param: {key}={value}");
                        continue;
                    }

                    if (value is int intVal)
                        firebaseParameters.Add(new Parameter(key, intVal));
                    else if (value is float floatVal)
                        firebaseParameters.Add(new Parameter(key, floatVal));
                    else if (value is double doubleVal)
                        firebaseParameters.Add(new Parameter(key, doubleVal));
                    else
                        firebaseParameters.Add(new Parameter(key, value.ToString()));
                }

                if (firebaseParameters.Count > 0)
                {
                    FirebaseAnalytics.LogEvent(eventName, firebaseParameters.ToArray());
                    ServicesLogger.Log($"[FirebaseManager] Logged Event: {eventName} (with parameters)");
                }
                else
                {
                    FirebaseAnalytics.LogEvent(eventName);
                    ServicesLogger.Log($"[FirebaseManager] Logged Event: {eventName} (no valid parameters)");
                }
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[FirebaseManager] Error logging event '{eventName}': {ex.Message}");
            }
        }

        public void LogIAPEvent(string transactionId, double price, string currency, string productId = "unknown_item")
        {
            string safeTxId = transactionId.Length > 100 ? transactionId.Substring(0, 100) : transactionId;

            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase,
                new Parameter(FirebaseAnalytics.ParameterTransactionID, safeTxId),
                new Parameter(FirebaseAnalytics.ParameterValue, price),
                new Parameter(FirebaseAnalytics.ParameterCurrency, currency),
                new Parameter(FirebaseAnalytics.ParameterItemID, productId),
                new Parameter(FirebaseAnalytics.ParameterItemName, productId)
            );
            Debug.Log($"[FirebaseManager] Purchase Event Logged: {safeTxId}, {price} {currency}, product: {productId}");
        }

        public void LogConsentEvent(bool consentGiven)
        {
            if (!core.isInitialized)
            {
                ServicesLogger.LogWarning("[FirebaseManager] Firebase is not initialized! Cannot log consent.");
                return;
            }

            string consentString = consentGiven ? "granted" : "denied";

            var consentMap = new Dictionary<ConsentType, ConsentStatus>

            {
                { ConsentType.AdStorage, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied },
                { ConsentType.AnalyticsStorage, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied },
                { ConsentType.AdUserData, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied },
                { ConsentType.AdPersonalization, consentGiven ? ConsentStatus.Granted : ConsentStatus.Denied }
            };

            FirebaseAnalytics.SetConsent(consentMap);
            FirebaseAnalytics.SetUserProperty("allow_ad_personalization_signals", consentGiven ? "true" : "false");
            FirebaseAnalytics.LogEvent("ad_personalization_consent", new Parameter("granted", consentGiven ? "yes" : "no"));

            var requestConfiguration = new RequestConfiguration
            {
                TagForUnderAgeOfConsent = TagForUnderAgeOfConsent.False
            };

            MobileAds.SetRequestConfiguration(requestConfiguration);
            FirebaseAnalytics.LogEvent("user_consent", new Parameter("status", consentString));
            FirebaseAnalytics.SetUserProperty("user_consent", consentString);

            ServicesLogger.Log($"[FirebaseManager] User consent logged: {consentGiven}");
        }
    }
}