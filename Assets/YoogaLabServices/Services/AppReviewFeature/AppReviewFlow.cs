#if UNITY_ANDROID
using Google.Play.Review;
#endif
#if UNITY_IOS
using UnityEngine.iOS;
#endif
namespace ServicesPackage
{
    public class AppReviewFlow
    {
        private readonly AppReviewContext ctx;

        public AppReviewFlow(AppReviewContext context)
        {
            ctx = context;
        }

        public void Initialize()
        {
            SignalBusService.Subscribe<RequestReviewSignal>(RequestReview);
        }

        public void RequestReview(RequestReviewSignal sig)
        {
            ServicesLogger.Log("[AppReviewManager] Request Review");
    #if UNITY_ANDROID
            RequestGooglePlayReview();
    #elif UNITY_IOS
            RequestAppStoreReview();
    #else
            PluginLogger.LogWarning("[AppReviewManager] Review system is not supported on this platform.");
    #endif
        }

    #if UNITY_ANDROID
        private void RequestGooglePlayReview()
        {
            ctx.reviewManager = new ReviewManager();

            var requestFlowOperation = ctx.reviewManager.RequestReviewFlow();
            requestFlowOperation.Completed += request =>
            {
                if (request.Error != ReviewErrorCode.NoError)
                {
                    ServicesLogger.LogError($"[AppReviewManager] Error requesting review flow: {request.Error}");
                    return;
                }

                ctx.reviewInfo = request.GetResult();
                LaunchReviewFlow();
            };
        }

        private void LaunchReviewFlow()
        {
            var launchFlowOperation = ctx.reviewManager.LaunchReviewFlow(ctx.reviewInfo);
            launchFlowOperation.Completed += launch =>
            {
                if (launch.Error != ReviewErrorCode.NoError)
                {
                    ServicesLogger.LogError($"[AppReviewManager] Error launching review flow: {launch.Error}");
                }
                else
                {
                    ServicesLogger.Log("[AppReviewManager] Successfully launched Google Play Review.");
                }
            };
        }
    #endif

    #if UNITY_IOS
        private void RequestAppStoreReview()
        {
            try
            {
                Device.RequestStoreReview();
                ServicesLogger.Log("[AppReviewManager] Requested App Store Review.");
            }
            catch (System.Exception ex)
            {
                ServicesLogger.LogError($"[AppReviewManager] Error launching App Store Review: {ex.Message}");
            }
        }
    #endif
    }
}
