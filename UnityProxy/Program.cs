﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace UnityProxy
{
	class Program
	{
		/// <summary>
		/// Magic string our build pipeline writes to the log after a successful build.
		/// </summary>
		private const string SuccessMagicString = "Exiting batchmode successfully now!";

		static void Main(string[] args)
		{
			string unityPath, artifactsPath;
			int startingArgumentIndex = ParseArguments(args, out unityPath, out artifactsPath);
			string logPath = Path.GetTempFileName();

			Watcher watcher = new Watcher(logPath);
			Thread watcherThread = new Thread(watcher.Run);
			watcherThread.Start();

			Process unity = new Process();
			unity.StartInfo = new ProcessStartInfo(unityPath);

			unity.StartInfo.Arguments = "-logFile \"" + logPath + "\"";

			for (int i = startingArgumentIndex; i < args.Length; i++)
			{
				unity.StartInfo.Arguments += " \"" + args[i] + "\"";
			}

			Console.WriteLine("Starting Unity with arguments: " + unity.StartInfo.Arguments);

			unity.Start();

			Console.WriteLine("##teamcity[setParameter name='{0}' value='{1}']", "unityPID", unity.Id);

			unity.WaitForExit();
			watcher.Stop();
			watcherThread.Join();

			if (artifactsPath != null)
			{
				SaveArtifacts(artifactsPath, watcher.FullLog);
			}

			if (unity.ExitCode == 0 && watcher.FullLog.Contains(SuccessMagicString) && !watcher.Failed)
			{
				Console.WriteLine("Success.");
				Environment.Exit(0);
			}
			else
			{
				Console.WriteLine("Failure.");
				Environment.Exit(watcher.Failed ? 1 : unity.ExitCode);
			}
		}

		/// <summary>
		/// Saves build artifacts (log file) to the specified path.
		/// </summary>
		private static void SaveArtifacts(string artifactsPath, string logText)
		{
			Directory.CreateDirectory(artifactsPath);

			File.WriteAllText(artifactsPath + "/editor.log", logText);
		}

		/// <summary>
		/// Parses command line arguments.
		/// </summary>
		/// <param name="args">Arguments</param>
		/// <param name="unityPath">Path to the unity executable.</param>
		/// <param name="artifactsPath">PAth where the artifacts should be saved.</param>
		/// <returns>The number of arguments parsed.</returns>
		private static int ParseArguments(string[] args, out string unityPath, out string artifactsPath)
		{
			unityPath = args[0];
			artifactsPath = null;

			if (args.Length > 1)
			{
				if (args[1] == "-artifactsPath")
				{
					artifactsPath = args[2];
					return 3;
				}
			}
			return 1;
		}
	}
}
