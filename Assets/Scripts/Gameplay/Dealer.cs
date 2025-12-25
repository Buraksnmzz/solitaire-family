using System.Collections.Generic;
using Card;
using Core.Scripts.Services;
using DG.Tweening;
using Gameplay.PlacableRules;
using UI.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay
{
    public class Dealer : CardContainer
    {
        [SerializeField] private Button dealerButton;
        [SerializeField] private OpenDealer openDealer;
        [SerializeField] protected CanvasGroup dealerHint;
        private List<CardModel> _cardModels;
        private IEventDispatcherService _eventDispatcherService;
        private ISoundService _soundService;
        protected Sequence _hintSequence;
        private ITutorialMoveRestrictionService _tutorialMoveRestrictionService;
        IHapticService _hapticService;

        public void SetupDeck(List<CardModel> cardModels, List<CardPresenter> cardPresenters, List<CardView> cardViews)
        {
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _tutorialMoveRestrictionService = ServiceLocator.GetService<ITutorialMoveRestrictionService>();
            _soundService = ServiceLocator.GetService<ISoundService>();
            _hapticService = ServiceLocator.GetService<IHapticService>();
            _cardModels = new List<CardModel>(cardModels);
            CardPresenters = new List<CardPresenter>(cardPresenters);

            for (var i = 0; i < CardPresenters.Count && i < cardViews.Count; i++)
            {
                var presenter = CardPresenters[i];
                presenter.SetParent(transform, true);
                presenter.SetContainer(this);
            }
        }

        public void ShuffleDeck()
        {
            var count = CardPresenters.Count;
            if (count == 0) return;

            for (var i = count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                if (i == j) continue;

                (CardPresenters[i], CardPresenters[j]) = (CardPresenters[j], CardPresenters[i]);
                if (_cardModels != null && _cardModels.Count == count)
                {
                    (_cardModels[i], _cardModels[j]) = (_cardModels[j], _cardModels[i]);
                }
            }

            for (var index = 0; index < CardPresenters.Count; index++)
            {
                var presenter = CardPresenters[index];
                if (presenter.CardView == null) continue;
                presenter.CardView.transform.SetSiblingIndex(index);
            }
        }

        public void DealInitialCards(List<CardContainer> piles, int foundationCount)
        {
            if (piles == null || piles.Count == 0) return;

            var pileCounts = GetInitialPileCounts(foundationCount);
            if (pileCounts == null) return;
            _soundService.PlaySound(ClipName.InitialDeal);

            for (var pileIndex = 0; pileIndex < pileCounts.Length && pileIndex < piles.Count; pileIndex++)
            {
                var targetPile = piles[pileIndex];
                var cardsToDeal = pileCounts[pileIndex];

                for (var i = 0; i < cardsToDeal; i++)
                {
                    var topCardPresenter = GetTopCardPresenter();
                    if (topCardPresenter == null) return;

                    RemoveCard(topCardPresenter);
                    targetPile.AddCard(topCardPresenter, pileIndex * 0.02f * pileCounts.Length + i * 0.02f, 0.5f);
                }
            }

            for (var i = 0; i < piles.Count; i++)
            {
                var topCard = piles[i].GetTopCardPresenter();
                if (topCard == null) continue;
                topCard.SetFaceUp(true, FlipDuration * 2f);
            }
        }

        private int[] GetInitialPileCounts(int foundationCount)
        {
            switch (foundationCount)
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

        public override Vector3 GetCardLocalPosition(int index)
        {
            return Vector3.zero;
        }

        public override void Setup(IPlacableRule placableRule)
        {
            base.Setup(placableRule);
            _soundService = ServiceLocator.GetService<ISoundService>();
            _hapticService = ServiceLocator.GetService<IHapticService>();
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            dealerButton.onClick.RemoveAllListeners();
            dealerButton.onClick.AddListener(OnDealerButtonClick);
        }

        public virtual void PlayHintCue(float fadeDuration = 0.7f, float holdDuration = 0.2f)
        {
            if (dealerHint == null) return;

            _hintSequence?.Kill();
            dealerHint.DOKill();
            dealerHint.alpha = 0f;
            dealerHint.gameObject.SetActive(true);

            _hintSequence = DOTween.Sequence();
            _hintSequence.Append(dealerHint.DOFade(1f, fadeDuration));
            _hintSequence.AppendInterval(holdDuration);
            _hintSequence.Append(dealerHint.DOFade(0f, fadeDuration));
            _hintSequence.AppendInterval(holdDuration);
            _hintSequence.SetLoops(-1, LoopType.Restart);
            _hintSequence.OnKill(() => { _hintSequence = null; });
        }

        public void StopHintCue()
        {
            if (dealerHint == null) return;

            _hintSequence?.Kill();
            dealerHint.DOKill();
            dealerHint.gameObject.SetActive(false);
        }

        private void OnDealerButtonClick()
        {
            Debug.Log("OnDealerButtonClick");
            if (_tutorialMoveRestrictionService != null && _tutorialMoveRestrictionService.IsActive && !_tutorialMoveRestrictionService.IsDragAllowed(GetTopCard())) return;

            _hintSequence?.Kill();
            dealerHint.DOKill();
            dealerHint.gameObject.SetActive(false);
            var topCardPresenter = GetTopCard();
            if (topCardPresenter == null)
            {
                MoveAllCardsFromOpenDealerToDealer();
                return;
            }
            EventDispatcherService.Dispatch(new CardMovementStateChangedSignal(true));
            _soundService.PlaySound(ClipName.DealerToOpenDealer);
            _hapticService.HapticLow();
            var removedPresenter = RemoveCard(topCardPresenter);
            if (removedPresenter == null) return;
            var moved = new List<CardPresenter> { removedPresenter };
            var movedStates = new List<bool> { removedPresenter.IsFaceUp };
            _eventDispatcherService.Dispatch(new CardMovePerformedSignal(this, openDealer, moved, movedStates, null, false));
            _eventDispatcherService.Dispatch(new MoveCountRequestedSignal());
            removedPresenter.SetParent(openDealer.transform, true);
            openDealer.AddCard(removedPresenter);
            removedPresenter.SetFaceUp(true, FlipDuration);
        }

        private void MoveAllCardsFromOpenDealerToDealer()
        {
            if (openDealer == null) return;

            var openDealerCards = openDealer.GetAllCardPresenters();
            if (openDealerCards == null || openDealerCards.Count == 0) return;
            var movedPresenters = new List<CardPresenter>(openDealerCards);
            var movedStates = new List<bool>();
            for (var i = 0; i < movedPresenters.Count; i++)
            {
                movedStates.Add(movedPresenters[i].IsFaceUp);
            }

            _eventDispatcherService.Dispatch(new MoveCountRequestedSignal());
            _soundService.PlaySound(ClipName.OpenDealerToDealer);
            _hapticService.HapticLow();

            for (var i = movedPresenters.Count - 1; i >= 0; i--)
            {
                var presenter = movedPresenters[i];
                if (presenter == null) continue;

                var removedPresenter = openDealer.RemoveCard(presenter);
                if (removedPresenter == null) continue;

                removedPresenter.SetFaceUp(false, FlipDuration);
                removedPresenter.SetParent(transform, true);
                AddCard(removedPresenter);
            }

            if (movedPresenters.Count > 0)
            {
                _eventDispatcherService.Dispatch(new CardMovePerformedSignal(openDealer, this, movedPresenters, movedStates, null, false));
            }
        }
    }
}