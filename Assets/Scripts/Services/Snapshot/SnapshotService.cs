using System.Collections.Generic;
using Levels;

public class SnapshotService : ISnapshotService
{
    readonly ISavedDataService _savedDataService;

    public SnapshotService()
    {
        _savedDataService = ServiceLocator.GetService<ISavedDataService>();
    }

    public void SaveSnapshot(SnapShotModel snapshot)
    {
        if (snapshot == null)
            return;

        snapshot.Cards ??= new List<CardSnapshot>();
        var snapshotStore = _savedDataService.LoadData<GameModeSnapshotStoreModel>();
        snapshotStore.SetSnapshot(snapshot.GameMode, snapshot);
        _savedDataService.SaveData(snapshotStore);
    }

    public void ClearSnapshot()
    {
        var snapshotStore = _savedDataService.LoadData<GameModeSnapshotStoreModel>();
        snapshotStore.SetSnapshot(GetSelectedGameMode(), null);
        _savedDataService.SaveData(snapshotStore);
    }

    public bool HasSnapShot()
    {
        var snapshot = LoadSnapshot();
        if (snapshot == null)
            return false;

        return snapshot.Cards != null && snapshot.Cards.Count > 0;
    }

    public SnapShotModel LoadSnapshot()
    {
        var snapshotStore = _savedDataService.LoadData<GameModeSnapshotStoreModel>();
        return snapshotStore.GetSnapshot(GetSelectedGameMode());
    }

    private GameMode GetSelectedGameMode()
    {
        return _savedDataService.GetModel<GameModeSelectionModel>().SelectedGameMode;
    }
}
