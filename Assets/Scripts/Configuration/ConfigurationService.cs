using System.Collections.Generic;
using Core.Scripts.Helper;
using Goal;
using Newtonsoft.Json;
using UnityEngine;

namespace Configuration
{
	public class ConfigurationService : IConfigurationService
	{
		public ConfigurationService(string bootDataConfigurationJson)
		{
			Initialize(bootDataConfigurationJson);
		}

		public ConfigLayoutModel LayoutConfig { get; private set; }
		public GoalConfigModel GoalConfig { get; private set; }

		[System.Serializable]
		private class GameConfigurationJson
		{
			[JsonProperty("layoutId")]
			public int LayoutId { get; set; }

			[JsonProperty("goalConfig")]
			public GoalConfigModel GoalConfig { get; set; }
		}

		public int GetLevelGoal(int levelIndex, int columnCount)
		{
			var overrideGoal = GetOverrideLevelGoal(levelIndex);
			if (overrideGoal.HasValue)
			{
				return overrideGoal.Value;
			}

			return GetProbabilisticGoal(columnCount);
		}
		
		public void Initialize(string rawJson)
		{
			var root = JsonConvert.DeserializeObject<GameConfigurationJson>(rawJson);
			LayoutConfig = new ConfigLayoutModel
			{
				Layout = root.LayoutId
			};
			GoalConfig = root.GoalConfig;
		}

		private int? GetOverrideLevelGoal(int levelIndex)
		{
			if (GoalConfig == null || GoalConfig.LevelGoals == null)
			{
				return null;
			}

			for (var i = 0; i < GoalConfig.LevelGoals.Count; i++)
			{
				var levelGoalModel = GoalConfig.LevelGoals[i];
				if (levelGoalModel.Level == levelIndex)
				{
					return levelGoalModel.Goal;
				}
			}

			return null;
		}

		private int GetProbabilisticGoal(int columnCount)
		{
			if (GoalConfig == null || GoalConfig.ColumnGoals == null)
			{
				return 0;
			}

			var columnKey = columnCount.ToString();
			if (!GoalConfig.ColumnGoals.TryGetValue(columnKey, out var probabilityList) || probabilityList == null || probabilityList.Count == 0)
			{
				return 0;
			}

			var totalProbability = 0;
			for (var i = 0; i < probabilityList.Count; i++)
			{
				totalProbability += probabilityList[i].Probability;
			}

			if (totalProbability <= 0)
			{
				return probabilityList[0].Goal;
			}

			var randomValue = Random.Range(1, totalProbability + 1);
			var cumulative = 0;
			for (var i = 0; i < probabilityList.Count; i++)
			{
				cumulative += probabilityList[i].Probability;
				if (randomValue <= cumulative)
				{
					return probabilityList[i].Goal;
				}
			}

			return probabilityList[probabilityList.Count - 1].Goal;
		}
	}
}