using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    [CustomEditor(typeof(VersionHistory))]
    public class VersionHistoryInspector : UnityEditor.Editor
    {
        private ReorderableList _list;
        private SerializedProperty _elementsProp;

        private void OnEnable()
        {
            _elementsProp = serializedObject.FindProperty("_elements");
            
            _list = new ReorderableList(serializedObject, _elementsProp, true, true, true, true)
            {
                drawHeaderCallback = DrawHeader,
                drawElementCallback = DrawElement,
                elementHeightCallback = ElementHeight,
                drawElementBackgroundCallback = DrawElementBackground,
                onAddDropdownCallback = OnAddDropdown
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            _list.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(15);
            if (GUILayout.Button("Edit with Git", GUILayout.Height(30)))
            {
                VersionHistoryEditorWindow.Open((VersionHistory)target);
            }
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Version history source");
        }

        private float ElementHeight(int index)
        {
            var element = _elementsProp.GetArrayElementAtIndex(index);
            if (element.FindPropertyRelative("IsVersionHeader").boolValue)
            {
                return EditorGUIUtility.singleLineHeight + 8f;
            }

            var descProp = element.FindPropertyRelative("Description");
            
            // Calculate dynamic height for the multiline text area
            GUIStyle textAreaStyle = EditorStyles.textArea;
            float textWidth = EditorGUIUtility.currentViewWidth - 150f; 
            float descHeight = textAreaStyle.CalcHeight(new GUIContent(descProp.stringValue), textWidth);
            
            // Ensure minimum height to fit the category dropdown cleanly
            return Mathf.Max(EditorGUIUtility.singleLineHeight + 12f, descHeight + 12f);
        }

        private void DrawElementBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Always let Unity draw the base background (it handles index -1 internally)
            ReorderableList.defaultBehaviours.DrawElementBackground(rect, index, isActive, isFocused, true);
    
            if (index < 0 || index >= _elementsProp.arraySize)
            {
                return;
            }
    
            var element = _elementsProp.GetArrayElementAtIndex(index);
            if (element != null && element.FindPropertyRelative("IsVersionHeader").boolValue)
            {
                // Draw a strong separator background for Version headers to distinguish them
                Rect headerBg = rect;
                headerBg.yMin += 2;
                headerBg.yMax -= 2;
        
                Color bgColor = EditorGUIUtility.isProSkin 
                    ? new Color(0.2f, 0.4f, 0.6f, 0.4f) 
                    : new Color(0.6f, 0.8f, 1.0f, 0.5f);
            
                EditorGUI.DrawRect(headerBg, bgColor);
            }
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _elementsProp.GetArrayElementAtIndex(index);
            bool isHeader = element.FindPropertyRelative("IsVersionHeader").boolValue;

            rect.y += 4;
            rect.height -= 8;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            if (isHeader)
            {
                var versionProp = element.FindPropertyRelative("VersionName");
                var includedProp = element.FindPropertyRelative("IsIncluded");
                
                // Section Mass-Toggle
                Rect toggleRect = new Rect(rect.x, rect.y, 20, lineHeight);
                
                EditorGUI.BeginChangeCheck();
                bool newIncluded = EditorGUI.Toggle(toggleRect, includedProp.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    includedProp.boolValue = newIncluded;
                    ToggleEntireSection(index, newIncluded);
                }
                
                Rect labelRect = new Rect(rect.x + 20, rect.y, 60, lineHeight);
                EditorGUI.LabelField(labelRect, "Version:", EditorStyles.boldLabel);

                Rect fieldRect = new Rect(rect.x + 80, rect.y, rect.width - 80, lineHeight);
                versionProp.stringValue = EditorGUI.TextField(fieldRect, versionProp.stringValue, EditorStyles.boldLabel);
            }
            else
            {
                var includedProp = element.FindPropertyRelative("IsIncluded");
                var categoryProp = element.FindPropertyRelative("Category");
                var descProp = element.FindPropertyRelative("Description");

                Rect toggleRect = new Rect(rect.x, rect.y, 20, lineHeight);
                includedProp.boolValue = EditorGUI.Toggle(toggleRect, includedProp.boolValue);

                Rect catRect = new Rect(rect.x + 20, rect.y, 90, lineHeight);
                EditorGUI.PropertyField(catRect, categoryProp, GUIContent.none);

                Rect descRect = new Rect(rect.x + 115, rect.y, rect.width - 115, rect.height);
                descProp.stringValue = EditorGUI.TextArea(descRect, descProp.stringValue, EditorStyles.textArea);
            }
        }

        private void ToggleEntireSection(int headerIndex, bool state)
        {
            // Iterate from the item right after the header until the end of the array or the next header
            for (int i = headerIndex + 1; i < _elementsProp.arraySize; i++)
            {
                var el = _elementsProp.GetArrayElementAtIndex(i);
                
                // Stop if we hit the next version block
                if (el.FindPropertyRelative("IsVersionHeader").boolValue)
                {
                    break; 
                }
                
                el.FindPropertyRelative("IsIncluded").boolValue = state;
            }
        }

        private void OnAddDropdown(Rect buttonRect, ReorderableList list)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Version Header"), false, () => AddElement(true));
            menu.AddItem(new GUIContent("Change Item"), false, () => AddElement(false));
            menu.DropDown(buttonRect);
        }

        private void AddElement(bool isHeader)
        {
            serializedObject.Update();
            int index = _elementsProp.arraySize;
            _elementsProp.arraySize++;
            var element = _elementsProp.GetArrayElementAtIndex(index);
            
            element.FindPropertyRelative("IsVersionHeader").boolValue = isHeader;
            if (isHeader)
            {
                element.FindPropertyRelative("VersionName").stringValue = "v0.0.0";
            }
            else
            {
                element.FindPropertyRelative("Category").enumValueIndex = 0;
                element.FindPropertyRelative("Description").stringValue = "";
                element.FindPropertyRelative("IsIncluded").boolValue = true;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}