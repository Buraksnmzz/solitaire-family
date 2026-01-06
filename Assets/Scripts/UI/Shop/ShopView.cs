using System;
using System.Collections.Generic;
using DG.Tweening;
using IAP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Shop
{
    public class ShopView : BaseView
    {
        [SerializeField] private Button closeButton;
        [SerializeField] public Button rewardedVideoButton;
        [SerializeField] private ShopNoAdsPackButton shopNoAdsPackButton;
        [SerializeField] private ShopNoAdsOnlyButton noAdsButton;
        [SerializeField] private Transform shopNoAdsPack;
        [SerializeField] private Transform noAdsOnly;
        [SerializeField] private Transform coinOffers;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI rewardedVideoCoinAmountText;
        [SerializeField] private Transform coinImage;
        [SerializeField] private RectTransform earnedCoinIconPrefab;
        private readonly int _coinAnimationCount = 8;

        private readonly float _coinScaleDuration = 0.2f;
        private readonly float _coinSpawnInterval = 0.08f;
        private readonly float _coinMoveDuration = 0.8f;
        public int totalCoins;

        private readonly float _showScaleDuration = 0.2f;
        private readonly float _showSpawnInterval = 0.12f;

        private static readonly Vector2 CenterPivot = new(0.5f, 0.5f);
        private static readonly Vector2 TopCenterPivot = new(0.5f, 1f);
        
        public event Action OnIconMoved;
        public event Action RewardedVideoButtonClicked;

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
            rewardedVideoButton.onClick.AddListener(()=>RewardedVideoButtonClicked?.Invoke());
        }

        public void SetRewardedVideoCoinAmount(int coinAmount)
        {
            rewardedVideoCoinAmountText.text = coinAmount.ToString();
        }

        private static void SetPivotKeepingCenter(Transform targetTransform, Vector2 targetPivot)
        {
            if (targetTransform == null)
                return;

            if (targetTransform is not RectTransform rectTransform)
                return;

            if (rectTransform.pivot == targetPivot)
                return;

            var centerWorldBefore = rectTransform.TransformPoint(rectTransform.rect.center);
            rectTransform.pivot = targetPivot;
            var centerWorldAfter = rectTransform.TransformPoint(rectTransform.rect.center);
            rectTransform.position += centerWorldBefore - centerWorldAfter;
        }

        public void AnimateOnShow(bool isNoAds)
        {
            if (shopNoAdsPack != null) shopNoAdsPack.localScale = Vector3.zero;
            if (noAdsOnly != null) noAdsOnly.localScale = Vector3.zero;
            if (coinOffers != null) coinOffers.localScale = Vector3.zero;

            if (isNoAds)
            {
                SetPivotKeepingCenter(shopNoAdsPack, TopCenterPivot);
                coinOffers.position = shopNoAdsPack.position;
                shopNoAdsPack.gameObject.SetActive(false);
                noAdsOnly.gameObject.SetActive(false);
            }
            else
            {
                SetPivotKeepingCenter(shopNoAdsPack, CenterPivot);
                shopNoAdsPack.gameObject.SetActive(true);
                noAdsOnly.gameObject.SetActive(true);
            }

            var seq = DOTween.Sequence();
            var step = 0;

            if (shopNoAdsPack != null && shopNoAdsPack.gameObject.activeSelf)
            {
                seq.Insert(step * _showSpawnInterval, shopNoAdsPack.DOScale(Vector3.one, _showScaleDuration).SetEase(Ease.OutBack));
                step++;
            }

            if (noAdsOnly != null && noAdsOnly.gameObject.activeSelf)
            {
                seq.Insert(step * _showSpawnInterval, noAdsOnly.DOScale(Vector3.one, _showScaleDuration).SetEase(Ease.OutBack));
                step++;
            }

            if (coinOffers != null && coinOffers.gameObject.activeSelf)
            {
                seq.Insert(step * _showSpawnInterval, coinOffers.DOScale(Vector3.one, _showScaleDuration).SetEase(Ease.OutBack));
            }
        }

        public void SetNoAdsButtons(bool isNoAds)
        {
            if (isNoAds)
            {
                SetPivotKeepingCenter(shopNoAdsPack, TopCenterPivot);
                shopNoAdsPackButton.gameObject.SetActive(false);
                noAdsButton.gameObject.SetActive(false);
                coinOffers.position = shopNoAdsPack.position;
            }
            else
            {
                SetPivotKeepingCenter(shopNoAdsPack, CenterPivot);
            }
        }

        public void SetCoinText(int coin)
        {
            coinText.text = coin.ToString();
        }

        public void PlayCoinAnimation(Action onCompleted, Transform buttonTransform)
        {
            var icons = new List<RectTransform>();

            for (var i = 0; i < _coinAnimationCount; i++)
            {
                var icon = Instantiate(earnedCoinIconPrefab, panel);
                icons.Add(icon);
                icon.localScale = Vector3.zero;
                icon.transform.position = buttonTransform.position;
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
                            icon.DOMove(coinImage.position, _coinMoveDuration)
                                .SetEase(Ease.InBack)
                                .OnComplete(() =>
                                {
                                    OnIconMoved?.Invoke();
                                    coinImage.DOComplete();
                                    coinImage.DOPunchScale(Vector3.one * 0.2f, 0.08f);
                                    Destroy(icon.gameObject);
                                    remainingIcons--;
                                    if (remainingIcons == 0)
                                    {
                                        coinText.text = totalCoins.ToString();
                                        onCompleted?.Invoke();
                                    }
                                });
                        }));
            }
        }
    }
}