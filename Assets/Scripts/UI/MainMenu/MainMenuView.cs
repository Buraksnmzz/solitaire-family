
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
        public Button continueButtonMath;
        public Button settingsButton;
        public TextMeshProUGUI continueLevelText;
        [SerializeField] private TextMeshProUGUI continueLevelTextMath;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private List<Sprite> backgroundSprites;
        [SerializeField] private Button coinButton;
        [SerializeField] private Button noAdsButton;

        private readonly float _continueButtonIntroScaleDuration = 0.4f;
        private Vector3 _continueButtonInitialScale;

        public event Action ContinueButtonClicked;
        public event Action ContinueButtonMathClicked;
        public event Action SettingsButtonClicked;
        public event Action CoinButtonClicked;
        public event Action NoAdsButtonClicked;
        protected override void Awake()
        {
            base.Awake();

            _continueButtonInitialScale = continueButton.transform.localScale;

            continueButton.onClick.AddListener(() => ContinueButtonClicked?.Invoke());
            continueButtonMath.onClick.AddListener(() => ContinueButtonMathClicked?.Invoke());
            settingsButton.onClick.AddListener(() => SettingsButtonClicked?.Invoke());
            coinButton.onClick.AddListener(() => CoinButtonClicked?.Invoke());
            noAdsButton.onClick.AddListener(() => NoAdsButtonClicked?.Invoke());
        }

        protected override void OnShown()
        {
            base.OnShown();
            continueButton.transform.DOKill();
            continueButton.transform
                .DOScale(_continueButtonInitialScale, _continueButtonIntroScaleDuration)
                .SetEase(Ease.OutBack);
        }

        protected override void OnHidden()
        {
            base.OnHidden();
            continueButton.transform.DOKill();
        }

        public override void Show()
        {
            base.Show();
            continueButton.transform.DOKill();
            continueButton.transform.localScale = Vector3.zero;
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
