using Gameplay;
using Levels;

namespace Card
{
    public class CardModel
    {
        public CardType Type;
        public CardCategoryType CategoryType;
        public string CategoryName;
        public string ContentName;
        public int ContentCount;
        public int CurrentContentCount;
        public CardContainer Container;
        public bool IsFaceUp;
    }
}