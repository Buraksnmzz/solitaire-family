using System.Collections.Generic;

namespace ServicesPackage
{
    public class RegionContext
    {
        public const string CountryCodeKey = "RegionManager_CountryCode";
        public string countryCode = "";

        public  readonly HashSet<string> GDPRCountries = new HashSet<string>
        {
            "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "ES", "FI", "FR", "GR", "HR",
            "HU", "IE", "IT", "LT", "LU", "LV", "MT", "NL", "PL", "PT", "RO", "SE", "SI",
            "SK", "GB", "UK", "IS", "LI", "NO"
        };

        public string GetCountryCode()
        {
            return countryCode;
        }

        public bool IsGDPRCountry()
        {
            return GDPRCountries.Contains(countryCode);
        }
    }
}