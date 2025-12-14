public class CoinChangedSignal : ISignal
{
    public int Amount;

    public CoinChangedSignal(int amount)
    {
        Amount = amount;
    }
}

