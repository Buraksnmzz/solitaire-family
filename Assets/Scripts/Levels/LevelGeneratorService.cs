using System.Collections.Generic;
using Newtonsoft.Json;

namespace Levels
{
    public class LevelGeneratorService : ILevelGeneratorService
    {
	    ISavedDataService _savedDataService;
        public LevelGeneratorService(string levelJson)
        {
	        _savedDataService = ServiceLocator.GetService<ISavedDataService>();
            ParseLevelsJson(levelJson);
        }

        public LevelData GetCurrentLevelData()
		{
			var currentLevel = _savedDataService.GetModel<LevelProgressModel>().CurrentLevelIndex;
			return CurrentLevelMap.levelsList[currentLevel];
		}

		public LevelData GetLevelData(int levelIndex)
		{
			return CurrentLevelMap.levelsList[levelIndex];
		}

		public int GetLevelColumnCount(int levelIndex)
		{
			return CurrentLevelMap.levelsList[levelIndex].columns;
		}

		private LevelMap CurrentLevelMap { get; set; }

		public LevelMap ParseLevelsJson(string levelJson)
		{
			var levels = JsonConvert.DeserializeObject<List<LevelData>>(levelJson) ?? new List<LevelData>();

			var map = new LevelMap
			{
				levelsList = levels
			};

			CurrentLevelMap = map;
			return map;
		}
    }
}