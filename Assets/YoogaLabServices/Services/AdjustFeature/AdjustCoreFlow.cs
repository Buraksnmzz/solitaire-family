using AdjustSdk;
using System;
using UnityEngine;

namespace ServicesPackage
{
    public class AdjustCoreFlow
    {
        private readonly AdjustCoreContext ctx;
        private AdjustCallbacks callbacks;
        private RemoteConfigContext remoteConfigContext;
        public AdjustCoreFlow(AdjustCoreContext context)
        {
            ctx = context;
        }
        private const string AdjustInitTestKey = "adjust_init_test_fired";

        public void SetAppToken(string token)
        {
            ctx.currentAdjustAppToken = token;
            ServicesLogger.Log($"[AdjustManager] TokenSet called — Platform: {Application.platform}," +
                $" token: {ctx.currentAdjustAppToken}");
        }

        public void SetRemoteConfig(RemoteConfigContext context)
        {
            remoteConfigContext = context;
        }

        public void Initialize(bool consent)
        {
            if (!remoteConfigContext.AdjustOn)
            {
                ServicesLogger.Log("[AdjustManager] Adjust is disabled via Remote Config.");
                return;
            }
            callbacks = new AdjustCallbacks(ctx);

            if (ctx.isInitialized) return;

            if (string.IsNullOrEmpty(ctx.currentAdjustAppToken))
            {
                ServicesLogger.LogError("[AdjustManager] Adjust App Token is missing! Please set it in the inspector.");
                return;
            }

            AdjustEnvironment env = ctx.IsSandbox
                ? AdjustEnvironment.Sandbox
                : AdjustEnvironment.Production;

            Debug.Log($"[AdjustManager] Adjust production is {env}");
            AdjustConfig config = new AdjustConfig(ctx.currentAdjustAppToken, env, true)
            {
                AttributionChangedDelegate = this.AttributionChangedDelegate
            };
            config.EventSuccessDelegate = callbacks.OnAdjustEventSuccess;
            config.EventFailureDelegate = callbacks.OnAdjustEventFailure;
            config.IsAdServicesEnabled = ctx.AdsServices;
            //config.LogLevel = AdjustLogLevel.Verbose;

            Adjust.InitSdk(config);

            ctx.isInitialized = true;

            FetchLevelEventConfig();
            CheckPendingDelayedEvents();
            ServicesLogger.Log("[AdjustManager] Adjust Initialized and ready.");
            ServicesLogger.Log($"[Adjust] Init ThreadId = {System.Threading.Thread.CurrentThread.ManagedThreadId}");

            FireInitValidationEvent();
        }

        private void FireInitValidationEvent()
        {
            if (PlayerPrefs.GetInt(AdjustInitTestKey, 0) == 1)
            {
                ServicesLogger.Log("[Adjust TEST] Init test event already fired. Skipping.");
                return;
            }

            const string TEST_EVENT_TOKEN = "dabqpo";

            var testEvent = new AdjustEvent(TEST_EVENT_TOKEN);
            Adjust.TrackEvent(testEvent);

            PlayerPrefs.SetInt(AdjustInitTestKey, 1);
            PlayerPrefs.Save();

            ServicesLogger.Log(
                $"[Adjust TEST] Init validation event fired. Token={TEST_EVENT_TOKEN}, " +
                $"Thread={System.Threading.Thread.CurrentThread.ManagedThreadId}, " +
                $"Time={DateTime.UtcNow:O}"
            );
        }

        public void SetUserConsent(bool consentGiven)
        {
            if (!ctx.isInitialized)
            {
                ServicesLogger.LogWarning("[AdjustManager] Cannot set user consent. Adjust is not initialized.");
                return;
            }
            string consentStatus = consentGiven ? "granted" : "denied";

            SignalBusService.Fire(new SetFirebaseUserPropertiesSignal
            {
                key = "network",
                value = consentStatus
            });
            ServicesLogger.Log($"[AdjustManager] User consent set and logged to Firebase only: {consentStatus}");
        }

        public void TrackLevelEvent(int levelId)
        {
            if (!ctx.isInitialized || ctx.levelEvents == null || ctx.levelEvents.Length < 2)
            {
                ServicesLogger.LogWarning($"[TrackLevelEvent] Skipped - isInitialized: {ctx.isInitialized}, levelEvents null/invalid");
                return;
            }

            ServicesLogger.Log($"[TrackLevelEvent] Called with ID: {levelId}, total entries: {ctx.levelEvents.Length}");

            for (int i = 0; i < ctx.levelEvents.Length - 1; i += 2)
            {
                if (int.TryParse(ctx.levelEvents[i], out int triggerLevel))
                {
                    string token = ctx.levelEvents[i + 1];
                    int delayIndex = i / 2;
                    int delay = (ctx.levelEventDelays != null && delayIndex < ctx.levelEventDelays.Length) ? ctx.levelEventDelays[delayIndex] : 0;

                    ServicesLogger.Log($"[TrackLevelEvent] Checking Level {triggerLevel} with token {token} and delay {delay}");

                    if (triggerLevel == levelId)
                    {
                        string lastPlayedKey = $"Level_{levelId}_LastPlayed";
                        long lastPlayedTimestamp = PlayerPrefs.GetInt(lastPlayedKey, 0);
                        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                        if (lastPlayedTimestamp == 0)
                        {
                            if (delay == 0)
                            {
                                AdjustEvent adjustEvent = new AdjustEvent(token);
                                Adjust.TrackEvent(adjustEvent);
                                ServicesLogger.Log($"[AdjustManager] Tracked level event on first completion! Token: {token}, Level: {levelId}");
                            }
                            else
                            {
                                ServicesLogger.Log($"[AdjustManager] Saved initial completion time for Level {levelId}. Waiting {delay} days.");
                            }

                            PlayerPrefs.SetInt(lastPlayedKey, (int)currentTime);
                            PlayerPrefs.Save();
                            return;
                        }

                        long daysPassed = (currentTime - lastPlayedTimestamp) / 86400;
                        if (daysPassed >= delay)
                        {
                            AdjustEvent adjustEvent = new AdjustEvent(token);
                            Adjust.TrackEvent(adjustEvent);

                            ServicesLogger.Log($"[AdjustManager] Tracked level event! Token: {token}, Level: {levelId}, Days Passed: {daysPassed}");

                            PlayerPrefs.SetInt(lastPlayedKey, (int)currentTime);
                            PlayerPrefs.Save();
                        }
                        else
                        {
                            ServicesLogger.Log($"[AdjustManager] Not enough days passed for Level {levelId}. Days: {daysPassed}, Required: {delay}");
                        }

                        return;
                    }
                }
                else
                {
                    ServicesLogger.LogWarning($"[TrackLevelEvent] Invalid level entry: {ctx.levelEvents[i]}");
                }
            }

            ServicesLogger.LogWarning($"[TrackLevelEvent] No matching level found for ID {levelId}");
        }

        public void AttributionChangedDelegate(AdjustAttribution attribution)
        {
            if (attribution == null)
            {
                ServicesLogger.LogWarning("[AdjustManager] Attribution data is null.");
                return;
            }

            ServicesLogger.Log("[AdjustManager] Attribution changed. Network: " + attribution.Network + ", Campaign: " + attribution.Campaign);

            if (attribution.Network.ToLower().Contains("organic"))
                return;

            TrackAttribution(attribution.Network, attribution.Campaign);
        }

        public void TrackAttribution(string network, string campaign)
        {
            if (!ctx.isInitialized) return;

            network = network.ToLower();
            bool logToFirebase = true;

            if (network.Contains("unity"))
            {
                network = "unityads";
            }
            else if (network.Contains("google"))
            {
                network = "adwords";
                campaign = campaign.Split(new[] { " (" }, StringSplitOptions.None)[0];
                logToFirebase = false;
            }
            else if (network.Contains("vungle"))
            {
                network = "vungle";
                campaign = campaign.Split('_')[0];
            }
            else if (network.Contains("facebook"))
            {
                network = "facebookads";
                campaign = campaign.Split(new[] { " (" }, StringSplitOptions.None)[0];
            }
            else if (network.Contains("applovin"))
            {
                network = "applovin";
                campaign = campaign.Split(new[] { " (" }, StringSplitOptions.None)[0];
            }
            else if (network.Contains("ironsource"))
            {
                network = "ironsource";
                campaign = campaign.Split(new[] { " (" }, StringSplitOptions.None)[0];
            }
            else
            {
                logToFirebase = false;
            }

            ServicesLogger.Log($"[AdjustManager] Attribution Info: Network={network}, Campaign={campaign}");

            if (logToFirebase)
            {
                SignalBusService.Fire(new SetFirebaseUserPropertiesSignal
                {
                    key = "network",
                    value = network
                });

                SignalBusService.Fire(new SetFirebaseUserPropertiesSignal
                {
                    key = "campaign",
                    value = campaign
                });

                ServicesLogger.Log("[AdjustManager] Sent attribution signals.");
            }

        }

        public void TrackAdRevenue(MaxSdkBase.AdInfo adInfo)
        {
            if (!ctx.isInitialized) return;

            var adRevenue = new AdjustAdRevenue("applovin_max_sdk");

            adRevenue.SetRevenue(adInfo.Revenue, "USD");

            adRevenue.AdRevenueNetwork = adInfo.NetworkName;
            adRevenue.AdRevenueUnit = adInfo.AdUnitIdentifier;
            adRevenue.AdRevenuePlacement = adInfo.Placement;
            adRevenue.AdImpressionsCount = 1;

            adRevenue.AddCallbackParameter("ad_format", adInfo.AdFormat ?? "unknown_format");

            Adjust.TrackAdRevenue(adRevenue);

            ServicesLogger.Log($"[AdjustManager][REVENUELOG] Ad Revenue Event Sent -> " +
                      $"Source: applovin_max_sdk, " +
                      $"Network: {adInfo.NetworkName}, " +
                      $"Unit: {adInfo.AdUnitIdentifier}, " +
                      $"Placement: {adInfo.Placement}, " +
                      $"Revenue: {adInfo.Revenue} USD, " +
                      $"Impressions: 1");
        }

        private void FetchLevelEventConfig()
        {
            string levelEventsConfig = remoteConfigContext.LevelEvents;
            string delayConfig = remoteConfigContext.LevelEventsDelay;

            if (!string.IsNullOrEmpty(levelEventsConfig))
            {
                ctx.levelEvents = levelEventsConfig.Split(',');
            }

            if (!string.IsNullOrEmpty(delayConfig))
            {
                var delayStrings = delayConfig.Split(',');
                ctx.levelEventDelays = new int[delayStrings.Length];

                for (int i = 0; i < delayStrings.Length; i++)
                {
                    if (int.TryParse(delayStrings[i], out int parsedDelay))
                        ctx.levelEventDelays[i] = parsedDelay;
                    else
                        ctx.levelEventDelays[i] = 0;
                }
            }
        }

        private void CheckPendingDelayedEvents()
        {
            if (ctx.levelEvents == null || ctx.levelEvents.Length < 2)
                return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            for (int i = 0; i < ctx.levelEvents.Length - 1; i += 2)
            {
                if (int.TryParse(ctx.levelEvents[i], out int levelId))
                {
                    string token = ctx.levelEvents[i + 1];
                    int delayIndex = i / 2;
                    int delay = (ctx.levelEventDelays != null && delayIndex < ctx.levelEventDelays.Length) ? ctx.levelEventDelays[delayIndex] : 0;

                    string lastPlayedKey = $"Level_{levelId}_LastPlayed";
                    long lastPlayedTimestamp = PlayerPrefs.GetInt(lastPlayedKey, 0);

                    if (lastPlayedTimestamp == 0) continue;

                    long daysPassed = (currentTime - lastPlayedTimestamp) / 86400;
                    if (daysPassed >= delay)
                    {
                        AdjustEvent adjustEvent = new AdjustEvent(token);
                        Adjust.TrackEvent(adjustEvent);

                        ServicesLogger.Log($"[AdjustManager] Tracked delayed level event! Token: {token}, Level: {levelId}, Days Passed: {daysPassed}");
                        PlayerPrefs.DeleteKey(lastPlayedKey);
                        PlayerPrefs.Save();
                    }
                }
            }
        }
    }
}

//3ead408c-125a-4a01-b113-c69a466e8f26