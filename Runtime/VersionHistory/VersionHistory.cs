// todo: consider adding version release dates for chronological sorting
// idea: support parsing from external JSON or ScriptableObject directly via a builder

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameLib.VersionHistory
{
    [CreateAssetMenu(fileName = "VersionHistory", menuName = "GameLib/VersionHistory/Version History", order = 1)]
    public class VersionHistory : ScriptableObject
    {
        [SerializeField] private List<HistoryElement> _elements = new();

        public IReadOnlyList<VersionEntry> Versions
        {
            get
            {
                var versions = new List<VersionEntry>();
                List<ChangeItem> currentChanges = null;

                foreach (var el in _elements)
                {
                    if (el.IsVersionHeader)
                    {
                        currentChanges = new List<ChangeItem>();
                        versions.Add(new VersionEntry(el.VersionName, el.IsIncluded, currentChanges));
                    }
                    else if (currentChanges != null)
                    {
                        currentChanges.Add(new ChangeItem(el.Category, el.Description, el.IsIncluded));
                    }
                }
                return versions;
            }
        }

        internal void AddOrUpdateVersion(string versionName, IEnumerable<ChangeItem> changes)
        {
            var newElements = changes.Select(c => new HistoryElement(c.Category, c.Description, c.IsIncluded));
            
            int headerIndex = _elements.FindIndex(e => e.IsVersionHeader && e.VersionName == versionName);
            
            if (headerIndex != -1)
            {
                _elements.InsertRange(headerIndex + 1, newElements);
            }
            else
            {
                _elements.Add(new HistoryElement(versionName));
                _elements.AddRange(newElements);
            }
        }
    }

    [Serializable]
    public class HistoryElement
    {
        public bool IsVersionHeader;
        
        public string VersionName;
        public ChangeCategory Category;
        [Multiline] public string Description;
        public bool IsIncluded = true;

        public HistoryElement() { }

        public HistoryElement(string versionName)
        {
            IsVersionHeader = true;
            VersionName = versionName;
        }

        public HistoryElement(ChangeCategory category, string description, bool isIncluded)
        {
            IsVersionHeader = false;
            Category = category;
            Description = description;
            IsIncluded = isIncluded;
        }
    }

    public class VersionEntry
    {
        public string VersionName { get; }
        public bool IsIncluded { get; }
        public IReadOnlyList<ChangeItem> Changes { get; }

        public VersionEntry(string versionName, bool isIncluded, List<ChangeItem> changes)
        {
            VersionName = versionName;
            IsIncluded = isIncluded;
            Changes = changes;
        }
    }

    public class ChangeItem
    {
        public ChangeCategory Category { get; }
        public string Description { get; }
        public bool IsIncluded { get; }

        public ChangeItem(ChangeCategory category, string description, bool isIncluded = true)
        {
            Category = category;
            Description = description;
            IsIncluded = isIncluded;
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