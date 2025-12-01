
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Levels
{
    [System.Serializable]
    public class LevelData
    {
        [JsonProperty("LevelID")]
        public int levelId;

        [JsonProperty("Columns")]
        public int columns;

        [JsonProperty("Categories")]
        public List<CategoryData> categories;
    }
}
