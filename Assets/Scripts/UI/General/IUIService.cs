public interface IUIService : IService
{
    T ShowPopup<T>(bool shouldPlaySound = true) where T : class, IPresenter, new();
    T ShowPopup<T, TData>(TData data) where T : class, IPresenterWithData<TData>, new();
    void HidePopup<T>() where T : class, IPresenter;
    void HideAllPopups();
}
