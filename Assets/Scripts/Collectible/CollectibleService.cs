namespace Collectible
{
    public class CollectibleService : ICollectibelService
    {
        private readonly ISavedDataService _savedDataService;
        private readonly IEventDispatcherService _eventDispatcher;
        private readonly CollectibleModel _model;

        public CollectibleService()
        {
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _model = _savedDataService.LoadData<CollectibleModel>();
        }

        public int Total => _model.totalCoins;

        private void PersistAndDispatchChangedCoin(int amount)
        {
            _savedDataService.SaveData(_model);
            _eventDispatcher.Dispatch(new CoinChangedSignal(amount));
        }

        private void PersistAndDispatchChangedHint(int amount)
        {
            _savedDataService.SaveData(_model);
            _eventDispatcher.Dispatch(new HintChangedSignal(amount));
        }

        private void PersistAndDispatchChangedJoker(int amount)
        {
            _savedDataService.SaveData(_model);
            _eventDispatcher.Dispatch(new JokerChangedSignal(amount));
        }

        public int TotalCoin { get; }
        public void AddCoin(int amount)
        {
            if (amount <= 0) return;
            checked { _model.totalCoins += amount; }
            PersistAndDispatchChangedCoin(amount);
        }

        public void AddHint(int amount)
        {
            if (amount <= 0) return;
            checked { _model.totalHints += amount; }
            PersistAndDispatchChangedHint(amount);
        }

        public void AddJoker(int amount)
        {
            if (amount <= 0) return;
            checked { _model.totalJokers += amount; }
            PersistAndDispatchChangedJoker(amount);
        }

        public bool TrySpendCoin(int amount)
        {
            if (amount <= 0) return true;
            if (_model.totalCoins < amount) return false;
            _model.totalCoins -= amount;
            PersistAndDispatchChangedCoin(-amount);
            return true;
        }

        public bool TrySpendHint(int amount)
        {
            if (amount <= 0) return true;
            if (_model.totalHints < amount) return false;
            _model.totalHints -= amount;
            PersistAndDispatchChangedHint(-amount);
            return true;
        }

        public bool TrySpendJoker(int amount)
        {
            if (amount <= 0) return true;
            if (_model.totalJokers < amount) return false;
            _model.totalJokers -= amount;
            PersistAndDispatchChangedJoker(-amount);
            return true;
        }
    }
}
