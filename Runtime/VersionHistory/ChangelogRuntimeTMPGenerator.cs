// todo: allow custom styling configurations (font size, spacing) as an injected config parameter
// idea: support parsing markdown-like bold/italic natively into TMP tags for descriptions

using System.Linq;
using System.Text;

namespace GameLib.VersionHistory.Runtime
{
    public static class ChangelogRuntimeTMPGenerator
    {
        public static string Generate(VersionHistory history, bool includeAll = false)
        {
            if (history == null || history.Versions == null) return string.Empty;

            var sb = new StringBuilder();

            foreach (var version in history.Versions)
            {
                if (!includeAll && !version.IsIncluded) continue;

                var changes = includeAll 
                    ? version.Changes 
                    : version.Changes.Where(c => c.IsIncluded).ToList();

                if (changes.Count == 0) continue;

                // Version Header
                sb.AppendLine($"<size=110%><b>{version.VersionName}</b></size>");

                // Version Changes
                foreach (var change in changes)
                {
                    string colorHex = GetCategoryColor(change.Category);
                    sb.AppendLine($"  • <color={colorHex}><b>[{change.Category}]</b></color> {change.Description}");
                }
                
                sb.AppendLine(); // Spacer between versions
            }

            return sb.ToString().TrimEnd();
        }

        private static string GetCategoryColor(ChangeCategory category)
        {
            return category switch
            {
                ChangeCategory.Feature => "#4CAF50",
                ChangeCategory.Fix => "#F44336",
                ChangeCategory.Performance => "#2196F3",
                ChangeCategory.BreakingChange => "#FF9800",
                ChangeCategory.Internal => "#9E9E9E",
                ChangeCategory.Build => "#795548",
                ChangeCategory.Documentation => "#9C27B0",
                _ => "#FFFFFF"
            };
        }
    }
}