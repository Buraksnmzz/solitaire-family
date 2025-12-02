namespace Levels
{
    public interface ILevelGeneratorService: IService
    {
        public LevelMap ParseLevelsJson(string levelJson);
        public LevelData GetCurrentLevelData();
        public LevelData GetLevelData(int levelIndex);
        public int GetLevelColumnCount(int levelIndex);
        int GetLevelCategoryCardCount(int levelIndex);
    }
}