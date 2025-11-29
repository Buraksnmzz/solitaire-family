using UnityEngine;

namespace Loading
{
	[CreateAssetMenu]
	public class BootData : ScriptableObject
	{
		public string configurationJson;
		public string levelsJson;
	}
}
