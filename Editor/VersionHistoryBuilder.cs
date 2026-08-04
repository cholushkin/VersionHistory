using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    public static class VersionHistoryBuilder
    {
        public static async UniTask SyncManifestWithGitAsync(VersionBuilderSettings settings, VersionManifest manifest)
        {
            var tags = await GitRepositoryService.FetchTagsAsync(settings.GitRepositoryPath);
            var newVersions = new List<VersionEntry>();

            for (int i = 0; i < tags.Count; i++)
            {
                string currentTag = tags[i];
                string previousTag = (i + 1 < tags.Count) ? tags[i + 1] : string.Empty;

                string revisionRange = string.IsNullOrEmpty(previousTag) ? currentTag : $"{previousTag}..{currentTag}";
                
                if (currentTag == "not-versioned-yet")
                {
                    revisionRange = string.IsNullOrEmpty(previousTag) ? "" : $"{previousTag}..HEAD";
                }

                var rawCommits = await GitRepositoryService.FetchCommitsAsync(settings.GitRepositoryPath, revisionRange);
                var changeItems = new List<ChangeItem>();

                foreach (var raw in rawCommits)
                {
                    var parsed = ConventionalCommitParser.Parse(raw);
                    
                    /// Internal and Uncategorized items are excluded by default to keep the changelog clean
                    bool isIncluded = parsed.Category != ChangeCategory.Internal && 
                                      parsed.Category != ChangeCategory.Uncategorized;
                                      
                    changeItems.Add(new ChangeItem(parsed.Category, parsed.Description, isIncluded));
                }

                newVersions.Add(new VersionEntry(currentTag, changeItems));
            }

            // Strongly-typed internal setter call
            manifest.SetVersions(newVersions);
            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
        }
    }
}