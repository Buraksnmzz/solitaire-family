public interface IPlacableErrorPersistenceService : IService
{
    bool ShouldShow(string message);
    void RecordShown(string message);
    int GetCount(string message);
    void Reset(string message);
}
