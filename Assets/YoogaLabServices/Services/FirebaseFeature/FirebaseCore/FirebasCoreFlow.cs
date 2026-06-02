
using Firebase;
using System;
using System.Threading.Tasks;

namespace ServicesPackage
{
    public class FirebasCoreFlow
    {
        private readonly FirebaseCoreContext ctx;
        private FeatureRegistry registry;

        public FirebasCoreFlow(FirebaseCoreContext context)
        {
            ctx = context;
        }

        public  async Task Initialize()
        {
            if (ctx.isInitialized) return;

            try
            {
                var options = new AppOptions();

                if (ctx.dependencyTask == null)
                    ctx.dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

                var dependencyStatus = await ctx.dependencyTask;
                if (dependencyStatus != DependencyStatus.Available)
                {
                    ServicesLogger.LogError($"[FirebaseManager] Firebase dependencies not met: {dependencyStatus}");
                    return;
                }

    #if UNITY_IOS
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create();
                    ServicesLogger.Log("[FirebaseManager] FirebaseApp manually created on iOS.");
                }
                await Task.Delay(100);
    #endif
                FirebaseApp app = FirebaseApp.DefaultInstance;
                ServicesLogger.Log($"[FirebaseManager] FirebaseApp ready: {app.Name}");

                ctx.isInitialized = true;

                ServicesLogger.Log("[FirebaseManager] Firebase Core Initialized (Analytics not yet enabled).");
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[FirebaseManager] Initialization Exception: {ex.Message}");
            }
        }
    }
}