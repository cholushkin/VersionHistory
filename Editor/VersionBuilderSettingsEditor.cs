using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameLib.VersionHistory.Editor
{
    [CustomEditor(typeof(VersionBuilderSettings))]
    public class VersionBuilderSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var settings = (VersionBuilderSettings)target;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            bool hasManifests = settings.TargetManifests != null && settings.TargetManifests.Count > 0;
            GUI.enabled = hasManifests;
            
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("1. Sync Git History", GUILayout.Height(30)))
            {
                ExecuteSyncPipeline(settings).Forget();
            }

            if (GUILayout.Button("2. Export Markdown", GUILayout.Height(30)))
            {
                ExecuteMarkdownExport(settings);
            }
            
            GUILayout.EndHorizontal();
            
            GUI.enabled = true;

            if (!hasManifests)
            {
                EditorGUILayout.HelpBox("Add at least one VersionManifest to enable actions.", MessageType.Info);
            }
        }

        private async UniTaskVoid ExecuteSyncPipeline(VersionBuilderSettings settings)
        {
            Debug.Log($"[VersionHistory] Syncing {settings.TargetManifests.Count} manifest(s) with Git...");
            
            foreach (var manifest in settings.TargetManifests)
            {
                if (manifest == null) continue;
                await VersionHistoryBuilder.SyncManifestWithGitAsync(settings, manifest);
            }
            
            Debug.Log("[VersionHistory] Git Sync completed successfully.");
        }

        private void ExecuteMarkdownExport(VersionBuilderSettings settings)
        {
            Debug.Log($"[VersionHistory] Exporting Markdown for {settings.TargetManifests.Count} manifest(s)...");
            
            foreach (var manifest in settings.TargetManifests)
            {
                if (manifest == null) continue;
                ChangelogMarkdownGenerator.Generate(settings, manifest);
            }
        }
    }
}