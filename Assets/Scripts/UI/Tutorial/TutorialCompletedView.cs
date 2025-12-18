using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Tutorial
{
    public class TutorialCompletedView : BaseView
    {
        [SerializeField] Button continueButton;
        [SerializeField] private Transform flag1;
        [SerializeField] private Transform flag2;
        [SerializeField] private Transform completedImage;
        [SerializeField] private CanvasGroup completedImageCanvasGroup;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Transform jokerImage;
        
        private readonly float _introSlideOffset = 600f;
        private readonly float _introSlideDuration = 0.8f;
        private readonly float _completedScaleDuration = 0.8f;
        private readonly float _completedScaleMultiplier = 2f;
        private readonly float _completedFadeRatio = 0.5f;
        private readonly float _jokerScaleDelay = 0.7f;
        private readonly float _jokerScaleDuration = 0.35f;
        private readonly float _continueButtonDelayAfterJoker = 0.15f;
        private readonly float _continueButtonScaleDuration = 0.35f;
        
        private Vector3 _flag1InitialPosition;
        private Vector3 _flag2InitialPosition;
        private Vector3 _completedImageInitialScale;
        private Vector3 _jokerImageInitialScale;
        private Vector3 _continueButtonInitialScale;

        public event Action ContinueClicked;
        
        private void Awake()
        {
            CacheInitialTransforms();
        }

        void Start()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(() => ContinueClicked?.Invoke());
        }
        
        public override void Show()
        {
            base.Show();
            InitializeViewsBeforeAnimation();
            PlayIntroAnimation();
        }
        
        private void PlayIntroAnimation()
        {
            var slideStagger = 0.08f;
            var sequence = DOTween.Sequence();

            sequence.Join(flag1.DOLocalMove(_flag1InitialPosition, _introSlideDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(slideStagger * 0));

            sequence.Join(flag2.DOLocalMove(_flag2InitialPosition, _introSlideDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(slideStagger * 1));

            completedImage.localScale = _completedImageInitialScale * _completedScaleMultiplier;
            completedImageCanvasGroup.alpha = 0f;

            sequence.Join(completedImage.DOScale(_completedImageInitialScale, _completedScaleDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(slideStagger * 3));

            var fadeDuration = _completedScaleDuration * _completedFadeRatio;
            sequence.Join(completedImageCanvasGroup.DOFade(1f, fadeDuration));

            sequence.Join(levelText.DOFade(1, _completedScaleDuration)
                .SetDelay(slideStagger * 3));

            sequence.Insert(_jokerScaleDelay + 3 * slideStagger, jokerImage.DOScale(_jokerImageInitialScale, _jokerScaleDuration).SetEase(Ease.OutBack));
            sequence.Insert(_jokerScaleDelay + _continueButtonDelayAfterJoker + 3 * slideStagger,
                continueButton.transform
                    .DOScale(_continueButtonInitialScale, _continueButtonScaleDuration)
                    .SetEase(Ease.OutBack));
        }
        
        private void InitializeViewsBeforeAnimation()
        {
            flag1.DOKill();
            flag2.DOKill();
            completedImage.DOKill();
            jokerImage.DOKill();
            continueButton.transform.DOKill();

            flag1.localPosition = _flag1InitialPosition + Vector3.up * _introSlideOffset;
            flag2.localPosition = _flag2InitialPosition + Vector3.up * _introSlideOffset;

            completedImageCanvasGroup.alpha = 0f;
            completedImage.localScale = _completedImageInitialScale;
            jokerImage.localScale = Vector3.zero;
            continueButton.transform.localScale = Vector3.zero;
            continueButton.interactable = true;
            levelText.DOFade(0, 0);
        }
        
        private void CacheInitialTransforms()
        {
            _flag1InitialPosition = flag1.localPosition;
            _flag2InitialPosition = flag2.localPosition;
            _completedImageInitialScale = completedImage.localScale;
            _jokerImageInitialScale = jokerImage.localScale;
            _continueButtonInitialScale = continueButton.transform.localScale;
        }

        public void DisableContinueButton()
        {
            continueButton.interactable = false;
            continueButton.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }
}
