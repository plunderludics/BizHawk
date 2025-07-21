// String constants used in Unity-BizHawk communication


// Userdata args (passed over CLI)
public static class Args {
	public const string InputBuffer = "unityhawk-input-buffer";
	public const string TextureBuffer = "unityhawk-texture-buffer";
	public const string AudioRpc = "unityhawk-audio-rpc";
	public const string ApiCommandBuffer = "unityhawk-api-command-buffer";
	// public const string ApiCallRpc = "unityhawk-api-call-rpc";
	public const string CallMethodRpc = "unityhawk-call-method-rpc";
}

// API commands (write-only commands from unity to bizhawk)
// This could be an enum I guess but doesn't really matter
public static class ApiCommands {
	public const string LoadRom = nameof(LoadRom);
	public const string LoadState = nameof(LoadState);
	public const string SaveState = nameof(SaveState);
	public const string Pause = nameof(Pause);
	public const string Unpause = nameof(Unpause);
	public const string FrameAdvance = nameof(FrameAdvance);
	public const string SetVolume = nameof(SetVolume);
	public const string WriteUnsigned = nameof(WriteUnsigned);
	public const string WriteSigned = nameof(WriteSigned);
	public const string WriteFloat = nameof(WriteFloat);
	public const string Freeze = nameof(Freeze);
	public const string Unfreeze = nameof(Unfreeze);
	public const string Watch = nameof(Watch);
	public const string Unwatch = nameof(Unwatch);
	public const string SetSoundOn = nameof(SetSoundOn);
}

// Idk what to call these but these are sent from bizhawk to unity over the CallMethod rpc buffer
// (Which is the same 'namespace' as user-registered lua methods)
// TODO: These could also just use a separate buffer
//       (even a queue rather than an rpc buffer, since they don't need return values
//        - that way they would run on the main thread in unity as well)
public static class SpecialCommands {
	public const string ReceiveWatchedValue = "_UnityHawk_ReceiveWatchedValue";
	public const string OnRomLoaded = "_UnityHawk_OnRomLoaded";

	public static readonly string[] All = {
		ReceiveWatchedValue,
		OnRomLoaded
	};
}