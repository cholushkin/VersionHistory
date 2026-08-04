// todo: consider adding a release date field to VersionEntry in the future
// idea: add an internal method to sort versions by semver if tags get added out of order

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLib.VersionHistory
{

    [CreateAssetMenu(fileName = "VersionHistory", menuName = "GameLib/VersionHistory/Version History", order = 1)]
    public class VersionHistory : ScriptableObject
    {
        [SerializeField] private List<VersionEntry> _versions = new();

        public IReadOnlyList<VersionEntry> Versions => _versions;

        internal void AddOrUpdateVersion(string versionName, IEnumerable<ChangeItem> changes)
        {
            var existingVersion = _versions.FirstOrDefault(v => v.VersionName == versionName);

            if (existingVersion != null)
            {
                existingVersion.AddChanges(changes);
            }
            else
            {
                _versions.Add(new VersionEntry(versionName, changes.ToList()));
            }
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

        internal void AddChanges(IEnumerable<ChangeItem> newChanges)
        {
            _changes.AddRange(newChanges);
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