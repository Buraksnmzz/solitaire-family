using Collectible;
using Configuration;
using Core.Scripts.Services;
using IAP;
using Levels;
using Services;
using Services.Hint;
using UnityEngine;
using Loading;
using Services.Drag;
using UI.MainMenu;
using UI.Gameplay;
using UI.Shop;

public class Installer : MonoBehaviour
{
    [Header("UI References")]
    public Transform uiRoot;

    void Awake()
    {
        Vibration.Init();
    }

    private void Start()
    {
        InstallServices();
        SetOptimalFrameRate();
    }

    private void SetOptimalFrameRate()
    {
        var memoryMb = SystemInfo.systemMemorySize;
        var processorCount = SystemInfo.processorCount;

        var isLowEndDevice = memoryMb <= 2100 || processorCount <= 4;

        if (isLowEndDevice)
        {
            Application.targetFrameRate = 30;
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;
        }
    }

    private void InstallServices()
    {
        Debug.Log("Installing Services");
        ServiceLocator.Register<IPlacableErrorPersistenceService>(new PlacableErrorPersistenceService());
        ServiceLocator.Register<IEventDispatcherService>(new EventDispatcherService());
        ServiceLocator.Register<IStyleService>(new StyleService());
        ServiceLocator.Register<ISoundService>(new SoundService());
        ServiceLocator.Register<IHapticService>(new HapticService());
        ServiceLocator.Register<IUIService>(new UIService(uiRoot));
        ServiceLocator.Register<ISnapshotService>(new SnapshotService());
        ServiceLocator.Register<ICollectibelService>(new CollectibleService());
        ServiceLocator.Register<ILevelGeneratorService>(new LevelGeneratorService(BootCache.LevelsJson));
        ServiceLocator.Register<ILevelMapLanguageSyncService>(new LevelMapLanguageSyncService());
        ServiceLocator.Register<IConfigurationService>(new ConfigurationService(BootCache.ConfigurationJson));
        ServiceLocator.Register<IDailyAdsService>(new DailyAdsService());
        ServiceLocator.Register<IUndoService>(new UndoService());
        ServiceLocator.Register<IDragStateService>(new DragStateService());
        ServiceLocator.Register<IHintService>(new HintService());
        ServiceLocator.Register<ITutorialMoveRestrictionService>(new TutorialMoveRestrictionService());
        ServiceLocator.Register<IIAPService>(new IAPService());
        ServiceLocator.Register<IAdsService>(new AdsService());

        var uiService = ServiceLocator.GetService<IUIService>();
        if (PlayerPrefs.GetInt(StringConstants.IsTutorialShown) == 0)
        {
            uiService.ShowPopup<TutorialGamePresenter>();
        }
        else
        {
            uiService.ShowPopup<MainMenuPresenter>();
        }
    }

}
