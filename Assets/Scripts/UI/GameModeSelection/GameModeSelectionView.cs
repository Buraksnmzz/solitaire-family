using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.GameModeSelection
{
    public class GameModeSelectionView : BaseView
    {
        [SerializeField] private Button classicButton;
        [SerializeField] private Button mathButton;
        [SerializeField] private Image infoBar;
        [SerializeField] private Transform jokerImage;

        private const float InfoBarFadeDuration = 0.4f;
        private const float JokerScaleDuration = 0.4f;
        private const float ButtonScaleDuration = 0.4f;
        private const float AnimationStartInterval = 0.4f;

        private Sequence _showSequence;
        private Vector3 _jokerImageInitialScale;
        private Vector3 _classicButtonInitialScale;
        private Vector3 _mathButtonInitialScale;

        public event Action ClassicButtonClicked;
        public event Action MathButtonClicked;

        protected override void Awake()
        {
            base.Awake();
            _jokerImageInitialScale = jokerImage.localScale;
            _classicButtonInitialScale = classicButton.transform.localScale;
            _mathButtonInitialScale = mathButton.transform.localScale;
            ResetShowAnimationState();
            classicButton.onClick.AddListener(() => ClassicButtonClicked?.Invoke());
            mathButton.onClick.AddListener(() => MathButtonClicked?.Invoke());
        }

        public override void Show()
        {
            _showSequence?.Kill();
            ResetShowAnimationState();
            base.Show();
            PlayShowAnimation();
        }

        public override void Hide()
        {
            _showSequence?.Kill();
            base.Hide();
        }

        protected override void OnDestroy()
        {
            _showSequence?.Kill();
            base.OnDestroy();
        }

        private void PlayShowAnimation()
        {
            classicButton.interactable = false;
            mathButton.interactable = false;

            _showSequence = DOTween.Sequence();
            _showSequence.Insert(0f, infoBar.DOFade(1f, InfoBarFadeDuration).SetEase(Ease.Linear));
            _showSequence.Insert(AnimationStartInterval,
                jokerImage.DOScale(_jokerImageInitialScale, JokerScaleDuration).SetEase(Ease.OutBack));
            _showSequence.Insert(AnimationStartInterval * 1.5f,
                classicButton.transform.DOScale(_classicButtonInitialScale, ButtonScaleDuration).SetEase(Ease.OutBack));
            _showSequence.Insert(AnimationStartInterval * 1.5f,
                mathButton.transform.DOScale(_mathButtonInitialScale, ButtonScaleDuration).SetEase(Ease.OutBack));
            _showSequence.OnComplete(() =>
            {
                classicButton.interactable = true;
                mathButton.interactable = true;
            });
        }

        private void ResetShowAnimationState()
        {
            var infoBarColor = infoBar.color;
            infoBarColor.a = 0f;
            infoBar.color = infoBarColor;
            jokerImage.localScale = Vector3.zero;
            classicButton.transform.localScale = Vector3.zero;
            mathButton.transform.localScale = Vector3.zero;
        }
    }
}