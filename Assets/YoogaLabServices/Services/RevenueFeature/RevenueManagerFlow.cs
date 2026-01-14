using AdjustSdk;
using System;
using UnityEngine;
using static MaxSdkBase;

namespace ServicesPackage
{
    public class RevenueManagerFlow
    {
        private readonly RevenueManagerContext ctx;
        private RemoteConfigContext remoteConfigContext;
        private SessionManagerContext sessionManagerContext;

        public RevenueManagerFlow(RevenueManagerContext context)
        {
            ctx = context;
        }

        public void SetRevenueAPIKey(string key)
        {
            ctx.currToken = key;
            ServicesLogger.Log($"[RevenueManager] TokenSet called ? Platform: {Application.platform}, token: {ctx.currToken}");
        }

        internal void Setup(RemoteConfigContext remoteConfig, SessionManagerContext session)
        {
            remoteConfigContext = remoteConfig;
            sessionManagerContext = session;
        }

        //public void TrackAdRevenue(AdInfo adInfo)
        //{
        //    try
        //    {
        //        if (adInfo == null)
        //        {
        //            ServicesLogger.LogError("[RevenueManager] adInfo is null!");
        //            return;
        //        }

        //        if (string.IsNullOrEmpty(adInfo.NetworkName) || string.IsNullOrEmpty(adInfo.AdFormat) || string.IsNullOrEmpty(adInfo.AdUnitIdentifier))
        //        {
        //            ServicesLogger.LogError("[RevenueManager][REVENUELOG] One or more adInfo properties are null or empty!");
        //            return;
        //        }
        //        ServicesLogger.Log($"[REVENUE MANAGER][REVENUELOG] RevenueManager RECEIVE START AD is valid, ");

        //        SignalBusService.Fire(new LogFirebaseEventSignal
        //        {
        //            eventName = "ad_impression",
        //            parameters = new (string, object)[]
        //            {
        //                    ("ad_platform", "AppLovin"),
        //                    ("ad_source", adInfo.NetworkName ?? "unknown"),
        //                    ("ad_format", adInfo.AdFormat?? "unknown"),
        //                    ("ad_unit_name", adInfo.AdUnitIdentifier ?? "unknown"),
        //                    ("value", adInfo.Revenue),
        //                    ("currency", "USD")
        //            }
        //        });

        //        if (sessionManagerContext.DaysSinceInstall >= remoteConfigContext.AdThresholdDay)
        //        {
        //            SignalBusService.Fire(new LogFirebaseEventSignal
        //            {
        //                eventName = "ad_impression",
        //                parameters = new (string, object)[]
        //                {
        //                    ("ad_platform", "AppLovin"),
        //                    ("ad_source", adInfo.NetworkName),
        //                    ("ad_format", adInfo.AdFormat),
        //                    ("ad_unit_name", adInfo.AdUnitIdentifier),
        //                    ("value", adInfo.Revenue),
        //                    ("currency", "USD")
        //                }
        //            });

        //            Debug.Log("[=] Threshold ad_impression sent.");
        //        }

        //        ServicesLogger.Log(
        //            $"[REVENUE MANAGER][REVENUELOG] RevenueManager received ? " +
        //            $"revenue={adInfo.Revenue}, " +
        //            $"time={Time.realtimeSinceStartup:F2}"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        ServicesLogger.LogError($"[RevenueManager] Error in TrackAdRevenue: {ex.Message}\n{ex.StackTrace}");
        //    }
        //}

        public void TrackAdRevenue(AdInfo adInfo)
        {
            if (adInfo == null) return;

            ServicesLogger.Log(
                $"[REVENUE] Impression {adInfo.NetworkName} ${adInfo.Revenue} " +
                $"day={sessionManagerContext.DaysSinceInstall}"
            );

            // Always log true revenue
            SignalBusService.Fire(new LogFirebaseEventSignal
            {
                eventName = "ad_impression",
                parameters = BuildParams(adInfo)
            });

            // Only log threshold revenue after X days
            if (sessionManagerContext.DaysSinceInstall >= remoteConfigContext.AdThresholdDay)
            {
                SignalBusService.Fire(new LogFirebaseEventSignal
                {
                    eventName = "ad_impression_threshold",
                    parameters = BuildParams(adInfo)
                });

                ServicesLogger.Log("[REVENUE] Threshold impression sent");
            }
        }

        private (string, object)[] BuildParams(AdInfo adInfo)
        {
            return new (string, object)[]
            {
        ("ad_platform", "AppLovin"),
        ("ad_source", adInfo.NetworkName ?? "unknown"),
        ("ad_format", adInfo.AdFormat ?? "unknown"),
        ("ad_unit_name", adInfo.AdUnitIdentifier ?? "unknown"),
        ("value", adInfo.Revenue),
        ("currency", "USD")
            };
        }

        public void TrackInAppPurchase(string transactionId, double price, string currency, string productId)
        {
            Debug.Log($"[IAPCHECK] Will send IAP to Adjust. TransactionId: {transactionId}, Price: {price}, " +
                $"Currency: {currency}, Token: {ctx.currToken}");

            if (string.IsNullOrEmpty(ctx.currToken))
            {
                Debug.LogWarning("[IAPCHECK] Adjust IAP event token is missing!");
                return;
            }

            AdjustEvent adjustEvent = new AdjustEvent(ctx.currToken);

            adjustEvent.SetRevenue(price, currency);
            adjustEvent.DeduplicationId = transactionId;

            Adjust.TrackEvent(adjustEvent);
            Debug.Log($"[IAPCHECK] IAP Tracked and sent: {transactionId}, {price} {currency}");
        }

    }
}
