namespace Configuration
{
    public interface IConfigurationService: IService
    {
        ConfigLayoutModel LayoutConfig { get; }
        Goal.GoalConfigModel GoalConfig { get; }
        void Initialize(string rawJson);
        int GetLevelGoal(int levelIndex, int columnCount);
    }
}