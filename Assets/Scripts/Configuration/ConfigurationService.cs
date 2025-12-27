using System;
using Collectible;
using Goal;
using Newtonsoft.Json;
using Random = UnityEngine.Random;

namespace Configuration
{
	public class ConfigurationService : IConfigurationService
	{
		public ConfigurationService(string bootDataConfigurationJson)
		{
			Initialize(bootDataConfigurationJson);
		}

		public GoalConfigModel GoalConfig { get; private set; }
		private int[] _rateUsTriggerLevels;

		[System.Serializable]
		private class GameConfigurationJson
		{
			[JsonProperty("layoutId")]
			public int LayoutId { get; set; }

			[JsonProperty("backgroundImageId")]
			public int BackgroundImageId { get; set; }

			[JsonProperty("goalConfig")]
			public GoalConfigModel GoalConfig { get; set; }

			[JsonProperty("totalHintGiven")]
			public int TotalHintGiven { get; set; }

			[JsonProperty("totalJokerGiven")]
			public int TotalJokerGiven { get; set; }

			[JsonProperty("totalUndoGiven")]
			public int TotalUndoGiven { get; set; }

			[JsonProperty("totalCoinGiven")]
			public int TotalCoinGiven { get; set; }

			[JsonProperty("earnedCoinAtLevelEnd")]
			public int EarnedCoinAtLevelEnd { get; set; }

			[JsonProperty("earnedCoinPerMoveLeft")]
			public int EarnedCoinPerMoveLeft { get; set; }

			[JsonProperty("hintCost")]
			public int HintCost { get; set; }

			[JsonProperty("undoCost")]
			public int UndoCost { get; set; }

			[JsonProperty("jokerCost")]
			public int JokerCost { get; set; }

			[JsonProperty("extraMovesCost")]
			public int ExtraMovesCost { get; set; }
			
			[JsonProperty("dailyAdsWatchAmount")]
			public int DailyAdsWatchAmount { get; set; }

			[JsonProperty("rateUsTrigger")]
			public string RateUsTrigger { get; set; }
			
			[JsonProperty("rewardedVideoCoinAmount")]
			public int RewardedVideoCoinAmount { get; set; }
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
			if (root == null)
			{
				GoalConfig = null;
				return;
			}
			GoalConfig = root.GoalConfig;
			InitializeCollectibleModel(root);
			InitializeConfigModel(root);
		}

		private void InitializeCollectibleModel(GameConfigurationJson root)
		{
			var savedDataService = ServiceLocator.GetService<ISavedDataService>();
			if (savedDataService.HasData<CollectibleModel>())
			{
				return;
			}

			var collectibleModel = savedDataService.LoadData<CollectibleModel>();
			collectibleModel.totalCoins = root.TotalCoinGiven;
			collectibleModel.totalHints = root.TotalHintGiven;
			collectibleModel.totalJokers = root.TotalJokerGiven;
			collectibleModel.totalUndo = root.TotalUndoGiven;
			savedDataService.SaveData(collectibleModel);
		}

		private void InitializeConfigModel(GameConfigurationJson root)
		{
			var savedDataService = ServiceLocator.GetService<ISavedDataService>();
			var gameConfigModel = savedDataService.LoadData<GameConfigModel>();
			gameConfigModel.earnedCoinAtLevelEnd = root.EarnedCoinAtLevelEnd;
			gameConfigModel.earnedCoinPerMoveLeft = root.EarnedCoinPerMoveLeft;
			gameConfigModel.hintCost = root.HintCost;
			gameConfigModel.undoCost = root.UndoCost;
			gameConfigModel.jokerCost = root.JokerCost;
			gameConfigModel.layout = root.LayoutId;
			gameConfigModel.backgroundImageId = root.BackgroundImageId;
			gameConfigModel.extraMovesCost = root.ExtraMovesCost;
			gameConfigModel.dailyAdsWatchAmount = root.DailyAdsWatchAmount;
			_rateUsTriggerLevels = ParseRateUsTrigger(root.RateUsTrigger);
			gameConfigModel.rateUsTriggerLevels = _rateUsTriggerLevels;
			gameConfigModel.rewardedVideoCoinAmount = root.RewardedVideoCoinAmount;
			savedDataService.SaveData(gameConfigModel);
		}

		private int[] ParseRateUsTrigger(string rateUsTrigger)
		{
			if (string.IsNullOrEmpty(rateUsTrigger))
			{
				return Array.Empty<int>();
			}

			var parts = rateUsTrigger.Split(',');
			var result = new int[parts.Length];
			for (var i = 0; i < parts.Length; i++)
			{
				int level;
				if (int.TryParse(parts[i], out level))
				{
					result[i] = level;
				}
			}

			return result;
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