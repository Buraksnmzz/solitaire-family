using UnityEngine;

namespace Levels
{
    public interface ILevelMapCacheService : IService
    {
        bool TryGetLevelsJson(GameMode gameMode, SystemLanguage language, out string levelsJson);
        void SaveLevelsJson(GameMode gameMode, SystemLanguage language, string levelsJson);
    }
}
