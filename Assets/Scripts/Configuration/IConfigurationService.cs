namespace Configuration
{
    public interface IConfigurationService: IService
    {
        Goal.GoalConfigModel GoalConfig { get; }
        void Initialize(string rawJson);
        int GetLevelGoal(int levelIndex, int columnCount);
    }
}