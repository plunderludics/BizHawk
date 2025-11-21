using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;

using Plunderludics.UnityHawk.SharedBuffers;
using Plunderludics.UnityHawk.Shared;

using InputEvent = Plunderludics.UnityHawk.Shared.InputEvent;

namespace Plunderludics.UnityHawk.Tool
{
	[ExternalTool("UnityHawk", Description = "UnityHawk")]
	public class UnityHawkMainForm : ToolFormBase, IExternalToolForm
	{
		// Supposed to use this syntax for anything unsupported by APIs
		[RequiredService]
		private IEmulator _emu { get; set; }

		
		[RequiredService]
		private IMemoryDomains _memoryDomains { get; set; }

		// (This gets magically set by EmuHawk somehow)
		public ApiContainer _apiContainer { get; set; }

		private ApiContainer APIs => _apiContainer!;

		private IVideoProvider _videoProvider;
		private ISoundProvider _soundProvider;

		protected override string WindowTitleStatic => "UnityHawk";

		private SharedInputBuffer _inputBuffer;
		// private ApiCallRpc _apiCallRpc; // Disabled
		private ApiCommandBuffer _apiCommandBuffer;
		private SharedTextureBuffer _sharedTextureBuffer;
		private UnityHawkSound _unityHawkSound;

		private Dictionary<(string, int?), bool> buttonState = new(); // Current button state - need to pass to JoypadApi every frame
		private Dictionary<(string, int?), int?> analogState = new(); // Current analog state

		private Dictionary<(long Addr, int Size, string Domain), uint> _freezes = new(); // List of memory addresses to keep frozen
		private HashSet<(long Addr, int Size, bool IsBigEndian, WatchType Type, string Domain)> _watches = new(); // List of memory addresses that Unity wants to watch
		private Dictionary<(long Addr, int Size, bool IsBigEndian, WatchType Type, string Domain), string> _previousWatchValues = new(); // Track previous values to detect changes

		private Label text;

		// Copied this from HelloWorld/CustomMainForm.cs, it says it's a very bad idea but doesn't say why - how else can we modify config values?
		private Config GlobalConfig => (APIs.Emulation as EmulationApi ?? throw new Exception("required API wasn't fulfilled")).ForbiddenConfigReference;

		public UnityHawkMainForm()
		{
			// Console.WriteLine("UnityHawkMainForm()");
			this.text = new System.Windows.Forms.Label();
			this.SuspendLayout();

			// Text gets cut off for some reason
			// this.text.Location = new System.Drawing.Point(12, 57);
			this.text.Name = "text";
			this.text.Size = new System.Drawing.Size(100, 13);
			this.text.TabIndex = 0;
			this.text.Text = "UnityHawk :)"; // TODO could be nice to display some debug stuff here (are buffers open, etc)
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.text);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		/// When emuhawk opens, and whenever new rom is loaded
		public override void Restart() {
			Console.WriteLine("Restarting UnityHawk plugin...");

			// Clear freezes and watches
			// TODO: what if we're restarting with the same rom? Should we keep the watches+freezes in that case?
			_freezes.Clear();
			_watches.Clear();
			_previousWatchValues.Clear();
			
			// Open sharedmemory buffers
			if (_inputBuffer == null) {
				string inputBufferName = (string)APIs.UserData.Get(Args.InputBuffer);
				if (!string.IsNullOrEmpty(inputBufferName)) {
					_inputBuffer = new(inputBufferName);
				}
			}

			// API command buffer is for write-only commands that don't require a return value, run in main thread before each framae
			if (_apiCommandBuffer == null) {
				string apiCommandBufferName = (string)APIs.UserData.Get(Args.ApiCommandBuffer);
				if (!string.IsNullOrEmpty(apiCommandBufferName)) {
					// Init RPC buffer for API commands from unity
					_apiCommandBuffer = new(apiCommandBufferName);
				}
			}

			// API call buffer is for read-only commands that require a return value (and run in separate thread w arbitrary timing)
			// (Disabled for thread-safety issues)
			// if (_apiCallRpc == null) {
			// 	string apiCallRpcName = (string)APIs.UserData.Get(Args.ApiCallRpc);
			// 	if (!string.IsNullOrEmpty(apiCallRpcName)) {
			// 		_apiCallRpc = new(apiCallRpcName, ProcessApiRpcCall);
			// 	}
			// }
			
			// For lua callbacks to unity
			if (CallMethodRpc.Instance == null) {
				string callMethodBufferName = (string)APIs.UserData.Get(Args.CallMethodRpc);
				if (!string.IsNullOrEmpty(callMethodBufferName)) {
					// Init RPC buffer for CallMethod calls to unity (from lua)
					CallMethodRpc.Init(callMethodBufferName);
				}
			}

			// Texture buffer
			_videoProvider = _emu.AsVideoProviderOrDefault();
			// Need to init/re-init texture buffer here when rom changes because size depends on the video resolution of the platform
			string texBufName = (string)APIs.UserData.Get(Args.TextureBuffer);
			if (!string.IsNullOrEmpty(texBufName)) {
				// Init shared texture buffer for passing to unity
				int[] texbuf = _videoProvider.GetVideoBuffer();
				if (_sharedTextureBuffer == null) {
					_sharedTextureBuffer = new(texBufName, texbuf.Length);
				} else {
					// If buffer already exists, resize it to fit new emulator
					_sharedTextureBuffer.SetSize(texbuf.Length);
				}
			}

			// Audio buffer
			_soundProvider = _emu.AsSoundProviderOrDefault();
			string audioRpcName = (string)APIs.UserData.Get(Args.AudioRpc);
			if (!string.IsNullOrEmpty(audioRpcName)) {
				GlobalConfig.SoundVolume = 0; // Hack to disable native sound: TODO unity should probably mute via cli arg instead
				// (Don't set SoundEnabled = false, that disables the sound provider completely)

				// Init rpc buffer for passing audio to unity
				if (_unityHawkSound == null) {
					_unityHawkSound = new (audioRpcName, _soundProvider);
				} else {
					_unityHawkSound.SetSoundProvider(_soundProvider);
				}
			}

			OnRomLoaded(); // Notify unity that rom was loaded
		}

		protected override void FastUpdateBefore() {
			// Keep unityhawk running in turbo mode
			// TODO allow configuring this?
			UpdateBefore();
		}

		protected override void UpdateBefore() {
			// Console.WriteLine("UnityHawk: UpdateBefore");
			// Before frame
			if (_inputBuffer != null) {
				// Get input from input buffer and pass to emulator
				InputEvent? mie;
				while ((mie = _inputBuffer.Read()).HasValue) {
					InputEvent ie = mie.Value;
					int? controller = ie.controller > 0 ? ie.controller : null; // 0 is null controller
					if (ie.isAnalog) { // [We could maybe get this from the API somehow, but easier to just get unity to tell us]
						analogState[(ie.name, controller)] = ie.value;
						// Hm, if multiple analog input values come within the same frame, we currently drop all but the latest one
						// TODO: maybe should be averaging or some more complicated smoothing type thing
					} else {
						buttonState[(ie.name, controller)] = ie.value > 0;
					}
				}
			}
			// Send input state to JoypadApi (need to do this every frame)

			// (Instead of sending the full controller state [Joypad.Set(buttonState)],
			//  we only send input for pressed buttons -
			//  this means interacting through the bizhawk window
			//  still works which is convenient for dev)
			foreach (var button in buttonState) {
				(string name, int? controller) = button.Key;
				bool pressed = button.Value;
				if (pressed) {
					APIs.Joypad.Set(name, pressed, controller);
				}
			}
			// For analog just override (so native input won't work)
			// (api has no way to add two input sources)
			foreach (var axis in analogState) {
				(string name, int? controller) = axis.Key;
				APIs.Joypad.SetAnalog(name, axis.Value, controller);
			}

			// Process api commands from unity
			if (_apiCommandBuffer != null) {
				ProcessApiCommands();
			}
			// Apply ram freezes (need to reset the value at the beginning of each frame)
			ApplyFreezes();
		}

		protected override void FastUpdateAfter() {
			// Keep unityhawk running in turbo mode
			UpdateAfter();
		}

		protected override void UpdateAfter() {
			// Console.WriteLine("UnityHawk: UpdateAfter");
			// After frame
			// Send texture through texture buffer
			if (_sharedTextureBuffer != null) {
				int[] pixels = _videoProvider.GetVideoBuffer();
				int width =  _videoProvider.BufferWidth;
				int height =  _videoProvider.BufferHeight;
				int frame = APIs.Emulation.FrameCount();
				// Pass current frame index along with texture to make it possible to sync with lua rpc calls
				// (due to small unpredictable lag in the shared buffer write)
				_sharedTextureBuffer.Write(pixels, width, height, frame);
			}
			
			// Send any values that unity wants to watch over rpc
			// (Do it after the frame means unity can check if frozen values were changed)
			ProcessWatches();

			// Send audio through audio buffer
			_unityHawkSound?.Update();
		}

		protected override void UpdatePaused() {
			// Called when the emulator is paused
			// Need to keep processing api commands so we can receive Unpause or FrameAdvance
			// Console.WriteLine("UnityHawk: Paused");
			if (_apiCommandBuffer != null) {
				ProcessApiCommands();
			}
		}

		// For write-only api calls from unity that don't require a return value - run on main thread before each frame
		// [Should probably go in different file]
		private void ProcessApiCommands() {
			MethodCall? mcq;
			while ((mcq = _apiCommandBuffer.Read()).HasValue) {
				MethodCall mc = mcq.Value;
				Console.WriteLine($"Receiving api command {mc}");
				switch (mc.MethodName) {
				case ApiCommands.LoadRom:
					APIs.EmuClient.OpenRom(mc.Argument);
					break;
				case ApiCommands.LoadState:
					bool success = APIs.EmuClient.LoadState(mc.Argument, isFullPath: true);
					if (!success) {
						Console.WriteLine($"Warning: Unity attempted to load state {mc.Argument} but it failed");
					}
					break;
				case ApiCommands.SaveState:
					APIs.EmuClient.SaveState(mc.Argument, isFullPath: true);
					break;
				case ApiCommands.Pause:
					APIs.EmuClient.Pause();
					break;
				case ApiCommands.Unpause:
					APIs.EmuClient.Unpause();
					break;
				case ApiCommands.FrameAdvance:
					APIs.EmuClient.DoFrameAdvance();
					break;
				case ApiCommands.SetVolume:
					APIs.EmuClient.SetVolume(int.Parse(mc.Argument));
					break;
				case ApiCommands.SetSoundOn:
					APIs.EmuClient.SetSoundOn(bool.Parse(mc.Argument));
					break;
				case ApiCommands.SetSpeedPercent:
					// arg: int percentage (>=0)
					GlobalConfig.SpeedPercent = int.Parse(mc.Argument); // No api for this, set config directly
					break;
				// Note: For Write, Freeze and Watch methods,
				// `domain`, if not provided, defaults to the main memory domain (NOT the most recent used domain which is what MemoryApi does)
				// TODO hm maybe WriteXXX should all be merged into one Write method with a type parameter
				case ApiCommands.WriteUnsigned: {
					/*(long address, uint value, int size, bool isBigEndian, string domain = null)*/
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					uint value = uint.Parse(args[1]);
					int size = int.Parse(args[2]);
					bool isBigEndian = bool.Parse(args[3]);
					string domain = (args.Length > 4) ? args[4] : APIs.Memory.MainMemoryName;

					APIs.Memory.SetBigEndian(isBigEndian);
					switch (size)
					{
					case 1:
						APIs.Memory.WriteU8(address, value, domain);
						break;
					case 2:
						APIs.Memory.WriteU16(address, value, domain);
						break;
					case 3:
						APIs.Memory.WriteU24(address, value, domain);
						break;
					case 4:
						APIs.Memory.WriteU32(address, value, domain);
						break;
					default:
						throw new InvalidOperationException($"Invalid size {size} for WriteUnsigned");
					}
					break;
				}
				case ApiCommands.WriteSigned: {
					/*(long address, int value, int size, bool isBigEndian, string domain = null)*/
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					int value = int.Parse(args[1]);
					int size = int.Parse(args[2]);
					bool isBigEndian = bool.Parse(args[3]);
					string domain = (args.Length > 4) ? args[4] : APIs.Memory.MainMemoryName;

					APIs.Memory.SetBigEndian(isBigEndian);
					switch (size)
					{
					case 1:
						APIs.Memory.WriteS8(address, value, domain);
						break;
					case 2:
						APIs.Memory.WriteS16(address, value, domain);
						break;
					case 3:
						APIs.Memory.WriteS24(address, value, domain);
						break;
					case 4:
						APIs.Memory.WriteS32(address, value, domain);
						break;
					default:
						throw new InvalidOperationException($"Invalid size {size} for WriteSigned");
					}
					break;
				}
				case ApiCommands.WriteFloat: {
					/*(long address, float value, bool isBigEndian, string domain = null)*/
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					float value = float.Parse(args[1], CultureInfo.InvariantCulture);
					bool isBigEndian = bool.Parse(args[2]);
					string domain = (args.Length > 3) ? args[3] : APIs.Memory.MainMemoryName;
					APIs.Memory.SetBigEndian(isBigEndian);
					APIs.Memory.WriteFloat(address, value, domain);
					break;
				}

				// WatchXXX methods a request for bizhawk to read the given address and send it to Unity via RPC after each frame
				// (We do it like this to avoid thread-safety issues with a more straightforward ReadXXX method, see below)
				// (I guess we could allow passing in an id for these? To get in the callback? But maybe not necessary)
				case ApiCommands.Watch : {
					/*(long address, int size, bool isBigEndian, WatchType type, string domain = null)*/
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					int size = int.Parse(args[1]);
					bool isBigEndian = bool.Parse(args[2]);
					WatchType type = (WatchType)Enum.Parse(typeof(WatchType), args[3], ignoreCase: true);
					string domain = (args.Length > 4) ? args[4] : null;

					if (type == WatchType.Float && size != 4) {
						throw new InvalidOperationException($"Invalid size {size} for WatchType.Float");
					}

					Console.WriteLine($"UnityHawk: Watching {domain} {address} size {size} type {type} big-endian {isBigEndian}");

					_watches.Add((address, size, isBigEndian, type, domain));

					break;
				}
				case ApiCommands.Unwatch: {					
					/*(long address, int size, bool isBigEndian, WatchType type, string domain = null)*/
					// Only unwatch if a watch was created for same (address, size, isBigEndian, type, domain)
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					int size = int.Parse(args[1]);
					bool isBigEndian = bool.Parse(args[2]);
					WatchType type = (WatchType)Enum.Parse(typeof(WatchType), args[3], ignoreCase: true);
					string domain = (args.Length > 4) ? args[4] : null;
					
					var key = (address, size, isBigEndian, type, domain);
					_watches.Remove(key);
					_previousWatchValues.Remove(key); // Clear previous value when unwatching
					break;
				}

				case ApiCommands.Freeze: {
					/*(long address, int size, string domain = null)*/
					// (Do we need to provide a way to freeze to a specific value?)
					// Console.WriteLine($"UnityHawk: Freeze {mc.Argument}");
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					int size = int.Parse(args[1]);
					string domain = (args.Length > 2) ? args[2] : APIs.Memory.MainMemoryName;
					
					// Need to know current value so we can freeze to that value (could also just take this as arg, idk)
					// (Cheat constructor seems to take value as signed int?)
					APIs.Memory.SetBigEndian(true);

					uint currentValue = size switch {
						1 => APIs.Memory.ReadU8(address, domain),
						2 => APIs.Memory.ReadU16(address, domain),
						3 => APIs.Memory.ReadU24(address, domain),
						4 => APIs.Memory.ReadU32(address, domain),
						_ => throw new InvalidOperationException($"Invalid size {size} for Freeze")
					};

					Console.WriteLine($"UnityHawk: Freezing {domain} {address} to uint {currentValue}");
					_freezes[(address, size, domain)] = currentValue;

					// Previously tried implementing using Watch/CheatList but it does weird stuff for some reason:

					// WatchSize watchSize = size switch {
					// 	1 => WatchSize.Byte,
					// 	2 => WatchSize.Word,
					// 	4 => WatchSize.DWord,
					// 	_ => throw new InvalidOperationException($"Invalid size {size} for Freeze")
					// };

					// MemoryDomain memoryDomain = _memoryDomains[domain];
					// if (memoryDomain == null) {
					// 	Console.WriteLine($"Warning: Freeze: Domain {domain} doesn't exist");
					// 	break;
					// }

					// Watch w = Watch.GenerateWatch(
					// 	domain: memoryDomain,
					// 	address: address,
					// 	size: watchSize,
					// 	type: WatchDisplayType.Binary, // Only for display, doesn't actually matter
					// 	bigEndian: true // Again only for display, doesn't matter
					// );

					// Console.WriteLine($"UnityHawk: Freezing {domain} {address} to {currentValue}");

					// MainForm.CheatList.Add(new Cheat(w, currentValue));
					
					break;
				}
				case ApiCommands.Unfreeze: {
					/*(long address, int size, string domain = null)*/
					// Only unfreezes if a freeze was created for same (address, size, domain)
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					int size = int.Parse(args[1]);
					string domain = (args.Length > 2) ? args[2] : APIs.Memory.MainMemoryName;

					_freezes.Remove((address, size, domain));

					// MemoryDomain memoryDomain = _memoryDomains[domain];
					// if (memoryDomain == null) {
					// 	Console.WriteLine($"Warning: FreezeBytes: Domain {domain} doesn't exist");
					// 	break;
					// }

					// Cheat existingCheat = MainForm.CheatList[memoryDomain, address];
					// if (existingCheat is null) {
					// 	Console.WriteLine($"Warning: Can't unfreeze {domain} {address} because not currently frozen");
					// 	break;
					// }

					// MainForm.CheatList.Remove(existingCheat);

					break;
				}
				default:
					Console.WriteLine($"Warning: Unity attempting to send unsupported bizhawk api command {mc.MethodName}");
					break;
				}
			}
		}

		// This is only for api calls that require a return value - others are handled on the main thread in UpdateValues
		// 2025-06-19: This whole setup seems to be a bad idea for thread safety reasons
		// (Unity calls these via RPC at an arbitrary time on a separate thread, which might be while bizhawk is doing some other memory-related stuff)
		// (At least I'm guessing that's what was happening, was having periodic crashes on Medal of Honor PSX when using ReadXXX methods)
		// So instead of ReadXXX methods we have WatchXXX methods, and instead of GetSystemId we have a special rpc callback that bizhawk calls when a rom is loaded

		// private string ProcessApiRpcCall(string methodName, string argString) {
		// 	switch (methodName) {
		// 		case ApiCommands.GetSystemId:
		// 			return APIs.Emulation.GetSystemId();

		// 		case "ReadUnsigned": {
		// 			/*(long address, int size, bool isBigEndian, string domain = null)*/
		// 			// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
		// 			var args = argString.Split(',');
		// 			long address = long.Parse(args[0]);
		// 			int size = int.Parse(args[1]);
		// 			bool isBigEndian = bool.Parse(args[2]);
		// 			string domain = (args.Length > 3) ? args[3] : APIs.Memory.MainMemoryName;

		// 			APIs.Memory.SetBigEndian(isBigEndian); // (Idk why MemoryApi is weird like this..)

		// 			// Annoyingly MemoryApi has a ReadUnsigned method but only exposes the ReadU* methods..
		// 			return (size switch {
		// 				1 => APIs.Memory.ReadU8(address, domain).ToString(),
		// 				2 => APIs.Memory.ReadU16(address, domain).ToString(),
		// 				3 => APIs.Memory.ReadU24(address, domain).ToString(),
		// 				4 => APIs.Memory.ReadU32(address, domain).ToString(),
		// 				_ => throw new InvalidOperationException($"Invalid size {size} for ReadUnsigned")
		// 			});
		// 		}
		// 		case "ReadSigned": {
		// 			/*(long address, int size, bool isBigEndian, string domain = null)*/
		// 			// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
		// 			var args = argString.Split(',');
		// 			long address = long.Parse(args[0]);
		// 			int size = int.Parse(args[1]);
		// 			bool isBigEndian = bool.Parse(args[2]);
		// 			string domain = (args.Length > 3) ? args[3] : APIs.Memory.MainMemoryName;

		// 			APIs.Memory.SetBigEndian(isBigEndian);

		// 			return (size switch {
		// 				1 => APIs.Memory.ReadS8(address, domain).ToString(),
		// 				2 => APIs.Memory.ReadS16(address, domain).ToString(),
		// 				3 => APIs.Memory.ReadS24(address, domain).ToString(),
		// 				4 => APIs.Memory.ReadS32(address, domain).ToString(),
		// 				_ => throw new InvalidOperationException($"Invalid size {size} for ReadUnsigned")
		// 			});
		// 		}
		// 		case "ReadFloat": {
		// 			/*(long address, bool isBigEndian, string domain = null)*/
		// 			// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
		// 			var args = argString.Split(',');
		// 			long address = long.Parse(args[0]);
		// 			bool isBigEndian = bool.Parse(args[1]);
		// 			string domain = (args.Length > 2) ? args[2] : APIs.Memory.MainMemoryName;

		// 			return APIs.Memory.ReadFloat(address, domain).ToString();
		// 		}
		// 		default:
		// 			Console.WriteLine($"UnityHawk: Unknown API call {methodName} with args {argString}");
		// 			return null;
		// 	}
		// }

		private void ProcessWatches() {
			// Process all the watches and send them to unity only when values change
   			foreach (var watch in _watches) {
				(long addr, int size, bool isBigEndian, WatchType type, string domain) = watch;
				string actualDomain = domain ?? APIs.Memory.MainMemoryName; // Default to main memory domain if not provided
				APIs.Memory.SetBigEndian(isBigEndian);
				string currentValue = type switch
				{
					// (This is kind of overkill since we're reading the same bytes in each case,
					//  but this way don't have to worry about bit conversion and just trust bizhawk)
					WatchType.Unsigned => (size switch
					{
						1 => APIs.Memory.ReadU8(addr, actualDomain),
						2 => APIs.Memory.ReadU16(addr, actualDomain),
						3 => APIs.Memory.ReadU24(addr, actualDomain),
						4 => APIs.Memory.ReadU32(addr, actualDomain),
						_ => throw new InvalidOperationException($"Invalid size {size} for unsigned watch")
					}).ToString(),
					WatchType.Signed => (size switch
					{
						1 => APIs.Memory.ReadS8(addr, actualDomain),
						2 => APIs.Memory.ReadS16(addr, actualDomain),
						3 => APIs.Memory.ReadS24(addr, actualDomain),
						4 => APIs.Memory.ReadS32(addr, actualDomain),
						_ => throw new InvalidOperationException($"Invalid size {size} for signed watch")
					}).ToString(),
					WatchType.Float => (size switch
					{
						4 => APIs.Memory.ReadFloat(addr, actualDomain),
						_ => throw new InvalidOperationException($"Invalid size {size} for float watch")
					}).ToString("R", CultureInfo.InvariantCulture), // Full-precision
					_ => throw new InvalidOperationException($"Unknown WatchType {type}")
				};

				// Check if the value has changed since last frame
				if (!_previousWatchValues.TryGetValue(watch, out string previousValue) || currentValue != previousValue) {
					// Value has changed (or this is the first time we're reading it), send to unity
                	// (Sort of abuse CallMethodRpc here to avoid having to have a separate rpc buffer)
					string arg = $"{addr},{size},{isBigEndian},{type},{domain},{currentValue}";

					// Console.WriteLine($"UnityHawk: Sending watch {domain} {addr} size {size} type {type} value {currentValue} (changed from {previousValue})");

					_ = CallMethodRpc.Instance.CallMethod(SpecialCommands.ReceiveWatchedValue, arg); // Ignore return value from unity
					
					// Update the previous value for next frame
					_previousWatchValues[watch] = currentValue;
				}
			}
		}

		private void ApplyFreezes() {
			// Apply all the freezes
			foreach (var kvp in _freezes) {
				var (addr, size, domain) = kvp.Key;
				uint value = kvp.Value;
				// Console.WriteLine($"UnityHawk: Applying freeze ({domain} {addr} to uint {value})");
				// (At freeze time we assumed big-endian uint so do the same here)
				APIs.Memory.SetBigEndian(true);
				switch (size) {
					case 1:
						APIs.Memory.WriteU8(addr, value, domain);
						break;
					case 2:
						APIs.Memory.WriteU16(addr, value, domain);
						break;
					case 3:
						APIs.Memory.WriteU24(addr, value, domain);
						break;
					case 4:
						APIs.Memory.WriteU32(addr, value, domain);
						break;
					default:
						throw new InvalidOperationException($"Invalid size {size} for freeze");
				}
			}
		}

		private void OnRomLoaded() {
			// Notify unity that a rom was loaded, and pass over whatever important metadata (just SystemId for now)
			string arg = $"{APIs.Emulation.GetSystemId()}";
			_ = CallMethodRpc.Instance.CallMethod(SpecialCommands.OnRomLoaded, arg);
		}
	}
}
