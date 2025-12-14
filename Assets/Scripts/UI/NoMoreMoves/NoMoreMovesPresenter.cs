namespace UI.NoMoreMoves
{
    public class NoMoreMovesPresenter:  BasePresenter<NoMoreMovesView>
    {
        IEventDispatcherService _eventDispatcher;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            View.RestartButtonClicked += OnRestartButtonClick;
            View.AddMovesClicked += OnAddMovesClick;
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

    public class AddMovesClickedSignal : ISignal
    {
    }

    public class RestartButtonClickSignal : ISignal
    {
    }
}