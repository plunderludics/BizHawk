#nullable enable

using System.Collections.Generic;
using System.Net.Sockets;

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

		public readonly ProtocolType SocketProtocol;

		public readonly IReadOnlyList<(string Key, string Value)>? UserdataUnparsedPairs;

		public readonly string? MMFFilename;

		public readonly (string? UrlGet, string? UrlPost)? HTTPAddresses;

		public readonly bool? audiosync;

		public readonly string? openExtToolDll;

		public readonly string? customWindowTitle;

		public readonly string? cmdRom;
		public readonly string? firmwareDir;
		public readonly string? savestateDir;
		public readonly string? savestateExtension;
		public readonly string? ramWatchFile;
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
			ProtocolType socketProtocol,
			IReadOnlyList<(string Key, string Value)>? userdataUnparsedPairs,
			string? cmdRom,
			// [UnityHawk]:
			string? customWindowTitle,
			string? firmwareDir,
			string? savestateDir,
			string? savestateExtension,
			string? ramWatchFile,
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
<<<<<<< HEAD
			this.customWindowTitle = customWindowTitle;
=======
			SocketProtocol = socketProtocol;
			UserdataUnparsedPairs = userdataUnparsedPairs;
>>>>>>> 2.9.1-mirror
			this.cmdRom = cmdRom;
			this.firmwareDir = firmwareDir;
			this.savestateDir = savestateDir;
			this.savestateExtension = savestateExtension;
			this.ramWatchFile = ramWatchFile;
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
