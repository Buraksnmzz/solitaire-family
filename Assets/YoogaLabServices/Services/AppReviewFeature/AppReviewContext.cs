#if UNITY_ANDROID
using Google.Play.Review;
#endif

namespace ServicesPackage
{ 
    public class AppReviewContext
    {
    #if UNITY_ANDROID
        public Google.Play.Review.ReviewManager reviewManager;
        public PlayReviewInfo reviewInfo;
    #endif
    }
}
