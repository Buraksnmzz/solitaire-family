
using UnityEngine;

namespace Services
{
    public class HapticService : IHapticService
    {
        private readonly ISavedDataService _savedDataService;
        
        public HapticService()
        {
            Debug.Log("HapticService initialized");
            _savedDataService = ServiceLocator.GetService<ISavedDataService>();
        }
       
        public void HapticLow()
        {
            if(_savedDataService.GetModel<SettingsModel>().IsHapticOn)
            {
                Vibration.VibratePop();
            }
        }
        
        public void HapticMedium()
        {
            if(_savedDataService.GetModel<SettingsModel>().IsHapticOn)
            {
                Vibration.VibratePeek();
            }
        }
        
        public void HapticHigh()
        {
            if(_savedDataService.GetModel<SettingsModel>().IsHapticOn)
            {
                Vibration.Vibrate();
            }
        }

        public void Dispose()
        {
            
        }
    }
}