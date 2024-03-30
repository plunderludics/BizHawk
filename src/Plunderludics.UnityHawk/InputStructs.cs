// Struct types used for sending serialized input data (key events and analog input signals)
// over IPC from Unity to Bizhawk.
#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Plunderludics.UnityHawk
{
	// This is basically the same as BizHawk.Client.Common.InputEvent,
	// but duplicated here so that Unity can use this class without having a dependency on the BizHawk.Client.Common dll
	// Currently has no support for modifier keys
	[Serializable]
	public struct InputEvent
	{
		public InputEventType EventType;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string ButtonName;

		public ClientInputFocus Source;

		public override string ToString() => $"{EventType}:{ButtonName}";
	}

	public enum InputEventType
	{
		Press, Release
	}

	[Flags]
	public enum ClientInputFocus
	{
		None = 0,
		Mouse = 1,
		Keyboard = 2,
		Pad = 4
	}

	// This is added in order to make a serializable version of a Dictionary<string, int>
	// which is the type Bizhawk uses for analog input values
	[Serializable]
	public struct AxisValue
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string Name;
		public int Value;
		public override string ToString() => $"{Name}:{Value}";
	}

	public static class InputStructConsts {
		public const int AxisValueArrayLength = 256;
	}

	// [This is kinda weird, could probably be done in a nicer way, idk]
	[Serializable]
	public struct AxisValuesStruct
	{
    	[MarshalAs(UnmanagedType.ByValArray, SizeConst = InputStructConsts.AxisValueArrayLength)]
		public AxisValue[] axisValues;
	}
}