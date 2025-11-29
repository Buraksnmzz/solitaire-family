/// <summary>
/// Base class for presenters that require data initialization
/// </summary>
/// <typeparam name="TView">Type of view this presenter controls</typeparam>
/// <typeparam name="TData">Type of data needed by the presenter</typeparam>
public abstract class BasePresenterWithData<TView, TData> : BasePresenter<TView>, IPresenterWithData<TData>
    where TView : IView
{
    protected TData Data { get; private set; }

    public virtual void SetData(TData data)
    {
        Data = data;
        OnDataSet();
    }

    /// <summary>
    /// Called after data is set
    /// </summary>
    protected virtual void OnDataSet() { }

    public override void Cleanup()
    {
        base.Cleanup();
        Data = default;
    }
}
