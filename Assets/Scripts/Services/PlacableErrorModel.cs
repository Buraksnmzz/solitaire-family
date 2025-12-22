using System;
using System.Collections.Generic;

[Serializable]
public class PlacableErrorModel : IModel
{
    public Dictionary<string, int> errorShowCounts = new Dictionary<string, int>();
}
