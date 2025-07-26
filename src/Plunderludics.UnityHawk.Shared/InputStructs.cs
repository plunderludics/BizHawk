// Struct types used for sending serialized input data (key events and analog input signals)
// over IPC from Unity to Bizhawk.

// TODO: move into UnityHawk external tool project

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Plunderludics.UnityHawk.Shared
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct InputEvent 
	{
		// For simplicity, use this for both axis and button input: 0 is unpressed, > 0 is pressed
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string name; // E.g. "D-Pad Left" (Don't include "P1 " controller prefix!)
		public int value;
		public int controller; // 0 for None, 1 for P1, etc
		public bool isAnalog;
	 	public override string ToString() => $"{name}:{value}";
	}
}