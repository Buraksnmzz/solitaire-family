using System.Collections.Generic;
using Card;
using Levels;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class Board: MonoBehaviour
    {
        public List<Transform> foundations;
        public Transform foundationParent;
        public List<Transform> piles;
        public Transform pileParent;
        public RectTransform dealer;
        public Transform dealerCardsHolder;
        public RectTransform openDealer;
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

        public void Setup(LevelData levelData, int currentLevelIndex)
        {
            _levelData = levelData;
            _foundationCount = levelData.columns;
            _categoryCardCount = levelData.categories.Count;
            _categoryDatas = levelData.categories;
            CalculateContentCardCount();
            SetFoundationsAndPiles();
            GenerateCardModelsAndPresenters();
            InstantiateCardViews();
            ShuffleCards();
            DealCards();
        }

        private void DealCards()
        {
            
        }

        public void GenerateCardModelsAndPresenters()
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
                    var cardPresenter = new CardPresenter
                    {
                        cardModel = cardModel
                    };
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
                var cardPresenter = new CardPresenter
                {
                    cardModel = cardModel
                };
                CardPresenters.Add(cardPresenter);
            }
        }

        public void InstantiateCardViews()
        {
            foreach (var cardModel in CardModels)
            {
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

                var cardView = Instantiate(prefab, dealerCardsHolder);
                cardView.Initialize(cardModel);
                cardViews.Add(cardView);
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
            dealer.sizeDelta = new Vector2(_itemWidth, _itemHeight);
            openDealer.sizeDelta = new Vector2(_itemWidth * openDealerWidthMultiplier, _itemHeight);
        }

        [ContextMenu("Set Foundations And Piles")]
        private void ContextMenuSetFoundationsAndPiles()
        {
            SetFoundationsAndPiles();
        }
        
        private void SetItems(List<Transform> items, Transform parent)
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
                var rect = (RectTransform)items[i];
                rect.SetParent(parent, false);
                rect.sizeDelta = new Vector2(_itemWidth, _itemHeight);
                var x = leftX + i * (_itemWidth + distanceBetweenFoundations);
                float y = 0;
                rect.anchoredPosition = new Vector2(x, y);
            }
        }

    }
}