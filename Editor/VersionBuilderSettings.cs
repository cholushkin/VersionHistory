using System.Collections.Generic;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    [CreateAssetMenu(fileName = "VersionBuilderSettings", menuName = "GameLib/VersionHistory/Builder Settings", order = 2)]
    public class VersionBuilderSettings : ScriptableObject
    {
        [field: Header("Target Manifests")]
        [field: SerializeField, Tooltip("Drop VersionManifest assets here to generate Markdown for them.")] 
        public List<VersionManifest> TargetManifests { get; private set; } = new();

        [field: Header("Git Integration")]
        [field: SerializeField] public string GitRepositoryPath { get; private set; } = "./";

        [field: Header("Markdown Export")]
        [field: SerializeField] public bool GenerateMarkdown { get; private set; } = true;
        [field: SerializeField] public string MarkdownExportPath { get; private set; } = "CHANGELOG.md";
        [field: SerializeField, Multiline] public string IntroText { get; private set; } = "All significant updates to this product will be recorded in this file.";
        [field: SerializeField] public string CompanyName { get; private set; } = "My Company";
        [field: SerializeField] public string CopyrightFormat { get; private set; } = "© {0} {1}. All rights reserved.";
    }
}