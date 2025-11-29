/// <summary>
/// Interface for presenters that need data to initialize
/// </summary>
/// <typeparam name="T">Type of data needed by the presenter</typeparam>
public interface IPresenterWithData<T> : IPresenter
{
    /// <summary>
    /// Set data for the presenter
    /// </summary>
    /// <param name="data">Data to initialize the presenter with</param>
    void SetData(T data);
}
