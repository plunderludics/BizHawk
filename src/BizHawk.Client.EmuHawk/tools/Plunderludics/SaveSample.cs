using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BizHawk.Client.Common;
using BizHawk.Emulation.Common;

namespace BizHawk.Client.EmuHawk
{
	internal class SaveSample: Form, IDialogParent {
		public IDialogController DialogController => _mainForm;

		private readonly MainForm _mainForm;
		private readonly GameInfo _game;
		private readonly Config _config;
		private readonly IEmulator _emulator;
		private readonly IMovieSession _movieSession;
		private readonly ToolManager _tools;

		private WinForms.Controls.LocSzLabelEx lblSample;
		private System.Windows.Forms.TextBox tbSampleName;
		private WinForms.Controls.LocSzButtonEx btnOK;
		private WinForms.Controls.LocSzButtonEx btnCancel;

		public SaveSample(MainForm mainForm, Config config, GameInfo game, IEmulator emulator, IMovieSession movieSession, ToolManager tools)
		{
			_mainForm = mainForm;
			_config = config;
			_game = game;
			_emulator = emulator;
			_movieSession = movieSession;
			_tools = tools;
			
			InitializeComponent();

			Icon = Properties.Resources.Logo;
		}

		private void InitializeComponent()
		{
      this.lblSample = new BizHawk.WinForms.Controls.LocSzLabelEx();
      this.tbSampleName = new System.Windows.Forms.TextBox();
      this.btnOK = new BizHawk.WinForms.Controls.LocSzButtonEx();
      this.btnCancel = new BizHawk.WinForms.Controls.LocSzButtonEx();
      this.SuspendLayout();
      // 
      // lblSample
      // 
      this.lblSample.Location = new System.Drawing.Point(12, 9);
      this.lblSample.Name = "lblSample";
      this.lblSample.Size = new System.Drawing.Size(153, 13);
      this.lblSample.Text = "Name your sample";
      // 
      // tbSampleName
      // 
      this.tbSampleName.Location = new System.Drawing.Point(15, 25);
      this.tbSampleName.Name = "tbSampleName";
      this.tbSampleName.Size = new System.Drawing.Size(242, 20);
      this.tbSampleName.TabIndex = 1;
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(101, 100);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(75, 23);
      this.btnOK.Text = "OK";
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(182, 100);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(75, 23);
      this.btnCancel.Text = "Cancel";
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // SaveSample
      // 
      this.AcceptButton = this.btnOK;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(268, 132);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.tbSampleName);
      this.Controls.Add(this.lblSample);
      this.Name = "SaveSample";
      this.Text = "Save Sample...";
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			SaveNamedSample(tbSampleName.Text);
		}

		private void SaveNamedSample(string name) {
			// TODO: get from config
			string SAMPLES_PATH = "G:\\gamedev\\plunderludics\\samples\\";
			string ROMS_PATH = "G:\\gamedev\\plunderludics\\roms\\";
			string RELATIVE_ROMS_PATH = "..\\roms\\";
			string samplePath = Path.Combine(SAMPLES_PATH, name);
			Directory.CreateDirectory(samplePath);

			// create rompath
			// TODO: this could be very smart by using the GameDB
			// we could scan the rom directory and have a file that creates the whole mapping
			// so that romPath becomes unnecessary, maybe we just have a romHash file
			//var romName = _game.Name;
			string romPathName = "rompath.txt";

			var currentRomPath = _mainForm.CurrentlyOpenRom;
			if (!currentRomPath.ToLowerInvariant().StartsWith(ROMS_PATH.ToLowerInvariant())) {
				// TODO: show dialog error?
				throw new Exception($"rom {currentRomPath} not in roms path: {ROMS_PATH}");
			}

			var relativeRomPath = currentRomPath.Substring(ROMS_PATH.Length); 
			string romPath = Path.Combine(RELATIVE_ROMS_PATH, relativeRomPath);
			string romPathPath = Path.Combine(samplePath, romPathName);
			using (FileStream fs = File.Create(romPathPath))
			using (StreamWriter sw = new StreamWriter(fs)) {
				sw.WriteLine(romPath);
			}

			// copy config
			string configName = "config.ini";
			string configPath = Path.Combine(samplePath, $"{configName}");

			ConfigService.Save(configPath, _config);

			// create new save state
			string saveStateName = "save.state";
			string saveStatePath = Path.Combine(samplePath, $"{saveStateName}");
			
			_mainForm.SaveState(saveStatePath, saveStateName);

			//// copy lua files, if any

			if (_tools.Has<LuaConsole>()) {
				var scripts = _tools.LuaConsole?.LuaImp?.ScriptList;
				string[] luaScriptPaths = new string[scripts.Count];
				for (var i = 0; i < scripts.Count; i++)
				{
					var script = scripts[i];
					var luaFileName = script.Name;
					var luaFilePath = Path.Combine(samplePath, $"{luaFileName}.lua");
					luaScriptPaths[i] = luaFilePath;

					// Copy current lua file to the sample path
					File.Copy(script.Path, luaFileName, true);
				}
			}
		}
	}

}
