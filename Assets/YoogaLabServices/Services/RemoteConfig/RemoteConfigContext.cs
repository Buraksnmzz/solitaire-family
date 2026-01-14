using System.Collections.Generic;

namespace ServicesPackage
{
    public class RemoteConfigContext
    {
        public bool IsInitialized { get; set; } = false;
        public Dictionary<string, object> configDataValues = new Dictionary<string, object>();
        public string cachedJsonConfig = "{}";

        public string GetString(string key, string defaultValue = "")
        {
            return configDataValues.TryGetValue(key, out object value) && value is string s ? s : defaultValue;
        }

        public bool TryParseBool(string key, bool defaultValue = false)
        {
            string raw = GetString(key, null);
            if (string.IsNullOrEmpty(raw)) return defaultValue;

            raw = raw.Trim().ToLowerInvariant();
            if (raw == "true" || raw == "1") return true;
            if (raw == "false" || raw == "0") return false;

            return defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            string raw = GetString(key, null);
            return int.TryParse(raw, out int result) ? result : defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            string raw = GetString(key, null);
            return float.TryParse(raw, out float result) ? result : defaultValue;
        }

        public bool AdjustOn => TryParseBool("adjust_on", true);
        public bool BannerFirstSession => TryParseBool("banner_first_session", true);
        public int FrequencyCapIS => GetInt("FrequencyCapIS", 60);
        public bool GoogleCMP => TryParseBool("google_cmp", true);
        public bool InterstitialFirstSession => TryParseBool("interstitial_first_session", true);
        public string LevelEvents => GetString("level_events", "");
        public string LevelEventsDelay => GetString("level_events_delay", "");
        public bool ShowIdfaConsent => TryParseBool("show_idfa_consent", true);
        public bool ShowInterstitial => TryParseBool("show_interstitial", true);
        public bool ShowBanner => TryParseBool("show_banner", true);
        public int AdDelayFirstSession => GetInt("ad_delay_first_session", 30);
        public int StartIS => GetInt("StartIS", 600);
        public int AdThresholdDay => GetInt("ad_threshold_day", 1);
    }
}