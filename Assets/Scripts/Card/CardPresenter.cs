using Gameplay;
using UnityEngine;

namespace Card
{
    public class CardPresenter
    {
        public CardModel CardModel;
        public CardView CardView;
        public bool IsFaceUp => CardModel.IsFaceUp;


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

        public CardContainer GetContainer()
        {
            return CardModel.Container;
        }

        public void SetContainer(CardContainer container)
        {
            CardModel.Container = container;
        }

        public void SetFaceUp(bool isFaceUp, float duration, float delay = 0)
        {
            CardModel.IsFaceUp = isFaceUp;
            if (CardView != null)
            {
                CardView.Rotate(isFaceUp, duration, delay);
            }
        }

        public void SetLocalPosition(Vector3 localPosition)
        {
            if (CardView == null) return;
            CardView.SetLocalPosition(localPosition);
        }

        public void SetParent(Transform parent, bool worldPositionStays)
        {
            if (CardView == null) return;
            CardView.SetParent(parent, worldPositionStays);
        }

        public void MoveToLocalPosition(Vector3 targetLocalPosition, float duration, float delay = 0f, DG.Tweening.Ease ease = DG.Tweening.Ease.OutQuad)
        {
            if (CardView == null) return;
            CardView.MoveToLocalPosition(targetLocalPosition, duration, delay, ease);
        }

        public void ApplyViewState(CardViewState state)
        {
            if (CardView == null) return;
            CardView.SetState(state);
        }

        public void SetContentCount(int currentCount, int totalCount)
        {
            CardModel.CurrentContentCount = currentCount;
            CardView.SetContentCountText(currentCount, totalCount);
        }
    }
}
