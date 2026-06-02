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

        private void DebugConsent(string source, string message)
        {
            ServicesLogger.Log($"[CMP DEBUG] [{source}] {message}");

            SignalBusService.Fire(new ConsentDebugSignal
            {
                source = source,
                message = message
            }, true);
        }

        public void Initialize(RemoteConfigContext configContext, RegionContext regionContext)
        {
            remoteConfigCtx = configContext;
            regionCtx = regionContext;
        }

        public async Task GatherConsentAsync(Action<string> onComplete = null)
        {
            DebugConsent("GATHER START", $"started={ctx._hasStartedConsentFlow} | resolved={ctx.IsConsentResolved}");
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
                DebugConsent("ATT", $"Requesting ATT before UMP | current={attStatus}");

                bool attComplete = await RequestATTConsent();
                if (!attComplete)
                {
                    ServicesLogger.LogWarning("[ConsentManager] ATT timed out or failed.");
                    DebugConsent("ATT", "ATT timed out or failed");
                }
                attStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

                DebugConsent("ATT", $"ATT status AFTER request = {attStatus}");
            }

            if (attStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED)
            {
                ServicesLogger.Log("[ConsentManager] ATT denied → skipping UMP/CMP flow.");

                DebugConsent("ATT", "ATT denied -> skipping UMP/CMP");
                SaveConsent(false);
                onComplete?.Invoke(null);
                return;
            }
#endif

            ServicesLogger.Log("[ConsentManager] Requesting UMP consent...");

            DebugConsent(
                "UMP",
                $"Starting UMP | " +
                $"ConsentStatus={ConsentInformation.ConsentStatus} | " +
                $"CanRequestAds={ConsentInformation.CanRequestAds()} | " +
                $"PrivacyOptions={ConsentInformation.PrivacyOptionsRequirementStatus}"
            );

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
                DebugConsent(
                    "UMP UPDATE",
                    $"PrivacyRequired={ConsentInformation.PrivacyOptionsRequirementStatus} | " +
                    $"ConsentStatus={ConsentInformation.ConsentStatus} | " +
                    $"CanRequestAds={ConsentInformation.CanRequestAds()} | " +
                    $"IsGDPR={regionCtx.IsGDPRCountry()}"
                );

                if (updateError != null)
                {
                    tcs.SetResult(updateError.Message);
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired(showError =>
                {
                    DebugConsent(
                        "UMP FORM",
                        $"PrivacyRequired={ConsentInformation.PrivacyOptionsRequirementStatus} | " +
                        $"ConsentStatus={ConsentInformation.ConsentStatus} | " +
                        $"CanRequestAds={ConsentInformation.CanRequestAds()} | " +
                        $"IsGDPR={regionCtx.IsGDPRCountry()}"
                    );

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

        // private void SaveConsent(bool canRequestAds)
        // {
        //     bool personalized = ConsentInformation.ConsentStatus == ConsentStatus.Obtained;

        //     ctx.IsPersonalizedConsent = personalized;

        //     bool trackingAllowed = !remoteConfigCtx.GoogleCMP || !regionCtx.IsGDPRCountry() || personalized;

        //     ServicesLogger.Log(
        //         $"[ConsentManager] ResolveConsent → " +
        //         $"trackingAllowed={trackingAllowed}, " +
        //         $"gdpr={regionCtx.IsGDPRCountry()}, " +
        //         $"cmp={remoteConfigCtx.GoogleCMP}, " +
        //         $"personalized={personalized}"
        //     );

        //     ResolveOnce(trackingAllowed);
        // }

        private void SaveConsent(bool canRequestAds)
        {
            ctx.IsPersonalizedConsent =
                ConsentInformation.ConsentStatus == ConsentStatus.Obtained;

            DebugConsent(
                "SAVE",
                $"ConsentStatus={ConsentInformation.ConsentStatus} | " +
                $"CanRequestAds={ConsentInformation.CanRequestAds()} | " +
                $"Personalized={ctx.IsPersonalizedConsent}"
            );

            ResolveOnce(canRequestAds);
        }

        public async Task ResolveConsentAsync()
        {
            if (ctx.IsConsentResolved)
            {
                ServicesLogger.Log("[ConsentManager] Consent already resolved. Skipping.");
                DebugConsent("FLOW", "Consent already resolved");
                return;
            }

            DebugConsent(
                "RESOLVE",
                $"country={regionCtx.countryCode} | " +
                $"googleCMP={remoteConfigCtx.GoogleCMP} | " +
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
                    DebugConsent("ATT", "Requesting ATT from ResolveConsentAsync");
                    await RequestATTConsent();
                }

                attStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                ServicesLogger.Log($"[ConsentManager] ATT status after request: {attStatus}");
                DebugConsent("ATT", $"Final ATT status = {attStatus}");

                if (attStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED)
                {
                    ServicesLogger.Log("[ConsentManager] ATT denied → personalized tracking disabled.");

                    DebugConsent(
                        "ATT DENIED",
                        $"ATT={attStatus} | " +
                        $"CMP skipped=true | " +
                        $"PersonalizedAds=false | " +
                        $"NonPersonalizedAds=true | " +
                        $"CanRequestAds=true"
                    );

                    ResolveOnce(false);
                    return;
                }
            }
#endif
            // ===== NON-GDPR FAST PATH =====
            // if (!regionCtx.IsGDPRCountry())
            // {
            //     ServicesLogger.Log("[ConsentManager] Non-GDPR country detected → forcing consent ALLOWED.");
            //     ResolveOnce(true);
            //     return;
            // }

            // ===== GDPR BUT CMP DISABLED =====
            if (!remoteConfigCtx.GoogleCMP)
            {
                ServicesLogger.Log("[ConsentManager] GDPR country but Google CMP disabled → forcing consent ALLOWED.");

                DebugConsent(
                    "FLOW",
                    "GDPR + CMP enabled -> GatherConsentAsync"
                );
                ResolveOnce(true);
                return;
            }

            // ===== FULL GDPR + CMP FLOW =====
            ServicesLogger.Log("[ConsentManager] GDPR country + CMP enabled → starting UMP consent flow.");
            DebugConsent("FLOW", "Starting UMP consent flow");

            // await GatherConsentAsync(err =>
            // {
            //     if (!string.IsNullOrEmpty(err))
            //         ServicesLogger.LogWarning("[ConsentManager] UMP failed, fallback to non-personalized.");
            // });
            await GatherConsentAsync(err =>
            {
                if (!string.IsNullOrEmpty(err))
                {
                    DebugConsent("UMP ERROR", err);
                    ServicesLogger.LogWarning("[ConsentManager] UMP failed, fallback non-personalized.");
                    ResolveOnce(false);
                }
                else
                {
                    DebugConsent("UMP", "GatherConsentAsync completed successfully");
                }
            });
        }

        private void ResolveOnce(bool allowed)
        {
            if (ctx.IsConsentResolved)
            {
                ServicesLogger.Log($"[ConsentManager] ResolveOnce skipped (already resolved, allowed={ctx.IsConsentGiven}).");
                return;
            }

            ctx.IsConsentResolved = true;

            DebugConsent("FINAL", $"ResolveOnce allowed={allowed}");

            ctx.IsConsentGiven = allowed;

            PlayerPrefs.SetInt(ConsentContext.GDPRConsentKey, allowed ? 1 : 0);
            PlayerPrefs.Save();

            DebugConsent(
                "FINAL STATE",
                $"APP_ALLOWED={allowed} | " +
                $"APP_PERSONALIZED={ctx.IsPersonalizedConsent} | " +
                $"APP_CONSENT_GIVEN={ctx.IsConsentGiven} | " +
                $"APP_GDPR={regionCtx.IsGDPRCountry()} | " +
                $"UMP_ConsentStatus={ConsentInformation.ConsentStatus} | " +
                $"UMP_CanRequestAds={ConsentInformation.CanRequestAds()} | " +
                $"UMP_PrivacyOptions={ConsentInformation.PrivacyOptionsRequirementStatus} | " +
                $"StoredGDPRKey={PlayerPrefs.GetInt(ConsentContext.GDPRConsentKey, -1)}"
            );

            ServicesLogger.Log($"[ConsentManager] Consent RESOLVED FINAL | allowed={allowed}");

            SignalBusService.Fire(
                new ResolveConsentSignal { value = allowed },
                true
            );
        }
    }
}