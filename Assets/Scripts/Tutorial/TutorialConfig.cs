using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    public enum TutorialContainerType
    {
        Dealer,
        OpenDealer,
        Foundation,
        Pile
    }

    [Serializable]
    public class TutorialMovementConfig
    {
        public int cardIndex = -1;
        public bool isCategory;
        public TutorialContainerType toContainerType;
        public int toContainerIndex;
    }

    [Serializable]
    public class TutorialStep
    {
        [TextArea(3, 6)]
        public string instructionText;
        public List<TutorialMovementConfig> movements = new();
    }

    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "Tutorial/Tutorial Config")]
    public class TutorialConfig : ScriptableObject
    {
        public TutorialDeckConfig deckConfig;
        public List<TutorialStep> steps = new();
    }
}