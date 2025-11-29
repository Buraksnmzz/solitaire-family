using System.Collections.Generic;

namespace Goal
{
    public class GoalConfigModel
    {
        public Dictionary<string, List<GoalProbabilityModel>> ColumnGoals;
        public List<LevelGoalModel> LevelGoals;
    }
}