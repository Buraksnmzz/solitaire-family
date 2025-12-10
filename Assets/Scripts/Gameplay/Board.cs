using System.Collections.Generic;
using System.Linq;
using Card;
using Gameplay.PlacableRules;
using Levels;
using UI.Signals;
using UI.Win;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class Board : MonoBehaviour
    {
        [SerializeField] Dealer dealer;
        [SerializeField] private OpenDealer openDealer;
        public List<CardContainer> foundations;
        public RectTransform foundationParent;
        public List<CardContainer> piles;
        public RectTransform pileParent;
        [FormerlySerializedAs("dealer")] public RectTransform dealerRectTransform;
        public Transform dealerCardsHolder;
        [FormerlySerializedAs("openDealer")] public RectTransform openDealerRectTransform;
        [SerializeField] private Transform goalCounterTransform;
        [SerializeField] private RectTransform dealerHint;
        [SerializeField] private RectTransform dealerEmptyImage;
        private int _foundationCount;
        public float distanceBetweenFoundations;
        public float widhtHeightRatio = 0.731f;
        private float _itemWidth;
        private float _itemHeight;
        private readonly List<int> _totalCardsCountHolder = new List<int> { 30, 54, 80 };
        private int _categoryCardCount;
        public List<CardModel> CardModels = new();
        public List<CardView> cardViews = new();
        public List<CardPresenter> CardPresenters = new();
        private LevelData _levelData;
        private List<CategoryData> _categoryDatas;
        public CardView contentCardView;
        public CardView categoryCardView;
        public CardView jokerCardView;
        private IPlacableRule _pileRule;
        private IPlacableRule _foundationRule;
        private IPlacableRule _noPlacableRule;
        IEventDispatcherService _eventDispatcher;
        IUIService _uiService;
        private Transform _parent;

        public Dealer Dealer => dealer;
        public OpenDealer OpenDealer => openDealer;
        public IReadOnlyList<CardContainer> Foundations => foundations;
        public IReadOnlyList<CardContainer> Piles => piles;
        public Transform BoardParent => _parent;


        public void Setup(LevelData levelData, int currentLevelIndex, Transform parent)
        {
            _parent = parent;
            _eventDispatcher = ServiceLocator.GetService<IEventDispatcherService>();
            _uiService = ServiceLocator.GetService<IUIService>();
            _eventDispatcher.AddListener<MoveCountRequestedSignal>(OnMoveCountRequested);
            _levelData = levelData;
            _foundationCount = levelData.columns;
            _categoryCardCount = levelData.categories.Count;
            _categoryDatas = levelData.categories;
            SetContainerTransforms();
            InitializeContainers();
            GenerateCardModelsAndPresenters();
            InstantiateCardViews();
            dealer.SetupDeck(CardModels, CardPresenters, cardViews);
            dealer.ShuffleDeck();
            dealer.DealInitialCards(piles, _foundationCount);
        }

        private void OnDisable()
        {
            _eventDispatcher.RemoveListener<MoveCountRequestedSignal>(OnMoveCountRequested);
        }

        private void OnMoveCountRequested(MoveCountRequestedSignal _)
        {
            if (dealer.GetCardsCount() > 0)
                return;
            if (openDealer.GetCardsCount() > 0)
                return;
            if (piles.Any(pile => pile.GetCardsCount() > 0))
            {
                return;
            }

            if (foundations.Any(foundation => foundation.GetCardsCount() > 0))
            {
                return;
            }

            _uiService.ShowPopup<WinPresenter>();
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
                        Type = CardType.Content,
                        ContentName = content,
                        ContentCount = category.contentValues.Count,
                        IsFaceUp = false
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
                    Type = CardType.Category,
                    ContentCount = category.contentValues.Count,
                    IsFaceUp = false
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

                if (index < CardPresenters.Count)
                {
                    var presenter = CardPresenters[index];
                    presenter.Initialize(cardModel, cardView, _parent);
                    dealer.AddCard(presenter);
                }
            }
        }

        private void SetContainerTransforms()
        {
            TrimContainerLists();
            foundationParent.anchoredPosition =
                _foundationCount <= 4 ? new Vector3(0, -404, 0) : new Vector3(0, -355, 0);
            pileParent.anchoredPosition =
                _foundationCount <= 4 ? new Vector3(0, -642, 0) : new Vector3(0, -540, 0);

            _itemWidth = _foundationCount <= 4 ? 152f : 116f;
            _itemHeight = _itemWidth / widhtHeightRatio;
            for (var index = 0; index < piles.Count; index++)
            {
                piles[index].gameObject.SetActive(index < _foundationCount);
                var pile = piles[index];
                var rect = pile.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(_itemWidth, _itemHeight);
            }

            for (var index = 0; index < foundations.Count; index++)
            {
                foundations[index].gameObject.SetActive(index < _foundationCount);
                var foundation = foundations[index];
                var rect = foundation.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(_itemWidth, _itemHeight);
            }

            SetDealer();
        }

        void TrimContainerLists()
        {
            while (foundations.Count > _foundationCount)
            {
                var index = foundations.Count - 1;
                var container = foundations[index];
                if (container != null)
                {
                    Destroy(container.gameObject);
                }
                foundations.RemoveAt(index);
            }

            while (piles.Count > _foundationCount)
            {
                var index = piles.Count - 1;
                var container = piles[index];
                if (container != null)
                {
                    Destroy(container.gameObject);
                }
                piles.RemoveAt(index);
            }
        }

        private void SetDealer()
        {
            var openDealerWidthMultiplier = 1.737f;
            dealerRectTransform.sizeDelta = new Vector2(_itemWidth, _itemHeight);
            openDealerRectTransform.sizeDelta = new Vector2(_itemWidth * openDealerWidthMultiplier, _itemHeight);
            dealerRectTransform.pivot = new Vector2(0.5f, 0.5f);
            dealerRectTransform.anchoredPosition = new Vector2(-_itemWidth / 2, -148 - _itemHeight / 2);
            
            dealerHint.sizeDelta = dealerRectTransform.sizeDelta;
            dealerHint.anchoredPosition = dealerRectTransform.anchoredPosition;
            dealerEmptyImage.sizeDelta = dealerRectTransform.sizeDelta;
            dealerEmptyImage.anchoredPosition = dealerRectTransform.anchoredPosition;
        }

        public void GenerateJokerCard()
        {
            var cardModel = new CardModel
            {
                CategoryType = CardCategoryType.Image,
                Type = CardType.Joker,
                IsFaceUp = true
            };
            CardModels.Add(cardModel);
            var cardPresenter = new CardPresenter();
            CardPresenters.Add(cardPresenter);
            var cardView = Instantiate(jokerCardView, piles[0].transform);
            cardViews.Add(cardView);
            cardPresenter.Initialize(cardModel, cardView, _parent);
            piles[0].AddCard(cardPresenter);
        }
    }
}