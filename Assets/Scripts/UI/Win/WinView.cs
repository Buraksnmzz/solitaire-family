using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Win
{
    public class WinView : BaseView
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Transform flag1;
        [SerializeField] private Transform flag2;
        [SerializeField] private Transform completedImage;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI coinAmountToCollectText;
        [SerializeField] private Transform jokerImage;
        [SerializeField] private Transform topCoinImage;
        [SerializeField] private Transform coinHolder;
        [SerializeField] private CanvasGroup completedImageCanvasGroup;
        [SerializeField] private RectTransform earnedCoinIconPrefab;
        [SerializeField] private ParticleSystem coinParticle;
        [SerializeField] private ParticleSystem fireworkParticle1;
        [SerializeField] private ParticleSystem fireworkParticle2;
        [SerializeField] private ParticleSystem fireworkParticle3;
        private readonly float _introSlideOffset = 600f;
        private readonly float _introSlideDuration = 0.8f;
        private readonly float _completedScaleDuration = 0.8f;
        private readonly float _completedScaleMultiplier = 2f;
        private readonly float _completedFadeRatio = 0.5f;
        private readonly float _jokerScaleDelay = 0.7f;
        private readonly float _jokerScaleDuration = 0.35f;
        private readonly float _continueButtonDelayAfterJoker = 0.15f;
        private readonly float _continueButtonScaleDuration = 0.35f;
        private readonly float _continueButtonLoopScaleMultiplier = 1.08f;
        private readonly float _continueButtonLoopScaleDuration = 0.8f;
        private readonly float _coinScaleDuration = 0.2f;
        private readonly float _coinSpawnInterval = 0.08f;
        private readonly float _coinMoveDuration = 1.2f;
        private readonly float _coinMoveInterval = 0.2f;

        private Tween _continueButtonLoopTween;

        public int finalCoins;
        private readonly int _coinAnimationCount = 8;
        private int _coinBaseTotal;
        private Vector3 _flag1InitialPosition;
        private Vector3 _flag2InitialPosition;
        private Vector3 _coinHolderInitialPosition;
        private Vector3 _completedImageInitialScale;
        private Vector3 _jokerImageInitialScale;
        private Vector3 _continueButtonInitialScale;

        public event Action ContinueButtonClicked;
        public event Action OnCoinCreated;
        public event Action OnIconMoved;
        public event Action IntroAnimationFinished;

        private void Awake()
        {
            CacheInitialTransforms();
        }

        private void Start()
        {
            continueButton.onClick.AddListener(() => ContinueButtonClicked?.Invoke());
        }

        public override void Show()
        {
            base.Show();
            InitializeViewsBeforeAnimation();
            PlayIntroAnimation();
        }

        protected override void OnShown()
        {
            base.OnShown();

        }

        protected override void OnHidden()
        {
            base.OnHidden();
            StopContinueButtonLoopAnimation();
        }

        protected override void OnDestroy()
        {
            StopContinueButtonLoopAnimation();
            base.OnDestroy();
        }

        public void SetLevelText(int level)
        {
            levelText.text = "Level " + level;
        }

        public void SetCoinText(int coin)
        {
            coinText.text = coin.ToString();
        }

        public void SetCoinAmountToCollectText(int coin)
        {
            coinAmountToCollectText.text = "x" + coin;
        }

        private void PlayIntroAnimation()
        {
            PlayParticles();

            var slideStagger = 0.08f;
            var sequence = DOTween.Sequence();

            sequence.Join(flag1.DOLocalMove(_flag1InitialPosition, _introSlideDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(slideStagger * 0));

            sequence.Join(flag2.DOLocalMove(_flag2InitialPosition, _introSlideDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(slideStagger * 1));

            sequence.Join(coinHolder.DOLocalMove(_coinHolderInitialPosition, _introSlideDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(slideStagger * 2));

            completedImage.localScale = _completedImageInitialScale * _completedScaleMultiplier;
            completedImageCanvasGroup.alpha = 0f;

            sequence.Join(completedImage.DOScale(_completedImageInitialScale, _completedScaleDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(slideStagger * 3));

            var fadeDuration = _completedScaleDuration * _completedFadeRatio;
            sequence.Join(completedImageCanvasGroup.DOFade(1f, fadeDuration));

            sequence.Join(levelText.DOFade(1, _completedScaleDuration)
                .SetDelay(slideStagger * 3));

            if (completedImageCanvasGroup != null)
            {

            }

            sequence.Insert(_jokerScaleDelay + 3 * slideStagger, jokerImage.DOScale(_jokerImageInitialScale, _jokerScaleDuration).SetEase(Ease.OutBack));
            sequence.Insert(_jokerScaleDelay + _continueButtonDelayAfterJoker + 3 * slideStagger,
                continueButton.transform
                    .DOScale(_continueButtonInitialScale, _continueButtonScaleDuration)
                    .SetEase(Ease.OutBack)
                    .OnStart(() => IntroAnimationFinished?.Invoke())
                    .OnComplete(StartContinueButtonLoopAnimation));
        }

        [ContextMenu("Continue")]
        private void PlayParticles()
        {
            DOVirtual.DelayedCall(0.5f, () => fireworkParticle1.Play());
            DOVirtual.DelayedCall(0.8f, () => fireworkParticle2.Play());
            DOVirtual.DelayedCall(1.1f, () => fireworkParticle3.Play());
        }

        private void InitializeViewsBeforeAnimation()
        {
            StopContinueButtonLoopAnimation();

            flag1.DOKill();
            flag2.DOKill();
            coinHolder.DOKill();
            completedImage.DOKill();
            jokerImage.DOKill();
            continueButton.transform.DOKill();

            flag1.localPosition = _flag1InitialPosition + Vector3.up * _introSlideOffset;
            flag2.localPosition = _flag2InitialPosition + Vector3.up * _introSlideOffset;
            coinHolder.localPosition = _coinHolderInitialPosition + Vector3.up * _introSlideOffset;

            completedImageCanvasGroup.alpha = 0f;
            completedImage.localScale = _completedImageInitialScale;
            jokerImage.localScale = Vector3.zero;
            continueButton.transform.localScale = Vector3.zero;
            continueButton.interactable = true;
            levelText.DOFade(0, 0);
        }

        private void StartContinueButtonLoopAnimation()
        {
            StopContinueButtonLoopAnimation();

            if (!continueButton.interactable)
            {
                return;
            }

            continueButton.transform.localScale = _continueButtonInitialScale;

            _continueButtonLoopTween = continueButton.transform
                .DOScale(_continueButtonInitialScale * _continueButtonLoopScaleMultiplier, _continueButtonLoopScaleDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopContinueButtonLoopAnimation()
        {
            _continueButtonLoopTween?.Kill();
            _continueButtonLoopTween = null;
        }

        public void PlayCoinAnimation(Action onCompleted)
        {
            var icons = new List<RectTransform>();
            OnCoinCreated?.Invoke();

            for (var i = 0; i < _coinAnimationCount; i++)
            {
                var icon = Instantiate(earnedCoinIconPrefab, panel);
                icons.Add(icon);
                icon.localScale = Vector3.zero;
                icon.position += new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
            }

            var scaleSequence = DOTween.Sequence();
            var remainingIcons = icons.Count;

            for (var i = 0; i < icons.Count; i++)
            {
                var startTime = _coinSpawnInterval * i;
                var icon = icons[i];
                scaleSequence.Insert(startTime,
                    icon.DOScale(Vector3.one, _coinScaleDuration)
                        .SetEase(Ease.OutBack).OnComplete(() =>
                        {
                            icon.DOMove(topCoinImage.position, _coinMoveDuration)
                                .SetEase(Ease.InBack)
                                .OnComplete(() =>
                                {
                                    OnIconMoved?.Invoke();
                                    topCoinImage.DOComplete();
                                    topCoinImage.DOPunchScale(Vector3.one * 0.2f, 0.08f);
                                    Destroy(icon.gameObject);
                                    remainingIcons--;
                                    if (remainingIcons == icons.Count - 1)
                                    {
                                        coinParticle.Play();
                                    }
                                    if (remainingIcons == 0)
                                    {
                                        coinText.text = finalCoins.ToString();
                                        onCompleted?.Invoke();
                                    }
                                });
                        }));
            }
        }

        private void CacheInitialTransforms()
        {
            _flag1InitialPosition = flag1.localPosition;
            _flag2InitialPosition = flag2.localPosition;
            _coinHolderInitialPosition = coinHolder.localPosition;
            _completedImageInitialScale = completedImage.localScale;
            _jokerImageInitialScale = jokerImage.localScale;
            _continueButtonInitialScale = continueButton.transform.localScale;
        }

        public void DisableContinueButton()
        {
            continueButton.interactable = false;
            StopContinueButtonLoopAnimation();
            continueButton.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);
        }
    }
}