using System.Collections.Generic;
using Gameplay;
using UnityEngine.UI;

namespace Services.Hint
{
    public interface IHintService : IService
    {
        List<HintMovement> GetPlayableMovements(Board board);
        HintMovement GetBestMovement(Board board);
        void ShowHint(Board board, bool showHand = false, Image handImage = null, float moveDuration = 0.4f, float fadeDuration = 0.25f);
        void ShowHintForMovement(Board board, HintMovement movement, bool showHand = false, Image handImage = null, float moveDuration = 0.4f, float fadeDuration = 0.25f);
    }
}
