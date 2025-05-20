// TODO: move into UnityHawk external tool project

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Plunderludics.UnityHawk
{
	[Serializable]
	// Just a pair of strings (method name, argument)
	// Used for calling Unity methods from Lua (CallMethodRpc)
	// and for calling Bizhawk API methods from Unity (ApiCallRpc and ApiCommandBuffer)
	// method name and argument are limited to 256 chars which is a bit dumb but it's a lot easier that way
	public struct MethodCall
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string MethodName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
		public string Argument;

		public override string ToString() => $"{MethodName}({Argument})";
	}
}