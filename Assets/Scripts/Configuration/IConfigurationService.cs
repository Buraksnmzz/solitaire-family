namespace Configuration
{
    public interface IConfigurationService : IService
    {
        Goal.GoalConfigModel GoalConfig { get; }
        Goal.GoalConfigModel MathGoalConfig { get; }
        void Initialize(string rawJson);
        int GetLevelGoal(Levels.GameMode gameMode, int levelIndex, int columnCount);
        //int[] GetRateUsTriggerLevels();
    }
}