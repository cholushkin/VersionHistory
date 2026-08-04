// todo: wire up Export HTML button to its respective generator once implemented
// idea: add visual feedback (like a green checkmark ping) next to the button upon successful export

using UnityEditor;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    [CustomEditor(typeof(VersionHistoryExporter))]
    public class VersionHistoryExporterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var exporter = (VersionHistoryExporter)target;

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Export Actions", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            bool canExport = exporter.TargetHistory != null;
            GUI.enabled = canExport;
            
            if (GUILayout.Button("Export Markdown", GUILayout.Height(30)))
            {
                ChangelogMarkdownGenerator.Generate(exporter);
            }

            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Export HTML", GUILayout.Height(25)))
            {
                Debug.Log("[VersionHistory] Export HTML is not yet implemented.");
            }

            if (GUILayout.Button("Export Plain Text", GUILayout.Height(25)))
            {
                Debug.Log("[VersionHistory] Export Plain Text is not yet implemented.");
            }
            GUILayout.EndHorizontal();
            
            GUI.enabled = true;

            if (!canExport)
            {
                EditorGUILayout.HelpBox("Assign a Target History asset to enable exports.", MessageType.Info);
            }
        }
    }
}