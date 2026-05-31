using System.IO;
using Levels;
using UnityEditor;
using UnityEngine;

public static class MathLevelMapProtectionUtility
{
    [MenuItem("Tools/Level Maps/Protect Selected Text Asset")]
    private static void ProtectSelectedTextAsset()
    {
        TransformSelectedTextAsset(protect: true);
    }

    [MenuItem("Tools/Level Maps/Unprotect Selected Text Asset")]
    private static void UnprotectSelectedTextAsset()
    {
        TransformSelectedTextAsset(protect: false);
    }

    private static void TransformSelectedTextAsset(bool protect)
    {
        if (Selection.activeObject is not TextAsset textAsset)
        {
            EditorUtility.DisplayDialog("Level Map Protection", "Select a TextAsset first.", "OK");
            return;
        }

        var assetPath = AssetDatabase.GetAssetPath(textAsset);
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            EditorUtility.DisplayDialog("Level Map Protection", "Selected asset path is invalid.", "OK");
            return;
        }

        var fullPath = Path.GetFullPath(assetPath);
        var currentContent = File.ReadAllText(fullPath);

        if (protect)
        {
            var contentToProtect = MathLevelMapProtection.TryUnprotect(currentContent, out var alreadyProtected)
                ? alreadyProtected
                : currentContent;
            File.WriteAllText(fullPath, MathLevelMapProtection.Protect(contentToProtect));
        }
        else
        {
            if (!MathLevelMapProtection.TryUnprotect(currentContent, out var plainText))
            {
                EditorUtility.DisplayDialog("Level Map Protection", "Selected asset is not protected with the expected format.", "OK");
                return;
            }

            File.WriteAllText(fullPath, plainText);
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Level Map Protection", protect ? "Asset protected." : "Asset unprotected.", "OK");
    }
}