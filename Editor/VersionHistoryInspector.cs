using UnityEditor;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    [CustomEditor(typeof(VersionHistory))]
    public class VersionHistoryInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var history = (VersionHistory)target;

            EditorGUILayout.Space(15);
            if (GUILayout.Button("Edit with Git", GUILayout.Height(30)))
            {
                VersionHistoryEditorWindow.Open(history);
            }
        }
    }
}