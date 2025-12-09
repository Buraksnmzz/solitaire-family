using System;
using Card;

namespace Gameplay.PlacableRules
{
    public class PilePlacableRule : IPlacableRule
    {
        public string ErrorMessage { get; set; } = null;

        public bool IsPlaceable(CardModel targetCardModel, CardModel sourceCardModel)
        {
            if (targetCardModel == null)
                return true;
            
            if(sourceCardModel.Type == CardType.Joker || targetCardModel.Type == CardType.Joker)
                return true;

            if (targetCardModel.Type == CardType.Category)
            {
                ErrorMessage = StringConstants.ErrorOnlyStackCardsOnCategoryInFoundation;
                return false;
            }
            var sameCategory = targetCardModel.CategoryName == sourceCardModel.CategoryName;
            if (!sameCategory)
            {
                if (targetCardModel.Container == sourceCardModel.Container)
                    ErrorMessage = null;
                if(targetCardModel.Type == CardType.Category)
                    ErrorMessage = StringConstants.ErrorOnlyStackCardsOnCategoryInFoundation;
                else if(targetCardModel.Type == CardType.Content && sourceCardModel.Type == CardType.Content)
                    ErrorMessage = StringConstants.ErrorStackCardsOfSameCategory;
                else if (targetCardModel.Type == CardType.Content && sourceCardModel.Type == CardType.Category)
                    ErrorMessage = StringConstants.ErrorCategoryCanGoTopOfMatchCategory;
            }
            return sameCategory;
        }
    }
}