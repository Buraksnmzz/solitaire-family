namespace Loading
{
    public static class BootCache
    {
        public static string ConfigurationJson { get; private set; }
        public static string LevelsJson { get; private set; }

        public static void SetConfigurationJson(string value)
        {
            ConfigurationJson = value ?? string.Empty;
        }

        public static void SetLevelsJson(string value)
        {
            LevelsJson = value ?? string.Empty;
        }
    }
}
