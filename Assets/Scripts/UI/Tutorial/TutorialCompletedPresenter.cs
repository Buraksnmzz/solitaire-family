using Core.Scripts.Services;
using Services;
using UI.Gameplay;
using UI.MainMenu;
using UnityEngine;

namespace UI.Tutorial
{
    public class TutorialCompletedPresenter : BasePresenter<TutorialCompletedView>
    {
        IUIService _uiService;
        ISoundService _soundService;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            View.ContinueClicked += OnContinueClicked;
        }

        public override void ViewShown()
        {
            base.ViewShown();
            _soundService.PlaySound(ClipName.WinView);
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
