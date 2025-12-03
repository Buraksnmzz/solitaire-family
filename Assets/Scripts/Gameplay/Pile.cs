using Card;
using Gameplay.PlacableRules;
using UnityEngine;

namespace Gameplay
{
    public class Pile : CardContainer
    {
        protected override void SetCardPosition(CardView card)
        {
            var rectTransform = (RectTransform)transform;
            var cardYOffset = -rectTransform.rect.width * 0.25f;
            var index = _cardViews.Count - 1;
            var offset = new Vector3(0f, index * cardYOffset, 0f);
            card.transform.localPosition = offset;
        }

    }
}