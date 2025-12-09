using Services.Drag;

namespace Services
{
    public class DragStateService : IDragStateService
    {
        bool _isDragging;

        public bool CanStartDrag()
        {
            return !_isDragging;
        }

        public void StartDrag()
        {
            _isDragging = true;
        }

        public void EndDrag()
        {
            _isDragging = false;
        }
    }
}
