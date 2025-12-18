using Collectible;
using Configuration;
using UI.OutOfMoves;

namespace UI.NoMoreMoves
{
    public class NoMoreMovesPresenter : BasePresenter<NoMoreMovesView>
    {
        IEventDispatcherService _eventDispatcher;
        ISavedDataService _savedDataService;
        IDailyAdsService _dailyAdsService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _dailyAdsService = ServiceLocator.GetService<IDailyAdsService>();
            View.RestartButtonClicked += OnRestartButtonClick;
            View.ContinueButtonClicked += OnContinueClick;
            View.JokerButtonClicked += OnJokerClick;
            View.CloseButtonClicked += OnRestartButtonClick;
        }

        public override void ViewShown()
        {
            base.ViewShown();
            UpdateUsage();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            View.SetJokerCostText(gameConfigModel.jokerCost);
        }

        private void OnJokerClick()
        {
            if (!_dailyAdsService.CanUseAd())
            {
                UpdateUsage();
                return;
            }

            _dailyAdsService.UseAd();
            UpdateUsage();
            _eventDispatcher.Dispatch(new JokerClickedSignal());
            View.Hide();
        }

        private void OnContinueClick()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (collectibleModel.totalCoins < gameConfigModel.jokerCost) return;
            collectibleModel.totalCoins -= gameConfigModel.jokerCost;
            _savedDataService.SaveData(collectibleModel);
            _eventDispatcher.Dispatch(new ContinueWithCoinAddJokerSignal());
            View.Hide();
        }

        private void OnRestartButtonClick()
        {
            _eventDispatcher.Dispatch(new RestartButtonClickSignal());
            View.Hide();
        }

        private void UpdateUsage()
        {
            var total = _dailyAdsService.DailyLimit;
            var remaining = _dailyAdsService.Remaining;
            View.SetUsageText(remaining, total);
            View.SetJokerButtonActive(_dailyAdsService.CanUseAd());
        }
    }
}