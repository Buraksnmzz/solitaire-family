using GoogleMobileAds.Ump.Api;
using System.Threading.Tasks;
using System;
using UnityEngine;


#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace ServicesPackage
{
    public class ConsentFlow
    {
        private readonly ConsentContext ctx;
        private RemoteConfigContext remoteConfigCtx;
        private RegionContext regionCtx;

        public ConsentFlow(ConsentContext context)
        {
            ctx = context;
        }

        public void Initialize(RemoteConfigContext configContext,RegionContext regionContext)
        {
            remoteConfigCtx = configContext;
            regionCtx = regionContext;
        }

        public async Task GatherConsentAsync(Action<string> onComplete = null)
        {
            if (ctx._hasStartedConsentFlow)
            {
                ServicesLogger.Log("[ConsentManager] Consent flow already in progress or completed. Skipping.");
                onComplete?.Invoke(null);
                return;
            }
            ctx._hasStartedConsentFlow = true;

    #if UNITY_IOS
            var attStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (remoteConfigCtx.ShowIdfaConsent && attStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ServicesLogger.Log("[ConsentManager] Requesting ATT before UMP...");
                bool attComplete = await RequestATTConsent();
                if (!attComplete)
                {
                    ServicesLogger.LogWarning("[ConsentManager] ATT timed out or failed.");
                }
                attStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            }

            if (attStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED)
            {
                ServicesLogger.Log("[ConsentManager] ATT denied → skipping UMP/CMP flow.");
                SaveConsent(false);
                onComplete?.Invoke(null);
                return;
            }
    #endif

            ServicesLogger.Log("[ConsentManager] Requesting UMP consent...");

            var requestParameters = new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false,
                ConsentDebugSettings = new ConsentDebugSettings
                {
                    DebugGeography = DebugGeography.Disabled,
                }
            };

            var tcs = new TaskCompletionSource<string>();

            ConsentInformation.Update(requestParameters, updateError =>
            {
                if (updateError != null)
                {
                    tcs.SetResult(updateError.Message);
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired(showError =>
                {
                    if (showError != null)
                        tcs.SetResult(showError.Message);
                    else
                    {
                        SaveConsent(ConsentInformation.CanRequestAds());
                        tcs.SetResult(null);
                    }
                });


            });

            string result = await tcs.Task;

            if (string.IsNullOrEmpty(result))
            {
                ServicesLogger.Log("[ConsentManager] Consent flow completed successfully.");
            }
            else
            {
                ServicesLogger.LogError($"[ConsentManager] Consent flow failed: {result}");
                ctx._hasStartedConsentFlow = false;
            }
            onComplete?.Invoke(result);
        }

        public async Task<bool> RequestATTConsent()
        {

    #if UNITY_IOS
            ServicesLogger.Log("[ConsentManager] Requesting ATT Consent...");

            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                ServicesLogger.Log("[ConsentManager] Awaiting ATT response...");
                float elapsed = 0f;
                while (elapsed < 5f)
                {
                    status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                    if (status != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                    {
                        ServicesLogger.Log($"[ConsentManager] ATT authorization completed: {status}");
                        return true;
                    }
                    elapsed += Time.deltaTime;
                    await Task.Yield();
                }

                ServicesLogger.LogWarning("[ConsentManager] ATT consent timeout after 5 seconds.");
                return false;
            }
            else
            {
                ServicesLogger.Log("[ConsentManager] ATT already decided.");
                return true;
            }
    #else
            await Task.CompletedTask;
            return true;
    #endif
        }

        private void SaveConsent(bool canRequestAds)
        {
            bool personalized =
                ConsentInformation.ConsentStatus == ConsentStatus.Obtained;

            ctx.IsPersonalizedConsent = personalized;

            bool trackingAllowed =
                !remoteConfigCtx.GoogleCMP ||
                !regionCtx.IsGDPRCountry() ||
                personalized;

            ServicesLogger.Log(
                $"[ConsentManager] ResolveConsent → " +
                $"trackingAllowed={trackingAllowed}, " +
                $"gdpr={regionCtx.IsGDPRCountry()}, " +
                $"cmp={remoteConfigCtx.GoogleCMP}, " +
                $"personalized={personalized}"
            );

            ResolveOnce(trackingAllowed);
        }

        public async Task ResolveConsentAsync()
        {
            if (ctx.IsConsentResolved)
            {
                ServicesLogger.Log("[ConsentManager] Consent already resolved. Skipping.");
                return;
            }

            ServicesLogger.Log(
                $"[ConsentManager] Resolving consent | " +
                $"gdpr={regionCtx.IsGDPRCountry()}, " +
                $"googleCMP={remoteConfigCtx.GoogleCMP}, " +
                $"showIdfa={remoteConfigCtx.ShowIdfaConsent}"
            );

#if UNITY_IOS
    if (remoteConfigCtx.ShowIdfaConsent)
    {
        var attStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        ServicesLogger.Log($"[ConsentManager] ATT status before request: {attStatus}");

        if (attStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ServicesLogger.Log("[ConsentManager] Requesting ATT...");
            await RequestATTConsent();
        }

         attStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        ServicesLogger.Log($"[ConsentManager] ATT status after request: {attStatus}");

         if (attStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED)
        {
            ServicesLogger.Log("[ConsentManager] ATT denied → forcing tracking disabled.");
            ResolveOnce(false);
            return;
        }
    }
#endif
            // ===== NON-GDPR FAST PATH =====
            if (!regionCtx.IsGDPRCountry())
            {
                ServicesLogger.Log(
                    "[ConsentManager] Non-GDPR country detected → forcing consent ALLOWED."
                );
                ResolveOnce(true);
                return;
            }

            // ===== GDPR BUT CMP DISABLED =====
            if (!remoteConfigCtx.GoogleCMP)
            {
                ServicesLogger.Log(
                    "[ConsentManager] GDPR country but Google CMP disabled → forcing consent ALLOWED."
                );
                ResolveOnce(true);
                return;
            }

            // ===== FULL GDPR + CMP FLOW =====
            ServicesLogger.Log(
                "[ConsentManager] GDPR country + CMP enabled → starting UMP consent flow."
            );

            await GatherConsentAsync(err =>
            {
                if (!string.IsNullOrEmpty(err))
                    ServicesLogger.LogWarning("[ConsentManager] UMP failed, fallback to non-personalized.");
            });
        }

        private void ResolveOnce(bool allowed)
        {
            if (ctx.IsConsentResolved)
            {
                ServicesLogger.Log(
                    $"[ConsentManager] ResolveOnce skipped (already resolved, allowed={ctx.IsConsentGiven})."
                );
                return;
            }

            ctx.IsConsentResolved = true;
            ctx.IsConsentGiven = allowed;

            PlayerPrefs.SetInt(
                ConsentContext.GDPRConsentKey,
                allowed ? 1 : 0
            );
            PlayerPrefs.Save();

            ServicesLogger.Log(
                $"[ConsentManager] Consent RESOLVED FINAL | allowed={allowed}"
            );

            SignalBusService.Fire(
                new ResolveConsentSignal { value = allowed },
                true
            );
        }
    }
}