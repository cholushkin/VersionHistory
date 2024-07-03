using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace VersionHistory
{
    [CustomEditor(typeof(ChangeLogScriptableObject))]
    public class ChangeLogEditor : Editor
    {
        private List<ChangeLogScriptableObject.Item> items;
        private Dictionary<string, ReorderableList> reorderableLists = new Dictionary<string, ReorderableList>();
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
        private string selectedVersion;
        private readonly Dictionary<ChangeLogScriptableObject.Item.ChangeCategory, Color> categoryColors = new()
        {
            { ChangeLogScriptableObject.Item.ChangeCategory.Undetermined, Color.white },
            { ChangeLogScriptableObject.Item.ChangeCategory.Added, Color.green },
            { ChangeLogScriptableObject.Item.ChangeCategory.Changed, Color.yellow },
            { ChangeLogScriptableObject.Item.ChangeCategory.Deprecated, Color.magenta },
            { ChangeLogScriptableObject.Item.ChangeCategory.Removed, Color.red },
            { ChangeLogScriptableObject.Item.ChangeCategory.Fixed, Color.blue },
            { ChangeLogScriptableObject.Item.ChangeCategory.Security, Color.cyan }
        };

        public override void OnInspectorGUI()
        {
            ChangeLogScriptableObject changeLog = (ChangeLogScriptableObject)target;

            // "Versions" header and line separator
            EditorGUILayout.LabelField("Changelog", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Custom section for displaying versions and their items
            if (changeLog.Versions != null)
            {
                foreach (var version in changeLog.Versions)
                {
                    // Initialize foldout state if not already present
                    if (!foldoutStates.ContainsKey(version.VersionName))
                    {
                        foldoutStates[version.VersionName] = false;
                    }                    

                    bool foldoutState = foldoutStates[version.VersionName];
                    foldoutStates[version.VersionName] = EditorGUILayout.Foldout(foldoutState, version.VersionName);

                    // Check if this version is selected
                    bool isSelected = version.VersionName == selectedVersion;

                    // Draw the version label with yellow background if selected
                    Color previousBackgroundColor = GUI.backgroundColor;
                    if (isSelected)
                    {
                        GUI.backgroundColor = Color.yellow;
                    }

                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("select", GUILayout.Width(64)))
                    {
                        selectedVersion = version.VersionName;
                    }
                    EditorGUILayout.EndHorizontal();

                    GUI.backgroundColor = previousBackgroundColor;

                    if (foldoutStates[version.VersionName])
                    {
                        if (!reorderableLists.ContainsKey(version.VersionName))
                        {
                            var list = new ReorderableList(version.Items, typeof(ChangeLogScriptableObject.Item), true, false, true, true);

                            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                                var item = version.Items[index];
                                rect.y += 2;
                                float toggleWidth = 20;
                                float categoryWidth = 100;
                                float spacing = 5;

                                item.Exluded = EditorGUI.Toggle(new Rect(rect.x, rect.y, toggleWidth, EditorGUIUtility.singleLineHeight), item.Exluded);
                                item.Category = (ChangeLogScriptableObject.Item.ChangeCategory)EditorGUI.EnumPopup(
                                    new Rect(rect.x + toggleWidth + spacing, rect.y, categoryWidth, EditorGUIUtility.singleLineHeight), item.Category, GetEnumStyle(item.Category));
                                
                                item.Description = EditorGUI.TextArea(
                                    new Rect(rect.x + toggleWidth + categoryWidth + 2 * spacing, rect.y, rect.width - toggleWidth - categoryWidth - 2 * spacing, EditorGUIUtility.singleLineHeight),
                                    item.Description);

                            };

                            reorderableLists[version.VersionName] = list;
                        }

                        reorderableLists[version.VersionName].DoLayoutList();
                    }

                    EditorGUILayout.Space(); // Add space between versions
                }
            }

            // "Control" header and line separator
            EditorGUILayout.LabelField("Controls", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            string assetPath = AssetDatabase.GetAssetPath(changeLog);
            string workingDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(assetPath));

            if (GUILayout.Button("Fetch versions"))
            {
                Debug.Log($"Working with git repo: {new Git(workingDirectory).ExecuteCommand("rev-parse --show-toplevel").Output[0]}");
                var verFetcher = new VersionsFetcher(workingDirectory);
                var output = verFetcher.FetchTags();
                foreach (var versionTag in output)
                {
                    var v = changeLog.Versions.Find(x => x.VersionName == versionTag);
                    if (v == null)
                    {
                        changeLog.Versions.Add(new ChangeLogScriptableObject.Version { VersionName = versionTag });
                    }
                }
            }

            if (GUILayout.Button("Get Changes"))
            {
                var gitFetcher = new GitLogFetcher(workingDirectory);
                var commits = gitFetcher.FetchCommitMessages();

                items = new List<ChangeLogScriptableObject.Item>(128);
                foreach (var commit in commits)
                {
                    var item = GitCommitParser.Parse(commit);
                    items.Add(item);
                }

                EditorUtility.SetDirty(changeLog); // Mark the object as "dirty" to ensure changes are saved
            }

            if (GUILayout.Button("Export changelog"))
            {
                //changeLog.Settings.MarkdownChangeLogPath
                
            }
            
            // Display the list of items
            if (items != null)
            {
                foreach (var t in items)
                {
                    EditorGUILayout.BeginHorizontal();

                    t.Category = (ChangeLogScriptableObject.Item.ChangeCategory)EditorGUILayout.EnumPopup(t.Category,  GetEnumStyle(t.Category), GUILayout.Width(100));
                    
                    GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textField) { wordWrap = true };
                    t.Description = EditorGUILayout.TextArea(t.Description, textAreaStyle, GUILayout.ExpandHeight(true));

                    if (GUILayout.Button("+", GUILayout.Width(32)))
                    {
                        // Create a copy of the item 't'
                        ChangeLogScriptableObject.Item newItem = new ChangeLogScriptableObject.Item();
                        newItem.Category = t.Category;
                        newItem.Description = t.Description;

                        // Add the item copy to the currently selected version
                        if (!string.IsNullOrEmpty(selectedVersion))
                        {
                            var version = changeLog.Versions.Find(v => v.VersionName == selectedVersion);
                            if (version != null)
                            {
                                version.Items.Add(newItem);
                                EditorUtility.SetDirty(changeLog); // Mark the object as "dirty" to ensure changes are saved
                            }
                        }
                        else
                        {
                            Debug.Log("Please 'select' version first");
                        }
                    }


                    EditorGUILayout.EndHorizontal();
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(changeLog); // Mark the object as "dirty" to ensure changes are saved
            }
            
            DrawDefaultInspector();
        }
        
        // Helper method to get GUIStyle for enum popup with custom color
        private GUIStyle GetEnumStyle(ChangeLogScriptableObject.Item.ChangeCategory category)
        {
            GUIStyle style = new GUIStyle(EditorStyles.popup);
            if (categoryColors.ContainsKey(category))
            {
                Color color = categoryColors[category];
                style.normal.textColor = color;
            }
            return style;
        }
    }
}
