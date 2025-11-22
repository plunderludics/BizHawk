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

		public readonly string? cmdDumpName;

		public readonly bool _autoCloseOnDump;

		public readonly bool _chromeless;

		public readonly bool startFullscreen;

		public readonly bool GDIPlusRequested;

		public readonly string? luaScript;

		public readonly bool luaConsole;

		public readonly (string IP, ushort Port)? SocketAddress;

		public readonly ProtocolType SocketProtocol;

		public readonly IReadOnlyList<(string Key, string Value)>? UserdataUnparsedPairs;

		public readonly string? MMFFilename;

		public readonly (string? UrlGet, string? UrlPost)? HTTPAddresses;

		public readonly bool? audiosync;

		public readonly string? openExtToolDll;

		public readonly string? customWindowTitle;

		public readonly string? cmdRom;
		// [UnityHawk]
		public readonly string? extToolsDir;
		public readonly string? firmwareDir;
		public readonly string? savestateSaveDir;
		public readonly string? savestateExtension;
		public readonly string? ramWatchFile;
		public readonly string? ramWatchSaveDir;
		public readonly bool headless;
		public readonly bool? acceptBackgroundInput;
		public readonly bool suppressPopups;
		public readonly bool? mute;

		public ParsedCLIFlags(
			int? cmdLoadSlot,
			string? cmdLoadState,
			string? cmdConfigFile,
			string? cmdMovie,
			string? cmdDumpType,
			HashSet<int>? currAviWriterFrameList,
			int autoDumpLength,
			string? cmdDumpName,
			bool autoCloseOnDump,
			bool chromeless,
			bool startFullscreen,
			bool gdiPlusRequested,
			string? luaScript,
			bool luaConsole,
			(string IP, ushort Port)? socketAddress,
			string? mmfFilename,
			(string? UrlGet, string? UrlPost)? httpAddresses,
			bool? audiosync,
			string? openExtToolDll,
			ProtocolType socketProtocol,
			IReadOnlyList<(string Key, string Value)>? userdataUnparsedPairs,
			string? cmdRom,
			// [UnityHawk]
			string? customWindowTitle,
			string? extToolsDir,
			string? firmwareDir,
			string? savestateSaveDir,
			string? savestateExtension,
			string? ramWatchFile,
			string? ramWatchSaveDir,
			bool headless,
			bool? acceptBackgroundInput,
			bool suppressPopups,
			bool? mute
		) {
			this.cmdLoadSlot = cmdLoadSlot;
			this.cmdLoadState = cmdLoadState;
			this.cmdConfigFile = cmdConfigFile;
			this.cmdMovie = cmdMovie;
			this.cmdDumpType = cmdDumpType;
			_currAviWriterFrameList = currAviWriterFrameList;
			_autoDumpLength = autoDumpLength;
			this.cmdDumpName = cmdDumpName;
			_autoCloseOnDump = autoCloseOnDump;
			_chromeless = chromeless;
			this.startFullscreen = startFullscreen;
			GDIPlusRequested = gdiPlusRequested;
			this.luaScript = luaScript;
			this.luaConsole = luaConsole;
			SocketAddress = socketAddress;
			MMFFilename = mmfFilename;
			HTTPAddresses = httpAddresses;
			this.audiosync = audiosync;
			this.openExtToolDll = openExtToolDll;
			SocketProtocol = socketProtocol;
			UserdataUnparsedPairs = userdataUnparsedPairs;
			this.cmdRom = cmdRom;
			// [UnityHawk]
			this.customWindowTitle = customWindowTitle;
			this.extToolsDir = extToolsDir;
			this.firmwareDir = firmwareDir;
			this.savestateSaveDir = savestateSaveDir;
			this.savestateExtension = savestateExtension;
			this.ramWatchFile = ramWatchFile;
			this.ramWatchSaveDir = ramWatchSaveDir;
			this.headless = headless;
			this.acceptBackgroundInput = acceptBackgroundInput;
			this.suppressPopups = suppressPopups;
			this.mute = mute;
		}
	}
}
