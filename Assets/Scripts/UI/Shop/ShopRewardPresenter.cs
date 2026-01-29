namespace UI.Shop
{
    public class ShopRewardPresenter: BasePresenterWithData<ShopRewardView, ShopRewardData>
    {
        IEventDispatcherService _eventDispatcher;
        IUIService _uiService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            View.OkClicked += OnOkClicked;
        }

        private void OnOkClicked()
        {
            _uiService.HidePopup<ShopRewardPresenter>();
            _eventDispatcher.Dispatch(new ShopRewardClosedSignal(Data.RewardType == RewardType.NoAds));
        }

        protected override void OnDataSet()
        {
            base.OnDataSet();
            if (Data.RewardType == RewardType.NoAds)
                View.ShowNoAds();
            else if (Data.RewardType == RewardType.Pack)
                View.ShowPack(Data.CoinReward, Data.HintReward, Data.JokerReward, Data.UndoReward);
            else if (Data.RewardType == RewardType.Coin)
                View.ShowCoin(Data.CoinReward);
        }
    }
}