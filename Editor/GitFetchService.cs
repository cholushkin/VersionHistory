using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;

namespace GameLib.VersionHistory.Editor
{
    public static class GitFetchService
    {
        private const string COMMIT_SEPARATOR = "###!___commit begin___!###";
        private const string UNVERSIONED_LABEL = "not-versioned-yet";

        public static async UniTask<List<string>> FetchTagsAsync(string gitPath)
        {
            var tags = await GitCommandRunner.ExecuteAsync(gitPath, "tag --sort=-v:refname");
            var result = new List<string>(tags.Count + 1) { UNVERSIONED_LABEL };
            result.AddRange(tags);
            return result;
        }

        // Added includeUncategorized parameter
        public static async UniTask<List<VersionPoolGroup>> FetchCommitPoolAsync(string gitPath, string fromTag = null, string toTag = null, bool includeUncategorized = false)
        {
            var allTags = await FetchTagsAsync(gitPath);
            var poolGroups = new List<VersionPoolGroup>();

            if (allTags.Count == 0) return poolGroups;

            int toIndex = string.IsNullOrEmpty(toTag) ? 0 : allTags.IndexOf(toTag);
            int fromIndex = string.IsNullOrEmpty(fromTag) ? allTags.Count - 1 : allTags.IndexOf(fromTag);

            if (fromIndex < toIndex)
            {
                (fromIndex, toIndex) = (toIndex, fromIndex);
            }

            if (toIndex < 0) toIndex = 0;
            if (fromIndex < 0) fromIndex = allTags.Count - 1;

            for (int i = toIndex; i <= fromIndex; i++)
            {
                string currentTag = allTags[i];
                string previousTag = (i + 1 < allTags.Count) ? allTags[i + 1] : string.Empty;

                string revisionRange = string.IsNullOrEmpty(previousTag) ? currentTag : $"{previousTag}..{currentTag}";
                
                if (currentTag == UNVERSIONED_LABEL)
                {
                    revisionRange = string.IsNullOrEmpty(previousTag) ? "" : $"{previousTag}..HEAD";
                }

                var rawCommits = await FetchRawCommitsAsync(gitPath, revisionRange);
                var pooledCommits = new List<PooledCommit>();

                foreach (var raw in rawCommits)
                {
                    var parsed = ConventionalCommitParser.Parse(raw);
                    
                    // Filter out Uncategorized commits if the toggle is false
                    if (!includeUncategorized && parsed.Category == ChangeCategory.Uncategorized)
                    {
                        continue;
                    }
                                      
                    pooledCommits.Add(new PooledCommit(parsed.Category, parsed.Description, raw));
                }

                if (pooledCommits.Count > 0 || currentTag == UNVERSIONED_LABEL)
                {
                    poolGroups.Add(new VersionPoolGroup(currentTag, pooledCommits));
                }
            }

            return poolGroups;
        }

        private static async UniTask<List<string>> FetchRawCommitsAsync(string gitPath, string revisionRange)
        {
            string args = string.IsNullOrWhiteSpace(revisionRange) 
                ? $"log --pretty=format:\"{COMMIT_SEPARATOR}%B\"" 
                : $"log {revisionRange} --pretty=format:\"{COMMIT_SEPARATOR}%B\"";

            var rawLines = await GitCommandRunner.ExecuteAsync(gitPath, args);
            return ParseRawLogLines(rawLines);
        }

        private static List<string> ParseRawLogLines(IReadOnlyList<string> rawLines)
        {
            var commits = new List<string>();
            var currentCommit = new StringBuilder();

            foreach (var line in rawLines)
            {
                if (line.StartsWith(COMMIT_SEPARATOR))
                {
                    if (currentCommit.Length > 0)
                    {
                        commits.Add(currentCommit.ToString().Trim());
                        currentCommit.Clear();
                    }
                    currentCommit.AppendLine(line.Substring(COMMIT_SEPARATOR.Length));
                }
                else
                {
                    currentCommit.AppendLine(line);
                }
            }

            if (currentCommit.Length > 0) commits.Add(currentCommit.ToString().Trim());

            return commits;
        }
    }

    public class VersionPoolGroup
    {
        public string VersionTag { get; }
        public List<PooledCommit> Commits { get; }

        public VersionPoolGroup(string versionTag, List<PooledCommit> commits)
        {
            VersionTag = versionTag;
            Commits = commits;
        }
    }

    public class PooledCommit
    {
        public ChangeCategory Category { get; }
        public string FormattedDescription { get; }
        public string RawMessage { get; }
        public bool IsSelected { get; set; }

        public PooledCommit(ChangeCategory category, string formattedDescription, string rawMessage)
        {
            Category = category;
            FormattedDescription = formattedDescription;
            RawMessage = rawMessage;
            IsSelected = false; 
        }
    }
}