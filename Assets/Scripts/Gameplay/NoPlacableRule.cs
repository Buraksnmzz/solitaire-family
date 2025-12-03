using Card;
using Gameplay.PlacableRules;

namespace Gameplay
{
    internal class NoPlacableRule : IPlacableRule
    {
        public bool IsPlaceable(CardModel targetCardModel, CardModel sourceCardModel)
        {
            return false;
        }
    }
}