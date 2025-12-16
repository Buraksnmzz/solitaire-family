using Collectible;
using Configuration;
using UI.OutOfMoves;

namespace UI.NoMoreMoves
{
    public class NoMoreMovesPresenter:  BasePresenter<NoMoreMovesView>
    {
        IEventDispatcherService _eventDispatcher;
        ISavedDataService _savedDataService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            View.RestartButtonClicked += OnRestartButtonClick;
            View.ContinueButtonClicked += OnContinueClick;
            View.JokerButtonClicked += OnJokerClick;
        }

        private void OnJokerClick()
        {
            _eventDispatcher.Dispatch(new JokerClickedSignal());
            View.Hide();
        }

        private void OnContinueClick()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (collectibleModel.totalCoins < gameConfigModel.jokerCost) return;
            collectibleModel.totalCoins -= gameConfigModel.jokerCost;
            _eventDispatcher.Dispatch(new ContinueWithCoinAddJokerSignal());
            View.Hide();
        }

        private void OnRestartButtonClick()
        {
            _eventDispatcher.Dispatch(new RestartButtonClickSignal());
            View.Hide();
        }
    }
}