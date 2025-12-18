using System;
using Configuration;

namespace Services
{
    public class DailyAdsService : IDailyAdsService
    {
        private readonly ISavedDataService _savedDataService;
        private readonly GameConfigModel _gameConfigModel;
        private readonly DailyAdsUsageModel _model;

        public int DailyLimit => _gameConfigModel.dailyAdsWatchAmount;
        public int Remaining => _model.remainingAds;

        public DailyAdsService()
        {
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _gameConfigModel = _savedDataService.LoadData<GameConfigModel>();
            _model = _savedDataService.LoadData<DailyAdsUsageModel>();
            EnsureForToday();
        }

        public bool CanUseAd()
        {
            EnsureForToday();
            return DailyLimit > 0 && _model.remainingAds > 0;
        }

        public void UseAd()
        {
            EnsureForToday();
            if (_model.remainingAds <= 0)
            {
                return;
            }

            _model.remainingAds--;
            _savedDataService.SaveData(_model);
        }

        private void EnsureForToday()
        {
            var todayKey = int.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));

            if (_model.lastResetDayKey == 0)
            {
                ResetForToday(todayKey);
                return;
            }

            if (_model.lastResetDayKey != todayKey)
            {
                ResetForToday(todayKey);
            }
        }

        private void ResetForToday(int todayKey)
        {
            _model.remainingAds = DailyLimit;
            _model.lastResetDayKey = todayKey;
            _savedDataService.SaveData(_model);
        }
    }
}