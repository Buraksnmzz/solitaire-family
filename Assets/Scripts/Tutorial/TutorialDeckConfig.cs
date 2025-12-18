using System.Collections.Generic;
using UnityEngine;

namespace Tutorial
{
    [CreateAssetMenu(fileName = "TutorialDeckConfig", menuName = "Tutorial/Tutorial Deck Config")]
    public class TutorialDeckConfig : ScriptableObject
    {
        public List<string> orderedCardNames = new();
    }
}
