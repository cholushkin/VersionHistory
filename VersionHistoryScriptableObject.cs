﻿using System;
using System.Collections.Generic;
using UnityEngine;


namespace VersionHistory
{
    [CreateAssetMenu(fileName = "ChangeLog", menuName = "GameLib/VersionHistory/ChangeLog", order = 1)]
    public class ChangeLogScriptableObject : ScriptableObject
    {
        [Serializable]
        public class Item
        {
            public enum ChangeCategory
            {
                Undetermined = 0,
                Added,
                Changed,
                Deprecated,
                Removed,
                Fixed,
                Security
            }

            [Tooltip("Excluded from changelog generation")]
            public bool Included = true; 
            public ChangeCategory Category;
            [Multiline]
            public string Description;

        }
        [Serializable]
        public class Version
        {
            public string VersionName;
            public List<Item> Items = new();
        }
        
        [HideInInspector]
        public List<Version> Versions;
        public ChangeLogSettings Settings = new();


        public void Reset()
        {
            Settings.GenerateMarkdownChangeLog = true;
            Settings.MarkdownChangeLogPath = "ChangeLog.md";
            Settings.CompanyName = Application.companyName;
        }
    }
}
