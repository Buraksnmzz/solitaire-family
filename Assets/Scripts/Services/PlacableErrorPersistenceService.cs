using System;
using Services;

namespace Services
{
    public class PlacableErrorPersistenceService : IPlacableErrorPersistenceService
    {
        private const int SuppressAfterCount = 3;
        private readonly ISavedDataService _savedDataService;
        private readonly PlacableErrorModel _model;

        public PlacableErrorPersistenceService()
        {
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _model = _savedDataService.LoadData<PlacableErrorModel>();
        }

        public bool ShouldShow(string message)
        {
            if (string.IsNullOrEmpty(message)) return true;
            if (_model.errorShowCounts.TryGetValue(message, out var count))
            {
                return count < SuppressAfterCount;
            }
            return true;
        }

        public void RecordShown(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (_model.errorShowCounts.TryGetValue(message, out var count))
            {
                _model.errorShowCounts[message] = count + 1;
            }
            else
            {
                _model.errorShowCounts[message] = 1;
            }
            _savedDataService.SaveData(_model);
        }

        public int GetCount(string message)
        {
            if (string.IsNullOrEmpty(message)) return 0;
            return _model.errorShowCounts.TryGetValue(message, out var count) ? count : 0;
        }

        public void Reset(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (_model.errorShowCounts.ContainsKey(message))
            {
                _model.errorShowCounts.Remove(message);
                _savedDataService.SaveData(_model);
            }
        }
    }
}
