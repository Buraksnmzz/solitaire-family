using Core.Scripts.Helper;
using Goal;
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
			public int layoutId;
			public GoalConfigModel goalConfig;
		}

		public void Initialize(string rawJson)
		{
			var root = JsonUtility.FromJson<GameConfigurationJson>(rawJson);
			LayoutConfig = new ConfigLayoutModel
			{
				Layout = root.layoutId
			};
			GoalConfig = root.goalConfig;
		}
	}
}