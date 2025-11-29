using Card;

namespace Gameplay
{
    public class GameRuleService:IGameRuleService
    {
        
        public void IsMoveValid(CardModel source, CardContainer targetContainer, CardModel targetCard = null)
        {
            
        }

        public void CanStackBeDragged(CardModel bottomCard)
        {
            
        }

        public void CanPlaceOnPile(CardModel source, CardModel target = null)
        {
            
        }

        public void CanPlaceOnFoundation(CardModel source, Foundation targetFoundation)
        {
            
        }

        public void CanDragContentCardToCategoryCard(CardModel sourceContent, CardModel targetCategory)
        {
            
        }

        public void IsFoundationComplete(Foundation foundation, int totalContentCount)
        {
            
        }
    }
}