#nullable enable

using System.Collections.Generic;

namespace BizHawk.Client.Common
{
	public readonly struct ParsedCLIFlags
	{
		public readonly int? cmdLoadSlot;

		public readonly string? cmdLoadState;

		public readonly string? cmdConfigFile;

		public readonly string? cmdMovie;

		public readonly string? cmdDumpType;

		public readonly HashSet<int>? _currAviWriterFrameList;

		public readonly int _autoDumpLength;

		public readonly bool printVersion;

		public readonly string? cmdDumpName;

		public readonly bool _autoCloseOnDump;

		public readonly bool _chromeless;

		public readonly bool startFullscreen;

		public readonly string? luaScript;

		public readonly bool luaConsole;

		public readonly (string IP, int Port)? SocketAddress;

		public readonly string? MMFFilename;

		public readonly (string? UrlGet, string? UrlPost)? HTTPAddresses;

		public readonly bool? audiosync;

		public readonly string? openExtToolDll;

		public readonly string? customWindowTitle;

		public readonly string? cmdRom;
		public readonly string? firmwareDir;
		public readonly string? savestateDir;
		public readonly bool headless;
		public readonly bool acceptBackgroundInput;
		public readonly string? writeTextureToSharedBuffer;
		public readonly string? readKeyInputFromSharedBuffer;
		public readonly string? readAnalogInputFromSharedBuffer;
		public readonly string? shareAudioOverRpcBuffer;
		public readonly string? unityCallMethodBuffer;
		public readonly string? apiCallMethodBuffer;

		public ParsedCLIFlags(
			int? cmdLoadSlot,
			string? cmdLoadState,
			string? cmdConfigFile,
			string? cmdMovie,
			string? cmdDumpType,
			HashSet<int>? currAviWriterFrameList,
			int autoDumpLength,
			bool printVersion,
			string? cmdDumpName,
			bool autoCloseOnDump,
			bool chromeless,
			bool startFullscreen,
			string? luaScript,
			bool luaConsole,
			(string IP, int Port)? socketAddress,
			string? mmfFilename,
			(string? UrlGet, string? UrlPost)? httpAddresses,
			bool? audiosync,
			string? openExtToolDll,
			string? customWindowTitle,
			string? cmdRom,
			string? firmwareDir,
			string? savestateDir,
			bool headless,
			bool acceptBackgroundInput,
			string? writeTextureToSharedBuffer,
			string? readKeyInputFromSharedBuffer,
			string? readAnalogInputFromSharedBuffer,
			string? shareAudioOverRpcBuffer,
			string? unityCallMethodBuffer,
			string? apiCallMethodBuffer
		) {
			this.cmdLoadSlot = cmdLoadSlot;
			this.cmdLoadState = cmdLoadState;
			this.cmdConfigFile = cmdConfigFile;
			this.cmdMovie = cmdMovie;
			this.cmdDumpType = cmdDumpType;
			_currAviWriterFrameList = currAviWriterFrameList;
			_autoDumpLength = autoDumpLength;
			this.printVersion = printVersion;
			this.cmdDumpName = cmdDumpName;
			_autoCloseOnDump = autoCloseOnDump;
			_chromeless = chromeless;
			this.startFullscreen = startFullscreen;
			this.luaScript = luaScript;
			this.luaConsole = luaConsole;
			SocketAddress = socketAddress;
			MMFFilename = mmfFilename;
			HTTPAddresses = httpAddresses;
			this.audiosync = audiosync;
			this.openExtToolDll = openExtToolDll;
			this.customWindowTitle = customWindowTitle;
			this.cmdRom = cmdRom;
			this.firmwareDir = firmwareDir;
			this.savestateDir = savestateDir;
			this.headless = headless;
			this.acceptBackgroundInput = acceptBackgroundInput;
			this.writeTextureToSharedBuffer = writeTextureToSharedBuffer;
			this.readKeyInputFromSharedBuffer = readKeyInputFromSharedBuffer;
			this.readAnalogInputFromSharedBuffer = readAnalogInputFromSharedBuffer;
			this.shareAudioOverRpcBuffer = shareAudioOverRpcBuffer;
			this.unityCallMethodBuffer = unityCallMethodBuffer;
			this.apiCallMethodBuffer = apiCallMethodBuffer;
		}
	}
}
