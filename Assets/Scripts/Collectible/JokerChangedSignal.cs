namespace Collectible
{
    public class JokerChangedSignal : ISignal
    {
        public int Amount;
        public JokerChangedSignal(int amount)
        {
            Amount = amount;
        }
    }
}