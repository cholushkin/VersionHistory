using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;

namespace GameLib.VersionHistory.Editor
{
    public static class GitRepositoryService
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

        public static async UniTask<List<string>> FetchCommitsAsync(string gitPath, string revisionRange = "")
        {
            // If revisionRange is provided (e.g., "v1.0.0..v1.1.0"), it only fetches those commits.
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

            if (currentCommit.Length > 0)
            {
                commits.Add(currentCommit.ToString().Trim());
            }

            return commits;
        }
    }
}