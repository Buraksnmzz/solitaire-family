#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ServicesPackage
{
    [InitializeOnLoad]
    public class ServicesWelcomeWindow : EditorWindow
    {
        private const string DontShowKey = "YoogoLabServices_Welcome_DontShow";
        private const string ChangelogPath = "Assets/YoogaLabServices/changelog.txt";
        private const string PackageJsonPath = "Assets/YoogaLabServices/package.json";

        private static bool dontShowAgain = false;
        private Vector2 scroll;

        private string version = "Unknown";
        private string lastChangelogEntry = "No changelog found.";

        static ServicesWelcomeWindow()
        {
            EditorApplication.update += TryShowWindowAfterLoad;
        }
        private static void TryShowWindowAfterLoad()
        {
            EditorApplication.update -= TryShowWindowAfterLoad;

            if (!EditorPrefs.GetBool(DontShowKey, false))
            {
                ShowWindow();
            }
        }

        [MenuItem("Services/Services Welcome")]
        public static void ShowWindow()
        {
            var window = GetWindow<ServicesWelcomeWindow>(true, "Services Setup", true);

            Vector2 fixedSize = new Vector2(650, 640);

            window.minSize = fixedSize;
            window.maxSize = fixedSize;

            window.LoadInfo();
            window.Show();
        }


        private void LoadInfo()
        {
            // Read version from package.json
            if (File.Exists(PackageJsonPath))
            {
                string json = File.ReadAllText(PackageJsonPath);
                var pkg = JsonUtility.FromJson<PackageInfo>(json);
                version = pkg?.version ?? "Unknown";
            }

            if (File.Exists(ChangelogPath))
            {
                string[] lines = File.ReadAllLines(ChangelogPath);

                foreach (string line in lines)
                {
                    if (line.StartsWith("Changelog — Version"))
                    {
                        lastChangelogEntry = line + "\n";
                    }
                    else if (line.StartsWith("-"))
                    {
                        lastChangelogEntry += line + "\n";
                    }
                    else if (line.StartsWith("----"))
                    {
                        break; // end of this entry
                    }
                }

                if (string.IsNullOrWhiteSpace(lastChangelogEntry))
                    lastChangelogEntry = "Could not find a valid changelog entry.";
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(15);
            GUILayout.Label("Welcome to the YoogoLab Services Plugin!", EditorStyles.boldLabel);
            GUILayout.Label("Thanks for installing! Here's how to get started:", EditorStyles.wordWrappedLabel);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Plugin Version", version, EditorStyles.boldLabel);

            GUILayout.Space(5);
            EditorGUILayout.LabelField("Latest Changelog Entry", EditorStyles.boldLabel);

            // Fixed height scroll for changelog
            Vector2 changelogScroll = Vector2.zero;
            changelogScroll = EditorGUILayout.BeginScrollView(changelogScroll, GUILayout.Height(100));

            GUIStyle changelogStyle = new GUIStyle(EditorStyles.helpBox)
            {
                wordWrap = true,
                fontSize = 12,
                richText = false,
                alignment = TextAnchor.UpperLeft
            };

            EditorGUILayout.LabelField(lastChangelogEntry, changelogStyle);
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Installation Instructions", EditorStyles.boldLabel);

            // HYPERLINK
            GUIStyle linkStyle = new GUIStyle(EditorStyles.label);
            linkStyle.normal.textColor = new Color(0.2f, 0.6f, 1f); // light blue
            linkStyle.hover.textColor = Color.cyan;
            linkStyle.richText = true;

            GUILayout.Space(2);
            Rect linkRect = GUILayoutUtility.GetRect(new GUIContent("You can also check the instructions here"), linkStyle);
            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);

            if (GUI.Button(linkRect, "You can also check the instructions here", linkStyle))
            {
                Application.OpenURL("https://docs.google.com/document/d/1AFMbpfeAVrWeWXgZBwPhetV4yRyGwvCS1nBox79-rKE/edit?tab=t.0");
            }

            GUILayout.Space(5);

            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(300));

            // Save original GUI colors
            Color originalBg = GUI.backgroundColor;
            Color originalContent = GUI.contentColor;

            GUI.backgroundColor = Color.black;
            GUI.contentColor = Color.white;

            GUIStyle codeStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                fontStyle = FontStyle.Normal,
                fontSize = 12,
                normal =
            {
                textColor = Color.white,
                background = Texture2D.blackTexture
            },
                padding = new RectOffset(10, 10, 10, 10),
                richText = false,
                font = EditorStyles.label.font
            };

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.TextArea(@"• Create/import the game's analytics config through: YoogoLab > Plugin Config Editor.  
      → Assign app IDs specific to the injected game.
      → Navigate to Assets/GoogleMobileAds/Settings and validate that the App IDs per store are set, as well as the Privacy Declaration!

    • Validate and ensure mobile notification settings are set:

      Android:
        - Reschedule on device restart
        - Schedule at exact time: Exact when available
        - Use custom activity
          → Name: com.unity3d.player.UnityPlayerActivity

      iOS:
        - Enable Push Notifications
        - All others OFF

    • Services:
      → Connect the game to the Unity account.
      → Enable In-App Purchases via Services tab.

    • Ads Mediation:
      → Open AppLovin > Integration Manager.
      → Assign Android & iOS App IDs in each mediated network.

    • Firebase:
      → Add google-services.json (Android) & GoogleService-Info.plist (iOS).

    • Bootstrap:
      → Drag the Bootstrapper scene into the Hierarchy.
      → Edit the bootstrapper GameObject as needed.

    • Validate the Plugin Manager (YoogaLabPlugin GameObject):
      → Check that all dependencies are assigned.
      → Toggle 'Sandbox Mode' & 'Can Log' to enable logging if desired.", codeStyle);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = originalBg;
            GUI.contentColor = originalContent;

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            dontShowAgain = EditorGUILayout.ToggleLeft("Don't show this again", dontShowAgain);

            if (GUILayout.Button("Close"))
            {
                if (dontShowAgain)
                    EditorPrefs.SetBool(DontShowKey, true);
                Close();
            }

            GUILayout.Space(5);
        }


        [System.Serializable]
        private class PackageInfo
        {
            public string name;
            public string version;
            public string displayName;
            public string description;
        }
    }
}

#endif