using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Loading
{
    public static class RemoteConfigConfigurationMerger
    {
        private static readonly StringComparer KeyComparer = StringComparer.OrdinalIgnoreCase;

        private static readonly HashSet<string> IntegerKeys = new HashSet<string>(KeyComparer)
        {
            "backgroundImageId",
            "totalHintGiven",
            "totalJokerGiven",
            "totalUndoGiven",
            "totalCoinGiven",
            "earnedCoinAtLevelEnd",
            "earnedCoinPerMoveLeft",
            "hintCost",
            "undoCost",
            "jokerCost",
            "extraMovesCost",
            "dailyAdsWatchAmount",
            "rewardedVideoCoinAmount",
            "extraGivenMovesCount"
        };

        private static readonly HashSet<string> StringKeys = new HashSet<string>(KeyComparer)
        {
            "rateUsTrigger",
            "noAdsPackRewards",
            "shopCoinRewards"
        };

        private static readonly HashSet<string> BoolKeys = new HashSet<string>(KeyComparer)
        {
            "shouldShowIsOnFoundationComplete"
        };

        public static bool TryBuildConfigurationJson(string baseConfigurationJson, string remoteConfigRawJson, out string mergedConfigurationJson)
        {
            mergedConfigurationJson = string.Empty;

            if (!TryDeserializeRemoteConfig(remoteConfigRawJson, out var flatRemoteConfig))
            {
                mergedConfigurationJson = baseConfigurationJson ?? string.Empty;
                return !string.IsNullOrWhiteSpace(mergedConfigurationJson);
            }

            if (TryBuildStructuredConfig(flatRemoteConfig, out var structuredConfig))
            {
                mergedConfigurationJson = structuredConfig.ToString(Formatting.None);
                return true;
            }

            mergedConfigurationJson = baseConfigurationJson ?? string.Empty;
            return !string.IsNullOrWhiteSpace(mergedConfigurationJson);
        }

        private static bool TryDeserializeRemoteConfig(string remoteConfigRawJson, out Dictionary<string, string> flat)
        {
            flat = null;

            if (string.IsNullOrWhiteSpace(remoteConfigRawJson) || remoteConfigRawJson == "{}")
            {
                return false;
            }

            try
            {
                flat = JsonConvert.DeserializeObject<Dictionary<string, string>>(remoteConfigRawJson);
                return flat != null && flat.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryBuildStructuredConfig(Dictionary<string, string> remote, out JObject structuredConfig)
        {
            structuredConfig = null;

            if (remote == null || remote.Count == 0)
            {
                return false;
            }

            if (!TryGetValue(remote, "levelDataUrls", out var rawLevelDataUrls) || !TryParseJObject(rawLevelDataUrls, out var levelDataUrlsObject))
            {
                return false;
            }

            if (!TryGetValue(remote, "goalConfig", out var rawGoalConfig)
                && !TryGetValue(remote, "GoalConfig", out rawGoalConfig))
            {
                return false;
            }

            if (!TryParseJObject(rawGoalConfig, out var goalConfigObject))
            {
                return false;
            }

            var root = new JObject
            {
                ["levelDataUrls"] = levelDataUrlsObject,
                ["goalConfig"] = goalConfigObject
            };

            var allRequiredValuesFound = true;

            foreach (var key in IntegerKeys)
            {
                if (TryGetValue(remote, key, out var rawValue) && TryParseInt(rawValue, out var intValue))
                {
                    root[key] = intValue;
                    continue;
                }

                allRequiredValuesFound = false;
            }

            foreach (var key in StringKeys)
            {
                if (TryGetValue(remote, key, out var rawValue) && !string.IsNullOrWhiteSpace(rawValue))
                {
                    root[key] = rawValue;
                    continue;
                }

                allRequiredValuesFound = false;
            }
            if (!allRequiredValuesFound)
            {
                return false;
            }

            foreach (var key in BoolKeys)
            {
                if (TryGetValue(remote, key, out var rawValue)
                    && bool.TryParse(rawValue, out var boolValue))
                {
                    root[key] = boolValue;
                    continue;
                }

                allRequiredValuesFound = false;
            }

            structuredConfig = root;
            return true;
        }

        private static bool TryParseJObject(string rawJson, out JObject result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return false;
            }

            try
            {
                result = JObject.Parse(rawJson);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryGetValue(Dictionary<string, string> dictionary, string key, out string value)
        {
            value = string.Empty;

            foreach (var pair in dictionary)
            {
                if (!string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                value = pair.Value;
                return true;
            }

            return false;
        }

        private static bool TryParseInt(string rawValue, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
