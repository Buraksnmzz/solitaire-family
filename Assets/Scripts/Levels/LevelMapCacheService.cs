using System.IO;
using UnityEngine;

namespace Levels
{
    public class LevelMapCacheService : ILevelMapCacheService
    {
        public LevelMapCacheService()
        {
        }

        public bool TryGetLevelsJson(SystemLanguage language, out string levelsJson)
        {
            levelsJson = string.Empty;

            try
            {
                var filePath = GetLevelsJsonFilePath(language);
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

        public void SaveLevelsJson(SystemLanguage language, string levelsJson)
        {
            if (string.IsNullOrWhiteSpace(levelsJson) || levelsJson == "{}")
            {
                return;
            }

            try
            {
                EnsureCacheDirectoryExists();

                var filePath = GetLevelsJsonFilePath(language);
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

        private static string GetLevelsJsonFilePath(SystemLanguage language)
        {
            var fileName = $"level_map_{(int)language}.json";
            return Path.Combine(GetCacheDirectoryPath(), fileName);
        }

        private static string GetCacheDirectoryPath()
        {
            return Path.Combine(Application.persistentDataPath, "LevelMaps");
        }
    }
}
