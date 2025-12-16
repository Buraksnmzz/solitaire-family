using Collectible;
using Configuration;
using UI.NoMoreMoves;

namespace UI.OutOfMoves
{
    public class OutOfMovesPresenter: BasePresenter<OutOfMovesView>
    {
        IEventDispatcherService _eventDispatcher;
        ISavedDataService _savedDataService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            View.RestartButtonClicked += OnRestartButtonClick;
            View.AddMovesClicked += OnAddMovesClick;
            View.ContinueButtonClicked += OnContinueClick;
        }

        private void OnContinueClick()
        {
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var gameConfigModel = _savedDataService.GetModel<GameConfigModel>();
            if (collectibleModel.totalCoins < gameConfigModel.extraMovesCost) return;
            collectibleModel.totalCoins -= gameConfigModel.extraMovesCost;
            _eventDispatcher.Dispatch(new ContinueWithCoinAddMovesSignal());
            View.Hide();
        }

        private void OnAddMovesClick()
        {
            _eventDispatcher.Dispatch(new AddMovesClickedSignal());
            View.Hide();
        }

        private void OnRestartButtonClick()
        {
            _eventDispatcher.Dispatch(new RestartButtonClickSignal());
            View.Hide();
        }
    }
}