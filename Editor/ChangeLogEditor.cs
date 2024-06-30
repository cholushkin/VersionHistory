using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VersionHistory
{
	[CustomEditor(typeof(ChangeLogScriptableObject))]
	public class ChangeLogEditor : Editor
	{
		public List<ChangeLogScriptableObject.Item> items;
		private Vector2 scrollPosition;
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			ChangeLogScriptableObject changeLog = (ChangeLogScriptableObject)target;
			if (GUILayout.Button("Get Changes"))
			{
				string assetPath = AssetDatabase.GetAssetPath(changeLog);
				string workingDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(assetPath));

				var gitFetcher = new GitLogFetcher(workingDirectory);
				var commits = gitFetcher.FetchCommitMessages();

				items = new(128);
				foreach (var commit in commits)
				{
					var item = GitCommitParser.Parse(commit);
					//Debug.Log(commit);
					items.Add(item);
				}
				
				EditorUtility.SetDirty(changeLog); // Mark the object as "dirty" to ensure changes are saved
			}
			
			// Display the list of items
			if(items != null)
				foreach (var t in items)
				{
					EditorGUILayout.BeginHorizontal();

					t.Category = (ChangeLogScriptableObject.Item.ChangeCategory)EditorGUILayout.EnumPopup(t.Category, GUILayout.Width(100));
					//items[i].Description = EditorGUILayout.TextField(items[i].Description);
					// Make the Description field multiline
					GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textField) { wordWrap = true };
					// Create a scrollable text area
					//scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(60));
					t.Description = EditorGUILayout.TextArea(t.Description, textAreaStyle, GUILayout.ExpandHeight(true));
					//EditorGUILayout.EndScrollView();
					
					if (GUILayout.Button("+", GUILayout.Width(32)))
					{
						// Add your custom logic for the "Add" button here
						// For example, you might want to add this item to another list or process it in some way
					}

					EditorGUILayout.EndHorizontal();
				}

			if (GUI.changed)
			{
				EditorUtility.SetDirty(changeLog); // Mark the object as "dirty" to ensure changes are saved
			}
			
		}
	}
}