using Card;

namespace Gameplay.PlacableRules
{
    public class PilePlacableRule : IPlacableRule
    {
        public bool IsPlaceable(CardModel targetCardModel, CardModel sourceCardModel)
        {
            if (targetCardModel == null)
                return true;

            if (targetCardModel.Type == CardType.Category)
                return false;

            return targetCardModel.CategoryName == sourceCardModel.CategoryName;
        }
    }
}