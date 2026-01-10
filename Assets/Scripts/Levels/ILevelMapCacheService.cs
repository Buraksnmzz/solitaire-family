using UnityEngine;

namespace Levels
{
    public interface ILevelMapCacheService : IService
    {
        bool TryGetLevelsJson(SystemLanguage language, out string levelsJson);
        void SaveLevelsJson(SystemLanguage language, string levelsJson);
    }
}
