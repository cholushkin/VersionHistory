using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameLib.VersionHistory.Editor
{
    public static class GitCommandRunner
    {
        public static async UniTask<List<string>> ExecuteAsync(string workingDirectory, string arguments)
        {
            var output = new List<string>();
            var tcs = new UniTaskCompletionSource<int>();

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // Enable asynchronous event routing
            process.EnableRaisingEvents = true;

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    output.Add(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    UnityEngine.Debug.LogError($"[Git] Error: {e.Data}");
                }
            };

            process.Exited += (sender, args) =>
            {
                tcs.TrySetResult(process.ExitCode);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Yield and wait for the process to exit without blocking the Unity Editor
                int exitCode = await tcs.Task;

                if (exitCode != 0)
                {
                    UnityEngine.Debug.LogError($"[Git] Command 'git {arguments}' exited with code {exitCode}.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                tcs.TrySetException(ex);
            }

            return output;
        }
    }
}