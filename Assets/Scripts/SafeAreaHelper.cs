using UnityEngine;

namespace Core.Scripts.Helper
{
    public class SafeAreaHelper : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (_lastSafeArea != Screen.safeArea)
            {
                _lastSafeArea = Screen.safeArea;
                Refresh();
            }
        }

        private void Refresh()
        {
            var safeArea = Screen.safeArea;
            float screenHeight = Screen.height;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            float bannerHeightPercent = YoogoLabManager.GetBannerHeightPercent();
            float bannerHeight = screenHeight * bannerHeightPercent;
            anchorMin.y = (safeArea.y + bannerHeight) / screenHeight;
            anchorMin.x /= Screen.width;
            anchorMax.x /= Screen.width;
            anchorMax.y = (safeArea.y + safeArea.height) / screenHeight;
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    }
}