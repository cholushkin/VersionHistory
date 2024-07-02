using System.Collections.Generic;
using Debug = UnityEngine.Debug;


namespace VersionHistory
{
	public class VersionsFetcher
	{
		private string _gitPath;
		private List<string> _output;
		private const string _gitCommand = "tag";
		private string _notVersionedYetLabel = "not-versioned-yet";

		public VersionsFetcher(string pathToGidDir)
		{
			_gitPath = pathToGidDir;
		}

		public List<string> FetchTags()
		{
			var git = new Git(_gitPath);
			git.ExecuteCommand("tag --sort=-v:refname");

			_output = new List<string>(git.Output.Count + 1);
			_output.Add(_notVersionedYetLabel);
			_output.AddRange(git.Output);
			
			
			Debug.Log($"Fetched version tags:");
			foreach (var output in _output)
			{
				Debug.Log(output);
			}

			return null;
		}
	}
}