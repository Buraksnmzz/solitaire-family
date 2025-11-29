using System;

/// <summary>
/// Attribute to specify the view prefab path for a presenter
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ViewAttribute : Attribute
{
    public string PrefabPath { get; }

    /// <summary>
    /// Create a new ViewAttribute
    /// </summary>
    /// <param name="prefabPath">Path to the view prefab in Resources folder</param>
    public ViewAttribute(string prefabPath)
    {
        PrefabPath = prefabPath;
    }
}
