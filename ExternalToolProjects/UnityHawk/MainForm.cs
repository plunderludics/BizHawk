using System;
using System.Windows.Forms;
using System.IO;

using BizHawk.Client.Common;
using BizHawk.Client.EmuHawk;
using BizHawk.Emulation.Common;

namespace UnityHawk
{
	[ExternalTool("UnityHawk", Description = "UnityHawk")]
	public class MainForm : ToolFormBase, IExternalToolForm
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

		private Label text;
		public MainForm()
		{
			this.text = new System.Windows.Forms.Label();
			this.SuspendLayout();

			// Text gets cut off for some reason
			this.text.Location = new System.Drawing.Point(12, 57);
			this.text.Name = "text";
			this.text.Size = new System.Drawing.Size(35, 13);
			this.text.TabIndex = 0;
			this.text.Text = "Hello World!";
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
		}

		public override void UpdateValues(ToolFormUpdateType type)
		{
			this.text.Text = $"UpdateValues({type})";
		}
	}
}
