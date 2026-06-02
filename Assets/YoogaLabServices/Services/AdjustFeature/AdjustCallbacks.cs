using AdjustSdk;

namespace ServicesPackage
{
    public class AdjustCallbacks
    {
        private readonly AdjustCoreContext ctx;
        public AdjustCallbacks(AdjustCoreContext context) => ctx = context;

        internal void OnAdjustEventSuccess(AdjustEventSuccess success)
        {
            ServicesLogger.Log(
                $"[Adjust] Event SUCCESS | Token={success.EventToken}"
            );
        }

        internal void OnAdjustEventFailure(AdjustEventFailure failure)
        {
            if (!failure.WillRetry)
            {
                ServicesLogger.Log(
                    $"[Adjust] Event ignored (expected) | Token={failure.EventToken}"
                );
                return;
            }

            ServicesLogger.LogError(
                $"[Adjust] Event FAILED (unexpected) | " +
                $"Token={failure.EventToken}, Reason={failure.Message}"
            );
        }
    }
}