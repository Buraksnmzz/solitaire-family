using System;

/// <summary>
/// Signal to show or hide a popup
/// </summary>
public struct PopupSignal : ISignal
{
    /// <summary>
    /// The type of the view to show or hide
    /// </summary>
    public Type View { get; }

    /// <summary>
    /// Whether to show (true) or hide (false) the popup
    /// </summary>
    public bool IsShow { get; }

    /// <summary>
    /// Create a popup signal
    /// </summary>
    /// <param name="view">The type of the view</param>
    /// <param name="isShow">Whether to show (true) or hide (false)</param>
    public PopupSignal(Type view, bool isShow)
    {
        View = view;
        IsShow = isShow;
    }
}
