using Collectible;
using Configuration;
using Core.Scripts.Services;
using DG.Tweening;
using Levels;
using UI.Gameplay;
using UI.RateUs;
using UnityEngine;

namespace UI.Win
{
    public class WinPresenter : BasePresenter<WinView>
    {
        IUIService _uiService;
        ISavedDataService _savedDataService;
        ISoundService _soundService;
        IConfigurationService _configurationService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _uiService = ServiceLocator.GetService<IUIService>();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _configurationService = ServiceLocator.GetService<IConfigurationService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            View.ContinueButtonClicked += OnContinue;
            View.IntroAnimationFinished += OnIntroAnimationFinished;
            View.OnIconMoved += OnIconMoved;
        }

        private void OnIconMoved()
        {
            
        }

        private void OnIntroAnimationFinished()
        {
            if (PlayerPrefs.GetInt(StringConstants.HasRatedGame) == 1)
                return;
            var configModel = _savedDataService.GetModel<GameConfigModel>();
            var levelProgressModel = _savedDataService.GetModel<LevelProgressModel>();
            var triggerLevels = configModel.rateUsTriggerLevels;
            if (triggerLevels == null || triggerLevels.Length == 0)
                return;

            var currentLevel = levelProgressModel.CurrentLevelIndex;
            for (var i = 0; i < triggerLevels.Length; i++)
            {
                if (triggerLevels[i] == currentLevel)
                {
                    _uiService.ShowPopup<RateUsPresenter>();
                    break;
                }
            }
        }

        public override void ViewShown()
        {
            base.ViewShown();
            var collectibleModel = _savedDataService.GetModel<CollectibleModel>();
            var configModel = _savedDataService.GetModel<GameConfigModel>();
            var levelProgressModel = _savedDataService.GetModel<LevelProgressModel>();
            View.SetCoinText(collectibleModel.totalCoins);
            View.SetLevelText(levelProgressModel.CurrentLevelIndex);
            View.SetCoinAmountToCollectText(configModel.earnedCoinAtLevelEnd);
            collectibleModel.totalCoins += configModel.earnedCoinAtLevelEnd;
            View.finalCoins = collectibleModel.totalCoins;
            _savedDataService.SaveData(collectibleModel);
            _soundService.PlaySound(ClipName.WinView);
        }

        private void OnContinue()
        {
            View.DisableContinueButton();
            View.PlayCoinAnimation(() =>
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    _uiService.ShowPopup<GameplayPresenter>();
                    View.Hide();
                });
                
            });
        }
    }
}