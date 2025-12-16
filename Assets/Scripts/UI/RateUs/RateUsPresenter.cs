using Levels;
using UnityEngine;

namespace UI.RateUs
{
    public class RateUsPresenter: BasePresenter<RateUsView>
    {
        ISavedDataService _savedDataService;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            View.OnStarsClicked += OnStarRating;
        }

        private void OnStarRating(int rating)
        {
            if (rating > 3)
            {
                YoogoLabManager.ShowNativeReview();
                SetHasRatedGame();
            }
            else
            {
                SetLastRateUsShownGameId(_savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex);
            }
        }

        private void SetHasRatedGame()
        {
            PlayerPrefs.SetInt(StringConstants.HasRatedGame, 1);
            PlayerPrefs.Save();
        }

        private static void SetLastRateUsShownGameId(int id)
        {
            PlayerPrefs.SetInt(StringConstants.LastRateUsShownGameID, id);
            PlayerPrefs.Save();
        }
    }
}