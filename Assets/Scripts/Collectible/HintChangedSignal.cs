namespace Collectible
{
    public class HintChangedSignal : ISignal
    {
        public int Amount;
        public HintChangedSignal(int amount)
        {
            Amount =  amount;
        }
    }
}