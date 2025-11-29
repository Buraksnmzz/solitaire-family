using System.Collections.Generic;

namespace Levels
{
    [System.Serializable]
    public class CategoryData
    {
        public string name; 
        public CategoryType type;
        public List<string> contentValues; 
    }

    public enum CategoryType
    {
        Text,
        Image
    }
}
