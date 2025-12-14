using UI.Gameplay;

namespace UI.Win
{
    public class WinPresenter: BasePresenter<WinView>
    {
        IUIService _uiService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            View.ContinueButtonClicked += OnContinue;
        }

        private void OnContinue()
        {
            _uiService.ShowPopup<GameplayPresenter>();
            View.Hide();
        }
    }
}