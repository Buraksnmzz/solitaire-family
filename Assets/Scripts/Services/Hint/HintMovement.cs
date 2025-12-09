using System.Collections.Generic;
using Card;
using Gameplay;

namespace Services.Hint
{
    public enum HintPriority
    {
        CompleteFoundation = 1,
        EmptyColumn = 2,
        FoundationStandard = 3,
        FoundationCategory = 4,
        CategoryOnFullGroup = 5,
        MergeGroups = 6,
        ColumnStandard = 7,
        RevealDealer = 8
    }

    public class HintMovement
    {
        public HintMovement(CardContainer fromContainer, CardContainer toContainer, List<CardPresenter> presenters, bool isReveal)
        {
            FromContainer = fromContainer;
            ToContainer = toContainer;
            Presenters = presenters;
            IsReveal = isReveal;
        }

        public CardContainer FromContainer { get; }
        public CardContainer ToContainer { get; }
        public List<CardPresenter> Presenters { get; }
        public HintPriority Priority { get; set; }
        public bool IsReveal { get; }
        public bool CompletesFoundation { get; set; }
        public bool LeavesColumnEmpty { get; set; }
        public bool IsStandardMove { get; set; }
        public bool IsCategoryMove { get; set; }
        public bool FromPile { get; set; }
        public bool FromOpenDealer { get; set; }
        public bool ToFoundation { get; set; }
        public bool TargetsFullGroup { get; set; }
        public string CategoryName { get; set; }
        public int CategoryTotal { get; set; }
        public int StackSize => Presenters?.Count ?? 0;
    }
}
