// This should probably move under the Plunderludics.UnityHawk namespace/directory? idk

using System;
using System.ComponentModel;
using Plunderludics.UnityHawk.SharedBuffers;
using NLua;
using System.Text;

// Right now only supports string-to-string methods but generic type support would be useful
// (or at least just ints and floats as well)

namespace BizHawk.Client.Common
{
	[Description("A library for communicating with Unity")]
	public sealed class UnityHawkLuaLibrary : LuaLibraryBase
	{
		public UnityHawkLuaLibrary(ILuaLibraries luaLibsImpl, ApiContainer apiContainer, Action<string> logOutputCallback)
			: base(luaLibsImpl, apiContainer, logOutputCallback) {}

		public override string Name => "unityhawk";
		[LuaMethodExample("local resultString = unityhawk.callmethod(\"MethodName\", <argString>);")]
		[LuaMethod("callmethod", "Calls a method registered in Unity and returns the result. Supports a single string arg and string return value")]
		public string CallMethod(string methodName, string arg) {
			Console.WriteLine($"CallMethod {methodName} {arg}");
			byte[] argBytes = Encoding.ASCII.GetBytes(arg);
			byte[] retBytes = CallMethodRpc.Instance.CallMethod(methodName, argBytes);
			return Encoding.ASCII.GetString(retBytes);
		}
	}
}
