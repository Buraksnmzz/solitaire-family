using System.IO;
using UnityEngine;

namespace Levels
{
    public class LevelMapCacheService : ILevelMapCacheService
    {
        public LevelMapCacheService()
        {
        }

        public bool TryGetLevelsJson(GameMode gameMode, SystemLanguage language, out string levelsJson)
        {
            levelsJson = string.Empty;

            try
            {
                var filePath = GetLevelsJsonFilePath(gameMode, language);
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var cached = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(cached) || cached == "{}")
                {
                    return false;
                }

                levelsJson = cached;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SaveLevelsJson(GameMode gameMode, SystemLanguage language, string levelsJson)
        {
            if (string.IsNullOrWhiteSpace(levelsJson) || levelsJson == "{}")
            {
                return;
            }

            try
            {
                EnsureCacheDirectoryExists();

                var filePath = GetLevelsJsonFilePath(gameMode, language);
                var tempFilePath = filePath + ".tmp";
                File.WriteAllText(tempFilePath, levelsJson);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                File.Move(tempFilePath, filePath);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
        }

        private static void EnsureCacheDirectoryExists()
        {
            var directoryPath = GetCacheDirectoryPath();
            if (Directory.Exists(directoryPath))
            {
                return;
            }

            Directory.CreateDirectory(directoryPath);
        }

        private static string GetLevelsJsonFilePath(GameMode gameMode, SystemLanguage language)
        {
            var fileName = gameMode == GameMode.Classic
                ? $"level_map_{(int)language}.json"
                : $"level_map_{gameMode.ToString().ToLowerInvariant()}.json";
            return Path.Combine(GetCacheDirectoryPath(), fileName);
        }

        private static string GetCacheDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, "LevelMaps");
        }
    }
}
