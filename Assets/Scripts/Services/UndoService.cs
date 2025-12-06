using System.Collections.Generic;
using Card;
using Gameplay;
using UI.Signals;

namespace Services
{
    public class UndoService : IUndoService
    {
        private readonly IEventDispatcherService _eventDispatcherService;

        CardContainer _lastFromContainer;
        CardContainer _lastToContainer;
        List<CardPresenter> _lastMovedPresenters;
        List<bool> _lastMovedFaceUpStates;
        CardPresenter _lastPreviousCard;
        bool _lastPreviousCardWasFaceUp;

        public bool UndoAvailable { get; set; }

        public UndoService()
        {
            _eventDispatcherService = ServiceLocator.GetService<IEventDispatcherService>();
            _eventDispatcherService.AddListener<CardMovePerformedSignal>(OnCardMovePerformed);
            _eventDispatcherService.AddListener<UndoClickedSignal>(OnUndoClicked);
        }

        void OnCardMovePerformed(CardMovePerformedSignal signal)
        {
            _lastFromContainer = signal.FromContainer;
            _lastToContainer = signal.ToContainer;
            if (_lastToContainer is Foundation && _lastToContainer.GetCardsCount() == 0)
            {
                UndoAvailable = false;
                return;
            }
            
            _lastMovedPresenters = new List<CardPresenter>(signal.MovedPresenters);
            _lastMovedFaceUpStates = signal.MovedFaceUpStates != null
                ? new List<bool>(signal.MovedFaceUpStates)
                : null;
            _lastPreviousCard = signal.PreviousCard;
            _lastPreviousCardWasFaceUp = signal.PreviousCardWasFaceUp;
            UndoAvailable = true;
        }

        void OnUndoClicked(UndoClickedSignal signal)
        {
            if (!UndoAvailable) return;
            if (_lastFromContainer == null || _lastToContainer == null) return;
            if (_lastMovedPresenters == null || _lastMovedPresenters.Count == 0) return;

            for (var i = _lastMovedPresenters.Count - 1; i >= 0; i--)
            {
                var presenter = _lastMovedPresenters[i];
                if (presenter == null) continue;

                var removed = _lastToContainer.RemoveCard(presenter);
                if (removed == null) continue;

                if (_lastMovedFaceUpStates != null && i < _lastMovedFaceUpStates.Count)
                {
                    var shouldBeFaceUp = _lastMovedFaceUpStates[i];
                    removed.SetFaceUp(shouldBeFaceUp, 0.2f);
                }

                _lastFromContainer.AddCard(removed);
            }

            if (_lastPreviousCard != null && !_lastPreviousCardWasFaceUp && _lastPreviousCard.IsFaceUp)   
            {
                _lastPreviousCard.SetFaceUp(false, 0.2f);
            }

            UndoAvailable = false;
        }
    }
}