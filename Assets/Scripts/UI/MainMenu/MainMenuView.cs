
using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    public class MainMenuView : BaseView
    {
        public Button continueButton;
        public Button settingsButton;
        public TextMeshProUGUI continueLevelText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private List<Sprite> backgroundSprites;
        [SerializeField] private Button coinButton;
        [SerializeField] private Button noAdsButton;

        private readonly float _continueButtonIntroScaleDuration = 0.4f;
        private readonly float _continueButtonLoopScaleMultiplier = 1.06f;
        private readonly float _continueButtonLoopScaleDuration = 0.8f;

        private Tween _continueButtonLoopTween;
        private Vector3 _continueButtonInitialScale;
        
        public event Action ContinueButtonClicked;
        public event Action SettingsButtonClicked;
        public event Action CoinButtonClicked;
        public event Action NoAdsButtonClicked;
        protected override void Awake()
        {
            base.Awake();

            _continueButtonInitialScale = continueButton.transform.localScale;

            continueButton.onClick.AddListener(()=>ContinueButtonClicked?.Invoke());
            settingsButton.onClick.AddListener(()=>SettingsButtonClicked?.Invoke());
            coinButton.onClick.AddListener(()=>CoinButtonClicked?.Invoke());
            noAdsButton.onClick.AddListener(()=>NoAdsButtonClicked?.Invoke());
        }

        protected override void OnShown()
        {
            base.OnShown();
            continueButton.transform
                .DOScale(_continueButtonInitialScale, _continueButtonIntroScaleDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(StartContinueButtonLoopAnimation);
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

        public override void Show()
        {
            base.Show();
            StopContinueButtonLoopAnimation();
            continueButton.transform.localScale = Vector3.zero;
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

        public void SetLevelText(string currentLevel)
        {
            continueLevelText.SetText(currentLevel);
        }

        public void SetCoinText(int totalCoins)
        {
            coinText.SetText(totalCoins.ToString());
        }

        public void SetBackgroundImageFromRemote(int backgroundImageId)
        {
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
    }
    
}
