using Card;
using Gameplay;

public class TutorialMoveRestrictionService : ITutorialMoveRestrictionService
{
    CardPresenter _allowedPresenter;
    CardContainer _allowedTarget;

    public bool IsActive => _allowedPresenter != null && _allowedTarget != null;

    public bool IsDragAllowed(CardPresenter presenter)
    {
        if (!IsActive) return false;
        return presenter == _allowedPresenter;
    }

    public bool IsDropAllowed(CardPresenter presenter, CardContainer targetContainer)
    {
        if (!IsActive) return false;
        if (presenter != _allowedPresenter) return false;
        return targetContainer == _allowedTarget;
    }

    public void SetCurrentMove(CardPresenter presenter, CardContainer targetContainer)
    {
        _allowedPresenter = presenter;
        _allowedTarget = targetContainer;
    }

    public void ClearCurrentMove()
    {
        _allowedPresenter = null;
        _allowedTarget = null;
    }
}
