using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameLib.VersionHistory
{
    [CreateAssetMenu(fileName = "VersionManifest", menuName = "GameLib/VersionHistory/Version Manifest", order = 1)]
    public class VersionManifest : ScriptableObject
    {
        [SerializeField] private List<VersionEntry> _versions = new();

        public IReadOnlyList<VersionEntry> Versions => _versions;

        internal void SetVersions(List<VersionEntry> versions)
        {
            _versions = versions;
        }
    }

    [Serializable]
    public class VersionEntry
    {
        [SerializeField] private string _versionName;
        [SerializeField] private List<ChangeItem> _changes = new();

        public string VersionName => _versionName;
        public IReadOnlyList<ChangeItem> Changes => _changes;

        public VersionEntry(string versionName, List<ChangeItem> changes)
        {
            _versionName = versionName;
            _changes = changes;
        }
    }

    [Serializable]
    public class ChangeItem
    {
        [SerializeField] private ChangeCategory _category;
        [SerializeField, Multiline] private string _description;
        [SerializeField] private bool _isIncluded = true;

        public ChangeCategory Category => _category;
        public string Description => _description;
        public bool IsIncluded => _isIncluded;

        public ChangeItem(ChangeCategory category, string description, bool isIncluded = true)
        {
            _category = category;
            _description = description;
            _isIncluded = isIncluded;
        }
    }

    public enum ChangeCategory
    {
        Uncategorized = 0,
        Feature,
        Fix,
        Performance,
        BreakingChange,
        Internal,
        Build,
        Documentation
    }
}