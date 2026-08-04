// todo: implement HTML generation logic in a separate ChangelogHtmlGenerator
// idea: add an option to inject custom CSS if HTML export is triggered

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    public static class ChangelogMarkdownGenerator
    {
        public static void Generate(VersionHistoryExporter exporter)
        {
            if (exporter.TargetHistory == null)
            {
                Debug.LogError("[VersionHistory] TargetHistory is missing on the Exporter.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"# {exporter.AssetName}");
            
            if (!string.IsNullOrWhiteSpace(exporter.IntroText))
            {
                sb.AppendLine(exporter.IntroText);
                sb.AppendLine();
            }

            var versionsToExport = exporter.TakeOnlyLastXVersions > 0 
                ? exporter.TargetHistory.Versions.Take(exporter.TakeOnlyLastXVersions) 
                : exporter.TargetHistory.Versions;

            foreach (var version in versionsToExport)
            {
                var changes = exporter.IncludeAll 
                    ? version.Changes 
                    : version.Changes.Where(c => c.IsIncluded).ToList();

                if (!changes.Any()) continue;

                string versionTitle = version.VersionName.StartsWith("not-versioned-yet")
                    ? "## Unreleased"
                    : $"## {version.VersionName}";

                sb.AppendLine(versionTitle);
                sb.AppendLine();

                var grouped = changes
                    .GroupBy(c => c.Category)
                    .OrderBy(g => (int)g.Key);

                foreach (var group in grouped)
                {
                    sb.AppendLine($"### {GetCategoryDisplayName(group.Key)}");
                    sb.AppendLine();

                    foreach (var item in group)
                    {
                        sb.AppendLine($"- {item.Description}");
                    }
                    
                    sb.AppendLine();
                }
            }

            sb.AppendLine("---");
            string year = DateTime.Now.Year.ToString();
            
            if (!string.IsNullOrWhiteSpace(exporter.CopyrightFormat))
            {
                sb.AppendLine(string.Format(exporter.CopyrightFormat, year, exporter.CompanyName));
            }
            else
            {
                sb.AppendLine($"© {year} {exporter.CompanyName}. All rights reserved.");
            }

            SaveFile(exporter, sb.ToString(), "md");
        }

        private static void SaveFile(VersionHistoryExporter exporter, string content, string extension)
        {
            string exporterAssetPath = AssetDatabase.GetAssetPath(exporter);
            string directory = Path.GetDirectoryName(exporterAssetPath);
            
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullPath = Path.Combine(projectRoot, directory, $"{exporter.AssetName}.{extension}");
            
            bool fileExists = File.Exists(fullPath);
            
            File.WriteAllText(fullPath, content);
            
            if (fileExists)
            {
                Debug.Log($"[VersionHistory] Overwrote existing file at: {fullPath}");
            }
            else
            {
                Debug.Log($"[VersionHistory] Created new file at: {fullPath}");
            }
            
            AssetDatabase.Refresh();
        }

        private static string GetCategoryDisplayName(ChangeCategory category)
        {
            return category switch
            {
                ChangeCategory.Feature => "Features",
                ChangeCategory.Fix => "Bug Fixes",
                ChangeCategory.BreakingChange => "Breaking Changes",
                ChangeCategory.Performance => "Performance Improvements",
                ChangeCategory.Build => "Build & CI",
                ChangeCategory.Documentation => "Documentation",
                ChangeCategory.Internal => "Internal & Refactoring",
                _ => "Other Changes"
            };
        }
    }
}