using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class VersionHistoryImporter : AssetPostprocessor
{
    private const string TxtDir = "VersionHistory"; // all txt with CL should be placed here
    private static string OutputDataDirectory = "Assets/Game/Data/VersionHistory"; // output dir for scriptable objects

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        var scriptableObject =
            ScriptableObjectUtility.GetInstanceOfScriptableObject<VersionHistoryScriptableObject>(
                OutputDataDirectory, GetScriptableObjectFileName());
        if(scriptableObject == null)
            return;

        if(scriptableObject.Versions == null)
            scriptableObject.Versions = new List<VersionHistoryScriptableObject.Version>();

        var isDataChanged = false;
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".txt") && assetPath.Contains(TxtDir))
            {
                // remove if exists 
                var verString = GetVersion(assetPath);
                scriptableObject.Versions.RemoveAll(x => x.Name == verString);

                // add new
                var version = new VersionHistoryScriptableObject.Version();
                version.Name = verString;
                version.Text = (TextAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset));
                scriptableObject.Versions.Add(version);

                isDataChanged = true;
            }
        }

        if (isDataChanged)
        {
            scriptableObject.Versions = scriptableObject.Versions.OrderByDescending(x=>x.Name).ToList();
            EditorUtility.SetDirty(scriptableObject);
        }
    }

    private static string GetVersion(string assetPath)
    {
        Assert.IsFalse(string.IsNullOrEmpty(assetPath));
        var ver = Path.GetFileNameWithoutExtension(assetPath);
        return ver;
    }

    private static string GetScriptableObjectFileName()
    {
        return "VersionHistory.asset";
    }
}
