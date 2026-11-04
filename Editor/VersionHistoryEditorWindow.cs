using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameLib.VersionHistory.Editor
{
    public class VersionHistoryEditorWindow : EditorWindow
    {
        private VersionHistory _targetHistory;
        
        // Settings & State
        private string _gitRepositoryPath = "./";
        private bool _checkForDuplicates = true;
        private bool _includeUncategorized = true; // Defaulted to true so your "Initial commit" isn't hidden by default
        
        // Fetching States
        private bool _isFetchingMessages = false;
        private bool _isFetchingTags = false;
        
        // Range Selection
        private string[] _availableTags = Array.Empty<string>();
        private int _fromTagIndex = -1;
        private int _toTagIndex = -1;

        // In-memory pool data
        private List<VersionPoolGroup> _pool = new();
        private List<PooledCommit> _flatPoolReference = new();
        private int _lastSelectedIndex = -1;
        
        private Vector2 _scrollPosition;
        
        // Styles
        private GUIStyle _selectedStyle;
        private GUIStyle _normalStyle;
        private GUIStyle _versionBlockStyle;
        private GUIStyle _leftAlignedLabel;

        [MenuItem("GameLib/Version History Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<VersionHistoryEditorWindow>("Git Version Editor");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        public static void Open(VersionHistory targetHistory)
        {
            var window = GetWindow<VersionHistoryEditorWindow>("Git Version Editor");
            window._targetHistory = targetHistory;
            window.minSize = new Vector2(500, 600);
            window.Show();
        }

        private void OnEnable()
        {
            _pool.Clear();
            _flatPoolReference.Clear();
            _lastSelectedIndex = -1;
            
            RefreshTagsAsync().Forget();
        }

        private void OnGUI()
        {
            InitializeStyles();
            DrawOptionsSection();

            if (_targetHistory == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("Please assign a Target Version History asset in the settings above.", MessageType.Warning);
                return;
            }

            DrawPoolList();
            DrawFooter();
        }

        private void InitializeStyles()
        {
            if (_selectedStyle == null)
            {
                _selectedStyle = new GUIStyle(GUI.skin.box);
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, new Color(1f, 0.92f, 0.016f, 0.4f)); 
                tex.Apply();
                _selectedStyle.normal.background = tex;
            }
            if (_normalStyle == null)
            {
                _normalStyle = new GUIStyle(GUI.skin.box);
            }
            if (_versionBlockStyle == null)
            {
                _versionBlockStyle = new GUIStyle(EditorStyles.helpBox);
                _versionBlockStyle.padding = new RectOffset(10, 10, 10, 10);
                _versionBlockStyle.margin = new RectOffset(5, 5, 5, 10);
            }
            if (_leftAlignedLabel == null)
            {
                _leftAlignedLabel = new GUIStyle(EditorStyles.wordWrappedLabel);
                _leftAlignedLabel.alignment = TextAnchor.MiddleLeft;
                _leftAlignedLabel.richText = true;
            }
        }

        private void DrawOptionsSection()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            GUILayout.Space(5);

            _targetHistory = (VersionHistory)EditorGUILayout.ObjectField("Target History", _targetHistory, typeof(VersionHistory), false);
            
            _gitRepositoryPath = EditorGUILayout.TextField("Git Path", _gitRepositoryPath);


            _checkForDuplicates = EditorGUILayout.Toggle("Check Duplicates on Add", _checkForDuplicates);
            _includeUncategorized = EditorGUILayout.Toggle("Uncategorized", _includeUncategorized);

            // Fetch Range Row
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Fetch Range", EditorStyles.boldLabel);
            
            if (_isFetchingTags)
            {
                EditorGUILayout.LabelField("Loading tags...", EditorStyles.centeredGreyMiniLabel);
            }
            else if (_availableTags.Length > 0)
            {
                GUILayout.BeginHorizontal();
                
                EditorGUI.BeginChangeCheck();
                
                _fromTagIndex = EditorGUILayout.Popup("From (Older)", _fromTagIndex, _availableTags);
                _toTagIndex = EditorGUILayout.Popup("To (Newer)", _toTagIndex, _availableTags);
                
                if (EditorGUI.EndChangeCheck())
                {
                    if (_toTagIndex > _fromTagIndex)
                    {
                        _toTagIndex = _fromTagIndex;
                    }
                }
                GUILayout.EndHorizontal();
                if (GUILayout.Button("🔄tags", GUILayout.Width(60)))
                    RefreshTagsAsync().Forget();
            }

            // Fetch Button
            GUILayout.Space(10);
            GUI.enabled = !_isFetchingMessages && !_isFetchingTags && _targetHistory != null;
            if (GUILayout.Button("Fetch Git Messages", GUILayout.Height(30)))
            {
                FetchGitMessagesAsync().Forget();
            }
            GUI.enabled = true;
            
            GUILayout.EndVertical();
        }

        private void DrawPoolList()
        {
            if (_isFetchingMessages)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Fetching Git History...", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                return;
            }

            if (_pool.Count == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("Pool is empty. Fetch messages to begin.", EditorStyles.centeredGreyMiniLabel);
                GUILayout.FlexibleSpace();
                return;
            }

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            int flatIndex = 0;

            foreach (var group in _pool)
            {
                GUILayout.BeginVertical(_versionBlockStyle); 
                
                EditorGUILayout.LabelField(group.VersionTag, EditorStyles.boldLabel);
                GUILayout.Space(5);
                
                foreach (var commit in group.Commits)
                {
                    DrawCommitItem(commit, flatIndex);
                    flatIndex++;
                }
                
                GUILayout.EndVertical();
            }
            
            GUILayout.EndScrollView();
        }

        private void DrawCommitItem(PooledCommit commit, int currentIndex)
        {
            GUIStyle style = commit.IsSelected ? _selectedStyle : _normalStyle;
            
            string labelText = $"<b>[{commit.Category}]</b> {commit.FormattedDescription}";
            GUIContent content = new GUIContent(labelText);
            
            float labelHeight = _leftAlignedLabel.CalcHeight(content, position.width - 50);
            float totalHeight = Mathf.Max(24f, labelHeight + 8f);

            Rect itemRect = GUILayoutUtility.GetRect(content, style, GUILayout.ExpandWidth(true), GUILayout.Height(totalHeight));
            
            GUI.Box(itemRect, GUIContent.none, style);
            
            Rect textRect = new Rect(itemRect.x + 5, itemRect.y + 4, itemRect.width - 10, itemRect.height - 8);
            GUI.Label(textRect, content, _leftAlignedLabel);
            
            HandleMouseInput(itemRect, commit, currentIndex);
        }

        private void HandleMouseInput(Rect rect, PooledCommit commit, int currentIndex)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                if (e.control || e.command)
                {
                    commit.IsSelected = !commit.IsSelected;
                    _lastSelectedIndex = currentIndex;
                }
                else if (e.shift && _lastSelectedIndex != -1)
                {
                    int start = Mathf.Min(_lastSelectedIndex, currentIndex);
                    int end = Mathf.Max(_lastSelectedIndex, currentIndex);
                    
                    for (int i = start; i <= end; i++)
                    {
                        _flatPoolReference[i].IsSelected = true;
                    }
                }
                else
                {
                    foreach (var c in _flatPoolReference) c.IsSelected = false;
                    commit.IsSelected = true;
                    _lastSelectedIndex = currentIndex;
                }
                
                e.Use(); 
                Repaint();
            }
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space();
            int selectedCount = _flatPoolReference.Count(c => c.IsSelected);
            
            GUI.enabled = selectedCount > 0;
            if (GUILayout.Button($"Add Selected to Version History ({selectedCount})", GUILayout.Height(35)))
            {
                InjectSelectedCommits();
            }
            GUI.enabled = true;
        }

        private async UniTaskVoid RefreshTagsAsync()
        {
            _isFetchingTags = true;
            Repaint();

            try
            {
                var tags = await GitFetchService.FetchTagsAsync(_gitRepositoryPath);
                _availableTags = tags.ToArray();
                
                if (_availableTags.Length > 0)
                {
                    _toTagIndex = 0; 
                    _fromTagIndex = _availableTags.Length - 1; 
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VersionHistory] Failed to fetch tags: {ex.Message}");
                _availableTags = Array.Empty<string>();
            }
            finally
            {
                _isFetchingTags = false;
                Repaint();
            }
        }

        private async UniTaskVoid FetchGitMessagesAsync()
        {
            _isFetchingMessages = true;
            Repaint();

            string fromTag = _availableTags.Length > 0 ? _availableTags[_fromTagIndex] : null;
            string toTag = _availableTags.Length > 0 ? _availableTags[_toTagIndex] : null;

            _pool = await GitFetchService.FetchCommitPoolAsync(_gitRepositoryPath, fromTag, toTag, _includeUncategorized);
            
            _flatPoolReference.Clear();
            foreach (var group in _pool)
            {
                _flatPoolReference.AddRange(group.Commits);
            }
            
            _lastSelectedIndex = -1;
            _isFetchingMessages = false;
            Repaint();
        }

        private void InjectSelectedCommits()
        {
            int addedCount = 0;
            int skippedCount = 0;

            foreach (var group in _pool)
            {
                var selectedInGroup = group.Commits.Where(c => c.IsSelected).ToList();
                if (selectedInGroup.Count == 0) continue;

                var changesToAdd = new List<ChangeItem>();
                var existingVersion = _targetHistory.Versions.FirstOrDefault(v => v.VersionName == group.VersionTag);

                foreach (var commit in selectedInGroup)
                {
                    bool isDuplicate = false;

                    if (_checkForDuplicates && existingVersion != null)
                    {
                        isDuplicate = existingVersion.Changes.Any(c => c.Description == commit.FormattedDescription);
                    }

                    if (!isDuplicate)
                    {
                        changesToAdd.Add(new ChangeItem(commit.Category, commit.FormattedDescription, true));
                        addedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                if (changesToAdd.Count > 0)
                {
                    _targetHistory.AddOrUpdateVersion(group.VersionTag, changesToAdd);
                }
            }

            if (addedCount > 0)
            {
                EditorUtility.SetDirty(_targetHistory);
                AssetDatabase.SaveAssets();
            }

            string log = $"[VersionHistory] Added {addedCount} commits.";
            if (skippedCount > 0) log += $" Skipped {skippedCount} duplicates.";
            Debug.Log(log);

            Repaint();
        }
    }
}