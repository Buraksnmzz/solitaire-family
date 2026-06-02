using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace ServicesPackage
{
    public class ConsoleLogger : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI logText;
        public ScrollRect scrollRect;
        public Button clearButton;

        private string logCache = "";

        private void Awake()
        {
            logText.text = "";
            Application.logMessageReceived += HandleLog;
            if (clearButton != null)
            {
                clearButton.onClick.AddListener(ClearLogs);
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            string formattedLog;

            switch (type)
            {
                case LogType.Warning:
                    formattedLog = $"<color=#FFD700>[Warning]</color> {logString}\n";
                    break;
                case LogType.Error:
                case LogType.Exception:
                    formattedLog = $"<color=#FF6347>[Error]</color> {logString}\n";
                    break;
                default:
                    formattedLog = $"<color=#FFFFFF>[Log]</color> {logString}\n";
                    break;
            }

            logCache += formattedLog;
            logText.text = logCache;

            StartCoroutine(ScrollToBottom());
        }


        private IEnumerator ScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        public void ClearLogs()
        {
            logCache = "";
            logText.text = "";
        }
    }
}
