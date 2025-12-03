namespace Card
{
    public class CardPresenter
    {
        public CardModel CardModel;
        public CardView CardView;
        public void Initialize(CardModel cardModel, CardView cardView)
        {
            CardModel = cardModel;
            CardView = cardView;

            if (CardView != null)
            {
                CardView.Initialize(CardModel);
                var dragger = CardView.GetComponent<CardDragger>();
                if (dragger == null)
                {
                    dragger = CardView.gameObject.AddComponent<CardDragger>();
                }

                dragger.Setup(this);
            }
        }
    }
}
