using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace VersionHistory
{
    public class VersionHistoryImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var versionHistorySources = ScriptableObjectUtility.GetInstancesOfScriptableObject<VersionHistoryImportSource>();

            // If txt belongs to any import source than reimport entire source
            HashSet<VersionHistoryImportSource> toReimport = new HashSet<VersionHistoryImportSource>();
            foreach (string assetPath in importedAssets)
            {
                var source = GetSource(assetPath, versionHistorySources);
                if (source != null)
                    toReimport.Add(source);
            }

            foreach (var versionHistoryImportSource in toReimport)
                ReimportVersionHistoryScriptableObject(versionHistoryImportSource);
        }

        private static void ReimportVersionHistoryScriptableObject(VersionHistoryImportSource versionHistoryImportSource)
        {
            Assert.IsNotNull(versionHistoryImportSource);

            // Create new or get the old one
            var versionHistorySO = ScriptableObjectUtility.GetInstanceOfScriptableObject<VersionHistoryScriptableObject>(
                versionHistoryImportSource.OutputDirectory, GetScriptableObjectFileName(versionHistoryImportSource.ModuleName));

            Assert.IsNotNull(versionHistoryImportSource);

            // Clear old version items
            versionHistorySO.Versions = new List<VersionHistoryScriptableObject.Version>();

            // Get all CL of this source
            DirectoryInfo dir = new DirectoryInfo(versionHistoryImportSource.UserCLDirectory);
            FileInfo[] info = dir.GetFiles("*.txt");
            foreach (FileInfo f in info)
            {
                var verString = GetVersion(f.Name);

                string fullPath = f.FullName.Replace(@"\", "/");
                string assetPath = "Assets" + fullPath.Replace(Application.dataPath, "");

                var version = new VersionHistoryScriptableObject.Version
                {
                    Name = verString,
                    Text = (TextAsset)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TextAsset))
                };
                versionHistorySO.Versions.Add(version);
            }
            versionHistorySO.Versions = versionHistorySO.Versions.OrderByDescending(x => x.Name).ToList();
            EditorUtility.SetDirty(versionHistorySO);
            AssetDatabase.Refresh();
        }

        public static VersionHistoryImportSource GetSource(string assetFullPath, VersionHistoryImportSource[] sources)
        {
            var extension = Path.GetExtension(assetFullPath);
            if (!extension.Equals(".txt"))
                return null;
            
            foreach (var src in sources)
                if (assetFullPath.StartsWith(src.UserCLDirectory))
                    return src;
            return null;
        }

        private static string GetVersion(string assetPath)
        {
            Assert.IsFalse(string.IsNullOrEmpty(assetPath));
            var ver = Path.GetFileNameWithoutExtension(assetPath);
            return ver;
        }

        private static string GetScriptableObjectFileName(string moduleName)
        {
            return $"{moduleName}VersionHistory.asset";
        }
    }
}