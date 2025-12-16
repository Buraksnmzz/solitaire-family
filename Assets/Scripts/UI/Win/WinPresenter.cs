using Collectible;
using Configuration;
using Levels;
using UI.Gameplay;

namespace UI.Win
{
    public class WinPresenter: BasePresenter<WinView>
    {
        IUIService _uiService;
        ISavedDataService _savedDataService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            View.ContinueButtonClicked += OnContinue;
        }

        public override void ViewShown()
        {
            base.ViewShown();
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var configModel = _savedDataService.GetModel<GameConfigModel>();
            var levelProgressModel = _savedDataService.GetModel<LevelProgressModel>();
            View.SetCoinText(collectibleModel.totalCoins);
            View.SetLevelText(levelProgressModel.CurrentLevelIndex);
            collectibleModel.totalCoins += configModel.earnedCoinAtLevelEnd;
        }

        private void OnContinue()
        {
            _uiService.ShowPopup<GameplayPresenter>();
            View.Hide();
        }
    }
}