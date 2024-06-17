// This is basically the same as BizHawk.Client.Common.InputEvent,
// but duplicated here so that Unity can use this class without having a dependency on the BizHawk.Client.Common dll
// Currently has no support for modifier keys

#nullable enable

using System;
using System.Runtime.InteropServices;

namespace Plunderludics.UnityHawk
{
	// Copied these from internet somewhere
	public static class Serialization {
		public static byte[] Serialize(object obj) {
			var len = Marshal.SizeOf(obj);
			var arr = new byte[len];
			var ptr = Marshal.AllocHGlobal(len);
			
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, len);
			Marshal.FreeHGlobal(ptr);
			
			return arr;
		}

		public static T RawDeserialize<T>(byte[] rawData, int position = 0) {
			var rawSize = Marshal.SizeOf(typeof(T));
			if (rawSize > rawData.Length - position)
				throw new ArgumentException($"Not enough data to fill struct. Array length from position: {rawData.Length-position} Struct length: {rawSize}", nameof(rawData));
			
			var buffer = Marshal.AllocHGlobal(rawSize);
			Marshal.Copy(rawData, position, buffer, rawSize);
			
			var returnObj = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			
			return returnObj;
		}
	}
}