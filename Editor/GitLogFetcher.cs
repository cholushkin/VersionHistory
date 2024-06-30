using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace VersionHistory
{
	public class GitLogFetcher
	{
		private string _gitPath;
		private List<string> _output;
		private StringBuilder _currentCommitMessage;
		private const string CommitSeparator = "###!___commit begin___!###";
		private const string GitLogCommand = "log --pretty=format:\"{0}%B\"";

		public GitLogFetcher(string pathToGidDir)
		{
			_gitPath = pathToGidDir;
		}

		public List<string> FetchCommitMessages()
		{
			// Create the process start info
			ProcessStartInfo processStartInfo = new ProcessStartInfo("git");
			processStartInfo.Arguments =
				string.Format(GitLogCommand,
					CommitSeparator); // This formats the log output to show commit hash and message
			processStartInfo.CreateNoWindow = true;
			processStartInfo.UseShellExecute = false;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			processStartInfo.WorkingDirectory = _gitPath; // Set the working directory (useful for submodules)

			// Create the process and execute the command
			using (Process process = new Process())
			{
				process.StartInfo = processStartInfo;
				process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
				process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);


				_output = new List<string>(128);
				_currentCommitMessage = new StringBuilder();

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				process.WaitForExit();
			}

			FlushCurrentCommit();
			return _output;
		}

		private void FlushCurrentCommit()
		{
			if(_currentCommitMessage.Length < 1)
				return;
			_currentCommitMessage.Remove(0, CommitSeparator.Length);
			_output.Add(_currentCommitMessage.ToString().Trim());
			_currentCommitMessage.Clear();
		}

		private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			if (outLine.Data == null || string.IsNullOrWhiteSpace(outLine.Data))
			{
				return;
			}

			if (outLine.Data.StartsWith(CommitSeparator))
				FlushCurrentCommit();

			_currentCommitMessage.AppendLine(outLine.Data);
		}

		void ErrorHandler(object sendingProcess, DataReceivedEventArgs errLine)
		{
			if (!string.IsNullOrEmpty(errLine.Data))
			{
				UnityEngine.Debug.LogError(errLine.Data);
			}
		}
	}
}