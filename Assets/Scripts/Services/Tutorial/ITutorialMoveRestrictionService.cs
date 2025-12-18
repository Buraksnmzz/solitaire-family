using Card;
using Gameplay;

public interface ITutorialMoveRestrictionService : IService
{
    bool IsActive { get; }
    bool IsDragAllowed(CardPresenter presenter);
    bool IsDropAllowed(CardPresenter presenter, CardContainer targetContainer);
    void SetCurrentMove(CardPresenter presenter, CardContainer targetContainer);
    void ClearCurrentMove();
}
