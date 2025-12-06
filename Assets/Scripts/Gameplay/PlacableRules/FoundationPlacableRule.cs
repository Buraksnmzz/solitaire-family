using Card;

namespace Gameplay.PlacableRules
{
    public class FoundationPlacableRule : IPlacableRule
    {
        public bool IsPlaceable(CardModel targetCardModel, CardModel sourceCardModel)
        {
            if (targetCardModel == null)
            {
                var isCategoryCard = sourceCardModel.Type == CardType.Category;
                if (!isCategoryCard)
                {
                    ErrorMessage = StringConstants.ErrorCategoryCanGoInEmptyFoundation;
                }
                return isCategoryCard;
            }

            var sameCategory = targetCardModel.CategoryName == sourceCardModel.CategoryName;
            if (!sameCategory)
                ErrorMessage = null;
            return sameCategory;
        }

        public string ErrorMessage { get; set; } = null;
    }
}