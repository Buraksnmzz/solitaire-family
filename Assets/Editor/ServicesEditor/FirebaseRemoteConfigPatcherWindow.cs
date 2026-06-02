using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FirebaseRemoteConfigPatcherWindow : EditorWindow
{
    private string inputJsonPath;
    private string outputJsonPath;

    [SerializeField]
    private List<RCParameterDefinition> parameters = new();

    private Vector2 scroll;

    [MenuItem("Services/YoogaLabPackage/Remote Config Patcher")]
    public static void Open()
    {
        GetWindow<FirebaseRemoteConfigPatcherWindow>("RC Patcher");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Firebase Remote Config Patcher", EditorStyles.boldLabel);
        GUILayout.Space(10);
        DrawInputSection();
        GUILayout.Space(10);
        DrawParametersSection();
        GUILayout.Space(10);
        DrawExportSection();
    }

    private void DrawInputSection()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Base Config", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.TextField(inputJsonPath);

        if (GUILayout.Button("Browse", GUILayout.Width(100)))
        {
            string path = EditorUtility.OpenFilePanel(
                "Select Firebase Remote Config JSON",
                "",
                "json"
            );

            if (!string.IsNullOrEmpty(path))
            {
                inputJsonPath = path;
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private void DrawParametersSection()
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Parameters", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Parameter", GUILayout.Width(140)))
        {
            parameters.Add(new RCParameterDefinition());
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < parameters.Count; i++)
        {
            DrawParameter(parameters[i], i);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    private void DrawParameter(RCParameterDefinition param, int index)
    {
        bool remove = false;

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField($"Element {index + 1}", EditorStyles.boldLabel);

        GUI.backgroundColor = Color.red;

        if (GUILayout.Button("X", GUILayout.Width(30)))
        {
            remove = true;
        }

        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        param.key = EditorGUILayout.TextField("Key", param.key);

        param.valueType = (RCValueType)EditorGUILayout.EnumPopup("Type", param.valueType);

        param.value = EditorGUILayout.TextField("Value", param.value);

        EditorGUILayout.EndVertical();

        if (remove)
        {
            parameters.RemoveAt(index);
            GUIUtility.ExitGUI();
        }
    }

    private void DrawExportSection()
    {
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Patch And Export", GUILayout.Height(40)))
        {
            PatchAndExport();
        }

        EditorGUILayout.EndVertical();
    }

    private void PatchAndExport()
    {
        if (string.IsNullOrEmpty(inputJsonPath))
        {
            EditorUtility.DisplayDialog(
                "Missing Input",
                "Please select a Firebase Remote Config JSON file.",
                "OK"
            );

            return;
        }

        if (!File.Exists(inputJsonPath))
        {
            EditorUtility.DisplayDialog(
                "Invalid File",
                "Selected JSON file does not exist.",
                "OK"
            );

            return;
        }

        try
        {
            string json = File.ReadAllText(inputJsonPath);

            int parametersIndex = json.IndexOf("\"parameters\"");

            if (parametersIndex < 0)
            {
                Debug.LogError("[RC PATCHER] Could not locate parameters object.");
                return;
            }

            int openingBrace = json.IndexOf("{", parametersIndex);

            if (openingBrace < 0)
            {
                Debug.LogError("[RC PATCHER] Invalid parameters object.");
                return;
            }

            int closingBrace = FindMatchingBrace(json, openingBrace);

            if (closingBrace < 0)
            {
                Debug.LogError("[RC PATCHER] Failed finding parameters closing brace.");
                return;
            }

            string parametersContent =
                json.Substring(openingBrace + 1, closingBrace - openingBrace - 1);

            List<string> additions = new();

            int addedCount = 0;

            foreach (var param in parameters)
            {
                if (string.IsNullOrWhiteSpace(param.key))
                    continue;

                if (parametersContent.Contains($"\"{param.key}\""))
                {
                    Debug.Log($"[RC PATCHER] Skipping existing parameter: {param.key}");
                    continue;
                }

                additions.Add(CreateFirebaseParameterText(param));

                addedCount++;

                Debug.Log($"[RC PATCHER] Added parameter: {param.key}");
            }

            if (additions.Count > 0)
            {
                string insertion =
                    ",\n" + string.Join(",\n", additions);

                json = json.Insert(closingBrace, insertion);
            }

            string savePath = EditorUtility.SaveFilePanel(
                "Save Patched Remote Config",
                Path.GetDirectoryName(inputJsonPath),
                "remote_config_patched.json",
                "json"
            );

            if (string.IsNullOrEmpty(savePath))
                return;

            File.WriteAllText(savePath, json);

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Success",
                $"Patched JSON exported successfully.\n\nAdded {addedCount} parameters.",
                "OK"
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"[RC PATCHER] ERROR:\n{e}");

            EditorUtility.DisplayDialog(
                "Patch Failed",
                e.ToString(),
                "OK"
            );
        }
    }

    private int FindMatchingBrace(string text, int startIndex)
    {
        int depth = 0;

        for (int i = startIndex; i < text.Length; i++)
        {
            if (text[i] == '{')
                depth++;

            if (text[i] == '}')
            {
                depth--;

                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private string CreateFirebaseParameterText(RCParameterDefinition param)
    {
        string firebaseType = param.valueType switch
        {
            RCValueType.String => "STRING",
            RCValueType.Number => "NUMBER",
            RCValueType.Boolean => "BOOLEAN",
            _ => "STRING"
        };

        string value = param.value;

        if (param.valueType == RCValueType.String)
        {
            value = $"\\\"{value}\\\"";
        }

        return
    $@"  ""{param.key}"": {{
    ""defaultValue"": {{
      ""value"": ""{value}""
    }},
    ""valueType"": ""{firebaseType}""
  }}";
    }
}