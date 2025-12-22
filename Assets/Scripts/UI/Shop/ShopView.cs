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
        [SerializeField] private ShopNoAdsPackButton shopNoAdsPackButton;
        [SerializeField] private ShopNoAdsOnlyButton noAdsButton;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private Transform coinImage;
        [SerializeField] private RectTransform earnedCoinIconPrefab;
        private readonly int _coinAnimationCount = 8;
        
        private readonly float _coinScaleDuration = 0.2f;
        private readonly float _coinSpawnInterval = 0.08f;
        private readonly float _coinMoveDuration = 0.8f;
        private readonly float _coinMoveInterval = 0.1f;
        public int totalCoins;

        public event Action OnIconMoved;

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
            CatalogService.LoadCatalog();
        }

        public void SetNoAdsButtons(bool isNoAds)
        {
            if (isNoAds)
            {
                if (shopNoAdsPackButton != null) shopNoAdsPackButton.gameObject.SetActive(false);
                if (noAdsButton != null) noAdsButton.gameObject.SetActive(false);
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