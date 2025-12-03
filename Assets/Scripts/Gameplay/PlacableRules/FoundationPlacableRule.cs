using Card;

namespace Gameplay.PlacableRules
{
    public class FoundationPlacableRule : IPlacableRule
    {
        public bool IsPlaceable(CardModel targetCardModel, CardModel sourceCardModel)
        {
            if (targetCardModel == null)
                return sourceCardModel.Type == CardType.Category;

            return targetCardModel.CategoryName == sourceCardModel.CategoryName;
        }
    }
}