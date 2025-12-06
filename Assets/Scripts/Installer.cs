
using Configuration;
using Core.Scripts.Helper;
using Core.Scripts.Services;
using Levels;
using Services;
using UnityEngine;
using Loading;
using UI.MainMenu;

public class Installer : MonoBehaviour
{
    [Header("UI References")]
    public Transform uiRoot;
	[SerializeField] private BootData bootData;

    void Awake()
    {
        Vibration.Init();
    }

    private void Start()
    {
        InstallServices();
        SetOptimalFrameRate();
        YoogoLabManager.ShowBanner();
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
        ServiceLocator.Register<ISavedDataService>(new SavedDataService());
        ServiceLocator.Register<IEventDispatcherService>(new EventDispatcherService());
        ServiceLocator.Register<IStyleService>(new StyleService());
        ServiceLocator.Register<ISoundService>(new SoundService());
        ServiceLocator.Register<IHapticService>(new HapticService());
        ServiceLocator.Register<IUIService>(new UIService(uiRoot));
        ServiceLocator.Register<ISnapshotService>(new SnapshotService());
        ServiceLocator.Register<ICollectibelService>(new CollectibleService());
        ServiceLocator.Register<ILevelGeneratorService>(new LevelGeneratorService(bootData.levelsJson));
        ServiceLocator.Register<IConfigurationService>(new ConfigurationService(bootData.configurationJson));
        ServiceLocator.Register<IUndoService>(new UndoService());
        
        ServiceLocator.GetService<IUIService>().ShowPopup<MainMenuPresenter>();
        // if (PlayerPrefs.GetInt(StringConstants.IsTutorialShown) == 0)
        // {
        //     ServiceLocator.GetService<IUIService>().ShowPopup<TutorialGamePresenter>();
        // }
    }
    
}
