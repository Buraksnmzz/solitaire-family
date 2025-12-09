using System.Collections.Generic;
using Gameplay;

namespace Services.Hint
{
    public interface IHintService : IService
    {
        List<HintMovement> GetPlayableMovements(Board board);
        HintMovement GetBestMovement(Board board);
        void ShowHint(Board board);
    }
}
