using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;

using Plunderludics.UnityHawk.SharedBuffers;

namespace Plunderludics.UnityHawk.Tool
{
	[ExternalTool("UnityHawk", Description = "UnityHawk")]
	public class UnityHawkMainForm : ToolFormBase, IExternalToolForm
	{
		// Supposed to use this for anything unsupported by APIs
		[RequiredService]
		private IEmulator? _emu { get; set; }

		// (This gets magically set by EmuHawk somehow)
		public ApiContainer? _apiContainer { get; set; }

		private ApiContainer APIs => _apiContainer!;

		private IVideoProvider _videoProvider;
		private ISoundProvider _soundProvider;

		protected override string WindowTitleStatic => "UnityHawk";

		private SharedInputBuffer _inputBuffer;
		private ApiCallRpc _apiCallRpc;
		private SharedTextureBuffer _sharedTextureBuffer;
		private UnityHawkSound _unityHawkSound;

		// private SharedAnalogInputBuffer _analogInputBuffer; // [TODO I think this can actually just be merged w KeyInputBuffer]
		private Dictionary<string, bool> buttonState = new();  // Current button state - need to pass to JoypadApi every frame
		private Dictionary<string, int?> analogState = new();  // Current analog axis state

		private Label text;

		// Copied this from HelloWorld/CustomMainForm.cs, it says it's a very bad idea but doesn't say why - how else can we modify config values?
		private Config GlobalConfig => (APIs.Emulation as EmulationApi ?? throw new Exception("required API wasn't fulfilled")).ForbiddenConfigReference;

		public UnityHawkMainForm()
		{
			this.text = new System.Windows.Forms.Label();
			this.SuspendLayout();

			// Text gets cut off for some reason
			// this.text.Location = new System.Drawing.Point(12, 57);
			this.text.Name = "text";
			this.text.Size = new System.Drawing.Size(100, 13);
			this.text.TabIndex = 0;
			this.text.Text = "UnityHawk :)";
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

			if (_apiCallRpc == null) {
				string apiCallRpcName = (string)APIs.UserData.Get("unityhawk-api-call-rpc");
				if (!string.IsNullOrEmpty(apiCallRpcName)) {
					_apiCallRpc = new(apiCallRpcName, ProcessApiRpcCall);
				}
			}

			_videoProvider = _emu.AsVideoProviderOrDefault();
			// Need to init/re-init texture buffer here because size depends on the video resolution of the platform
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
			// Before frame
			if (_inputBuffer != null) {
				// Get input from input buffer and pass to emulator
				Plunderludics.UnityHawk.InputEvent? mie;
				while ((mie = _inputBuffer.Read()).HasValue) {
					Plunderludics.UnityHawk.InputEvent ie = mie.Value;
					
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

			// TODO Process api calls from api call buffer
		}

		protected override void UpdateAfter() {
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

		// This is only for api calls that require a return value - others are handled on the main thread in UpdateValues
		private bool ProcessApiRpcCall(string methodName, string argString, out string output) {
			switch (methodName) {
				case "GetSystemId":
					output = APIs.Emulation.GetSystemId();
					return true;
				default:
					output = "";
					Console.WriteLine($"UnityHawk: Unknown API call {methodName} with args {argString}");
					return false;
			}
		}
	}
}
