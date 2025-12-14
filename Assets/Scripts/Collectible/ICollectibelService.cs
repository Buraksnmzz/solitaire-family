public interface ICollectibelService : IService
{
    int TotalCoin{ get; }
    void AddCoin(int amount);
    void AddHint(int amount);
    void AddJoker(int amount);
    bool TrySpendCoin(int amount);
    bool TrySpendHint(int amount);
    bool TrySpendJoker(int amount);
}