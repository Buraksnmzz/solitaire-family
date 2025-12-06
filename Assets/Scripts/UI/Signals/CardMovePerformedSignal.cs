using System.Collections.Generic;
using Card;
using Gameplay;

namespace UI.Signals
{
    public class CardMovePerformedSignal : ISignal
    {
        public CardContainer FromContainer { get; }
        public CardContainer ToContainer { get; }
        public IReadOnlyList<CardPresenter> MovedPresenters { get; }
        public IReadOnlyList<bool> MovedFaceUpStates { get; }
        public CardPresenter PreviousCard { get; }
        public bool PreviousCardWasFaceUp { get; }

        public CardMovePerformedSignal(
            CardContainer fromContainer,
            CardContainer toContainer,
            IReadOnlyList<CardPresenter> movedPresenters,
            IReadOnlyList<bool> movedFaceUpStates,
            CardPresenter previousCard,
            bool previousCardWasFaceUp)
        {
            FromContainer = fromContainer;
            ToContainer = toContainer;
            MovedPresenters = movedPresenters;
            MovedFaceUpStates = movedFaceUpStates;
            PreviousCard = previousCard;
            PreviousCardWasFaceUp = previousCardWasFaceUp;
        }
    }
}