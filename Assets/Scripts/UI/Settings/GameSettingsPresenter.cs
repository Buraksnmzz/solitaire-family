using DG.Tweening;
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
            _eventDispatcher.Dispatch(new MainMenuButtonClickSignal());
            View.Hide();
            DOVirtual.DelayedCall(0.3f, () =>
            {
                UIService.HidePopup<GameplayPresenter>();
                UIService.ShowPopup<MainMenuPresenter>();
            });
            
        }

        private void OnRestartButtonClicked()
        {
            _eventDispatcher.Dispatch(new RestartButtonClickSignal());
            View.Hide();
        }

    }
}