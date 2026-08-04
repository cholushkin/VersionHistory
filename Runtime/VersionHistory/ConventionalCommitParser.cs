using System.Text.RegularExpressions;

namespace GameLib.VersionHistory.Editor
{
    public static class ConventionalCommitParser
    {
        // Matches: <type>[optional(scope)][optional!]: <description>
        // Example: feat(ui)!: added new button
        private static readonly Regex CommitPattern = new(
            @"^(?<type>[a-zA-Z]+)(?:\((?<scope>[a-zA-Z0-9_\-\.]+)\))?(?<breaking>!)?:\s*(?<description>[\s\S]*)",
            RegexOptions.Compiled);

        public static ParsedCommit Parse(string rawCommitMessage)
        {
            if (string.IsNullOrWhiteSpace(rawCommitMessage))
            {
                return new ParsedCommit(ChangeCategory.Uncategorized, rawCommitMessage);
            }

            var match = CommitPattern.Match(rawCommitMessage.Trim());
            
            if (!match.Success)
            {
                return new ParsedCommit(ChangeCategory.Uncategorized, rawCommitMessage.Trim());
            }

            string type = match.Groups["type"].Value.ToLowerInvariant();
            string scope = match.Groups["scope"].Success ? match.Groups["scope"].Value : string.Empty;
            bool isBreaking = match.Groups["breaking"].Success || rawCommitMessage.Contains("BREAKING CHANGE:");
            string description = match.Groups["description"].Value.Trim();

            ChangeCategory category = DetermineCategory(type, isBreaking);

            string formattedDescription = string.IsNullOrEmpty(scope) 
                ? description 
                : $"**{scope}**: {description}";

            return new ParsedCommit(category, formattedDescription);
        }

        private static ChangeCategory DetermineCategory(string type, bool isBreaking)
        {
            if (isBreaking) return ChangeCategory.BreakingChange;

            return type switch
            {
                "feat" => ChangeCategory.Feature,
                "fix" => ChangeCategory.Fix,
                "perf" => ChangeCategory.Performance,
                "docs" => ChangeCategory.Documentation,
                "build" or "ci" => ChangeCategory.Build,
                "refactor" or "chore" or "style" or "test" or "ops" => ChangeCategory.Internal,
                _ => ChangeCategory.Uncategorized
            };
        }
    }

    public readonly struct ParsedCommit
    {
        public ChangeCategory Category { get; }
        public string Description { get; }

        public ParsedCommit(ChangeCategory category, string description)
        {
            Category = category;
            Description = description;
        }
    }
}