using Collectible;
using Configuration;
using Core.Scripts.Services;
using UI.NoMoreMoves;
using UI.Shop;

namespace UI.OutOfMoves
{
    public class OutOfMovesPresenter : BasePresenter<OutOfMovesView>
    {
        IEventDispatcherService _eventDispatcher;
        ISavedDataService _savedDataService;
        IDailyAdsService _dailyAdsService;
        IUIService _uiService;
        ISoundService _soundService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _dailyAdsService = ServiceLocator.GetService<IDailyAdsService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            View.RestartButtonClicked += OnRestartButtonClick;
            View.AddMovesClicked += OnAddMovesClick;
            View.ContinueButtonClicked += OnContinueClick;
            View.CloseButtonClicked += OnRestartButtonClick;
        }

        public override void ViewShown()
        {
            base.ViewShown();
            _soundService.PlaySound(ClipName.Lose);
            UpdateUsage();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            View.SetExtraMovesCostText(gameConfigModel.extraMovesCost);
        }

        private void OnContinueClick()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (collectibleModel.totalCoins < gameConfigModel.extraMovesCost)
            {
                _uiService.ShowPopup<ShopPresenter>();
                return;
            }
            collectibleModel.totalCoins -= gameConfigModel.extraMovesCost;
            _savedDataService.SaveData(collectibleModel);
            _eventDispatcher.Dispatch(new ContinueWithCoinAddMovesSignal());
            View.Hide();
        }

        private void OnAddMovesClick()
        {
            if (!_dailyAdsService.CanUseAd())
            {
                UpdateUsage();
                return;
            }

            _dailyAdsService.UseAd();
            UpdateUsage();
            _eventDispatcher.Dispatch(new AddMovesClickedSignal());
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
            View.SetAddMovesButtonActive(_dailyAdsService.CanUseAd());
        }
    }
}