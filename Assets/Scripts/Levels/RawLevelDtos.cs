using System.Collections.Generic;

namespace Levels
{
	[System.Serializable]
	public class RawLevelData
	{
		public int LevelID;
		public int Columns;
		public List<RawCategoryData> Categories;
	}

	[System.Serializable]
	public class RawCategoryData
	{
		public string TopicName;
		public string TopicType;
		public List<string> ContentValues;
	}

	[System.Serializable]
	public class RawLevelMap
	{
		public List<RawLevelData> levelsList;
	}
}
