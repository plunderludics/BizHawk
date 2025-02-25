﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

using BizHawk.Common;
using BizHawk.Emulation.Common;
using BizHawk.Client.Common;
using BizHawk.Common.StringExtensions;

// todo - perks - pause, copy to clipboard, backlog length limiting

namespace BizHawk.Client.EmuHawk
{
	public partial class LogWindow : ToolFormBase, IToolFormAutoConfig
	{
		// TODO: only show add to game db when this is a Rom details dialog
		// Let user decide what type (instead of always adding it as a good dump)
		private readonly List<string> _lines = new List<string>();
		private LogStream _logStream;

		[RequiredService]
		private IEmulator Emulator { get; set; }

		private string _windowTitle = "Log Window";

		protected override string WindowTitle => _windowTitle;

		protected override string WindowTitleStatic => "Log Window";

		public LogWindow()
		{
			InitializeComponent();
			Icon = Properties.Resources.CommandWindow;
			AddToGameDbBtn.Image = Properties.Resources.Add;
			Closing += (o, e) =>
			{
				Detach();
			};
			ListView_ClientSizeChanged(null, null);
			Attach();
		}

		private void Attach()
		{
			_logStream = new LogStream();
			Log.HACK_LOG_STREAM = _logStream;
			Console.SetOut(new StreamWriter(_logStream) { AutoFlush = true });
			_logStream.Emit = appendInvoked;
		}

		private void Detach()
		{
			Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
			{
				AutoFlush = true
			});
			_logStream.Close();
			_logStream = null;
			Log.HACK_LOG_STREAM = null;
		}

		public void ShowReport(string title, string report)
		{
			var ss = report.Split('\n');
			
			lock (_lines)
				foreach (var s in ss)
				{
					_lines.Add(s.TrimEnd('\r'));
				}

			virtualListView1.VirtualListSize = ss.Length;
			_windowTitle = title;
			UpdateWindowTitle();
			btnClear.Visible = false;
		}

		private void append(string str, bool invoked)
		{
			var ss = str.Split('\n');
			foreach (var s in ss)
			{
				if (!string.IsNullOrWhiteSpace(s))
				{
					lock (_lines)
					{
						_lines.Add(s.TrimEnd('\r'));
						if (invoked)
						{
							//basically an easy way to post an update message which should hopefully happen before anything else happens (redraw or user interaction)
							BeginInvoke((Action)doUpdateListSize);
						}
						else
							doUpdateListSize();
					}
				}
			}
		}

		private void doUpdateListSize()
		{
			virtualListView1.VirtualListSize = _lines.Count;
			virtualListView1.EnsureVisible(_lines.Count - 1);
		}

		private void appendInvoked(string str)
		{
			append(str, true);
		}

		private void BtnClear_Click(object sender, EventArgs e)
		{
			lock (_lines)
			{
				_lines.Clear();
				virtualListView1.VirtualListSize = 0;
				virtualListView1.SelectedIndices.Clear();
			}
		}

		private void BtnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void LogWindow_Load(object sender, EventArgs e)
		{
			HideShowGameDbButton();
		}

		private void ListView_QueryItemText(object sender, RetrieveVirtualItemEventArgs e)
		{
			e.Item = new ListViewItem(_lines[e.ItemIndex]);
		}

		private void ListView_ClientSizeChanged(object sender, EventArgs e)
		{
			virtualListView1.Columns[0].Width = virtualListView1.ClientSize.Width;
		}

		private void ListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.IsCtrl(Keys.C))
			{
				ButtonCopy_Click(null, null);
			}
		}

		private void ButtonCopy_Click(object sender, EventArgs e)
		{
			string s;
			lock (_lines)
			{
				if (virtualListView1.SelectedIndices.Count > 1)
				{
					StringBuilder sb = new();
					foreach (int i in virtualListView1.SelectedIndices) sb.AppendLine(_lines[i]);
					s = sb.ToString();
				}
				else if (virtualListView1.SelectedIndices.Count is 1)
				{
					s = _lines[virtualListView1.SelectedIndices[0]]
						.RemovePrefix(SHA1Checksum.PREFIX + ":")
						.RemovePrefix(MD5Checksum.PREFIX + ":");
				}
				else
				{
					return;
				}
			}
			s = s.Trim();
			if (!string.IsNullOrWhiteSpace(s)) Clipboard.SetText(s, TextDataFormat.Text);
		}

		private void ButtonCopyAll_Click(object sender, EventArgs e)
		{
			var sb = new StringBuilder();
			lock(_lines)
				foreach (var s in _lines)
					sb.AppendLine(s);
			if (sb.Length > 0)
				Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
		}

		private void HideShowGameDbButton()
		{
			AddToGameDbBtn.Visible = Emulator.CanGenerateGameDBEntries()
				&& (Game.Status == RomStatus.Unknown || Game.Status == RomStatus.NotInDatabase);
		}

		private void AddToGameDbBtn_Click(object sender, EventArgs e)
		{
			using var picker = new RomStatusPicker();
			var result = picker.ShowDialog();
			if (result.IsOk())
			{
				var gameDbEntry = Emulator.AsGameDBEntryGenerator().GenerateGameDbEntry();
				gameDbEntry.Status = picker.PickedStatus;
				Database.SaveDatabaseEntry(gameDbEntry);
				MainForm.UpdateDumpInfo(gameDbEntry.Status);
				HideShowGameDbButton();
			}
		}

		private class LogStream : Stream
		{
			public override bool CanRead => false;
			public override bool CanSeek => false;
			public override bool CanWrite => true;

			public override void Flush()
			{
				//TODO - maybe this will help with decoding
			}

			public override long Length => throw new NotImplementedException();

			public override long Position
			{
				get => throw new NotImplementedException();
				set => throw new NotImplementedException();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				throw new NotImplementedException();
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotImplementedException();
			}

			public override void SetLength(long value)
			{
				throw new NotImplementedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				// TODO - buffer undecoded characters (this may be important)
				//(use decoder = System.Text.Encoding.Unicode.GetDecoder())
				string str = Encoding.ASCII.GetString(buffer, offset, count);
				Emit(str);
			}

			public Action<string> Emit;
		}
	}
}
