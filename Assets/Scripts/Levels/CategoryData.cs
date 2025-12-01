using System.Collections.Generic;
using Newtonsoft.Json;

namespace Levels
{
    [System.Serializable]
    public class CategoryData
    {
        [JsonProperty("TopicName")]
        public string name; 

        [JsonProperty("TopicType")]
        public CategoryType type;

        [JsonProperty("ContentValues")]
        public List<string> contentValues; 
    }
}
