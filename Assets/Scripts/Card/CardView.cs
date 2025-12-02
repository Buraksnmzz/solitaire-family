using System;
using JetBrains.Annotations;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Card
{
    public class CardView: MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI contentCountText;
        [SerializeField] TextMeshProUGUI mainText;
        [SerializeField] TextMeshProUGUI rightText;
        [SerializeField] [CanBeNull] TextMeshProUGUI upText;
        [SerializeField] [CanBeNull] Image mainImage;
        [SerializeField] [CanBeNull] Image upImage;
        [SerializeField] [CanBeNull] Image rightImage;
        [SerializeField] [CanBeNull] Image crownImage;
        [SerializeField] RectTransform rightTextParent;
        [SerializeField] GameObject frontSide;
        [SerializeField] GameObject backSide;
        [SerializeField] [CanBeNull] GameObject upCategoryInfoImage;
        [SerializeField] [CanBeNull] TextMeshProUGUI upCategoryName;

        public void SetRightTextTransform()
        {
            var cardRectTransform = (RectTransform) transform;
            var cardWidth = cardRectTransform.rect.width;
            var cardHeight = cardRectTransform.rect.height;

            var sizeDelta = rightTextParent.sizeDelta;
            sizeDelta.x = 0.76f * cardHeight;
            sizeDelta.y = 0.35f * cardWidth;
            rightTextParent.sizeDelta = sizeDelta;
        }

        public void Initialize(CardModel cardModel)
        {
            if (cardModel.Type == CardType.Category)
            {
                if (mainText != null) mainText.text = cardModel.CategoryName;
                if (rightText != null) rightText.text = cardModel.CategoryName;
                if (crownImage != null) crownImage.gameObject.SetActive(true);
                if (contentCountText != null) contentCountText.SetText(0 + "/" + cardModel.ContentCount);
                if (upCategoryName != null) upCategoryName.SetText(cardModel.CategoryName);
                if (upCategoryInfoImage != null) upCategoryInfoImage.SetActive(false);
                SetCategoryTopState();
            }
            else
            {
                if (cardModel.CategoryType == CardCategoryType.Text)
                {
                    if (mainText != null) mainText.text = cardModel.ContentName;
                    if (rightText != null) rightText.text = cardModel.ContentName;
                    if (upText != null) upText.text = cardModel.ContentName;
                    SetContentTextTopNoCategoryInfoState();
                }
                else
                {
                    var sprite = Resources.Load<Sprite>("PuzzleImages/" + cardModel.ContentName);
                    if (mainImage != null) mainImage.sprite = sprite;
                    if (upImage != null) upImage.sprite = sprite;
                    if (rightImage != null) rightImage.sprite = sprite;
                    SetContentImageTopNoCategoryInfoState();
                }

                if (crownImage != null) crownImage.gameObject.SetActive(false);
            }

            SetRotation(false);
        }

        public void SetRotation(bool isFront)
        {
            transform.eulerAngles  = isFront ? new Vector3(0, 0, 0) : new Vector3(0, -180, 0);
            if (frontSide != null) frontSide.SetActive(isFront);
            if (backSide != null) backSide.SetActive(!isFront);
        }

        public void SetAllInactive()
        {
            if (contentCountText != null) contentCountText.gameObject.SetActive(false);
            if (upCategoryInfoImage != null) upCategoryInfoImage.SetActive(false);
            if (mainText != null) mainText.gameObject.SetActive(false);
            if (rightText != null) rightText.gameObject.SetActive(false);
            if (upText != null) upText.gameObject.SetActive(false);
            if (rightImage != null) rightImage.gameObject.SetActive(false);
            if (upImage != null) upImage.gameObject.SetActive(false);
            if (mainImage != null) mainImage.gameObject.SetActive(false);
            if (crownImage != null) crownImage.gameObject.SetActive(false);
        }

        public void SetCategoryTopState()
        {
            SetAllInactive();
            if (contentCountText != null) contentCountText.gameObject.SetActive(true);
            if (mainText != null) mainText.gameObject.SetActive(true);
            if (crownImage != null) crownImage.gameObject.SetActive(true);
        }
        
        public void SetCategoryBelowNoCategoryInfoState()
        {
            SetAllInactive();
            if (rightText != null) rightText.gameObject.SetActive(true);
        }
        
        public void SetCategoryBelowWithCategoryInfoState()
        {
            SetAllInactive();
            if (upCategoryInfoImage != null) upCategoryInfoImage.SetActive(true);
        }

        public void SetContentTextTopNoCategoryInfoState()
        {
            SetAllInactive();
            if (mainText != null) mainText.gameObject.SetActive(true);
        }
        
        public void SetContentImageTopNoCategoryInfoState()
        {
            SetAllInactive();
            if (mainImage != null) mainImage.gameObject.SetActive(true);
        }
    }
}