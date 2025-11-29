using UI.Gameplay;

namespace UI.MainMenu
{
    public class MainMenuPresenter : BasePresenter<MainMenuView>
    {
        IUIService _uiService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            View.LevelButtonClicked += OnLevelButtonClicked;
        }

        private void OnLevelButtonClicked()
        {
            _uiService.HidePopup<MainMenuPresenter>();
            _uiService.ShowPopup<GameplayPresenter>();
        }
    }
}