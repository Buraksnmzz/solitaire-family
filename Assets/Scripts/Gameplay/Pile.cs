using Card;
using Gameplay.PlacableRules;
using UnityEngine;

namespace Gameplay
{
    public class Pile : CardContainer
    {
        public override Vector3 GetCardLocalPosition(int index)
        {
            var rectTransform = (RectTransform)transform;
            var cardYOffset = -rectTransform.rect.width * 0.25f;
            var offset = new Vector3(0f, index * cardYOffset, 0f);
            return offset;
        }

    }
}