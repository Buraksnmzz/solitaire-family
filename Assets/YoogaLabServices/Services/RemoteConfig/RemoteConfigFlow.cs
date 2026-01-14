using Firebase.RemoteConfig;
using Firebase;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServicesPackage
{
    public class RemoteConfigFlow
    {
        private readonly RemoteConfigContext ctx;

        public RemoteConfigFlow(RemoteConfigContext context)
        {
            ctx = context;
        }

        public async Task Initialize()
        {
            if (ctx.IsInitialized) return;

            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus != DependencyStatus.Available)
                {
                    ServicesLogger.LogError($"[RemoteConfigFetcher] Firebase dependencies are not met: {dependencyStatus}");
                    return;
                }

                var remoteConfig = FirebaseRemoteConfig.DefaultInstance;
                var settings = new Firebase.RemoteConfig.ConfigSettings { MinimumFetchIntervalInMilliseconds = 0 };
                await remoteConfig.SetConfigSettingsAsync(settings);

                var fetchTask = remoteConfig.FetchAsync(TimeSpan.Zero);
                if (await Task.WhenAny(fetchTask, Task.Delay(4000)) != fetchTask)
                {
                    ServicesLogger.LogWarning("[RemoteConfigFetcher] FetchAsync timeout. Using defaults.");
                }

                await remoteConfig.ActivateAsync();

                ApplyConfig();

                if (ctx.configDataValues.Count == 0)
                {
                    ServicesLogger.LogWarning("[RemoteConfigFetcher] Remote Config EMPTY. Using defaults.");
                }

                ctx.IsInitialized = true;
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[RemoteConfigFetcher] Initialization Error: {ex.Message}");
            }
        }

        private  object ParseConfigValue(ConfigValue value)
        {
            if (value.StringValue == "true" || value.StringValue == "false") return value.BooleanValue;
            if (long.TryParse(value.StringValue, out long longResult)) return longResult;
            if (double.TryParse(value.StringValue, out double doubleResult)) return doubleResult;
            return value.StringValue;
        }

        public void ApplyConfig()
        {
            ctx.configDataValues.Clear();
            foreach (var key in FirebaseRemoteConfig.DefaultInstance.Keys)
            {
                var value = FirebaseRemoteConfig.DefaultInstance.GetValue(key);
                ctx.configDataValues[key] = value.StringValue;
                //configDataValues[key] = ParseConfigValue(value);
            }
            ctx.cachedJsonConfig = JsonConvert.SerializeObject(ctx.configDataValues, Formatting.Indented);
            ServicesLogger.Log($"[RemoteConfigFetcher] Config fetched and parsed: \n{ctx.cachedJsonConfig}");
        }

        public string GetRemoteConfig()
        {
            if (!ctx.IsInitialized)
            {
                ServicesLogger.LogWarning("[RemoteConfigFetcher] Remote Config is not initialized yet. Returning cached config.");
                return ctx.cachedJsonConfig;
            }

            if (ctx.configDataValues.Count == 0)
            {
                ServicesLogger.LogWarning("[RemoteConfigFetcher] ConfigDataValues is EMPTY. Returning cached config.");
                return ctx.cachedJsonConfig;
            }

            string jsonConfig = JsonConvert.SerializeObject(ctx.configDataValues, Formatting.Indented);
            ctx.cachedJsonConfig = jsonConfig;
            return jsonConfig;
        }
    }
}