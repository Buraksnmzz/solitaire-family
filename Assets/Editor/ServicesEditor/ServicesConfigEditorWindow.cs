#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace ServicesPackage
{
    public class ServicesConfigEditorWindow : EditorWindow
    {
        private const string configPath = "Assets/Resources/YoogaLabPluginConfig.json";
        private ServicesConfigData configData;
        private Vector2 scrollPos;

        [MenuItem("Services/Services Config Editor")]
        public static void ShowWindow()
        {
            GetWindow<ServicesConfigEditorWindow>("Services Config Editor");
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Services Config", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (configData == null)
            {
                EditorGUILayout.HelpBox("No config loaded. Click 'Create New Config' to start fresh.", MessageType.Warning);

                if (GUILayout.Button("Create New Config"))
                {
                    configData = new ServicesConfigData();
                }

                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            DrawHeader("Adjust App Tokens");
            configData.adjustAppTokenAndroid = EditorGUILayout.TextField("Adjust Token Android", configData.adjustAppTokenAndroid);
            configData.adjustAppTokenIOS = EditorGUILayout.TextField("Adjust Token iOS", configData.adjustAppTokenIOS);

            DrawHeader("Ad Unit IDs - Android");
            configData.adBannerKeyAndroid = EditorGUILayout.TextField("Banner Ad", configData.adBannerKeyAndroid);
            configData.adInterstitialKeyAndroid = EditorGUILayout.TextField("Interstitial Ad", configData.adInterstitialKeyAndroid);
            configData.adRewardedKeyAndroid = EditorGUILayout.TextField("Rewarded Ad", configData.adRewardedKeyAndroid);

            DrawHeader("Ad Unit IDs - iOS");
            configData.adBannerKeyIOS = EditorGUILayout.TextField("Banner Ad", configData.adBannerKeyIOS);
            configData.adInterstitialKeyIOS = EditorGUILayout.TextField("Interstitial Ad", configData.adInterstitialKeyIOS);
            configData.adRewardedKeyIOS = EditorGUILayout.TextField("Rewarded Ad", configData.adRewardedKeyIOS);

            DrawHeader("Revenue Keys");
            configData.revenueAPIKeyAndroid = EditorGUILayout.TextField("Revenue Key Android", configData.revenueAPIKeyAndroid);
            configData.revenueAPIKeyIOS = EditorGUILayout.TextField("Revenue Key iOS", configData.revenueAPIKeyIOS);

            DrawHeader("Google Mobile Ads App IDs");
            configData.googleMobileAdsAppIdAndroid = EditorGUILayout.TextField("AdMob App ID Android", configData.googleMobileAdsAppIdAndroid);
            configData.googleMobileAdsAppIdIOS = EditorGUILayout.TextField("AdMob App ID iOS", configData.googleMobileAdsAppIdIOS);

            DrawHeader("Tracking Usage Description (iOS)");
            configData.iosTrackingUsageDescription = EditorGUILayout.TextField("Usage Description", configData.iosTrackingUsageDescription);

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Config"))
                SaveConfig();

            if (GUILayout.Button("Export"))
                ExportConfig();

            if (GUILayout.Button("Import"))
                ImportConfig();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader(string title)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private void LoadConfig()
        {
            if (!File.Exists(configPath))
            {
                configData = null;
                return;
            }

            string json = File.ReadAllText(configPath);
            configData = JsonUtility.FromJson<ServicesConfigData>(json);
        }

        private void SaveConfig()
        {
            string json = JsonUtility.ToJson(configData, true);
            File.WriteAllText(configPath, json);
            AssetDatabase.Refresh();

            Debug.Log("[PluginConfigEditor] Config saved to " + configPath);
        }

        private void ExportConfig()
        {
            string path = EditorUtility.SaveFilePanel("Export Plugin Config", "", "YoogaLabPluginConfig.json", "json");

            if (!string.IsNullOrEmpty(path))
            {
                string json = JsonUtility.ToJson(configData, true);
                File.WriteAllText(path, json);
                Debug.Log("[PluginConfigEditor] Exported config to: " + path);
            }
        }

        private void ImportConfig()
        {
            string path = EditorUtility.OpenFilePanel("Import Plugin Config", "", "json");

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                string json = File.ReadAllText(path);
                ServicesConfigData imported = JsonUtility.FromJson<ServicesConfigData>(json);

                if (imported != null)
                {
                    configData = imported;
                    SaveConfig(); // overwrite local asset copy too
                    Debug.Log("[PluginConfigEditor] Imported config from: " + path);
                }
                else
                {
                    Debug.LogError("[PluginConfigEditor] Failed to deserialize imported JSON.");
                }
            }
        }
    }
}

#endif