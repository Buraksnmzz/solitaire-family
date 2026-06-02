using UnityEngine.SceneManagement;
using UnityEngine;
using System.Threading.Tasks;

namespace ServicesPackage
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private string mainSceneName = "MainGameScene";
        [SerializeField] private float timeoutSeconds = 10f;
        [SerializeField] private float minSplashDuration = 3f;
        [SerializeField] public bool waitFullSplashTime = true;

        public Animator splashscreenAnim;
        private static readonly int OutHash = Animator.StringToHash("in");

        public bool handlesSceneChange = false;
        public Camera cam;
        private FeatureRegistry featureRegistry;

        private async void Start()
        {
            await Task.Delay(1000);
            featureRegistry = AppRegistry.Registry;

            ServicesLogger.Log("[Bootstrapper] Initialization started.");
            float splashStartTime = Time.realtimeSinceStartup;

            ServicesLogger.Log("[Bootstrapper] Waiting for Remote Config...");

            float elapsed = 0f;
            bool rcFetched = false;
            while (elapsed < timeoutSeconds)
            {
                if (!featureRegistry.Resolve<RemoteConfigContext>().IsInitialized)
                {
                    await Task.Delay(100);
                    elapsed += 0.1f;
                }
                else
                {
                    if (!rcFetched)
                    {
                        rcFetched = true;
                        ServicesLogger.Log("[Bootstrapper] Remote Config READY");
                    }

                    if (!waitFullSplashTime)
                        break;

                    await Task.Yield();
                    elapsed += Time.deltaTime;
                }
            }

            if (!rcFetched)
                ServicesLogger.LogWarning("[Bootstrapper] Remote Config TIMEOUT (waited full splash timeout).");
            else if (waitFullSplashTime)
                ServicesLogger.Log("[Bootstrapper] Remote Config fetched, waited full splash timeout.");

            var consentFlow = featureRegistry.Resolve<ConsentFlow>();

            ServicesLogger.Log("[Bootstrapper] Resolving user consent...");
            await consentFlow.ResolveConsentAsync();

            #region Boot into Game
            // BOOT into game system ready
            float splashElapsed = Time.realtimeSinceStartup - splashStartTime;

            if (splashElapsed < minSplashDuration)
                await Task.Delay((int)((minSplashDuration - splashElapsed) * 1000));


            if (handlesSceneChange)
            {
                ServicesLogger.Log($"[Bootstrapper] Loading main scene: {mainSceneName}...");
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);

                while (!loadOp.isDone)

                    await Task.Yield();

                ServicesLogger.Log("[Bootstrapper] Main scene loaded.");
            }
            else
            {
                ServicesLogger.Log("[Bootstrapper] Scene change skipped (canChangeScene is false).");
            }

            YoogoLabManager.MarkSystemReady();
            ServicesLogger.Log("[Bootstrapper] System marked ready");

            if (splashscreenAnim != null)
            {
                ServicesLogger.Log("[Bootstrapper] Playing splash screen exit animation.");
                splashscreenAnim.SetBool(OutHash, true);

                while (splashscreenAnim != null)
                {
                    AnimatorStateInfo state = splashscreenAnim.GetCurrentAnimatorStateInfo(0);
                    if (state.IsName("SplashScreenOut") || state.IsTag("SplashScreenOut"))
                    {
                        float timeLeft = (1f - state.normalizedTime) * state.length;
                        ServicesLogger.Log($"[Bootstrapper] Animation 'SplashScreenOut' running. Waiting {timeLeft:F2}s...");
                        await Task.Delay((int)(timeLeft * 1000));
                        break;
                    }

                    if (cam)
                        cam.enabled = false;

                    await Task.Yield();
                }
            }
            else
            {
                ServicesLogger.Log("[Bootstrapper] No splashscreen animator assigned.");
            }

            await Task.Delay(1000);

            ServicesLogger.Log("[Bootstrapper] Post-animation delay completed.");

            if (handlesSceneChange)
            {
                Scene bootstrapScene = SceneManager.GetSceneByName("BootstraperScene");
                if (bootstrapScene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync("BootstraperScene");

                    ServicesLogger.Log("[Bootstrapper] BootstrapperScene unloaded successfully.");
                }
                else
                {
                    ServicesLogger.LogWarning("[Bootstrapper] Tried to unload BootstrapperScene, but it's not loaded.");
                }
            }
            ServicesLogger.Log("[Bootstrapper] Initialization complete.");
            #endregion
        }
    }
}
