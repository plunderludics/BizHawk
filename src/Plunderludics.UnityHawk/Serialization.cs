// This is basically the same as BizHawk.Client.Common.InputEvent,
// but duplicated here so that Unity can use this class without having a dependency on the BizHawk.Client.Common dll
// Currently has no support for modifier keys

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Plunderludics.UnityHawk
{
	// Copied these from internet somewhere
	public class Serialization {
		public static byte[] Serialize(object obj) {
			int len = Marshal.SizeOf(obj);
			byte [] arr = new byte[len];
			IntPtr ptr = Marshal.AllocHGlobal(len);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, len);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}

		public static T RawDeserialize<T>(byte[] rawData, int position = 0) {
			int rawsize = Marshal.SizeOf(typeof(T));
			if (rawsize > rawData.Length - position)
				throw new ArgumentException("Not enough data to fill struct. Array length from position: "+(rawData.Length-position) + ", Struct length: "+rawsize);
			IntPtr buffer = Marshal.AllocHGlobal(rawsize);
			Marshal.Copy(rawData, position, buffer, rawsize);
			T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			return retobj;
		}
	}
}