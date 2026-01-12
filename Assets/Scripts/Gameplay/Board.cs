using System.Collections.Generic;
using System.Linq;
using Card;
using Gameplay.PlacableRules;
using Levels;
using UI.Signals;
using UI.Win;
using UnityEngine;
using UnityEngine.Serialization;
using Tutorial;
using UnityEngine.UI;

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
        public RectTransform dealerRectTransform;
        public RectTransform openDealerRectTransform;
        [SerializeField] private RectTransform dealerHint;
        [SerializeField] private RectTransform dealerEmptyImage;
        private int _foundationCount;
        public float distanceBetweenFoundations;
        public float widhtHeightRatio = 0.731f;
        private float _itemWidth;
        private float _itemHeight;
        private readonly List<int> _totalCardsCountHolder = new List<int> { 30, 54, 80 };
        public List<CardModel> CardModels = new();
        public List<CardView> cardViews = new();
        public List<CardPresenter> CardPresenters = new();
        private List<CategoryData> _categoryDatas;
        public CardView contentCardView;
        public CardView categoryCardView;
        public CardView jokerCardView;
        private IPlacableRule _pileRule;
        private IPlacableRule _foundationRule;
        private IPlacableRule _noPlacableRule;
        IEventDispatcherService _eventDispatcher;
        private Transform _parent;

        public Dealer Dealer => dealer;
        public OpenDealer OpenDealer => openDealer;
        public IReadOnlyList<CardContainer> Foundations => foundations;
        public IReadOnlyList<CardContainer> Piles => piles;
        public Transform BoardParent => _parent;


        public void Setup(LevelData levelData, int currentLevelIndex, Transform parent, SnapShotModel snapshot = null, bool shuffleDeck = true, TutorialDeckConfig deckConfig = null)
        {
            ResetBoardState();
            _parent = parent;
            _foundationCount = levelData.columns;
            _categoryDatas = levelData.categories;
            SetContainerTransforms();
            InitializeContainers();
            if (snapshot != null && TryRestoreSnapshot(snapshot, currentLevelIndex))
                return;
            GenerateCardModelsAndPresenters();
            if (deckConfig != null)
                ApplyDeckOrder(deckConfig);
            InstantiateCardViews();
            dealer.SetupDeck(CardModels, CardPresenters, cardViews);
            if (shuffleDeck)
                dealer.ShuffleDeck();
            dealer.DealInitialCards(piles, _foundationCount);
        }

        public bool IsGameWon()
        {
            if (dealer.GetCardsCount() > 0)
                return false;
            if (openDealer.GetCardsCount() > 0)
                return false;
            if (piles.Any(pile => pile.GetCardsCountWithoutJoker() > 0))
            {
                return false;
            }

            if (foundations.Any(foundation => foundation.GetCardsCountWithoutJoker() > 0))
            {
                return false;
            }

            return true;
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

        void ApplyDeckOrder(TutorialDeckConfig deckConfig)
        {
            if (deckConfig == null)
                return;

            if (deckConfig.orderedCardIndices == null || deckConfig.orderedCardIndices.Count == 0)
                return;

            var newModels = new List<CardModel>(CardModels.Count);
            var newPresenters = new List<CardPresenter>(CardPresenters.Count);
            var usedIndices = new HashSet<int>();

            for (var i = 0; i < deckConfig.orderedCardIndices.Count; i++)
            {
                var index = deckConfig.orderedCardIndices[i];
                if (index < 0) continue;
                if (index >= CardModels.Count) continue;
                if (index >= CardPresenters.Count) continue;
                if (!usedIndices.Add(index)) continue;

                var model = CardModels[index];
                var presenter = CardPresenters[index];
                if (model == null || presenter == null) continue;
                newModels.Add(model);
                newPresenters.Add(presenter);
            }

            for (var index = 0; index < CardModels.Count && index < CardPresenters.Count; index++)
            {
                if (usedIndices.Contains(index))
                    continue;

                var model = CardModels[index];
                var presenter = CardPresenters[index];
                if (model == null || presenter == null) continue;
                newModels.Add(model);
                newPresenters.Add(presenter);
            }

            if (newModels.Count != CardModels.Count || newPresenters.Count != CardPresenters.Count)
                return;

            CardModels.Clear();
            CardModels.AddRange(newModels);
            CardPresenters.Clear();
            CardPresenters.AddRange(newPresenters);
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
                cardView.SetRightTextParentSize(_itemWidth * 1.15f, _itemWidth * 0.36f);
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
            foundationParent.anchoredPosition =
                _foundationCount <= 4 ? new Vector3(0, -390, 0) : new Vector3(0, -340, 0);
            pileParent.anchoredPosition =
                _foundationCount <= 4 ? new Vector3(0, -610, 0) : new Vector3(0, -510, 0);

            _itemWidth = _foundationCount <= 4 ? 142f : 119f;
            _itemHeight = _itemWidth / widhtHeightRatio;
            foundationParent.GetComponent<HorizontalLayoutGroup>().spacing = _foundationCount <= 4 ? 25 : 18;
            pileParent.GetComponent<HorizontalLayoutGroup>().spacing = _foundationCount <= 4 ? 25 : 18;

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

        private void SetDealer()
        {
            var openDealerWidthMultiplier = 1.737f;
            dealerRectTransform.sizeDelta = new Vector2(_itemWidth, _itemHeight);
            openDealerRectTransform.sizeDelta = new Vector2(_itemWidth * openDealerWidthMultiplier, _itemHeight);
            dealerRectTransform.pivot = new Vector2(0.5f, 0.5f);
            dealerRectTransform.anchoredPosition = new Vector2(-_itemWidth / 2, -148 - _itemHeight / 2);

            dealerHint.sizeDelta = dealerRectTransform.sizeDelta * 1.175f;
            dealerHint.anchoredPosition = dealerRectTransform.anchoredPosition;
            dealerEmptyImage.sizeDelta = dealerRectTransform.sizeDelta;
            dealerEmptyImage.anchoredPosition = dealerRectTransform.anchoredPosition;
        }

        public void GenerateJokerCard(int pileIndex)
        {
            if (pileIndex < 0) return;
            if (pileIndex >= piles.Count) return;

            var pile = piles[pileIndex];
            if (pile == null) return;
            if (!pile.gameObject.activeInHierarchy) return;

            var cardModel = new CardModel
            {
                CategoryType = CardCategoryType.Image,
                Type = CardType.Joker,
                IsFaceUp = true
            };

            CardModels.Add(cardModel);
            var cardPresenter = new CardPresenter();
            CardPresenters.Add(cardPresenter);
            var cardView = Instantiate(jokerCardView, pile.transform);
            cardViews.Add(cardView);
            cardPresenter.Initialize(cardModel, cardView, _parent);
            pile.AddCard(cardPresenter, 0, 0);
            cardView.AnimateGlow();
        }

        public SnapShotModel CreateSnapshot(int movesCount, int levelIndex)
        {
            var snapshot = new SnapShotModel
            {
                LevelIndex = levelIndex,
                MovesCount = movesCount
            };

            if (!AddContainerSnapshots(dealer, SnapshotContainerType.Dealer, 0, snapshot))
                return null;
            if (!AddContainerSnapshots(openDealer, SnapshotContainerType.OpenDealer, 0, snapshot))
                return null;
            if (!AddContainerSnapshots(foundations, SnapshotContainerType.Foundation, snapshot))
                return null;
            if (!AddContainerSnapshots(piles, SnapshotContainerType.Pile, snapshot))
                return null;

            if (snapshot.Cards.Count != CardModels.Count)
                return null;

            return snapshot;
        }

        bool AddContainerSnapshots(CardContainer container, SnapshotContainerType containerType, int containerIndex, SnapShotModel snapshot)
        {
            if (container == null)
                return false;

            var cards = container.GetAllCards();
            foreach (var presenter in cards)
            {
                if (presenter == null)
                    return false;

                var model = presenter.CardModel;
                if (model == null)
                    return false;

                snapshot.Cards.Add(new CardSnapshot
                {
                    Type = model.Type,
                    CategoryType = model.CategoryType,
                    CategoryName = model.CategoryName,
                    ContentName = model.ContentName,
                    ContentCount = model.ContentCount,
                    CurrentContentCount = model.CurrentContentCount,
                    IsFaceUp = model.IsFaceUp,
                    ContainerType = containerType,
                    ContainerIndex = containerIndex
                });
            }

            return true;
        }

        bool AddContainerSnapshots(List<CardContainer> containers, SnapshotContainerType containerType, SnapShotModel snapshot)
        {
            var count = Mathf.Min(_foundationCount, containers.Count);
            for (var index = 0; index < count; index++)
            {
                var container = containers[index];
                if (!AddContainerSnapshots(container, containerType, index, snapshot))
                    return false;
            }

            return true;
        }

        bool TryRestoreSnapshot(SnapShotModel snapshot, int currentLevelIndex)
        {
            if (snapshot == null)
                return false;

            if (snapshot.LevelIndex != currentLevelIndex)
                return false;

            if (snapshot.Cards == null || snapshot.Cards.Count == 0)
                return false;

            if (!AreContainerIndicesValid(snapshot.Cards))
                return false;

            var tempModels = new List<CardModel>();
            var tempPresenters = new List<CardPresenter>();
            var tempViews = new List<CardView>();

            foreach (var cardData in snapshot.Cards)
            {
                var cardModel = new CardModel
                {
                    Type = cardData.Type,
                    CategoryType = cardData.CategoryType,
                    CategoryName = cardData.CategoryName,
                    ContentName = cardData.ContentName,
                    ContentCount = cardData.ContentCount,
                    CurrentContentCount = cardData.CurrentContentCount,
                    IsFaceUp = cardData.IsFaceUp
                };

                var prefab = GetPrefab(cardModel);
                if (prefab == null)
                {
                    DestroyCardViews(tempViews);
                    return false;
                }

                var cardView = Instantiate(prefab, dealerRectTransform);
                var presenter = new CardPresenter();
                presenter.Initialize(cardModel, cardView, _parent);

                tempModels.Add(cardModel);
                tempPresenters.Add(presenter);
                tempViews.Add(cardView);
                cardView.SetRightTextParentSize(_itemWidth * 1.1f, _itemWidth * 0.37f);
            }

            for (var index = 0; index < snapshot.Cards.Count; index++)
            {
                var cardData = snapshot.Cards[index];
                var presenter = tempPresenters[index];
                var container = ResolveContainer(cardData.ContainerType, cardData.ContainerIndex);
                if (container == null)
                {
                    DestroyCardViews(tempViews);
                    return false;
                }

                container.AddCard(presenter);
                presenter.SetFaceUp(cardData.IsFaceUp, 0);
            }

            CardModels.Clear();
            CardModels.AddRange(tempModels);
            CardPresenters.Clear();
            CardPresenters.AddRange(tempPresenters);
            cardViews.Clear();
            cardViews.AddRange(tempViews);

            return true;
        }

        bool AreContainerIndicesValid(IEnumerable<CardSnapshot> cards)
        {
            foreach (var card in cards)
            {
                if (card.ContainerType == SnapshotContainerType.Foundation || card.ContainerType == SnapshotContainerType.Pile)
                {
                    if (card.ContainerIndex < 0 || card.ContainerIndex >= _foundationCount)
                        return false;
                }
            }

            return true;
        }

        void ResetBoardState()
        {
            ClearContainerCards(dealer);
            ClearContainerCards(openDealer);

            if (foundations != null)
            {
                foreach (var foundation in foundations)
                {
                    ClearContainerCards(foundation);
                }
            }

            if (piles != null)
            {
                foreach (var pile in piles)
                {
                    ClearContainerCards(pile);
                }
            }

            CardModels.Clear();
            CardPresenters.Clear();
            cardViews.Clear();
        }

        void ClearContainerCards(CardContainer container)
        {
            if (container == null) return;
            container.ClearAllCards();
        }

        CardContainer ResolveContainer(SnapshotContainerType containerType, int containerIndex)
        {
            switch (containerType)
            {
                case SnapshotContainerType.Dealer:
                    return dealer;
                case SnapshotContainerType.OpenDealer:
                    return openDealer;
                case SnapshotContainerType.Foundation:
                    return containerIndex >= 0 && containerIndex < foundations.Count ? foundations[containerIndex] : null;
                case SnapshotContainerType.Pile:
                    return containerIndex >= 0 && containerIndex < piles.Count ? piles[containerIndex] : null;
                default:
                    return null;
            }
        }

        CardView GetPrefab(CardModel cardModel)
        {
            switch (cardModel.Type)
            {
                case CardType.Content:
                    return contentCardView;
                case CardType.Category:
                    return categoryCardView;
                case CardType.Joker:
                    return jokerCardView;
                default:
                    return null;
            }
        }

        void DestroyCardViews(IEnumerable<CardView> views)
        {
            foreach (var view in views)
            {
                if (view != null)
                    Destroy(view.gameObject);
            }
        }


    }
}