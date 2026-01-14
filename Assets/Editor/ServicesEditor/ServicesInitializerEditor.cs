#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ServicesPackage
{
    [CustomEditor(typeof(ServicesInitializer))]
    public class ServicesInitializerEditor : UnityEditor.Editor
    {
        private SerializedProperty canLogProp;

        private string pluginVersion = "Unknown";
        private bool showInfo = true;

        private void OnEnable()
        {
            canLogProp = serializedObject.FindProperty("canLog");

            string path = "Assets/YoogaLabServices/package.json";
            if (System.IO.File.Exists(path))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(path);
                    var pkg = JsonUtility.FromJson<PackageInfo>(json);
                    pluginVersion = pkg.version;
                }
                catch
                {
                    pluginVersion = "Error Reading Version";
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("YoogaLab Services", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Plugin Version:", pluginVersion);

            EditorGUILayout.Space();

            if (GUILayout.Button("Changelog"))
                ChangelogEditorWindow.ShowWindow();

            EditorGUILayout.Space();

            showInfo = EditorGUILayout.Foldout(showInfo, "Debug Settings", true);
            if (showInfo)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(canLogProp);

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            DrawDefaultInspectorExcept(nameof(canLogProp));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultInspectorExcept(string exclude)
        {
            SerializedProperty prop = serializedObject.GetIterator();
            bool expanded = true;

            while (prop.NextVisible(expanded))
            {
                expanded = false;

                if (prop.name == "m_Script")
                    continue;

                if (prop.name == exclude)
                    continue;

                if (prop.name == "canLog")
                    continue;

                if (prop.displayName == "Debug")
                    continue;

                EditorGUILayout.PropertyField(prop, true);
            }
        }


        [System.Serializable]
        private class PackageInfo { public string version; }
    }
}


#endif