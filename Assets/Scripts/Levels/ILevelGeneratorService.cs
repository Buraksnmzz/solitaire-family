namespace Levels
{
    public interface ILevelGeneratorService: IService
    {
        public LevelMap ParseLevelsJson(string levelJson);
    }
}