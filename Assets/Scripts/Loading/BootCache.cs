using System.Collections.Generic;
using Levels;

namespace Loading
{
    public static class BootCache
    {
        private static readonly Dictionary<GameMode, string> LevelsJsonByMode = new Dictionary<GameMode, string>();

        public static string ConfigurationJson { get; private set; }

        public static void SetConfigurationJson(string value)
        {
            ConfigurationJson = value ?? string.Empty;
        }

        public static string GetLevelsJson(GameMode gameMode)
        {
            return LevelsJsonByMode.TryGetValue(gameMode, out var value) ? value : string.Empty;
        }

        public static void SetLevelsJson(GameMode gameMode, string value)
        {
            LevelsJsonByMode[gameMode] = value ?? string.Empty;
        }
    }
}
