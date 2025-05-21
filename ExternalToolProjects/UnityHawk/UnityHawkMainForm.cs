using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

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
		// Supposed to use this for anything unsupported by APIs
		[RequiredService]
		private IEmulator _emu { get; set; }

		// (This gets magically set by EmuHawk somehow)
		public ApiContainer _apiContainer { get; set; }

		private ApiContainer APIs => _apiContainer!;

		private IVideoProvider _videoProvider;
		private ISoundProvider _soundProvider;

		protected override string WindowTitleStatic => "UnityHawk";

		private SharedInputBuffer _inputBuffer;
		private ApiCallRpc _apiCallRpc;
		private ApiCommandBuffer _apiCommandBuffer;
		private SharedTextureBuffer _sharedTextureBuffer;
		private UnityHawkSound _unityHawkSound;

		private Dictionary<string, bool> buttonState = new();  // Current button state - need to pass to JoypadApi every frame
		private Dictionary<string, int?> analogState = new();  // Current analog axis state

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
			
			// Open sharedmemory buffers
			// TODO: could put these arg names in a shared dll?
			if (_inputBuffer == null) {
				string inputBufferName = (string)APIs.UserData.Get("unityhawk-input-buffer");
				if (!string.IsNullOrEmpty(inputBufferName)) {
					_inputBuffer = new(inputBufferName);
				}
			}

			// API command buffer is for write-only commands that don't require a return value, run in main thread before each framae
			if (_apiCommandBuffer == null) {
				string apiCommandBufferName = (string)APIs.UserData.Get("unityhawk-api-command-buffer");
				if (apiCommandBufferName != null) {
					// Init RPC buffer for API commands from unity
					_apiCommandBuffer = new(apiCommandBufferName);
				}
			}

			// API call buffer is for read-only commands that require a return value (and run in separate thread w arbitrary timing)
			if (_apiCallRpc == null) {
				string apiCallRpcName = (string)APIs.UserData.Get("unityhawk-api-call-rpc");
				if (!string.IsNullOrEmpty(apiCallRpcName)) {
					_apiCallRpc = new(apiCallRpcName, ProcessApiRpcCall);
				}
			}
			
			// For lua callbacks to unity
			if (CallMethodRpc.Instance == null) {
				string callMethodBufferName = (string)APIs.UserData.Get("unityhawk-lua-callbacks-buffer");
				if (callMethodBufferName != null) {
					// Init RPC buffer for CallMethod calls to unity (from lua)
					CallMethodRpc.Init(callMethodBufferName);
				}
			}

			// Texture buffer
			_videoProvider = _emu.AsVideoProviderOrDefault();
			// Need to init/re-init texture buffer here when rom changes because size depends on the video resolution of the platform
			string texBufName = (string)APIs.UserData.Get("unityhawk-texture-buffer");
			if (texBufName != null) {
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
			string audioRpcName = (string)APIs.UserData.Get("unityhawk-audio-buffer");
			if (audioRpcName != null) {
				GlobalConfig.SoundVolume = 0; // Hack to disable native sound: TODO unity should probably mute via cli arg instead
				// (Don't set SoundEnabled = false, that disables the sound provider completely)

				// Init rpc buffer for passing audio to unity
				if (_unityHawkSound == null) {
					_unityHawkSound = new (audioRpcName, _soundProvider);
				} else {
					_unityHawkSound.SetSoundProvider(_soundProvider);
				}
			}
		}

		protected override void UpdateBefore() {
			// Console.WriteLine("UnityHawk: UpdateBefore");
			// Before frame
			if (_inputBuffer != null) {
				// Get input from input buffer and pass to emulator
				InputEvent? mie;
				while ((mie = _inputBuffer.Read()).HasValue) {
					InputEvent ie = mie.Value;
					
					if (ie.isAnalog) { // [We could maybe get this from the API somehow, but easier to just get unity to tell us]
						analogState[ie.name] = ie.value;
					} else {
						buttonState[ie.name] = ie.value > 0; // TODO should store the controller here I guess (or store string like "P1 A")
					}
				}
			}

			// Send input state to JoypadApi (need to do this every frame)
			APIs.Joypad.Set(buttonState, 1); // TODO: support controllers other than 1
			APIs.Joypad.SetAnalog(analogState, 1); // TODO: support controllers other than 1

			// Process api commands from unity
			if (_apiCommandBuffer != null) {
				ProcessApiCommands();
			}
		}

		protected override void UpdateAfter() {
			// Console.WriteLine("UnityHawk: UpdateAfter");
			// After frame
			// Send texture through texture buffer
			if (_sharedTextureBuffer != null) {
				int[] pixels = _videoProvider.GetVideoBuffer();
				int width =  _videoProvider.BufferWidth;
				int height =  _videoProvider.BufferHeight;
				// Pass current frame index along with texture to make it possible to sync with lua rpc calls
				// (due to small unpredictable lag in the shared buffer write)
				_sharedTextureBuffer.Write(pixels, width, height, APIs.Emulation.FrameCount());
			}

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
				// [Mmm these string constants should really go in a file shared between unity and bizhawk]
				case "LoadRom":
					APIs.EmuClient.OpenRom(mc.Argument);
					break;
				case "LoadState":
					bool success = APIs.EmuClient.LoadState(mc.Argument, isFullPath: true);
					if (!success) {
						Console.WriteLine($"Warning: Unity attempted to load state {mc.Argument} but it failed");
					}
					break;
				case "SaveState":
					APIs.EmuClient.SaveState(mc.Argument, isFullPath: true);
					break;
				case "Pause":
					APIs.EmuClient.Pause();
					break;
				case "Unpause":
					APIs.EmuClient.Unpause();
					break;
				case "FrameAdvance":
					APIs.EmuClient.DoFrameAdvance();
					break;
				case "SetVolume":
					APIs.EmuClient.SetVolume(int.Parse(mc.Argument));
					break;
				case "WriteUnsigned": {
					/*(long address, uint value, int size, bool isBigEndian, string domain = null)*/
					// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					uint value = uint.Parse(args[1]);
					int size = int.Parse(args[2]);
					bool isBigEndian = bool.Parse(args[3]);
					string domain = (args.Length > 4) ? args[4] : APIs.Memory.MainMemoryName;

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
				case "WriteSigned": {
					/*(long address, int value, int size, bool isBigEndian, string domain = null)*/
					// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					int value = int.Parse(args[1]);
					int size = int.Parse(args[2]);
					bool isBigEndian = bool.Parse(args[3]);
					string domain = (args.Length > 4) ? args[4] : APIs.Memory.MainMemoryName;

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
				case "WriteFloat": {
					/*(long address, float value, bool isBigEndian, string domain = null)*/
					// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
					var args = mc.Argument.Split(',');
					long address = long.Parse(args[0]);
					float value = float.Parse(args[1]);
					bool isBigEndian = bool.Parse(args[2]);
					string domain = (args.Length > 3) ? args[3] : APIs.Memory.MainMemoryName;
					APIs.Memory.SetBigEndian(isBigEndian);
					APIs.Memory.WriteFloat(address, value, domain);
					break;
				}
				case "FreezeFloat":
					// TODO
					break;
				default:
					Console.WriteLine($"Warning: Unity attempting to send unsupported bizhawk api command {mc.MethodName}");
					break;
				}
			}
		}

		// This is only for api calls that require a return value - others are handled on the main thread in UpdateValues
		private string ProcessApiRpcCall(string methodName, string argString) {
			switch (methodName) {
				case "GetSystemId":
					return APIs.Emulation.GetSystemId();
				case "ReadUnsigned": {
					/*(long address, int size, bool isBigEndian, string domain = null)*/
					// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
					var args = argString.Split(',');
					long address = long.Parse(args[0]);
					int size = int.Parse(args[1]);
					bool isBigEndian = bool.Parse(args[2]);
					string domain = (args.Length > 3) ? args[3] : APIs.Memory.MainMemoryName;

					APIs.Memory.SetBigEndian(isBigEndian); // (Idk why MemoryApi is weird like this..)

					// Annoyingly MemoryApi has a ReadUnsigned method but only exposes the ReadU* methods..
					return (size switch {
						1 => APIs.Memory.ReadU8(address, domain).ToString(),
						2 => APIs.Memory.ReadU16(address, domain).ToString(),
						3 => APIs.Memory.ReadU24(address, domain).ToString(),
						4 => APIs.Memory.ReadU32(address, domain).ToString(),
						_ => throw new InvalidOperationException($"Invalid size {size} for ReadUnsigned")
					});
				}
				case "ReadSigned": {
					/*(long address, int size, bool isBigEndian, string domain = null)*/
					// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
					var args = argString.Split(',');
					long address = long.Parse(args[0]);
					int size = int.Parse(args[1]);
					bool isBigEndian = bool.Parse(args[2]);
					string domain = (args.Length > 3) ? args[3] : APIs.Memory.MainMemoryName;

					APIs.Memory.SetBigEndian(isBigEndian);

					return (size switch {
						1 => APIs.Memory.ReadS8(address, domain).ToString(),
						2 => APIs.Memory.ReadS16(address, domain).ToString(),
						3 => APIs.Memory.ReadS24(address, domain).ToString(),
						4 => APIs.Memory.ReadS32(address, domain).ToString(),
						_ => throw new InvalidOperationException($"Invalid size {size} for ReadUnsigned")
					});
				}
				case "ReadFloat": {
					/*(long address, bool isBigEndian, string domain = null)*/
					// Domain defaults to main memory domain (NOT the most recent used domain which is what MemoryApi does)
					var args = argString.Split(',');
					long address = long.Parse(args[0]);
					bool isBigEndian = bool.Parse(args[1]);
					string domain = (args.Length > 2) ? args[2] : APIs.Memory.MainMemoryName;

					return APIs.Memory.ReadFloat(address, domain).ToString();
				}
				default:
					Console.WriteLine($"UnityHawk: Unknown API call {methodName} with args {argString}");
					return null;
			}
		}
	}
}
