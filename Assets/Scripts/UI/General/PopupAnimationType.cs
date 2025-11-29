using UnityEngine;

/// <summary>
/// Defines the type of animation to use for showing/hiding a popup
/// </summary>
public enum PopupAnimationType
{
    /// <summary>
    /// Simple fade animation for both background and panel
    /// </summary>
    Fade,

    /// <summary>
    /// Panel moves up from bottom of screen
    /// </summary>
    MoveUp,

    /// <summary>
    /// Panel moves down from top of screen
    /// </summary>
    MoveDown,

    /// <summary>
    /// Panel scales up/down from center
    /// </summary>
    Scale
}
