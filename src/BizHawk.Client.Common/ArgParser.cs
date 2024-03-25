﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using BizHawk.Common.CollectionExtensions;

namespace BizHawk.Client.Common
{
	/// <summary>
	/// Parses command line flags from a string array into various instance fields.
	/// </summary>
	/// <remarks>
	/// If a flag is given multiple times, the last is taken.<br/>
	/// If a flag that isn't recognised is given, it is parsed as a filename. As noted above, the last filename is taken.
	/// </remarks>
	public static class ArgParser
	{
		/// <exception cref="ArgParserException"><c>--socket_ip</c> passed without specifying <c>--socket_port</c> or vice-versa</exception>
		public static void ParseArguments(out ParsedCLIFlags parsed, string[] args)
		{
			string? cmdLoadSlot = null;
			string? cmdLoadState = null;
			string? cmdConfigFile = null;
			string? cmdMovie = null;
			string? cmdDumpType = null;
			HashSet<int>? currAviWriterFrameList = null;
			int? autoDumpLength = null;
			bool? printVersion = null;
			string? cmdDumpName = null;
			bool? autoCloseOnDump = null;
			bool? chromeless = null;
			bool? startFullscreen = null;
			string? luaScript = null;
			bool? luaConsole = null;
			int? socketPort = null;
			string? socketIP = null;
			string? mmfFilename = null;
			string? urlGet = null;
			string? urlPost = null;
			bool? audiosync = null;
			string? openExtToolDll = null;
			string? cmdRom = null;
			string? firmwareDir = null;
			string? savestateDir = null;
			string? customWindowTitle = null;
			bool? headless = null;
			bool? acceptBackgroundInput = null;
			string? writeTextureToSharedBuffer = null;
			string? readInputFromSharedBuffer = null;
			string? shareAudioOverRpcBuffer = null;
			string? unityCallMethodBuffer = null;
			string? apiCallMethodBuffer = null;

			for (var i = 0; i < args.Length; i++)
			{
				var arg = args[i];

				if (arg == ">")
				{
					// For some reason sometimes visual studio will pass this to us on the commandline. it makes no sense.
					var stdout = args[++i];
					Console.SetOut(new StreamWriter(stdout));
					continue;
				}

				var argDowncased = arg.ToLower();
				if (argDowncased.StartsWith("--load-slot="))
				{
					cmdLoadSlot = argDowncased.Substring(argDowncased.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--load-state="))
				{
					cmdLoadState = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--config="))
				{
					cmdConfigFile = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--movie="))
				{
					cmdMovie = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--dump-type="))
				{
					cmdDumpType = argDowncased.Substring(argDowncased.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--dump-frames="))
				{
					string list = argDowncased.Substring(argDowncased.IndexOf('=') + 1);
					string[] items = list.Split(',');
					currAviWriterFrameList = new HashSet<int>();
					foreach (string item in items)
					{
						currAviWriterFrameList.Add(int.Parse(item));
					}

					// automatically set dump length to maximum frame
					autoDumpLength = currAviWriterFrameList.Order().Last();
				}
				else if (argDowncased.StartsWith("--version"))
				{
					printVersion = true;
				}
				else if (argDowncased.StartsWith("--dump-name="))
				{
					cmdDumpName = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--dump-length="))
				{
					var len = int.TryParse(argDowncased.Substring(argDowncased.IndexOf('=') + 1), out var i1) ? i1 : default;
					autoDumpLength = len;
				}
				else if (argDowncased.StartsWith("--dump-close"))
				{
					autoCloseOnDump = true;
				}
				else if (argDowncased.StartsWith("--chromeless"))
				{
					// chrome is never shown, even in windowed mode
					chromeless = true;
				}
				else if (argDowncased.StartsWith("--headless"))
				{
					// don't open any gui at all
					headless = true;
				}
				else if (argDowncased.StartsWith("--accept-background-input"))
				{
					acceptBackgroundInput = true;
				}
				else if (argDowncased.StartsWith("--fullscreen"))
				{
					startFullscreen = true;
				}
				else if (argDowncased.StartsWith("--lua="))
				{
					luaScript = arg.Substring(arg.IndexOf('=') + 1);
					luaConsole = true;
				}
				else if (argDowncased.StartsWith("--luaconsole"))
				{
					luaConsole = true;
				}
				else if (argDowncased.StartsWith("--socket_port="))
				{
					var port = int.TryParse(argDowncased.Substring(argDowncased.IndexOf('=') + 1), out var i1) ? i1 : default;
					if (port > 0) socketPort = port;
				}
				else if (argDowncased.StartsWith("--socket_ip="))
				{
					socketIP = argDowncased.Substring(argDowncased.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--mmf="))
				{
					mmfFilename = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--url_get="))
				{
					urlGet = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--url_post="))
				{
					urlPost = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--audiosync="))
				{
					audiosync = argDowncased.Substring(argDowncased.IndexOf('=') + 1) == "true";
				}
				else if (argDowncased.StartsWith("--open-ext-tool-dll="))
				{
					// the first ext. tool from ExternalToolManager.ToolStripMenu which satisfies both of these will be opened:
					// - available (no load errors, correct system/rom, etc.)
					// - dll path matches given string; or dll filename matches given string with or without `.dll`
					openExtToolDll = arg.Substring(20);
				}
				else if (argDowncased.StartsWith("--firmware="))
				{
					firmwareDir = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--savestates="))
				{
					savestateDir = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--windowtitle="))
				{
					customWindowTitle = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--write-texture-to-shared-buffer="))
				{
					writeTextureToSharedBuffer = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--read-input-from-shared-buffer="))
				{
					readInputFromSharedBuffer = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--share-audio-over-rpc-buffer="))
				{
					shareAudioOverRpcBuffer = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--unity-call-method-buffer="))
				{
					unityCallMethodBuffer = arg.Substring(arg.IndexOf('=') + 1);
				}
				else if (argDowncased.StartsWith("--api-call-method-buffer="))
				{
					apiCallMethodBuffer = arg.Substring(arg.IndexOf('=') + 1);
				}
				else
				{
					cmdRom = arg;
				}
			}

			var httpAddresses = urlGet == null && urlPost == null
				? ((string?, string?)?) null // don't bother
				: (urlGet, urlPost);
			(string, int)? socketAddress;
			if (socketIP == null && socketPort == null)
			{
				socketAddress = null; // don't bother
			}
			else if (socketIP == null || socketPort == null)
			{
				throw new ArgParserException("Socket server needs both --socket_ip and --socket_port. Socket server was not started");
			}
			else
			{
				socketAddress = (socketIP, socketPort.Value);
			}

			parsed = new ParsedCLIFlags(
				cmdLoadSlot: cmdLoadSlot is null ? null : int.Parse(cmdLoadSlot),
				cmdLoadState: cmdLoadState,
				cmdConfigFile: cmdConfigFile,
				cmdMovie: cmdMovie,
				cmdDumpType: cmdDumpType,
				currAviWriterFrameList: currAviWriterFrameList,
				autoDumpLength: autoDumpLength ?? 0,
				printVersion: printVersion ?? false,
				cmdDumpName: cmdDumpName,
				autoCloseOnDump: autoCloseOnDump ?? false,
				chromeless: chromeless ?? false,
				startFullscreen: startFullscreen ?? false,
				luaScript: luaScript,
				luaConsole: luaConsole ?? false,
				socketAddress: socketAddress,
				mmfFilename: mmfFilename,
				httpAddresses: httpAddresses,
				audiosync: audiosync,
				openExtToolDll: openExtToolDll,
				customWindowTitle: customWindowTitle,
				cmdRom: cmdRom,
				firmwareDir: firmwareDir,
				savestateDir: savestateDir,
				headless: headless ?? false,
				acceptBackgroundInput: acceptBackgroundInput ?? false,
				writeTextureToSharedBuffer: writeTextureToSharedBuffer,
				readInputFromSharedBuffer: readInputFromSharedBuffer,
				shareAudioOverRpcBuffer: shareAudioOverRpcBuffer,
				unityCallMethodBuffer: unityCallMethodBuffer,
				apiCallMethodBuffer: apiCallMethodBuffer
			);
		}

		public sealed class ArgParserException : Exception
		{
			public ArgParserException(string message) : base(message) {}
		}
	}
}
