using Configuration;

namespace IAP
{
    public static class ProductRewardResolver
    {
        public static CatalogProduct Resolve(GameConfigModel configModel, string productId)
        {
            if (configModel == null || string.IsNullOrEmpty(productId))
            {
                return new CatalogProduct { Id = productId };
            }

            if (productId == ProductIds.NoAdsPack)
            {
                return new CatalogProduct
                {
                    Id = productId,
                    Coins = configModel.noAdsPackCoinReward,
                    Jokers = configModel.noAdsPackJokerReward,
                    Hints = configModel.noAdsPackHintReward,
                    Undos = configModel.noAdsPackUndoReward
                };
            }

            if (productId == ProductIds.CoinPack1)
            {
                return new CatalogProduct { Id = productId, Coins = configModel.shopCoinReward1 };
            }

            if (productId == ProductIds.CoinPack2)
            {
                return new CatalogProduct { Id = productId, Coins = configModel.shopCoinReward2 };
            }

            if (productId == ProductIds.CoinPack3)
            {
                return new CatalogProduct { Id = productId, Coins = configModel.shopCoinReward3 };
            }

            if (productId == ProductIds.CoinPack4)
            {
                return new CatalogProduct { Id = productId, Coins = configModel.shopCoinReward4 };
            }

            if (productId == ProductIds.CoinPack5)
            {
                return new CatalogProduct { Id = productId, Coins = configModel.shopCoinReward5 };
            }

            return new CatalogProduct { Id = productId };
        }
    }
}
