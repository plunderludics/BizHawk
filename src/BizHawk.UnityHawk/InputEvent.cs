// This is basically the same as BizHawk.Client.Common.InputEvent,
// but duplicated here so that Unity can use this class without having a dependency on the BizHawk.Client.Common dll
// Currently has no support for modifier keys

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace BizHawk.UnityHawk
{
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
}