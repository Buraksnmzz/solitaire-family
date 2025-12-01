using System;
using System.Collections.Generic;

namespace Goal
{
    [Serializable]
    public class GoalConfigModel
    {
        public Dictionary<string, List<GoalProbabilityModel>> ColumnGoals;
        public List<LevelGoalModel> LevelGoals;
    }
}