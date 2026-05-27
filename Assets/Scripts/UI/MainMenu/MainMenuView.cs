
using System;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Levels;

namespace UI.MainMenu
{
    public class MainMenuView : BaseView
    {
        public Button continueButton;
        public Button continueButtonMath;
        public Button settingsButton;
        public TextMeshProUGUI continueLevelText;
        [SerializeField] private TextMeshProUGUI continueLevelTextMath;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI gameModeText;
        [SerializeField] private List<Sprite> backgroundSprites;
        [SerializeField] private Sprite backgroundSpritesMath;
        [SerializeField] private Button coinButton;
        [SerializeField] private Button noAdsButton;
        [SerializeField] private RectTransform topBar;
        [SerializeField] private SkeletonGraphic _skeletonGraphic;

        private readonly float _topBarIntroDuration = 0.5f;
        private readonly float _topBarToNoAdsDelay = 0.2f;
        private readonly float _noAdsIntroScaleDuration = 0.5f;
        private readonly float _noAdsToContinueButtonsDelay = 0.2f;
        private readonly float _continueButtonsStaggerDelay = 0.2f;
        private readonly float _continueButtonIntroScaleDuration = 0.5f;
        private readonly float _gameModeTextFadeDelay = 1f;
        private readonly float _gameModeTextFadeDuration = 0.6f;
        private readonly float _topBarIntroOffset = 300f;

        private Sequence _introSequence;
        private Vector2 _topBarInitialAnchoredPosition;
        private Vector3 _continueButtonInitialScale;
        private Vector3 _continueButtonMathInitialScale;
        private Vector3 _noAdsButtonInitialScale;

        public event Action ContinueButtonClicked;
        public event Action ContinueButtonMathClicked;
        public event Action SettingsButtonClicked;
        public event Action CoinButtonClicked;
        public event Action NoAdsButtonClicked;
        protected override void Awake()
        {
            base.Awake();

            _topBarInitialAnchoredPosition = topBar.anchoredPosition;
            _continueButtonInitialScale = continueButton.transform.localScale;
            _continueButtonMathInitialScale = continueButtonMath.transform.localScale;
            _noAdsButtonInitialScale = noAdsButton.transform.localScale;

            continueButton.onClick.AddListener(() => ContinueButtonClicked?.Invoke());
            continueButtonMath.onClick.AddListener(() => ContinueButtonMathClicked?.Invoke());
            settingsButton.onClick.AddListener(() => SettingsButtonClicked?.Invoke());
            coinButton.onClick.AddListener(() => CoinButtonClicked?.Invoke());
            noAdsButton.onClick.AddListener(() => NoAdsButtonClicked?.Invoke());
        }

        private void OnEnable()
        {
            _skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
        }

        protected override void OnShown()
        {
            base.OnShown();
            KillIntroTweens();

            _introSequence = DOTween.Sequence();
            _introSequence.Join(topBar.DOAnchorPos(_topBarInitialAnchoredPosition, _topBarIntroDuration).SetEase(Ease.OutBack));

            var noAdsInsertTime = _topBarToNoAdsDelay;
            if (noAdsButton.gameObject.activeSelf)
            {
                _introSequence.Insert(
                    noAdsInsertTime,
                    noAdsButton.transform.DOScale(_noAdsButtonInitialScale, _noAdsIntroScaleDuration).SetEase(Ease.OutBack));
            }

            var continueButtonsStartTime = noAdsInsertTime + _noAdsToContinueButtonsDelay;
            _introSequence.Insert(
                continueButtonsStartTime,
                continueButton.transform.DOScale(_continueButtonInitialScale, _continueButtonIntroScaleDuration).SetEase(Ease.OutBack));
            _introSequence.Insert(
                continueButtonsStartTime + _continueButtonsStaggerDelay,
                continueButtonMath.transform.DOScale(_continueButtonMathInitialScale, _continueButtonIntroScaleDuration)
                    .SetEase(Ease.OutBack));
            _introSequence.Insert(_gameModeTextFadeDelay, gameModeText.DOFade(1f, _gameModeTextFadeDuration).SetEase(Ease.Linear));
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            KillIntroTweens();
        }

        public override void Show()
        {
            base.Show();
            KillIntroTweens();
            topBar.anchoredPosition = _topBarInitialAnchoredPosition + Vector2.up * _topBarIntroOffset;
            noAdsButton.transform.localScale = Vector3.zero;
            continueButton.transform.localScale = Vector3.zero;
            continueButtonMath.transform.localScale = Vector3.zero;
            gameModeText.alpha = 0f;
        }

        public void SetLevelTexts(string classicLevel, string mathLevel)
        {
            continueLevelText.SetText(classicLevel);
            continueLevelTextMath.SetText(mathLevel);
        }

        public void SetCoinText(int totalCoins)
        {
            coinText.SetText(totalCoins.ToString());
        }

        public void SetBackgroundImageFromRemote(int backgroundImageId, GameMode gameMode)
        {
            if (gameMode == GameMode.Math && backgroundSpritesMath != null)
            {
                backgroundImage.sprite = backgroundSpritesMath;
                return;
            }

            if (backgroundImageId >= backgroundSprites.Count || backgroundImageId < 0)
            {
                backgroundImage.sprite = backgroundSprites[0];
                return;
            }
            backgroundImage.sprite = backgroundSprites[backgroundImageId];
        }

        public void SetNoAdsButton(bool isTrue)
        {
            noAdsButton.gameObject.SetActive(isTrue);
        }

        private void KillIntroTweens()
        {
            _introSequence?.Kill();
            _introSequence = null;
            topBar.DOKill();
            noAdsButton.transform.DOKill();
            continueButton.transform.DOKill();
            continueButtonMath.transform.DOKill();
            gameModeText.DOKill();
        }

        public void SetLogoText(string modeText)
        {
            gameModeText.text = modeText;
        }
    }

}
