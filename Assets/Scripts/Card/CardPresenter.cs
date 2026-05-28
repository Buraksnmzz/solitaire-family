using Gameplay;
using UnityEngine;

namespace Card
{
    public class CardPresenter
    {
        public CardModel CardModel;
        public CardView CardView;
        private bool _useStackedDisplayTextOnNextTopState;
        public bool IsFaceUp => CardModel.IsFaceUp;


        public void Initialize(CardModel cardModel, CardView cardView, Transform parent)
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

                dragger.Setup(this, parent);
            }
        }

        public CardContainer GetContainer()
        {
            return CardModel.Container;
        }

        public void SetContainer(CardContainer container)
        {
            CardModel.Container = container;
            CardView?.SetMainTextUsesStackedDisplay(false);
            CardView?.SetRightTextUsesStackedDisplay(true);
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

        public void MoveToLocalPosition(Vector3 targetLocalPosition, float duration, float delay = 0f, DG.Tweening.Ease ease = DG.Tweening.Ease.OutQuad, System.Action onComplete = null)
        {
            if (CardView == null) return;
            CardView.MoveToLocalPosition(targetLocalPosition, duration, delay, ease, onComplete);
        }

        public void ApplyViewState(CardViewState state)
        {
            if (CardView == null) return;
            CardView.SetState(state);
        }

        public void MarkMainTextForStackedDisplay()
        {
            _useStackedDisplayTextOnNextTopState = true;
        }

        public bool ConsumeMainTextStackedDisplay()
        {
            var useStackedDisplayText = _useStackedDisplayTextOnNextTopState;
            _useStackedDisplayTextOnNextTopState = false;
            return useStackedDisplayText;
        }

        public void ClearPendingMainTextStackedDisplay()
        {
            _useStackedDisplayTextOnNextTopState = false;
            SetMainTextUsesStackedDisplay(false);
        }

        public void SetMainTextUsesStackedDisplay(bool useStackedDisplay)
        {
            if (CardView == null) return;
            CardView.SetMainTextUsesStackedDisplay(useStackedDisplay);
        }

        public void SetRightTextUsesStackedDisplay(bool useStackedDisplay)
        {
            if (CardView == null) return;
            CardView.SetRightTextUsesStackedDisplay(useStackedDisplay);
        }

        public void SetContentCount(int currentCount, int totalCount)
        {
            CardModel.CurrentContentCount = currentCount;
            CardView.SetContentCountText(currentCount, totalCount);
        }
    }
}
