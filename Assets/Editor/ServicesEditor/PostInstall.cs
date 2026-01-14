using UnityEditor;
using UnityEngine;
using System.IO;

namespace ServicesPackage
{
    public class PostInstallProcessor : AssetPostprocessor
    {
        private const string SourcePath = "Assets/YooglabServices/Services/YoogoLabManager.cs";
        private const string FileName = "YoogoLabManager.cs";

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string assetPath in importedAssets)
            {
                if (assetPath.EndsWith(FileName))
                {
                    if (assetPath.Replace("\\", "/") == SourcePath)
                        continue;

                    try
                    {
                        File.Copy(SourcePath, assetPath, true);
                        Debug.Log($"[YoogaLabPlugin] Replaced duplicate YoogoLabManager.cs at: {assetPath}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[YoogaLabPlugin] Failed to replace {assetPath}: {ex.Message}");
                    }

                    AssetDatabase.ImportAsset(assetPath);
                }
            }
        }
    }
}
