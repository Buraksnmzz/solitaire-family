namespace UI.Signals
{
    public class CardMovementStateChangedSignal: ISignal
    {
        public bool IsMoving { get; }

        public CardMovementStateChangedSignal(bool isMoving)
        {
            IsMoving = isMoving;
        }
    }
}
