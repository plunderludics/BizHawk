﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace BizHawk.Emulation.Cores.Consoles.Nintendo.QuickNES
{
	public static class LibQuickNES
	{
		public const string dllname = "libquicknes.dll";

		/// <summary>
		/// create a new quicknes context
		/// </summary>
		/// <returns>NULL on failure</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_new();
		/// <summary>
		/// destroy a quicknes context
		/// </summary>
		/// <param name="e">context previously returned from qn_new()</param>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern void qn_delete(IntPtr e);
		/// <summary>
		/// load an ines file
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="data">file</param>
		/// <param name="length">length of file</param>
		/// <returns></returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_loadines(IntPtr e, byte[] data, int length);
		/// <summary>
		/// set audio sample rate
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="rate">hz</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_set_sample_rate(IntPtr e, int rate);
		/// <summary>
		/// get required min dimensions of output video buffer (8bpp)
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="width">width</param>
		/// <param name="height">height</param>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern void qn_get_image_dimensions(IntPtr e, ref int width, ref int height);
		/// <summary>
		/// set output video buffer that will be used for all subsequent renders until replaced
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="dest">8bpp, at least as big as qn_get_image_dimensions()</param>
		/// <param name="pitch">byte pitch</param>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern void qn_set_pixels(IntPtr e, byte[] dest, int pitch);
		/// <summary>
		/// emulate a single frame
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="pad1">pad 1 input</param>
		/// <param name="pad2">pad 2 input</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_emulate_frame(IntPtr e, int pad1, int pad2);
		/// <summary>
		/// get number of times joypad was read in most recent frame
		/// </summary>
		/// <param name="e">context</param>
		/// <returns>0 means lag</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern int qn_get_joypad_read_count(IntPtr e);
		/// <summary>
		/// get audio info for most recent frame
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="sample_count">number of samples actually created</param>
		/// <param name="chan_count">1 for mono, 2 for stereo</param>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern void qn_get_audio_info(IntPtr e, ref int sample_count, ref int chan_count);
		/// <summary>
		/// get audio for most recent frame.  must not be called more than once per frame!
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="dest">sample buffer</param>
		/// <param name="max_samples">length to read into sample buffer</param>
		/// <returns>length actually read</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern int qn_read_audio(IntPtr e, short[] dest, int max_samples);
		/// <summary>
		/// reset the console
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="hard">true for powercycle, false for reset button</param>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern void qn_reset(IntPtr e, bool hard);
		/// <summary>
		/// get the required byte size of a savestate
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="size">size is returned</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_state_size(IntPtr e, ref int size);
		/// <summary>
		/// save state to buffer
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="dest">buffer</param>
		/// <param name="size">length of buffer</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_state_save(IntPtr e, byte[] dest, int size);
		/// <summary>
		/// load state from buffer
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="src">buffer</param>
		/// <param name="size">length of buffer</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_state_load(IntPtr e, byte[] src, int size);
		/// <summary>
		/// query battery ram state
		/// </summary>
		/// <param name="e">context</param>
		/// <returns>true if battery backup sram exists</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool qn_has_battery_ram(IntPtr e);
		/// <summary>
		/// query battery ram size
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="size">size is returned</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_battery_ram_size(IntPtr e, ref int size);
		/// <summary>
		/// save battery ram to buffer
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="dest">buffer</param>
		/// <param name="size">size</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_battery_ram_save(IntPtr e, byte[] dest, int size);
		/// <summary>
		/// load battery ram from buffer
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="src">buffer</param>
		/// <param name="size">size</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_battery_ram_load(IntPtr e, byte[] src, int size);
		/// <summary>
		/// clear battery ram
		/// </summary>
		/// <param name="e">context</param>
		/// <returns>string error</returns>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr qn_battery_ram_clear(IntPtr e);
		/// <summary>
		/// set sprite limit; does not affect emulation
		/// </summary>
		/// <param name="e">context</param>
		/// <param name="n">0 to hide, 8 for normal, 64 for all</param>
		[DllImport(dllname, CallingConvention = CallingConvention.Cdecl)]
		public static extern void qn_set_sprite_limit(IntPtr e, int n);

		/// <summary>
		/// handle "string error" as returned by some quicknes functions
		/// </summary>
		/// <param name="p"></param>
		public static void ThrowStringError(IntPtr p)
		{
			if (p == IntPtr.Zero)
				return;
			string s = Marshal.PtrToStringAnsi(p);
			throw new InvalidOperationException("LibQuickNES error: " + s);
		}
	}
}
