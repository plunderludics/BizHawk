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
		/// <remarks>
		/// <see cref="RequiredServiceAttribute">RequiredServices</see> are populated by EmuHawk at runtime.
		/// These remain supported, but you should only use them when there is no API that does what you want.
		/// </remarks>
		[RequiredService]
		private IEmulator? _emu { get; set; }

		/// <remarks>
		/// <see cref="RequiredApiAttribute">RequiredApis</see> are populated by EmuHawk at runtime.
		/// You can have props for any subset of the available APIs, or use an <see cref="ApiContainer"/> to get them all at once.
		/// </remarks>
		[RequiredApi]
		private IEmulationApi? _emuApi { get; set; }

		
		[RequiredApi]
		private IUserDataApi ? _userData { get; set; }

		/// <remarks>
		/// <see cref="ApiContainer"/> can be used as a shorthand for accessing the various APIs, more like the Lua syntax.
		/// </remarks>
		public ApiContainer? _apiContainer { get; set; }

		private ApiContainer APIs => _apiContainer!;

		/// <remarks>
		/// An example of a hack. Hacks should be your last resort because they're prone to break with new releases.
		/// </remarks>
		private Config GlobalConfig => (_emuApi as EmulationApi ?? throw new Exception("required API wasn't fulfilled")).ForbiddenConfigReference;

		protected override string WindowTitleStatic => "UnityHawk";

		private SharedKeyInputBuffer _keyInputBuffer;
		// private SharedAnalogInputBuffer _analogInputBuffer; // [TODO I think this can actually just be merged w KeyInputBuffer]
		private Dictionary<string, bool> buttonState = new(); // Current button state - need to pass to JoypadApi every frame
		private Dictionary<string, int?> analogState = new(); // Current analog axis state

		private Label text;
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

		/// <remarks>This is called once when the form is opened, and every time a new movie session starts.</remarks>
		public override void Restart()
		{
			Console.WriteLine("Restarting UnityHawk plugin...");
			
			// Open sharedmemory buffers
			// TODO: could put these arg names in a shared dll?
			string keyInputBufferName = (string)APIs.UserData.Get("unityhawk-input-buffer");
			if (!string.IsNullOrEmpty(keyInputBufferName)) {
				_keyInputBuffer = new(keyInputBufferName);
			}
		}

		public override void UpdateValues(ToolFormUpdateType type)
		{
			if (type == ToolFormUpdateType.PreFrame) {
				if (_keyInputBuffer != null) {
					// Get input from input buffer and pass to emulator
					Plunderludics.UnityHawk.InputEvent? mie;
					while ((mie = _keyInputBuffer.Read()).HasValue) {
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

				// Process api calls from api call buffer
			} else if (type == ToolFormUpdateType.PostFrame) {
				// Send texture through texture buffer
				// Send audio through audio buffer
			}
		}
	}
}
