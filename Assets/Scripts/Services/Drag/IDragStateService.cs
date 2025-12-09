namespace Services.Drag
{
    public interface IDragStateService : IService
    {
        bool CanStartDrag();
        void StartDrag();
        void EndDrag();
    }
}
