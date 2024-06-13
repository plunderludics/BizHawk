// This should probably move under the Plunderludics.UnityHawk namespace/directory? idk

using System;
using System.ComponentModel;
using System.Text;
using Plunderludics.UnityHawk.SharedBuffers;

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
			var argBytes = Encoding.ASCII.GetBytes(arg);
			var retBytes = CallMethodRpc.Instance.CallMethod(methodName, argBytes);
			return retBytes == null ? null : Encoding.ASCII.GetString(retBytes);
		}
	}
}
