// Used for sending watched memory values over rpc to unity
// TODO: move into UnityHawk external tool project

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Plunderludics.UnityHawk.Shared
{
	[Serializable]
	public struct WatchedValue
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string MethodName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string Argument;

		public override string ToString() => $"{MethodName}({Argument})";
	}
}