using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace ServicesPackage.Keystore
{
    public class YoogalabPackageBuildSettings : EditorWindow
    {
        // ===== STATE =====

        string keystorePath;
        string keystorePass;
        string aliasName;
        string aliasPass;

        string versionName;
        int versionCode;

        string applicationId;

        AndroidSdkVersions minSdk;
        AndroidSdkVersions targetSdk;

        ScriptingImplementation scriptingBackend;
        AndroidArchitecture architectures;
        AndroidBuildSystem buildSystem;

        bool buildAppBundle;
        bool showUnitySplash;

        const int PLAY_REQUIRED_TARGET_API = 34;

        bool iosAutomaticSigning;
        iOSTargetDevice iosTargetDevice;

        // ===== PREF KEYS =====

        const string KEY_KEYSTORE_PATH = "Services.Keystore.Path";
        const string KEY_ALIAS = "Services.Keystore.Alias";
        const string KEY_VERSION = "Services.Version";
        const string KEY_SPLASH = "Services.Splash";

        const string KEY_IOS_AUTO_SIGN = "Services.iOS.AutoSign";
        const string KEY_IOS_TARGET = "Services.iOS.Target";

        // ===== PLATFORM =====

        BuildTargetGroup CurrentGroup =>
            EditorUserBuildSettings.selectedBuildTargetGroup;

        string Key(string baseKey) => $"{baseKey}_{CurrentGroup}";

        // ===== INIT =====

        [MenuItem("Services/YoogaLabPackage/Services Build/Package Build Settings")]
        static void Open()
        {
            GetWindow<YoogalabPackageBuildSettings>("Package Build Settings");
        }

        void OnEnable()
        {
            LoadCurrent();
            LoadPrefs();
            SyncVersionCode();
        }

        // ===== UI =====

        void OnGUI()
        {
            DrawWarnings();

            GUILayout.Label($"Platform: {CurrentGroup}", EditorStyles.boldLabel);

            GUILayout.Space(10);

            if (CurrentGroup == BuildTargetGroup.Android)
                DrawKeystoreSection();

            DrawApplicationSection();
            DrawVersionSection();
            DrawSplashToggle();


            if (CurrentGroup == BuildTargetGroup.iOS)
                DrawiOSSection();

            if (CurrentGroup == BuildTargetGroup.Android)
                DrawAndroidSection();

            GUILayout.Space(20);

            if (GUILayout.Button(new GUIContent("Apply Settings", "Write values into PlayerSettings"), GUILayout.Height(35)))
            {
                if (CurrentGroup != BuildTargetGroup.Android || ValidateKeystore())
                    Apply();
            }
        }

        void DrawiOSSection()
        {
            GUILayout.Label("iOS Configuration", EditorStyles.boldLabel);

            bool newAuto = EditorGUILayout.ToggleLeft(
                new GUIContent("Automatic Signing", "Use Xcode automatic provisioning"),
                iosAutomaticSigning);

            if (newAuto != iosAutomaticSigning)
            {
                iosAutomaticSigning = newAuto;
                SavePrefs();
            }

            var newTarget = (iOSTargetDevice)EditorGUILayout.EnumPopup(
                new GUIContent("Target SDK", "Device or Simulator"),
                iosTargetDevice);

            if (newTarget != iosTargetDevice)
            {
                iosTargetDevice = newTarget;
                SavePrefs();
            }

            GUILayout.Space(10);
        }

        void DrawKeystoreSection()
        {
            GUILayout.Label("Keystore", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            keystorePath = EditorGUILayout.TextField(
                new GUIContent("Keystore Path", "Signing keystore used for Android builds."),
                keystorePath);

            if (GUILayout.Button(new GUIContent("Browse", "Select .keystore or .jks file"), GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFilePanel("Select Keystore", "", "keystore,jks");
                if (!string.IsNullOrEmpty(path))
                {
                    keystorePath = path;
                    SavePrefs();
                }
            }

            EditorGUILayout.EndHorizontal();

            keystorePass = EditorGUILayout.PasswordField(
                new GUIContent("Keystore Password"),
                keystorePass);

            aliasName = EditorGUILayout.TextField(
                new GUIContent("Key Alias"),
                aliasName);

            aliasPass = EditorGUILayout.PasswordField(
                new GUIContent("Alias Password"),
                aliasPass);

            if (GUILayout.Button(new GUIContent("Validate Keystore")))
                ValidateKeystore();

            GUILayout.Space(15);
        }

        void DrawApplicationSection()
        {
            GUILayout.Label("Application", EditorStyles.boldLabel);

            string newId = EditorGUILayout.TextField(
                new GUIContent("Package Name"),
                applicationId);

            if (newId != applicationId)
            {
                applicationId = newId;
                SavePrefs();
            }

            GUILayout.Space(15);
        }

        void DrawVersionSection()
        {
            GUILayout.Label("Versioning", EditorStyles.boldLabel);

            string newVersion = EditorGUILayout.TextField(
                new GUIContent("Bundle Version"),
                versionName);

            if (newVersion != versionName)
            {
                versionName = newVersion;
                SyncVersionCode();
                SavePrefs();
            }

            EditorGUILayout.LabelField(
                new GUIContent("Bundle Version Code"),
                versionCode.ToString());

            string nextPatch = GetNextPatchVersion();
            string nextMinor = GetNextMinorVersion();
            string nextMajor = GetNextMajorVersion();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent($"Patch → {nextPatch}")))
                IncrementPatch();

            if (GUILayout.Button(new GUIContent($"Minor → {nextMinor}")))
                IncrementMinor();

            if (GUILayout.Button(new GUIContent($"Major → {nextMajor}")))
                IncrementMajor();

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(15);
        }

        void DrawSplashToggle()
        {
            bool newVal = EditorGUILayout.ToggleLeft(
                new GUIContent("Show Unity Splash Screen"),
                showUnitySplash);

            if (newVal != showUnitySplash)
            {
                showUnitySplash = newVal;
                SavePrefs();
            }

            GUILayout.Space(10);
        }

        void DrawAndroidSection()
        {
            GUILayout.Label("Android API", EditorStyles.boldLabel);

            minSdk = (AndroidSdkVersions)EditorGUILayout.EnumPopup("Min SDK", minSdk);
            targetSdk = (AndroidSdkVersions)EditorGUILayout.EnumPopup("Target SDK", targetSdk);

            GUILayout.Space(15);

            GUILayout.Label("Build Configuration", EditorStyles.boldLabel);

            scriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", scriptingBackend);
            architectures = (AndroidArchitecture)EditorGUILayout.EnumFlagsField("Architectures", architectures);
            buildSystem = (AndroidBuildSystem)EditorGUILayout.EnumPopup("Build System", buildSystem);
            buildAppBundle = EditorGUILayout.Toggle("Build App Bundle (AAB)", buildAppBundle);
        }

        // ===== APPLY =====

        void Apply()
        {
            var group = CurrentGroup;

            PlayerSettings.bundleVersion = versionName;
            PlayerSettings.SetApplicationIdentifier(group, applicationId);
            PlayerSettings.SetScriptingBackend(group, scriptingBackend);
            PlayerSettings.SplashScreen.show = showUnitySplash;

            if (!showUnitySplash)
                UnityEngine.Debug.LogWarning("Unity splash requires Plus/Pro to disable.");

            if (group == BuildTargetGroup.Android)
                ApplyAndroid();
            else if (group == BuildTargetGroup.iOS)
                ApplyiOS();

            SavePrefs();

            UnityEngine.Debug.Log($"Applied settings for {group}");
        }

        void ApplyAndroid()
        {
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePass;
            PlayerSettings.Android.keyaliasName = aliasName;
            PlayerSettings.Android.keyaliasPass = aliasPass;

            PlayerSettings.Android.bundleVersionCode = versionCode;
            PlayerSettings.Android.minSdkVersion = minSdk;
            PlayerSettings.Android.targetSdkVersion = targetSdk;
            PlayerSettings.Android.targetArchitectures = architectures;

            EditorUserBuildSettings.androidBuildSystem = buildSystem;
            EditorUserBuildSettings.buildAppBundle = buildAppBundle;
        }

        void ApplyiOS()
        {
            PlayerSettings.iOS.buildNumber = versionCode.ToString();

            PlayerSettings.iOS.appleEnableAutomaticSigning = iosAutomaticSigning;
            PlayerSettings.iOS.targetDevice = iosTargetDevice;
        }

        // ===== VALIDATION =====

        bool ValidateKeystore()
        {
            if (string.IsNullOrEmpty(keystorePath) || !File.Exists(keystorePath))
                return Fail("Keystore file not found.");

            if (string.IsNullOrEmpty(aliasName))
                return Fail("Alias missing.");

            if (string.IsNullOrEmpty(keystorePass))
                return Fail("Password missing.");

            string keytool = GetKeytoolPath();

            if (string.IsNullOrEmpty(keytool) || !File.Exists(keytool))
                return Fail($"keytool not found:\n{keytool}");

            try
            {
                var process = new Process();

                process.StartInfo.FileName = keytool;
                process.StartInfo.Arguments = $"-list -keystore \"{keystorePath}\" -storepass \"{keystorePass}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (process.ExitCode != 0)
                    return Fail(string.IsNullOrEmpty(error) ? output : error);

                if (!output.Contains(aliasName))
                    return Fail($"Alias '{aliasName}' not found.");

                UnityEngine.Debug.Log("Keystore validation succeeded.");
                return true;
            }
            catch (Exception e)
            {
                return Fail(e.Message);
            }
        }

        bool Fail(string msg)
        {
            EditorUtility.DisplayDialog("Validation Error", msg, "OK");
            return false;
        }

        // ===== JDK =====

        string GetKeytoolPath()
        {
            string jdk = GetUnityJdkPath();

            if (string.IsNullOrEmpty(jdk))
                return null;

#if UNITY_EDITOR_WIN
            string exe = "keytool.exe";
#else
            string exe = "keytool";
#endif

            string[] paths =
            {
                Path.Combine(jdk, "bin", exe),
                Path.Combine(jdk, "Contents", "Home", "bin", exe),
                Path.Combine(jdk, "Home", "bin", exe)
            };

            foreach (var p in paths)
            {
                if (File.Exists(p))
                    return p;
            }

            UnityEngine.Debug.LogError($"[KEYTOOL] Not found in:\n{string.Join("\n", paths)}");
            return null;
        }

        string GetUnityJdkPath()
        {
            var type =
                Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions") ??
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("UnityEditor.Android.AndroidExternalToolsSettings"))
                    .FirstOrDefault(t => t != null);

            if (type == null)
            {
                UnityEngine.Debug.LogError("[KEYTOOL] AndroidExternalToolsSettings type not found");
                return null;
            }

            var prop = type.GetProperty("jdkRootPath",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            string path = prop?.GetValue(null) as string;

            UnityEngine.Debug.Log($"[KEYTOOL] Unity JDK Path → {path}");

            return path;
        }

        // ===== LOAD =====

        void LoadCurrent()
        {
            keystorePath = PlayerSettings.Android.keystoreName;
            aliasName = PlayerSettings.Android.keyaliasName;

            versionName = PlayerSettings.bundleVersion;
            applicationId = PlayerSettings.GetApplicationIdentifier(CurrentGroup);

            scriptingBackend = PlayerSettings.GetScriptingBackend(CurrentGroup);

            minSdk = PlayerSettings.Android.minSdkVersion;
            targetSdk = PlayerSettings.Android.targetSdkVersion;

            architectures = PlayerSettings.Android.targetArchitectures;

            buildSystem = EditorUserBuildSettings.androidBuildSystem;
            buildAppBundle = EditorUserBuildSettings.buildAppBundle;

            showUnitySplash = PlayerSettings.SplashScreen.show;

            if (CurrentGroup == BuildTargetGroup.Android)
                versionCode = PlayerSettings.Android.bundleVersionCode;
            else if (CurrentGroup == BuildTargetGroup.iOS)
                int.TryParse(PlayerSettings.iOS.buildNumber, out versionCode);

            if (CurrentGroup == BuildTargetGroup.iOS)
            {
                iosAutomaticSigning = PlayerSettings.iOS.appleEnableAutomaticSigning;
                iosTargetDevice = PlayerSettings.iOS.targetDevice;
            }
        }

        void LoadPrefs()
        {
            keystorePath = EditorPrefs.GetString(Key(KEY_KEYSTORE_PATH), keystorePath);
            aliasName = EditorPrefs.GetString(Key(KEY_ALIAS), aliasName);
            versionName = EditorPrefs.GetString(Key(KEY_VERSION), versionName);
            showUnitySplash = EditorPrefs.GetBool(Key(KEY_SPLASH), showUnitySplash);

            iosAutomaticSigning = EditorPrefs.GetBool(Key(KEY_IOS_AUTO_SIGN), iosAutomaticSigning);
            iosTargetDevice = (iOSTargetDevice)EditorPrefs.GetInt(Key(KEY_IOS_TARGET), (int)iosTargetDevice);
        }

        void SavePrefs()
        {
            EditorPrefs.SetString(Key(KEY_KEYSTORE_PATH), keystorePath ?? "");
            EditorPrefs.SetString(Key(KEY_ALIAS), aliasName ?? "");
            EditorPrefs.SetString(Key(KEY_VERSION), versionName ?? "");
            EditorPrefs.SetBool(Key(KEY_SPLASH), showUnitySplash);

            EditorPrefs.SetBool(Key(KEY_IOS_AUTO_SIGN), iosAutomaticSigning);
            EditorPrefs.SetInt(Key(KEY_IOS_TARGET), (int)iosTargetDevice);
        }
        // ===== VERSION =====

        void SyncVersionCode()
        {
            ParseVersion(out int major, out int minor, out int patch);
            versionCode = major * 10000 + minor * 100 + patch;
        }

        string GetNextPatchVersion()
        {
            ParseVersion(out int major, out int minor, out int patch);
            return $"{major}.{minor}.{patch + 1}";
        }

        string GetNextMinorVersion()
        {
            ParseVersion(out int major, out int minor, out _);
            return $"{major}.{minor + 1}.0";
        }

        string GetNextMajorVersion()
        {
            ParseVersion(out int major, out _, out _);
            return $"{major + 1}.0.0";
        }

        void IncrementPatch()
        {
            versionName = GetNextPatchVersion();
            SyncVersionCode();
            SavePrefs();
        }

        void IncrementMinor()
        {
            versionName = GetNextMinorVersion();
            SyncVersionCode();
            SavePrefs();
        }

        void IncrementMajor()
        {
            versionName = GetNextMajorVersion();
            SyncVersionCode();
            SavePrefs();
        }

        void ParseVersion(out int major, out int minor, out int patch)
        {
            major = 1; minor = 0; patch = 0;

            if (string.IsNullOrEmpty(versionName))
                return;

            var parts = versionName.Split('.');
            if (parts.Length > 0) int.TryParse(parts[0], out major);
            if (parts.Length > 1) int.TryParse(parts[1], out minor);
            if (parts.Length > 2) int.TryParse(parts[2], out patch);
        }

        // ===== WARNINGS =====

        void DrawWarnings()
        {
            if (CurrentGroup == BuildTargetGroup.Android)
            {
                if ((architectures & AndroidArchitecture.ARM64) == 0)
                    EditorGUILayout.HelpBox("ARM64 architecture is required by Google Play.", MessageType.Warning);

                if ((int)targetSdk < PLAY_REQUIRED_TARGET_API)
                    EditorGUILayout.HelpBox("Target API may be too low for Play Store requirements.", MessageType.Warning);
            }
        }
    }
}