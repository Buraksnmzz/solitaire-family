using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace ServicesPackage
{
    public class ServicesInitializer : MonoBehaviour
    {
        public static ServicesInitializer Instance { get; private set; }
        public FeatureManifest modules;
        private FeatureRegistry _registry;


        public bool canLog = false;

        private async void Awake()
        {


            DontDestroyOnLoad(gameObject);
            Instance = this;

            _registry = new FeatureRegistry();
            AppRegistry.Registry = _registry;

            var firBase = new FirebaseCoreFeatureMediator();
            firBase.Setup(_registry);
            await _registry.Resolve<FirebasCoreFlow>().Initialize();

            var rcCtx = new RemoteConfigContext();
            var rcFlow = new RemoteConfigFlow(rcCtx);
            _registry.Register(rcCtx);
            _registry.Register(rcFlow);

            var regionCtx = new RegionContext();
            var regionFlow = new RegionFlow(regionCtx);
            _registry.Register(regionCtx);
            _registry.Register(regionFlow);
            regionFlow.Initialize();

            var consentCtx = new ConsentContext();
            var consentFlow = new ConsentFlow(consentCtx);
            _registry.Register(consentCtx);
            _registry.Register(consentFlow);
            //consentFlow.Initialize(rcCtx);
            consentFlow.Initialize(rcCtx, regionCtx);

   

            var sessionCtx = new SessionManagerContext();
            var sessionFlow = new SessionManagerFlow(sessionCtx);
            _registry.Register(sessionCtx);
            _registry.Register(sessionFlow);
            sessionFlow.Initialize();

            await rcFlow.Initialize();

            var configInit = new ServicesConfigInitializer();
            _registry.Register(configInit);

            await InitializeUnityGamingServices();

            InitializeFeatures();

            Debug.Log("[NATIVE TEST] Yooga_IsLoggingEnabled = " + NativeLogging.IsEnabled());
        }

        private void InitializeFeatures()
        {
            foreach (var feature in modules.features)
                feature.Setup(_registry);
        }

        public async Task InitializeUnityGamingServices()
        {
            try
            {
                var options = new InitializationOptions();
                options.SetEnvironmentName("production");

                await UnityServices.InitializeAsync(options);

                ServicesLogger.Log("[SDKManager] Unity Gaming Services Initialized.");
            }
            catch (Exception ex)
            {
                ServicesLogger.LogError($"[SDKManager] Failed to initialize Unity Gaming Services: {ex.Message}");
            }
        }
    }
}
