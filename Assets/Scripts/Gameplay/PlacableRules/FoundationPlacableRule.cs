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
                if (isCategoryCard)
                {
                    return true;
                }

                var container = sourceCardModel.Container;
                if (container != null)
                {
                    var topPresenter = container.GetTopCardPresenter();
                    if (topPresenter.CardModel.Type == CardType.Category)
                        return true;
                }

                ErrorMessage = StringConstants.ErrorCategoryCanGoInEmptyFoundation;
                return false;
            }

            var sameCategory = targetCardModel.CategoryName == sourceCardModel.CategoryName;
            if (!sameCategory)
                ErrorMessage = StringConstants.ErrorStackCardsOfSameCategory;
            return sameCategory;
        }

        public string ErrorMessage { get; set; } = null;
    }
}