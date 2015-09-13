﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using BizHawk.Bizware.BizwareGL;

namespace BizHawk.Client.Common
{
	public class TasBranch
	{
		public int Frame { get; set; }
		public byte[] CoreData { get; set; }
		public List<string> InputLog { get; set; }
		public BitmapBuffer OSDFrameBuffer { get; set; }
		public TasLagLog LagLog { get; set; }
		public TasMovieChangeLog ChangeLog { get; set; }
		public DateTime TimeStamp { get; set; }
		public TasMovieMarkerList Markers { get; set; }
	}

	public class TasBranchCollection : List<TasBranch>
	{
		public void Save(BinaryStateSaver bs)
		{
			var nheader = new IndexedStateLump(BinaryStateLump.BranchHeader);
			var ncore = new IndexedStateLump(BinaryStateLump.BranchCoreData);
			var ninput = new IndexedStateLump(BinaryStateLump.BranchInputLog);
			var nframebuffer = new IndexedStateLump(BinaryStateLump.BranchFrameBuffer);
			var nlaglog = new IndexedStateLump(BinaryStateLump.BranchLagLog);
			var nmarkers = new IndexedStateLump(BinaryStateLump.BranchMarkers);
			foreach (var b in this)
			{
				bs.PutLump(nheader, delegate(TextWriter tw)
				{
					// if this header needs more stuff in it, handle it sensibly
					tw.WriteLine(JsonConvert.SerializeObject(new
					{
						Frame = b.Frame,
						TimeStamp = b.TimeStamp
					}));
				});

				bs.PutLump(ncore, delegate(Stream s)
				{
					s.Write(b.CoreData, 0, b.CoreData.Length);
				});

				bs.PutLump(ninput, delegate(TextWriter tw)
				{
					foreach (var line in b.InputLog)
						tw.WriteLine(line);
				});

				bs.PutLump(nframebuffer, delegate(Stream s)
				{
					var vp = new BitmapBufferVideoProvider(b.OSDFrameBuffer);
					QuickBmpFile.Save(vp, s, b.OSDFrameBuffer.Width, b.OSDFrameBuffer.Height);
				});

				bs.PutLump(nlaglog, delegate(BinaryWriter bw)
				{
					b.LagLog.Save(bw);
				});

				bs.PutLump(nmarkers, delegate (TextWriter tw)
				{
					tw.WriteLine(b.Markers.ToString());
				});

				nheader.Increment();
				ncore.Increment();
				ninput.Increment();
				nframebuffer.Increment();
				nlaglog.Increment();
				nmarkers.Increment();
			}
		}

		public void Load(BinaryStateLoader bl, TasMovie movie)
		{
			var nheader = new IndexedStateLump(BinaryStateLump.BranchHeader);
			var ncore = new IndexedStateLump(BinaryStateLump.BranchCoreData);
			var ninput = new IndexedStateLump(BinaryStateLump.BranchInputLog);
			var nframebuffer = new IndexedStateLump(BinaryStateLump.BranchFrameBuffer);
			var nlaglog = new IndexedStateLump(BinaryStateLump.BranchLagLog);
			var nmarkers = new IndexedStateLump(BinaryStateLump.BranchMarkers);

			Clear();

			while (true)
			{
				var b = new TasBranch();

				if (!bl.GetLump(nheader, false, delegate(TextReader tr)
				{
					var header = (dynamic)JsonConvert.DeserializeObject(tr.ReadLine());
					b.Frame = (int)header.Frame;

					var timestamp = (dynamic)header.TImeStamp;

					if (timestamp != null)
					{
						b.TimeStamp = (DateTime)timestamp;
					}
					else
					{
						b.TimeStamp = DateTime.Now;
					}
				}))
				{
					return;
				}

				bl.GetLump(ncore, true, delegate(Stream s, long length)
				{
					b.CoreData = new byte[length];
					s.Read(b.CoreData, 0, b.CoreData.Length);
				});

				bl.GetLump(ninput, true, delegate(TextReader tr)
				{
					b.InputLog = new List<string>();
					string line;
					while ((line = tr.ReadLine()) != null)
						b.InputLog.Add(line);
				});

				bl.GetLump(nframebuffer, true, delegate(Stream s, long length)
				{
					var vp = new QuickBmpFile.LoadedBMP();
					QuickBmpFile.Load(vp, s);
					b.OSDFrameBuffer = new BitmapBuffer(vp.BufferWidth, vp.BufferHeight, vp.VideoBuffer);
				});

				bl.GetLump(nlaglog, true, delegate(BinaryReader br)
				{
					b.LagLog = new TasLagLog();
					b.LagLog.Load(br);
				});

				b.Markers = new TasMovieMarkerList(movie);
				bl.GetLump(nmarkers, false, delegate (TextReader tr)
				{
					string line;
					while ((line = tr.ReadLine()) != null)
					{
						if (!string.IsNullOrWhiteSpace(line))
						{
							b.Markers.Add(new TasMovieMarker(line));
						}
					}
				});

				Add(b);

				nheader.Increment();
				ncore.Increment();
				ninput.Increment();
				nframebuffer.Increment();
				nlaglog.Increment();
				nmarkers.Increment();
			}
		}
	}
}
