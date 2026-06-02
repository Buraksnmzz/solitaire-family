using System.Text;
using GoogleMobileAds.Ump.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace ServicesPackage
{
    public class ConsentDebugOverlay : MonoBehaviour
    {
        [SerializeField] private bool enabledOverlay = true;

        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_Text text;

        private readonly StringBuilder log = new();

        private void Awake()
        {
            if (!enabledOverlay)
            {
                gameObject.SetActive(false);
                return;
            }

            DontDestroyOnLoad(gameObject);

            if (canvas != null)
                canvas.sortingOrder = 99999;

            SignalBusService.Subscribe<ConsentDebugSignal>(OnConsentDebugSignal);

            Append("OVERLAY", "Consent debug overlay created");

            Snapshot();
        }

        private void OnDestroy()
        {
            SignalBusService.Unsubscribe<ConsentDebugSignal>(OnConsentDebugSignal);
        }

        private void Update()
        {
            if (!enabledOverlay || text == null)
                return;

            Snapshot();
        }

        private void OnConsentDebugSignal(ConsentDebugSignal signal)
        {
            Append(signal.source, signal.message);
        }

        private void Snapshot()
        {
            string att = "N/A";

#if UNITY_IOS
            att = ATTrackingStatusBinding
                .GetAuthorizationTrackingStatus()
                .ToString();
#endif

            string snapshot =
                $"Time: {Time.realtimeSinceStartup:0.00}\n" +
                $"ATT: {att}\n" +
                $"UMP ConsentStatus: {ConsentInformation.ConsentStatus}\n" +
                $"UMP CanRequestAds: {ConsentInformation.CanRequestAds()}\n" +
                $"UMP PrivacyOptions: {ConsentInformation.PrivacyOptionsRequirementStatus}\n" +
                $"PP GDPRConsentKey: {PlayerPrefs.GetInt(ConsentContext.GDPRConsentKey, -1)}\n" +
                $"IABTCF_gdprApplies: {PlayerPrefs.GetString("IABTCF_gdprApplies", "MISSING")}\n" +
                $"IABTCF_TCString: {Trim(PlayerPrefs.GetString("IABTCF_TCString", "MISSING"))}\n" +
                $"IABTCF_PurposeConsents: {PlayerPrefs.GetString("IABTCF_PurposeConsents", "MISSING")}\n" +
                $"IABTCF_AddtlConsent: {Trim(PlayerPrefs.GetString("IABTCF_AddtlConsent", "MISSING"))}\n\n" +
                log;

            text.text = snapshot;
        }

        private void Append(string source, string message)
        {
            log.Insert(0, $"[{source}] {message}\n");
        }

        private static string Trim(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "EMPTY";

            if (value.Length <= 80)
                return value;

            return value.Substring(0, 80) + "...";
        }
    }

    public struct ConsentDebugSignal
    {
        public string source;
        public string message;
    }
}

