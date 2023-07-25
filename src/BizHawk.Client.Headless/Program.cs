using System;
using System.IO;

using BizHawk.Emulation.Common;
using BizHawk.Common.PathExtensions;
using BizHawk.Emulation.Cores.Nintendo.NES;
using BizHawk.Emulation.Cores.Arcades.MAME;

namespace BizHawk.Client.Headless
{
	internal static class Program
	{
		public static void Main() {
			Console.WriteLine("hi");

			Database.InitializeDatabase(
				bundledRoot: Path.Combine(PathUtils.ExeDirectoryPath, "gamedb"),
				userRoot: Path.Combine(PathUtils.DataDirectoryPath, "gamedb"),
				silent: true);
			BootGodDb.Initialize(Path.Combine(PathUtils.ExeDirectoryPath, "gamedb"));
			MAMEMachineDB.Initialize(Path.Combine(PathUtils.ExeDirectoryPath, "gamedb"));

			var e = new EmulatorInstance();
			bool success = e.InitEmulator(
				"config.ini",
        		"mario.nes",
        		null
			);

			e.FrameAdvance(null);
			e.FrameAdvance(null);
			e.FrameAdvance(null);
			e.FrameAdvance(null);
			e.FrameAdvance(null);
			e.FrameAdvance(null);
			e.FrameAdvance(null);

			Console.WriteLine($"Success? {success}");
		}
	}
}