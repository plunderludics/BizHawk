// using System;
// using System.IO;
// using System.Collections.Generic;

// using BizHawk.Client.Common;
// using BizHawk.Emulation.Common;
// using BizHawk.Common.PathExtensions;
// using BizHawk.Emulation.Cores.Nintendo.NES;
// using BizHawk.Emulation.Cores.Arcades.MAME;

// using Plunderludics.UnityHawk.SharedBuffers;

// namespace BizHawk.Client.Headless
// {
// 	internal static class Program
// 	{
// 		public static void Main(string[] args) {
// 			ParsedCLIFlags cliFlags = default;
// 			ArgParser.ParseArguments(out cliFlags, args);
// 			string texBufName = cliFlags.writeTextureToSharedBuffer;
// 			string inputBufferName = cliFlags.readKeyEventFromSharedBuffer;

// 			// TODO handle --firmware arg

// 			if (texBufName == null || inputBufferName == null) {
// 				throw new InvalidOperationException("must provide texture buffer name and input buffer name on command line");
// 			}

// 			// Init dbs
// 			Database.InitializeDatabase(
// 				bundledRoot: Path.Combine(PathUtils.ExeDirectoryPath, "gamedb"),
// 				userRoot: Path.Combine(PathUtils.DataDirectoryPath, "gamedb"),
// 				silent: true);
// 			BootGodDb.Initialize(Path.Combine(PathUtils.ExeDirectoryPath, "gamedb"));
// 			MAMEMachineDB.Initialize(Path.Combine(PathUtils.ExeDirectoryPath, "gamedb"));

// 			// Load rom
// 			var e = new EmulatorInstance();
// 			bool loaded = e.InitEmulator(
// 				cliFlags.cmdConfigFile,
//         		cliFlags.cmdRom,
//         		(cliFlags.luaScript != null) ? new List<string> {cliFlags.luaScript} : null,
// 				cliFlags.cmdLoadState
// 			);
// 			Console.WriteLine($"loaded? {loaded}");

// 			var inputProvider = new UnityHawkInput(inputBufferName);

// 			SharedTextureBuffer sharedTextureBuffer = null;
// 			// Init shared texture buffer for passing to unity
// 			sharedTextureBuffer = new(texBufName, e.GetVideoBuffer().Length);

// 			while (true) {
// 				e.FrameAdvance(inputProvider);

// 				// TODO FPS throttle

// 				if (sharedTextureBuffer != null) {
// 					int[] pixels = e.GetVideoBuffer();
// 					int width =  e.VideoBufferWidth;
// 					int height =  e.VideoBufferHeight;
// 					sharedTextureBuffer.Write(pixels, width, height);
// 					// ^ don't even need to do this every frame if running faster than unity, but maybe it's easier this way
// 				}
// 			}
// 		}
// 	}
// }