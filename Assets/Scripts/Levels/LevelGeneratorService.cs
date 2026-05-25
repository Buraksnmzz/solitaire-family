using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Levels
{
	public class LevelGeneratorService : ILevelGeneratorService
	{
		ISavedDataService _savedDataService;
		private readonly Dictionary<GameMode, LevelMap> _levelMaps = new Dictionary<GameMode, LevelMap>();

		public LevelGeneratorService(string classicLevelJson, string mathLevelJson)
		{
			_savedDataService = ServiceLocator.GetService<ISavedDataService>();
			ParseLevelsJson(GameMode.Classic, classicLevelJson);
			ParseLevelsJson(GameMode.Math, mathLevelJson);
		}


		public LevelData GetCurrentLevelData(GameMode gameMode)
		{
			var currentLevel = _savedDataService.GetModel<LevelProgressModel>().GetCurrentLevelIndex(gameMode);
			return GetLevelMap(gameMode).levelsList[currentLevel];
		}

		public LevelData GetLevelData(GameMode gameMode, int levelIndex)
		{
			return GetLevelMap(gameMode).levelsList[levelIndex];
		}

		public int GetLevelColumnCount(GameMode gameMode, int levelIndex)
		{
			return GetLevelMap(gameMode).levelsList[levelIndex].columns;
		}

		public int GetLevelCategoryCardCount(GameMode gameMode, int levelIndex)
		{
			return GetLevelMap(gameMode).levelsList[levelIndex].categories.Count;
		}

		public LevelMap ParseLevelsJson(GameMode gameMode, string levelJson)
		{
			var levels = JsonConvert.DeserializeObject<List<LevelData>>(levelJson) ?? new List<LevelData>();
			var map = new LevelMap
			{
				levelsList = levels
			};

			_levelMaps[gameMode] = map;
			return map;
		}

		private LevelMap GetLevelMap(GameMode gameMode)
		{
			return _levelMaps.TryGetValue(gameMode, out var levelMap) ? levelMap : new LevelMap { levelsList = new List<LevelData>() };
		}
	}
}