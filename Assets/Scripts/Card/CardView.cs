using System;
using JetBrains.Annotations;
using Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Card
{
    public class CardView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI contentCountText;
        [SerializeField] TextMeshProUGUI mainText;
        [SerializeField] TextMeshProUGUI rightText;
        [SerializeField][CanBeNull] TextMeshProUGUI upText;
        [SerializeField][CanBeNull] Image mainImage;
        [SerializeField][CanBeNull] Image upImage;
        [SerializeField][CanBeNull] Image rightImage;
        [SerializeField][CanBeNull] Image crownImage;
        [SerializeField] RectTransform rightTextParent;
        [SerializeField] GameObject frontSide;
        [SerializeField] GameObject backSide;
        [SerializeField][CanBeNull] GameObject upCategoryInfoImage;
        [SerializeField][CanBeNull] TextMeshProUGUI upCategoryName;

        public void SetContentCountText(int currentCount, int totalCount)
        {
            if (contentCountText == null) return;
            contentCountText.SetText(currentCount + "/" + totalCount);
        }

        public void SetRightTextTransform()
        {
            var cardRectTransform = (RectTransform)transform;
            var cardWidth = cardRectTransform.rect.width;
            var cardHeight = cardRectTransform.rect.height;

            var sizeDelta = rightTextParent.sizeDelta;
            sizeDelta.x = 0.76f * cardHeight;
            sizeDelta.y = 0.35f * cardWidth;
            rightTextParent.sizeDelta = sizeDelta;
        }

        public void Initialize(CardModel cardModel)
        {
            SetContentCountText(0, cardModel.ContentCount);
            if (cardModel.Type == CardType.Category)
            {
                if (mainText != null) mainText.text = cardModel.CategoryName;
                if (rightText != null) rightText.text = cardModel.CategoryName;
                if (crownImage != null) crownImage.gameObject.SetActive(true);
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

        public void SetParent(Transform parent, bool worldPositionStays)
        {
            transform.SetParent(parent, worldPositionStays);
        }

        public void SetRotation(bool isFront)
        {
            transform.eulerAngles = isFront ? new Vector3(0, 0, 0) : new Vector3(0, -180, 0);
            if (frontSide != null) frontSide.SetActive(isFront);
            if (backSide != null) backSide.SetActive(!isFront);
        }

        public void Rotate(bool isFront, float duration, float delay = 0f)
        {
            if (duration <= 0f && delay <= 0f)
            {
                SetRotation(isFront);
                return;
            }

            var currentRotation = transform.eulerAngles;
            var startY = currentRotation.y;
            if (startY > 180f)
            {
                startY -= 360f;
            }

            var targetY = isFront ? 0f : -180f;
            var targetRotation = new Vector3(0f, targetY, 0f);

            var sequence = DOTween.Sequence();
            sequence.SetDelay(delay);

            var halfDuration = duration * 0.5f;

            var midRotation = new Vector3(0f, -90f, 0f);

            sequence.Append(transform.DORotate(midRotation, halfDuration).OnComplete(() =>
            {
                if (frontSide != null) frontSide.SetActive(isFront);
                if (backSide != null) backSide.SetActive(!isFront);
            }));

            sequence.Append(transform.DORotate(targetRotation, halfDuration));

            sequence.OnComplete(() => { SetRotation(isFront); });
        }

        public void SetLocalPosition(Vector3 localPosition)
        {
            transform.localPosition = localPosition;
        }

        public void MoveToLocalPosition(Vector3 targetLocalPosition, float duration, float delay = 0f, Ease ease = Ease.Linear, System.Action onComplete = null)
        {
            if (duration <= 0f && delay <= 0f)
            {
                transform.localPosition = targetLocalPosition;
                onComplete?.Invoke();
                return;
            }

            transform.DOKill();
            transform.DOLocalMove(targetLocalPosition, duration)
                .SetDelay(delay)
                .SetEase(ease)
                .OnComplete(() => { onComplete?.Invoke(); });
        }

        private void SetAllInactive()
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

        public void SetState(CardViewState state)
        {
            switch (state)
            {
                case CardViewState.CategoryTop:
                    SetCategoryTopState();
                    break;
                case CardViewState.CategoryBelowNoCategoryInfo:
                    SetCategoryBelowNoCategoryInfoState();
                    break;
                case CardViewState.CategoryBelowWithCategoryInfo:
                    SetCategoryBelowWithCategoryInfoState();
                    break;
                case CardViewState.ContentTextTopNoCategoryInfo:
                    SetContentTextTopNoCategoryInfoState();
                    break;
                case CardViewState.ContentTextTopWithCategoryInfo:
                    SetContentTextTopWithCategoryInfoState();
                    break;
                case CardViewState.ContentTextBelowWithUpInfo:
                    SetContentTextBelowWithUpperInfoState();
                    break;
                case CardViewState.ContentTextBelowWithSideInfo:
                    SetContentTextBelowWithSideInfoState();
                    break;
                case CardViewState.ContentImageTopNoCategoryInfo:
                    SetContentImageTopNoCategoryInfoState();
                    break;
                case CardViewState.ContentImageTopWithCategoryInfo:
                    SetContentImageTopWithCategoryInfoState();
                    break;
                case CardViewState.ContentImageBelowWithUpInfo:
                    SetContentImageBelowWithUpperInfoState();
                    break;
                case CardViewState.ContentImageBelowWithSideInfo:
                    SetContentImageBelowWithSideInfoState();
                    break;
            }
        }

        private void SetCategoryTopState()
        {
            SetAllInactive();
            if (contentCountText != null) contentCountText.gameObject.SetActive(true);
            if (mainText != null) mainText.gameObject.SetActive(true);
            if (crownImage != null) crownImage.gameObject.SetActive(true);
        }

        private void SetCategoryBelowNoCategoryInfoState()
        {
            SetAllInactive();
            if (rightText != null) rightText.gameObject.SetActive(true);
        }

        private void SetCategoryBelowWithCategoryInfoState()
        {
            SetAllInactive();
            if (upCategoryInfoImage != null) upCategoryInfoImage.SetActive(true);
        }

        private void SetContentTextTopNoCategoryInfoState()
        {
            SetAllInactive();
            if (mainText != null) mainText.gameObject.SetActive(true);
        }

        private void SetContentTextTopWithCategoryInfoState()
        {
            SetAllInactive();
            if (mainText != null) mainText.gameObject.SetActive(true);
            if (contentCountText != null) contentCountText.gameObject.SetActive(true);
        }

        private void SetContentTextBelowWithUpperInfoState()
        {
            SetAllInactive();
            if (upText != null) upText.gameObject.SetActive(true);
        }

        private void SetContentTextBelowWithSideInfoState()
        {
            SetAllInactive();
            if (rightText != null) rightText.gameObject.SetActive(true);
        }

        private void SetContentImageTopNoCategoryInfoState()
        {
            SetAllInactive();
            if (mainImage != null) mainImage.gameObject.SetActive(true);
        }

        private void SetContentImageTopWithCategoryInfoState()
        {
            SetAllInactive();
            if (mainImage != null) mainImage.gameObject.SetActive(true);
            if (contentCountText != null) contentCountText.gameObject.SetActive(true);
        }

        private void SetContentImageBelowWithUpperInfoState()
        {
            SetAllInactive();
            if (upImage != null) upImage.gameObject.SetActive(true);
        }

        private void SetContentImageBelowWithSideInfoState()
        {
            SetAllInactive();
            if (rightImage != null) rightImage.gameObject.SetActive(true);
        }
    }
}