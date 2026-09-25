using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Levels
{
	public class LevelGeneratorService : ILevelGeneratorService
	{
		private const int LoopStartLevelIndex = 21;

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
			var levelMap = GetLevelMap(gameMode);
			return levelMap.levelsList[GetPlayableLevelIndex(levelMap, currentLevel)];
		}

		public LevelData GetLevelData(GameMode gameMode, int levelIndex)
		{
			var levelMap = GetLevelMap(gameMode);
			return levelMap.levelsList[GetPlayableLevelIndex(levelMap, levelIndex)];
		}

		public int GetLevelColumnCount(GameMode gameMode, int levelIndex)
		{
			var levelMap = GetLevelMap(gameMode);
			return levelMap.levelsList[GetPlayableLevelIndex(levelMap, levelIndex)].columns;
		}

		public int GetLevelCategoryCardCount(GameMode gameMode, int levelIndex)
		{
			var levelMap = GetLevelMap(gameMode);
			return levelMap.levelsList[GetPlayableLevelIndex(levelMap, levelIndex)].categories.Count;
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

		private int GetPlayableLevelIndex(LevelMap levelMap, int levelIndex)
		{
			var lastPlayableLevelIndex = levelMap.levelsList.Count - 1;
			if (levelIndex <= lastPlayableLevelIndex)
				return levelIndex;

			var loopLength = lastPlayableLevelIndex - LoopStartLevelIndex + 1;
			if (loopLength <= 0)
				return lastPlayableLevelIndex;

			return LoopStartLevelIndex + (levelIndex - lastPlayableLevelIndex - 1) % loopLength;
		}
	}
}