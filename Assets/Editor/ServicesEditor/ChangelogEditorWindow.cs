#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ServicesPackage
{
    public class ChangelogEditorWindow : EditorWindow
    {
        private string changelogPath = "Assets/YoogaLabServices/changelog.txt";
        private Vector2 scrollPos;

        private string newVersion = "";
        private string newEntry = "";

        private Dictionary<string, string> changelogEntries = new Dictionary<string, string>();
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        [MenuItem("Services/Changelog Viewer")]
        public static void ShowWindow()
        {
            GetWindow<ChangelogEditorWindow>("Changelog");
        }

        private void OnEnable()
        {
            LoadChangelog();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Add New Entry", EditorStyles.boldLabel);
            newVersion = EditorGUILayout.TextField("Version", newVersion);
            EditorGUILayout.LabelField("Changelog Details:");
            newEntry = EditorGUILayout.TextArea(newEntry, GUILayout.Height(80));

            if (GUILayout.Button("Add Changelog Entry"))
            {
                if (!string.IsNullOrEmpty(newVersion) && !string.IsNullOrEmpty(newEntry))
                {
                    string autoDate = System.DateTime.Now.ToString("yyyy-MM-dd");
                    AddEntry(newVersion, autoDate, newEntry);
                    SaveChangelog();
                    LoadChangelog(); // Reload updated data

                    //Clear all fields and remove focus from textarea
                    newVersion = "";
                    newEntry = "";
                    GUI.FocusControl(null);
                }
                else
                {
                    EditorUtility.DisplayDialog("Missing Fields", "Please fill in both version and log details.", "OK");
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Changelog History", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Reverse the order so latest version is shown first
            foreach (var version in GetReversedKeys(changelogEntries))
            {
                if (!foldouts.ContainsKey(version))
                    foldouts[version] = false;

                foldouts[version] = EditorGUILayout.Foldout(foldouts[version], version, true);
                if (foldouts[version])
                {
                    EditorGUILayout.HelpBox(changelogEntries[version], MessageType.None);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private List<string> GetReversedKeys(Dictionary<string, string> dict)
        {
            var keys = new List<string>(dict.Keys);
            keys.Reverse(); // Newest first
            return keys;
        }

        private void LoadChangelog()
        {
            changelogEntries.Clear();
            if (!File.Exists(changelogPath))
                return;

            string[] lines = File.ReadAllLines(changelogPath);
            string currentVersion = "";
            string currentContent = "";

            foreach (string line in lines)
            {
                if (line.StartsWith("Changelog — Version"))
                {
                    if (!string.IsNullOrEmpty(currentVersion))
                    {
                        changelogEntries[currentVersion] = currentContent.Trim();
                        currentContent = "";
                    }
                    currentVersion = line.Trim();
                }
                else if (!string.IsNullOrWhiteSpace(currentVersion))
                {
                    currentContent += line + "\n";
                }
            }

            if (!string.IsNullOrEmpty(currentVersion))
            {
                changelogEntries[currentVersion] = currentContent.Trim();
            }
        }

        private void AddEntry(string version, string date, string log)
        {
            string formatted = $"\nChangelog — Version {version}\nDate: {date}\n\n{log}\n\n-------------------------------------------------------------------------";
            File.AppendAllText(changelogPath, formatted + "\n");

            UpdatePackageJsonVersion(version);
        }

        private void UpdatePackageJsonVersion(string newVersion)
        {
            string jsonPath = "Assets/YoogaLabPlugin/package.json";
            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning("[ChangelogEditor] package.json not found.");
                return;
            }

            try
            {
                string json = File.ReadAllText(jsonPath);
                PackageInfo pkg = JsonUtility.FromJson<PackageInfo>(json);
                pkg.version = newVersion;

                string updatedJson = JsonUtility.ToJson(pkg, true);
                File.WriteAllText(jsonPath, updatedJson);
                AssetDatabase.Refresh();

                Debug.Log($"[ChangelogEditor] Updated package.json version to {newVersion}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ChangelogEditor] Failed to update package.json: {e.Message}");
            }
        }

        [System.Serializable]
        private class PackageInfo
        {
            public string name;
            public string version;
            public string displayName;
            public string description;
        }

        private void SaveChangelog()
        {
            AssetDatabase.Refresh();
        }
    }
}


#endif