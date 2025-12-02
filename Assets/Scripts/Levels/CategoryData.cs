using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace Levels
{
    [System.Serializable]
    public class CategoryData
    {
        [JsonProperty("TopicName")]
        public string name; 

        [FormerlySerializedAs("type")] [JsonProperty("TopicType")]
        public CardCategoryType cardCategoryType;

        [JsonProperty("ContentValues")]
        public List<string> contentValues; 
    }
}
