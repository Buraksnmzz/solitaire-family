using System.Collections.Generic;
using UnityEngine.Purchasing;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
#endif

namespace IAP
{
    // Central place to keep product id strings used across the project.
    // Replace the placeholder ids below with the real ids from your store/catalog.
    public static class ProductIds
    {
        const string CoinPack1 = "coin_pack_1";
        const string CoinPack2 = "coin_pack_2";
        const string CoinPack3 = "coin_pack_3";
        const string CoinPack4 = "coin_pack_4";
        const string CoinPack5 = "coin_pack_5";
        const string NoAdsPack = "noads_pack";
        const string NoAdsOnly = "noads_only"; 

        public static readonly Dictionary<string, ProductType> ProductTypeMap = new Dictionary<string, ProductType>
        {
            { CoinPack1, ProductType.Consumable },
            { CoinPack2, ProductType.Consumable },
            { CoinPack3, ProductType.Consumable },
            { CoinPack4, ProductType.Consumable },
            { CoinPack5, ProductType.Consumable },
            { NoAdsPack, ProductType.NonConsumable },
            { NoAdsOnly, ProductType.NonConsumable }
        };
    }
}
