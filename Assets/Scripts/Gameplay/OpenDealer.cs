using Card;
using UnityEngine;

namespace Gameplay
{
    public class OpenDealer : CardContainer
    {
        protected override void SetCardPosition(CardView card)
        {
            var cardRectTransform = (RectTransform)card.transform;
            var cardWidth = cardRectTransform.rect.width;
            var cardHeight = cardRectTransform.rect.height;
            var offsetX = cardWidth * 0.368f;

            var index = _cardViews.Count - 1;

            if (index == 0)
            {
                card.transform.localPosition = new Vector3(-cardWidth * 0.5f, -cardHeight * 0.5f, 0f);
                return;
            }

            if (index == 1)
            {
                card.transform.localPosition = new Vector3(-cardWidth * 0.5f - offsetX, -cardHeight * 0.5f, 0f);
                return;
            }

            if (index == 2)
            {
                card.transform.localPosition = new Vector3(-cardWidth * 0.5f - offsetX * 2f, -cardHeight * 0.5f, 0f);
                return;
            }

            var baseX = -cardWidth * 0.5f;
            var baseY = -cardHeight * 0.5f;

            _cardViews[index - 2].transform.localPosition = new Vector3(baseX, baseY, 0f);
            _cardViews[index - 1].transform.localPosition = new Vector3(baseX - offsetX, baseY, 0f);
            card.transform.localPosition = new Vector3(baseX - offsetX * 2f, baseY, 0f);
        }
    }
}