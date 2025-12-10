using System.Collections.Generic;

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
        _savedDataService.SaveData(snapshot);
    }

    public void ClearSnapshot()
    {
        _savedDataService.DeleteData<SnapShotModel>();
    }

    public bool HasSnapShot()
    {
        var snapshot = _savedDataService.LoadData<SnapShotModel>();
        if (snapshot == null)
            return false;

        return snapshot.Cards != null && snapshot.Cards.Count > 0;
    }

    public SnapShotModel LoadSnapshot()
    {
        return _savedDataService.LoadData<SnapShotModel>();
    }
}
