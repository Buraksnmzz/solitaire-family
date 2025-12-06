using Card;

namespace Gameplay.PlacableRules
{
    public interface IPlacableRule
    {
        public bool IsPlaceable(CardModel targetCardModel, CardModel sourceCardModel);
        public string ErrorMessage { get; }
    }
}