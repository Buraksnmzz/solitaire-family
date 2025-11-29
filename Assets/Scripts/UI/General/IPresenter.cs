/// <summary>
/// Base interface for all presenters in the MVP pattern
/// </summary>
public interface IPresenter
{
    /// <summary>
    /// Initialize the presenter with the corresponding view
    /// </summary>
    /// <param name="view">The view controlled by this presenter</param>
    void Initialize(IView view);

    /// <summary>
    /// Called when the view is shown
    /// </summary>
    void ViewShown();

    /// <summary>
    /// Called when the view is hidden
    /// </summary>
    void ViewHidden();

    /// <summary>
    /// Clean up any resources used by this presenter
    /// </summary>
    void Cleanup();
}
