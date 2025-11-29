public interface ICollectibelService : IService
{
    int Total { get; }
    bool TrySpend(int amount);
    void Add(int amount);
    void Set(int total);
}