using System;

namespace IAP
{
    public interface IIAPService : IService
    {
        void Purchase(string productId,  Action<bool> onComplete);
        string GetLocalizedPrice(string productId);
    }
}