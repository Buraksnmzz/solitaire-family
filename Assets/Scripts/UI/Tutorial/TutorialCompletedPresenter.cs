using Services;
using UI.Gameplay;
using UI.MainMenu;
using UnityEngine;

namespace UI.Tutorial
{
    public class TutorialCompletedPresenter : BasePresenter<TutorialCompletedView>
    {
        IUIService _uiService;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            View.ContinueClicked += OnContinueClicked;
        }

        void OnContinueClicked()
        {
            PlayerPrefs.Save();
            _uiService.HidePopup<TutorialCompletedPresenter>();
            _uiService.HidePopup<TutorialGamePresenter>();
            _uiService.ShowPopup<GameplayPresenter>();
            View.DisableContinueButton();
        }
    }
}
