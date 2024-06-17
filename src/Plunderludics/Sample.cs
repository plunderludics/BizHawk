using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileSystemGlobbing;
using BizHawk.Client;
using BizHawk.Client.Common;

#nullable enable

// Represents a 'sample', ie a set of pathnames for each of:
//  rom, config, savestate, lua script(s)

namespace Plunderludics
{
	public class Sample
	{
		public readonly string RomPath;
		public readonly string? ConfigPath;
		public readonly string? SaveStatePath;
		public readonly string[] LuaScriptPaths;

		private static readonly string rompathTxtFileName = "rompath.txt";

		public Sample(string romPath, string? configPath, string? saveStatePath, string[] luaScriptPaths) {
			this.RomPath = romPath;
			this.ConfigPath = configPath;
			this.SaveStatePath = saveStatePath;
			this.LuaScriptPaths = luaScriptPaths;
		}

		// Search the provided directory for files based on file extension
		// (this has a lot of issues, we should improve this format)
		// [ie maybe just a json that points to all the necessary files]
		public static Sample LoadFromDir(string sampleFilePath) {
			var sampleDirPath = Path.GetDirectoryName(sampleFilePath); // workaround for the fact that we don't have an easy way to open a 'select directory' dialog

			// Assume there's a file called rompath.txt that provides a path to the rom (on the first line)
			var rompathTxtPath = Path.Combine(sampleDirPath, rompathTxtFileName);
			var lines = File.ReadLines(rompathTxtPath).ToList();
			var romPathRelative = lines[0];
			// For dumb historical reasons the rompath is currently given relative to the parent of the sample directory
			// [we should fix this and update all the existing samples]
			var romPathFull = Path.Combine(Path.GetDirectoryName(sampleDirPath), romPathRelative);
			romPathFull = Path.GetFullPath(romPathFull); // partly 'normalize' the path ie avoid stuff like x/../y

			// Look for config, save state, and lua based on extension
			Matcher matcher = new();
			matcher.AddInclude("*.ini");
			List<string> configFiles = matcher.GetResultsInFullPath(sampleDirPath).ToList();
			if (configFiles.Count > 1) {
				Console.WriteLine($"Warn: sample at {sampleDirPath} contains more than one .ini file, will use only {configFiles[0]}");
			}

			string? configPath = configFiles.Count > 0 ? configFiles[0] : null;

			matcher = new();
			matcher.AddInclude("*.lua");
			List<string> luaFiles = matcher.GetResultsInFullPath(sampleDirPath).ToList();

			matcher = new();
			matcher.AddInclude("*.State");
			List<string> saveFiles = matcher.GetResultsInFullPath(sampleDirPath).ToList();
			if (saveFiles.Count > 1) {
				Console.WriteLine($"Warn: sample at {sampleDirPath} contains more than one .State file, will use only {saveFiles[0]}");
			}

			string? saveStatePath = saveFiles.Count > 0 ? saveFiles[0] : null;

			return new Sample(
				romPathFull,
				configPath,
				saveStatePath,
				luaFiles.ToArray()
			);
		}
	}
}
