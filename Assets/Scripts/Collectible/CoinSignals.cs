public class CoinChangedSignal : ISignal
{
    public int NewTotal;

    public CoinChangedSignal(int modelTotalCoins)
    {
        NewTotal = modelTotalCoins;
    }
}

