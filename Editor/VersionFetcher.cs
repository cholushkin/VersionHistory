using System.Collections.Generic;
using Debug = UnityEngine.Debug;


namespace VersionHistory
{
	public class VersionsFetcher
	{
		private string _gitPath;
		private List<string> _output;
		private const string _gitCommand = "tag";

		public VersionsFetcher(string pathToGidDir)
		{
			_gitPath = pathToGidDir;
		}

		public List<string> FetchTags()
		{
			var git = new Git(_gitPath);
			git.ExecuteCommand("tag");
			foreach (var output in git.Output)
			{
				Debug.Log(output);
			}

			return null;
		}
	}
}