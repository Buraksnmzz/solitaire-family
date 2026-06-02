namespace ServicesPackage
{
    public class AdjustCoreContext
    {
        internal string currentAdjustAppToken;

        public bool isInitialized = false;
        public bool IsInitialized => isInitialized;

        internal string[] levelEvents;
        internal int[] levelEventDelays;
        internal bool AdsServices = true;
        internal bool IsSandbox = false;
    }
}