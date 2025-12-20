using Collectible;
using Unity.VisualScripting;

namespace UI.Shop
{
    public class ShopPresenter: BasePresenter<ShopView>
    {
        ISavedDataService _savedDataService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
        }

        public override void ViewShown()
        {
            base.ViewShown();
            View.SetCoinText(_savedDataService.GetModel<CollectibleModel>().totalCoins);
        }
    }
}