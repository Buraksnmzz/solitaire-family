using System.Collections.Generic;
using Card;
using Gameplay.PlacableRules;
using Levels;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        [SerializeField] Dealer dealer;
        [SerializeField] private OpenDealer openDealer;
        public List<CardContainer> foundations;
        public Transform foundationParent;
        public List<CardContainer> piles;
        public Transform pileParent;
        [FormerlySerializedAs("dealer")] public RectTransform dealerRectTransform;
        public Transform dealerCardsHolder;
        [FormerlySerializedAs("openDealer")] public RectTransform openDealerRectTransform;
        [SerializeField] private Transform goalCounterTransform;
        private int _foundationCount;
        public float distanceBetweenFoundations;
        public float widhtHeightRatio = 0.731f;
        private float _itemWidth;
        private float _itemHeight;
        private readonly List<int> _totalCardsCountHolder = new List<int> { 30, 54, 80 };
        private int _contentCardCount;
        private int _categoryCardCount;
        public List<CardModel> CardModels = new();
        public List<CardView> cardViews = new();
        public List<CardPresenter> CardPresenters = new();
        private LevelData _levelData;
        private List<CategoryData> _categoryDatas;
        public CardView contentCardView;
        public CardView categoryCardView;
        private IPlacableRule _pileRule;
        private IPlacableRule _foundationRule;
        private IPlacableRule _noPlacableRule;


        public void Setup(LevelData levelData, int currentLevelIndex)
        {
            _levelData = levelData;
            _foundationCount = levelData.columns;
            _categoryCardCount = levelData.categories.Count;
            _categoryDatas = levelData.categories;
            CalculateContentCardCount();
            SetFoundationsAndPiles();
            InitializeContainers();
            GenerateCardModelsAndPresenters();
            InstantiateCardViews();
            ShuffleCards();
            DealCards();
        }

        private void InitializeContainers()
        {
            _pileRule = new PilePlacableRule();
            _foundationRule = new FoundationPlacableRule();
            _noPlacableRule = new NoPlacableRule();
            foreach (var pile in piles)
            {
                pile.Setup(_pileRule);
            }

            foreach (var foundation in foundations)
            {
                foundation.Setup(_foundationRule);
            }
            dealer.Setup(_noPlacableRule);
            openDealer.Setup(_noPlacableRule);
        }

        private void DealCards()
        {
            if (dealer == null)
                return;

            var pileCounts = GetInitialPileCounts();
            if (pileCounts == null)
                return;

            for (var pileIndex = 0; pileIndex < pileCounts.Length && pileIndex < piles.Count; pileIndex++)
            {
                var targetPile = piles[pileIndex];
                var cardsToDeal = pileCounts[pileIndex];

                for (var i = 0; i < cardsToDeal; i++)
                {
                    var topCardModel = dealer.GetTopCardModel();
                    var topCardView = dealer.RemoveCard(topCardModel);
                    if (topCardView == null)
                        return;

                    targetPile.AddCard(topCardView, topCardModel);
                }
            }
        }

        private int[] GetInitialPileCounts()
        {
            switch (_foundationCount)
            {
                case 3:
                    return new[] { 3, 4, 5 };
                case 4:
                    return new[] { 3, 4, 5, 6 };
                case 5:
                    return new[] { 5, 6, 7, 8, 9 };
                default:
                    return null;
            }
        }

        private void GenerateCardModelsAndPresenters()
        {
            foreach (var category in _categoryDatas)
            {
                foreach (var content in category.contentValues)
                {
                    var cardModel = new CardModel
                    {
                        CategoryType = category.cardCategoryType,
                        CategoryName = category.name,
                        ContainerType = CardContainerType.Dealer,
                        Type = CardType.Content,
                        ContentName = content,
                        ContentCount = category.contentValues.Count,
                    };
                    CardModels.Add(cardModel);
                    var cardPresenter = new CardPresenter();
                    CardPresenters.Add(cardPresenter);
                }
            }

            foreach (var category in _categoryDatas)
            {
                var cardModel = new CardModel
                {
                    CategoryType = category.cardCategoryType,
                    CategoryName = category.name,
                    ContainerType = CardContainerType.Dealer,
                    Type = CardType.Category,
                    ContentCount = category.contentValues.Count,
                };
                CardModels.Add(cardModel);
                var cardPresenter = new CardPresenter();
                CardPresenters.Add(cardPresenter);
            }
        }

        private void InstantiateCardViews()
        {
            for (var index = 0; index < CardModels.Count; index++)
            {
                var cardModel = CardModels[index];
                CardView prefab;

                if (cardModel.Type == CardType.Content)
                {
                    prefab = contentCardView;
                }
                else if (cardModel.Type == CardType.Category)
                {
                    prefab = categoryCardView;
                }
                else
                {
                    continue;
                }

                if (prefab == null)
                    continue;

                var cardView = Instantiate(prefab, dealerRectTransform);
                cardViews.Add(cardView);

                if (dealer != null)
                {
                    dealer.AddCard(cardView, cardModel);
                }

                if (index < CardPresenters.Count)
                {
                    var presenter = CardPresenters[index];
                    presenter.Initialize(cardModel, cardView);
                }
            }
        }

        private void ShuffleCards()
        {
            if (cardViews.Count == 0)
                return;

            var count = cardViews.Count;
            for (var i = count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                if (i == j)
                    continue;

                (cardViews[i], cardViews[j]) = (cardViews[j], cardViews[i]);
            }

            for (var index = 0; index < cardViews.Count; index++)
            {
                var cardView = cardViews[index];
                if (cardView == null)
                    continue;

                cardView.transform.SetSiblingIndex(index);
            }
        }


        private void CalculateContentCardCount()
        {
            var totalCardsCount = 0;
            switch (_foundationCount)
            {
                case 3:
                    totalCardsCount = _totalCardsCountHolder[0];
                    break;
                case 4:
                    totalCardsCount = _totalCardsCountHolder[1];
                    break;
                case 5:
                    totalCardsCount = _totalCardsCountHolder[2];
                    break;
            }

            _contentCardCount = totalCardsCount - _categoryCardCount;
        }

        private void SetFoundationsAndPiles()
        {
            SetItems(foundations, foundationParent);
            SetItems(piles, pileParent);
            SetDealer();
        }

        private void SetDealer()
        {
            var openDealerWidthMultiplier = 1.737f;
            dealerRectTransform.sizeDelta = new Vector2(_itemWidth, _itemHeight);
            openDealerRectTransform.sizeDelta = new Vector2(_itemWidth * openDealerWidthMultiplier, _itemHeight);
            dealerRectTransform.pivot = new Vector2(0.5f, 0.5f);
            dealerRectTransform.anchoredPosition = new Vector2(-_itemWidth / 2, -148 - _itemHeight / 2);
        }

        [ContextMenu("Set Foundations And Piles")]
        private void ContextMenuSetFoundationsAndPiles()
        {
            SetFoundationsAndPiles();
        }

        private void SetItems(List<CardContainer> items, Transform parent)
        {
            if (parent == null)
                return;

            for (var i = 0; i < items.Count; i++)
            {
                items[i].gameObject.SetActive(i < _foundationCount);
            }

            var activeCount = Mathf.Clamp(_foundationCount, 0, items.Count);
            if (activeCount == 0)
                return;

            var parentWidth = ((RectTransform)parent).rect.width;
            var layoutCountForWidth = activeCount == 3 ? 4 : activeCount;
            var totalSpacing = distanceBetweenFoundations * (layoutCountForWidth - 1);
            var availableWidth = parentWidth - totalSpacing;
            _itemWidth = availableWidth / layoutCountForWidth;
            _itemHeight = _itemWidth / widhtHeightRatio;

            var leftX = -parentWidth * 0.5f + _itemWidth * 0.5f;

            for (var i = 0; i < activeCount; i++)
            {
                var rect = items[i].GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.sizeDelta = new Vector2(_itemWidth, _itemHeight);
                var x = leftX + i * (_itemWidth + distanceBetweenFoundations);
                float y = 0;
                rect.anchoredPosition = new Vector2(x, y);
            }
        }

    }
}