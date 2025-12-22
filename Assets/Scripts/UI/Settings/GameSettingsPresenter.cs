using UI.Gameplay;
using UI.MainMenu;
using UI.NoMoreMoves;

namespace UI.Settings
{
    public class GameSettingsPresenter : BaseSettingsPresenter<GameSettingsView>
    {
        private IEventDispatcherService _eventDispatcher;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            View.RestartButtonClicked += OnRestartButtonClicked;
            View.MainMenuButtonClicked += OnMainMenuButtonClicked;
        }

        private void OnMainMenuButtonClicked()
        {
            UIService.HidePopup<GameplayPresenter>();
            View.Hide();
            UIService.ShowPopup<MainMenuPresenter>();
            _eventDispatcher.Dispatch(new MainMenuButtonClickSignal());
        }

        private void OnRestartButtonClicked()
        {
            _eventDispatcher.Dispatch(new RestartButtonClickSignal());
            View.Hide();
        }

    }
}