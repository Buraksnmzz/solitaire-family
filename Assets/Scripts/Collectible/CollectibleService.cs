using System;
using Collectible;
using UnityEngine;

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
        if (_model == null)
        {
            _model = new CollectibleModel();
            _savedDataService.SaveData(_model);
        }
    }

    public int Total => _model.totalCoins;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        checked { _model.totalCoins += amount; }
        PersistAndDispatchChanged();
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (_model.totalCoins < amount) return false;
        _model.totalCoins -= amount;
        PersistAndDispatchChanged();
        return true;
    }

    public void Set(int total)
    {
        _model.totalCoins = Math.Max(0, total);
        PersistAndDispatchChanged();
    }

    private void PersistAndDispatchChanged()
    {
        _savedDataService.SaveData(_model);
        _eventDispatcher.Dispatch(new CoinChangedSignal(_model.totalCoins));
    }

    public void Dispose() { }
}
