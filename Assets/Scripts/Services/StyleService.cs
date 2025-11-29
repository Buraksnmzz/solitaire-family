using UnityEngine;

public class StyleService : IStyleService
{
    private StyleHelper _styleHelper;

    public StyleHelper StyleHelper
    {
        get
        {
            if (_styleHelper == null)
            {
                _styleHelper = Resources.Load<StyleHelper>("StyleHelper");
                if (_styleHelper == null)
                {
                    Debug.LogError("StyleHelper asset not found in Resources folder! Please create one.");
                }
            }
            return _styleHelper;
        }
    }

    public void Dispose()
    {

    }
}
