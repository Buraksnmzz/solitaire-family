namespace Gameplay
{
    public class PlacableErrorSignal : ISignal
    {
        public readonly string PlacableErrorMessage;
        public PlacableErrorSignal(string error)
        {
            PlacableErrorMessage = error;
        }
    }
}