using System.Collections.Generic;
using UnityEngine.Purchasing;

namespace IAP
{
    // Central place to keep product id strings used across the project.
    // Replace the placeholder ids below with the real ids from your store/catalog.
    public static class ProductIds
    {
        public const string CoinPack1 = "solitairefam_pack_1";
        public const string CoinPack2 = "solitairefam_pack_2";
        public const string CoinPack3 = "solitairefam_pack_3";
        public const string CoinPack4 = "solitairefam_pack_4";
        public const string CoinPack5 = "solitairefam_pack_5";
        public const string NoAdsOnly = "solitairefam_noads";
        public const string NoAdsPack = "solitairefam_noadspack"; 

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
