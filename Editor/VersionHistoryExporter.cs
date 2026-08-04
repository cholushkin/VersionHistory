// todo: add validation to ensure AssetName doesn't contain invalid file path characters
// idea: add an option to group output by ChangeCategory globally instead of by version

using UnityEngine;

namespace GameLib.VersionHistory.Editor
{

    [CreateAssetMenu(fileName = "VersionHistoryExporter", menuName = "GameLib/VersionHistory/Exporter", order = 2)]
    public class VersionHistoryExporter : ScriptableObject
    {
        [field: Header("Target Data")]
        [field: SerializeField, Tooltip("The VersionHistory asset to export.")]
        public VersionHistory TargetHistory { get; private set; }

        [field: Header("Export Options")]
        [field: SerializeField, Tooltip("The base name of the exported file (e.g., CHANGELOG). Extension is added automatically.")]
        public string AssetName { get; private set; } = "CHANGELOG";

        [field: SerializeField, Tooltip("If true, ignores the IsIncluded flag in VersionHistory and exports all items.")]
        public bool IncludeAll { get; private set; }

        [field: SerializeField, Tooltip("If 0, exports all versions. If N, exports only the last N versions.")]
        public int TakeOnlyLastXVersions { get; private set; }

        [field: Header("Formatting")]
        [field: SerializeField, Multiline]
        public string IntroText { get; private set; } = "All significant updates to this product will be recorded in this file.";

        [field: SerializeField] public string CompanyName { get; private set; } = "My Company";
        [field: SerializeField] public string CopyrightFormat { get; private set; } = "© {0} {1}. All rights reserved.";
    }
}