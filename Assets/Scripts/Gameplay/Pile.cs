using Card;
using UnityEngine;

namespace Gameplay
{
    public class Pile : CardContainer
    {
        public override Vector3 GetCardLocalPosition(int index)
        {
            var rectTransform = (RectTransform)transform;
            var cardYOffset = -rectTransform.rect.width * 0.28f;
            var offset = new Vector3(0f, index * cardYOffset, 0f);
            return offset;
        }

        protected override void OnCardAdded(CardPresenter previousTop, CardPresenter newTop)
        {
            if (previousTop != null)
            {
                var previousModel = previousTop.CardModel;
                if (previousModel.Type == CardType.Joker)
                {
                    previousTop.ApplyViewState(CardViewState.JokerBelow);
                    return;
                }

                if (previousModel.Type == CardType.Category)
                {
                    previousTop.ApplyViewState(CardViewState.CategoryBelowWithTopText);
                }
                if (previousModel.CategoryType == Levels.CardCategoryType.Text)
                {
                    previousTop.ApplyViewState(CardViewState.ContentTextBelowWithUpInfo);
                }
                else
                {
                    previousTop.ApplyViewState(CardViewState.ContentImageBelowWithUpInfo);
                }
            }

            ApplyTopCardState(newTop);
        }

        protected override void OnTopCardChangedAfterRemove(CardPresenter newTop)
        {
            if (newTop == null) return;
            ApplyTopCardState(newTop);
        }

        void ApplyTopCardState(CardPresenter presenter)
        {
            var model = presenter.CardModel;

            if (model.Type == CardType.Joker)
            {
                presenter.ApplyViewState(CardViewState.JokerTop);
                return;
            }

            if (model.Type == CardType.Category)
            {
                presenter.ApplyViewState(CardViewState.CategoryTop);
                return;
            }

            if (model.CategoryType == Levels.CardCategoryType.Text)
            {
                presenter.ApplyViewState(CardViewState.ContentTextTopNoCategoryInfo);
            }
            else
            {
                presenter.ApplyViewState(CardViewState.ContentImageTopNoCategoryInfo);
            }
        }

    }
}