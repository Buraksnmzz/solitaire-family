using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Loading
{
    public static class LevelDataUrlResolver
    {
        public static string ResolveLevelDataUrl(string configurationJson, SystemLanguage language)
        {
            return ResolveLevelDataUrl(configurationJson, language, true, true);
        }

        public static string ResolveLevelDataUrl(string configurationJson, SystemLanguage language, bool allowFallbackToFirstUrl)
        {
            return ResolveLevelDataUrl(configurationJson, language, true, allowFallbackToFirstUrl);
        }

        public static string ResolveLevelDataUrl(string configurationJson, SystemLanguage language, bool allowFallbackToEnglish, bool allowFallbackToFirstUrl)
        {
            if (string.IsNullOrWhiteSpace(configurationJson))
            {
                return string.Empty;
            }

            GameConfigurationRoot root;
            try
            {
                root = JsonConvert.DeserializeObject<GameConfigurationRoot>(configurationJson);
            }
            catch
            {
                return string.Empty;
            }

            if (root == null)
            {
                return string.Empty;
            }

            if (root.levelDataUrls == null || root.levelDataUrls.Count == 0)
            {
                return string.Empty;
            }

            var languageSpecificUrl = GetLanguageSpecificLevelDataUrl(root, language, allowFallbackToEnglish);
            if (!string.IsNullOrEmpty(languageSpecificUrl))
            {
                return languageSpecificUrl;
            }

            if (!allowFallbackToFirstUrl)
            {
                return string.Empty;
            }

            return GetFirstLevelDataUrl(configurationJson);
        }

        private static string GetLanguageSpecificLevelDataUrl(GameConfigurationRoot root, SystemLanguage language, bool allowFallbackToEnglish)
        {
            var map = new Dictionary<string, string>(root.levelDataUrls, StringComparer.OrdinalIgnoreCase);
            var languageCode = GetLanguageCode(language);
            if (!string.IsNullOrWhiteSpace(languageCode)
                && map.TryGetValue(languageCode, out var url)
                && !string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            if (allowFallbackToEnglish && map.TryGetValue("en", out var englishUrl) && !string.IsNullOrWhiteSpace(englishUrl))
            {
                return englishUrl;
            }

            return string.Empty;
        }

        private static string GetFirstLevelDataUrl(string configurationJson)
        {
            try
            {
                var root = JObject.Parse(configurationJson);
                var levelDataUrls = root["levelDataUrls"] as JObject;
                if (levelDataUrls == null)
                {
                    return string.Empty;
                }

                foreach (var property in levelDataUrls.Properties())
                {
                    var value = property.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            catch
            {
            }

            return string.Empty;
        }

        private static string GetLanguageCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    return "en";
                case SystemLanguage.Turkish:
                    return "tr";
                case SystemLanguage.Thai:
                    return "th";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Italian:
                    return "it";
                case SystemLanguage.Portuguese:
                    return "pt";
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.Japanese:
                    return "ja";
                case SystemLanguage.Korean:
                    return "ko";
                case SystemLanguage.Indonesian:
                    return "id";
                case SystemLanguage.Vietnamese:
                    return "vi";
                case SystemLanguage.Arabic:
                    return "ar";
                case SystemLanguage.Hindi:
                    return "hi";
                default:
                    return string.Empty;
            }
        }

        [Serializable]
        private class GameConfigurationRoot
        {
            public Dictionary<string, string> levelDataUrls;
        }
    }
}
