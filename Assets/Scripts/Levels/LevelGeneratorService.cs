using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
    public class LevelGeneratorService : ILevelGeneratorService
    {
        public LevelGeneratorService(string levelJson)
        {
            ParseLevelsJson(levelJson);
        }
		public LevelMap CurrentLevelMap { get; private set; }

        public LevelMap ParseLevelsJson(string levelJson)
        {
            var wrapper = JsonUtility.FromJson<LevelMap>(levelJson);
            var map = new LevelMap
            {
                levelsList = new List<LevelData>()
            };

            if (wrapper == null || wrapper.levelsList == null)
            {
				CurrentLevelMap = map;
                return map;
            }

            foreach (var levelData in wrapper.levelsList)
            {
                if (levelData.categories == null)
                {
                    levelData.categories = new List<CategoryData>();
                }
                if (levelData.categories.Count > 0)
                {
                    for (var i = 0; i < levelData.categories.Count; i++)
                    {
                        var category = levelData.categories[i];
                        if (category.contentValues == null)
                        {
                            category.contentValues = new List<string>();
                        }
                    }
                }
                map.levelsList.Add(levelData);
            }

			CurrentLevelMap = map;

            return map;
        }
    }
}