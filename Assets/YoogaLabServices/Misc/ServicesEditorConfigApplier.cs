#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ServicesPackage
{
    [InitializeOnLoad]
    public static class ServicesEditorConfigApplier
    {
        static ServicesEditorConfigApplier()
        {
            ApplyGoogleMobileAdsConfig();
        }

        public static void ApplyGoogleMobileAdsConfig()
        {
            TextAsset configText = Resources.Load<TextAsset>("YoogaLabPluginConfig");
            if (configText == null)
            {
                Debug.LogWarning("[PluginEditorConfigApplier] Config JSON not found in Resources.");
                return;
            }

            ServicesConfigData config = JsonUtility.FromJson<ServicesConfigData>(configText.text);
            if (config == null)
            {
                Debug.LogError("[PluginEditorConfigApplier] Failed to parse config.");
                return;
            }

            string path = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (settings == null)
            {
                Debug.LogError("[PluginEditorConfigApplier] GoogleMobileAdsSettings.asset not found!");
                return;
            }

            SerializedObject serializedSettings = new SerializedObject(settings);

            bool didChange = false;

            SerializedProperty androidIdProp = serializedSettings.FindProperty("adMobAndroidAppId");
            if (string.IsNullOrEmpty(androidIdProp.stringValue))
            {
                androidIdProp.stringValue = config.googleMobileAdsAppIdAndroid;
                didChange = true;
                Debug.Log("[PluginEditorConfigApplier] Set AdMob Android App ID.");
            }

            SerializedProperty iosIdProp = serializedSettings.FindProperty("adMobIOSAppId");
            if (string.IsNullOrEmpty(iosIdProp.stringValue))
            {
                iosIdProp.stringValue = config.googleMobileAdsAppIdIOS;
                didChange = true;
                Debug.Log("[PluginEditorConfigApplier] Set AdMob iOS App ID.");
            }

            SerializedProperty trackingProp = serializedSettings.FindProperty("userTrackingUsageDescription");
            if (string.IsNullOrEmpty(trackingProp.stringValue))
            {
                trackingProp.stringValue = config.iosTrackingUsageDescription;
                didChange = true;
                Debug.Log("[PluginEditorConfigApplier] Set iOS Tracking Usage Description.");
            }

            if (didChange)
            {
                serializedSettings.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                Debug.Log("[PluginEditorConfigApplier] Google Ads config updated via JSON.");
            }
            else
            {
                Debug.Log("[PluginEditorConfigApplier] All AdMob settings already set. Skipping overwrite.");
            }
        }
    }
}

#endif