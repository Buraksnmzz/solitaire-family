namespace Levels
{
    public interface ILevelGeneratorService : IService
    {
        public LevelMap ParseLevelsJson(GameMode gameMode, string levelJson);
        public LevelData GetCurrentLevelData(GameMode gameMode);
        public LevelData GetLevelData(GameMode gameMode, int levelIndex);
        public int GetLevelColumnCount(GameMode gameMode, int levelIndex);
        int GetLevelCategoryCardCount(GameMode gameMode, int levelIndex);
    }
}