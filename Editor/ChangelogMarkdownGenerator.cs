using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    public static class ChangelogMarkdownGenerator
    {
        public static void Generate(VersionBuilderSettings settings, VersionManifest manifest)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Changelog");
            
            if (!string.IsNullOrWhiteSpace(settings.IntroText))
            {
                sb.AppendLine(settings.IntroText);
                sb.AppendLine();
            }

            foreach (var version in manifest.Versions)
            {
                var includedChanges = version.Changes.Where(c => c.IsIncluded).ToList();
                if (includedChanges.Count == 0) continue;

                string versionTitle = version.VersionName.StartsWith("not-versioned-yet")
                    ? "## Unreleased"
                    : $"## {version.VersionName}";

                sb.AppendLine(versionTitle);
                sb.AppendLine();

                var grouped = includedChanges
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
            
            if (!string.IsNullOrWhiteSpace(settings.CopyrightFormat))
            {
                sb.AppendLine(string.Format(settings.CopyrightFormat, year, settings.CompanyName));
            }
            else
            {
                sb.AppendLine($"© {year} {settings.CompanyName}. All rights reserved.");
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string fullPath = Path.Combine(projectRoot, settings.MarkdownExportPath);
            
            File.WriteAllText(fullPath, sb.ToString());
            Debug.Log($"[VersionHistory] Markdown exported to: {fullPath}");
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