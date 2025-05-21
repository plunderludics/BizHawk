using System;
using System.Text;

using SharedMemory;

using Plunderludics.UnityHawk.Shared;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	// For calling C# methods in Unity from BizHawk lua
	public class CallMethodRpc {
		private static CallMethodRpc _instance;
		public static CallMethodRpc Instance => _instance; // hacky singleton for convenience

		private RpcBuffer _callMethodRpc;

		// [static init method not really ideal but convenient for now]
		public static void Init(string callMethodBufferName) {
			_instance = new CallMethodRpc(callMethodBufferName);
		}

		private CallMethodRpc(string callMethodBufferName) {
			Console.WriteLine($"Init method call rpc buffer {callMethodBufferName}");
			_callMethodRpc = new RpcBuffer(name: callMethodBufferName);
		}

		public byte[] CallMethod(string methodName, byte[] arg) {
			// serialize (methodName, input) into a MethodCall struct
			MethodCall methodCall = new MethodCall {
				MethodName = methodName,
				Argument = arg != null ? Encoding.ASCII.GetString(arg) : string.Empty
			};
			byte[] bytes = Serialization.Serialize(methodCall);

			// Console.WriteLine($"Sending callmethod RPC request ({methodCall}) to Unity");
			// TODO this should be async
			var response = _callMethodRpc.RemoteRequest(bytes);
			if (!response.Success) {
				Console.WriteLine($"Warning: Unity failed to return a value for callmethod ({methodCall})");
				return null;
			}
			// no need to deserialize return value?
			return response.Data;
		}
	}
}