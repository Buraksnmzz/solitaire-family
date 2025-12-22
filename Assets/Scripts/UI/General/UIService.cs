using System;
using System.Collections.Generic;
using Core.Scripts.Services;
using UnityEngine;
using Object = UnityEngine.Object;

public class UIService : IUIService
{
    private readonly Dictionary<Type, IPresenter> _activePresenters = new Dictionary<Type, IPresenter>();
    private readonly Dictionary<Type, IView> _activeViews = new Dictionary<Type, IView>();
    private readonly Dictionary<Type, string> _presenterViewPathCache = new Dictionary<Type, string>();

    private Transform _uiRoot;
    private ISoundService _soundService;
    private IHapticService _hapticService;

    public UIService(Transform uiRoot)
    {
        _uiRoot = uiRoot;
        _soundService = ServiceLocator.GetService<ISoundService>();
        _hapticService = ServiceLocator.GetService<IHapticService>();
    }

    public T ShowPopup<T>(bool shouldPlaySound = true) where T : class, IPresenter, new()
    {
        var presenterType = typeof(T);

        if (_activePresenters.TryGetValue(presenterType, out var existingPresenter))
        {
            if (_activeViews.TryGetValue(presenterType, out var existingView))
            {
                existingView.Show();
                ((T)existingPresenter).ViewShown();
            }

            return (T)existingPresenter;
        }

        var view = CreateViewForPresenter(presenterType);
        if (view == null)
            return null;

        var presenter = new T();
        presenter.Initialize(view);
        _activePresenters[presenterType] = presenter;
        _activeViews[presenterType] = view;

        view.Show();
        presenter.ViewShown();
        return presenter;
    }

    public T ShowPopup<T, TData>(TData data) where T : class, IPresenterWithData<TData>, new()
    {
        var presenter = ShowPopup<T>();
        if (presenter != null)
        {
            presenter.SetData(data);
        }

        return presenter;
    }

    public void HidePopup<T>() where T : class, IPresenter
    {
        var presenterType = typeof(T);

        if (_activePresenters.TryGetValue(presenterType, out var presenter) &&
            _activeViews.TryGetValue(presenterType, out var view))
        {
            view.Hide();
            presenter.ViewHidden();
        }
    }

    public void HideAllPopups()
    {
        foreach (var presenterPair in new Dictionary<Type, IPresenter>(_activePresenters))
        {
            if (_activeViews.TryGetValue(presenterPair.Key, out var view))
            {
                view.Hide();
                presenterPair.Value.ViewHidden();
            }
        }
    }

    private IView CreateViewForPresenter(Type presenterType)
    {
        var viewType = GetViewTypeForPresenter(presenterType);
        if (viewType == null)
        {
            return null;
        }

        var all = Resources.LoadAll<GameObject>("UI");
        foreach (var prefab in all)
        {
            if (prefab != null && prefab.GetComponent(viewType) != null)
            {
                var resourcePath = "UI/" + prefab.name;
                var v = InstantiateViewPrefab(prefab, viewType);
                if (v != null)
                {
                    _presenterViewPathCache[presenterType] = resourcePath;
                    return v;
                }
            }
        }
        return null;
    }

    private Type GetViewTypeForPresenter(Type presenterType)
    {
        var currentType = presenterType;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(BasePresenter<>))
            {
                var genericArguments = currentType.GetGenericArguments();
                if (genericArguments.Length == 1)
                {
                    return genericArguments[0];
                }
            }

            currentType = currentType.BaseType;
        }

        return null;
    }

    private IView InstantiateViewPrefab(GameObject prefab, Type viewType)
    {
        var viewInstance = Object.Instantiate(prefab, _uiRoot);
        var view = viewInstance.GetComponent(viewType) as IView;
        if (view == null)
        {
            Object.Destroy(viewInstance);
            return null;
        }
        return view;
    }

    public void Dispose()
    {
        foreach (var presenter in _activePresenters.Values)
        {
            presenter.Cleanup();
        }

        foreach (var view in _activeViews.Values)
        {
            if (view is MonoBehaviour viewComponent)
            {
                Object.Destroy(viewComponent.gameObject);
            }
        }

        _activePresenters.Clear();
        _activeViews.Clear();
        _presenterViewPathCache.Clear();
    }
}
