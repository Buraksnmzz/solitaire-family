using System;
using UnityEngine;
#if UNITY_ANDROID
using Google.Play.Review;
#endif
#if UNITY_IOS
using System.Globalization;
using UnityEngine.iOS;
#endif
namespace ServicesPackage
{
    public class RegionFlow
    {
        private readonly RegionContext ctx;

        public RegionFlow(RegionContext context)
        {
            ctx = context;
        }

        public void Initialize()
        {
            ctx.countryCode = PlayerPrefs.GetString(RegionContext.CountryCodeKey, string.Empty);
            if (string.IsNullOrEmpty(ctx.countryCode))
            {
                ctx.countryCode = GetCountryCodeFromLocale();
                PlayerPrefs.SetString(RegionContext.CountryCodeKey, ctx.countryCode);
                PlayerPrefs.Save();
            }
            ServicesLogger.Log($"[RegionManager] Country code detected/set to: {ctx.countryCode}");
        }

        private string GetCountryCodeFromLocale()
        {
    #if UNITY_ANDROID
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject telephonyManager = activity.Call<AndroidJavaObject>("getSystemService", "phone"))
                {
                    string simCountryIso = telephonyManager.Call<string>("getSimCountryIso");
                    if (!string.IsNullOrEmpty(simCountryIso))
                    {
                        ServicesLogger.Log("[RegionManager] Got SIM country ISO: " + simCountryIso);
                        return simCountryIso.ToUpperInvariant();
                    }
                }

                using (AndroidJavaClass localeClass = new AndroidJavaClass("java.util.Locale"))
                using (AndroidJavaObject defaultLocale = localeClass.CallStatic<AndroidJavaObject>("getDefault"))
                {
                    string country = defaultLocale.Call<string>("getCountry");
                    ServicesLogger.Log("[RegionManager] Got Locale country: " + country);
                    return country.ToUpperInvariant();
                }
            }
            catch (Exception e)
            {
                ServicesLogger.LogError("[RegionManager] Failed to get country code on Android: " + e.Message);
                return "GR";
            }
    #elif UNITY_IOS
        return new RegionInfo(CultureInfo.CurrentCulture.Name).TwoLetterISORegionName;
    #else
        return new RegionInfo(CultureInfo.CurrentCulture.Name).TwoLetterISORegionName;
    #endif
        }

    }
}
