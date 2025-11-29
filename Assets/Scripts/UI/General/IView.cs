using System;

/// <summary>
/// Base interface for all UI views in the MVP pattern
/// </summary>
public interface IView
{
    /// <summary>
    /// Show the view with optional animation
    /// </summary>
    void Show();

    /// <summary>
    /// Hide the view with optional animation
    /// </summary>
    void Hide();
}
