public interface ISnapshotService : IService
{
    void SaveSnapshot(SnapShotModel snapshot);
    void ClearSnapshot();
    bool HasSnapShot();
}
