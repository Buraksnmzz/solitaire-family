# MVP-Based UI System

This document explains the Model-View-Presenter (MVP) UI system implemented in the project.

## Overview

The UI system follows the MVP pattern:
- **Model**: Data layer (using the existing `IModel` interface)
- **View**: UI components (MonoBehaviour-based)
- **Presenter**: Logic that controls the view (non-MonoBehaviour)

The system is designed to:
- Separate UI logic from UI presentation
- Make UI code more testable
- Simplify UI development and maintenance

## Key Components

### Core Interfaces

- **IView**: Interface for UI views
- **IPresenter**: Interface for presenters that control views
- **IUIService**: Service for managing UI views and presenters

### Base Classes

- **BaseView**: Base class for all UI views
- **BasePresenter<TView>**: Base class for presenters
- **BasePresenterWithData<TView, TData>**: Base class for presenters that need initialization data

### Attributes

- **ViewAttribute**: Links a presenter to its view prefab

## How To Use

### Creating a New UI Screen

1. **Create the View**:
   - Create a new class inheriting from `BaseView`
   - Implement UI components and event handling
   - Override `PresenterType` to return the correct presenter type

   ```csharp
   public class MyView : BaseView
   {
       [SerializeField] private Button okButton;
       
       public override Type PresenterType => typeof(MyPresenter);
       
       protected override void Awake()
       {
           base.Awake();
           okButton.onClick.AddListener(OnOkClicked);
       }
       
       private void OnOkClicked()
       {
           var presenter = ServiceLocator.GetService<IUIService>()
               .ShowPopup<MyPresenter>();
           presenter.OnOkClicked();
       }
   }
   ```

2. **Create the Presenter**:
   - Create a new class inheriting from `BasePresenter<TView>` or `BasePresenterWithData<TView, TData>`
   - Add the `[View("path/to/prefab")]` attribute to link it to the view prefab
   - Implement the presenter logic

   ```csharp
   [View("UI/MyPopup")]
   public class MyPresenter : BasePresenter<MyView>
   {
       public override void Initialize(IView view)
       {
           base.Initialize(view);
           // Initialization logic
       }
       
       public void OnOkClicked()
       {
           // Handle OK button click
           ServiceLocator.GetService<IUIService>().HidePopup<MyPresenter>();
       }
   }
   ```

3. **Create the Prefab**:
   - Create a prefab for your view
   - Attach the view component
   - Set up the UI elements
   - Save the prefab to the Resources folder at the path specified in the `[View]` attribute

### Using the UI Service

```csharp
// Show a popup
var presenter = ServiceLocator.GetService<IUIService>().ShowPopup<MyPresenter>();

// Show a popup with data
var data = new MyData();
var presenter = ServiceLocator.GetService<IUIService>().ShowPopup<MyPresenterWithData, MyData>(data);

// Hide a popup
ServiceLocator.GetService<IUIService>().HidePopup<MyPresenter>();

// Hide all popups
ServiceLocator.GetService<IUIService>().HideAllPopups();
```

## Best Practices

1. **Keep Views Thin**: Views should only handle UI rendering and event forwarding.

2. **Keep Presenters Independent**: Presenters should not reference MonoBehaviour or UnityEngine directly except for Debug logging.

3. **Use Models for Data**: Store UI state in model classes that implement `IModel`.

4. **Properly Dispose Resources**: Override the `Cleanup` method in presenters to clean up resources.

5. **Use Events for Communication**: Use the `IEventDispatcherService` for cross-popup communication.

## Example

See the `SettingsView` and `SettingsPresenter` classes for a complete example of how to implement a UI screen using the MVP pattern.
