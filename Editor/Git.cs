using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VersionHistory
{
	public class Git
	{
		private string _gitPath;
		public List<string> Output { get; private set; }


		public Git(string pathToGidDir)
		{
			_gitPath = pathToGidDir;
		}

		public void ExecuteCommand(string command, DataReceivedEventHandler dataEventHandler = null,
			DataReceivedEventHandler errorHandler = null)
		{
			Output = new List<string>();

			try
			{
				// Create the process start info
				ProcessStartInfo processStartInfo = new ProcessStartInfo("git");
				processStartInfo.Arguments = command;
				processStartInfo.CreateNoWindow = true;
				processStartInfo.UseShellExecute = false;
				processStartInfo.RedirectStandardOutput = true;
				processStartInfo.RedirectStandardError = true;
				processStartInfo.WorkingDirectory = _gitPath; // Set the working directory

				// Create and start the process
				Process process = new Process();
				process.StartInfo = processStartInfo;
				process.OutputDataReceived += dataEventHandler ?? DefaultOutputHandler;
				process.ErrorDataReceived += errorHandler ?? DefaultErrorHandler;

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit();
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogError(e);
				throw;
			}
		}

		private void DefaultOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			if(string.IsNullOrEmpty(outLine.Data))
				return;
			Output.Add(outLine.Data);
		}


		void DefaultErrorHandler(object sendingProcess, DataReceivedEventArgs errLine)
		{
			if (string.IsNullOrEmpty(errLine.Data))
				return;
			UnityEngine.Debug.LogError(errLine.Data);
		}
	}
}