#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

namespace ServicesPackage
{
    public class ServicesConfigInitializer
    {
        public ServicesConfigData ConfigData { get; private set; }

        public ServicesConfigInitializer()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            TextAsset configText = Resources.Load<TextAsset>("YoogaLabPluginConfig");
            if (configText != null)
            {
                ConfigData = JsonUtility.FromJson<ServicesConfigData>(configText.text);
                ServicesLogger.Log("[PluginConfigInitializer] Config loaded successfully.");

            }
            else
            {
                ServicesLogger.LogError("[PluginConfigInitializer] Plugin config not found in Resources!");
            }
        }
    }
}
