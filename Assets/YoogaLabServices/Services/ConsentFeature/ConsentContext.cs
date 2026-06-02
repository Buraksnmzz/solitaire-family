using GoogleMobileAds.Ump.Api;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
using UnityEngine;

namespace ServicesPackage
{
    public class ConsentContext
    {
        public bool IsConsentGiven { get; set; } = false;
        public const string GDPRConsentKey = "GDPR_Consent";
        public bool _hasStartedConsentFlow = false;
        public bool IsPersonalizedConsent { get; set; }
        public bool IsConsentResolved;

        public bool IsATTDenied()
        {
    #if UNITY_IOS
        var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
        return status == ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED;
    #else
            return false;
    #endif
        }

        public bool IsConsentStored() =>
           PlayerPrefs.HasKey(GDPRConsentKey);

        public bool GetStoredConsent() =>
            PlayerPrefs.GetInt(GDPRConsentKey, 0) == 1;

        public bool CanRequestAds => ConsentInformation.CanRequestAds();
    }
}
