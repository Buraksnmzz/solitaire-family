
public abstract class BasePresenter<TView> : IPresenter where TView : IView
{
    protected TView View { get; private set; }

    public virtual void Initialize(IView view)
    {
        if (view is TView typedView)
        {
            View = typedView;
            OnInitialize();
        }
        else
        {
            throw new System.InvalidOperationException($"View type mismatch. Expected {typeof(TView)}, got {view.GetType()}");
        }
    }

    /// <summary>
    /// Called after the view is set
    /// </summary>
    protected virtual void OnInitialize() { }

    public virtual void ViewShown() { }

    public virtual void ViewHidden() { }

    public virtual void Cleanup()
    {
        // Clean up resources
        View = default;
    }
}
