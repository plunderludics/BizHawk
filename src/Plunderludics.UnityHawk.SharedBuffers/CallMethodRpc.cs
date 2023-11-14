using System;
using System.Text;

using SharedMemory;

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

		public byte[] CallMethod(string methodName, byte[] input) {
			// serialize (methodName, input) into one byte array (separated by null byte)
			byte[] methodNameBytes = Encoding.ASCII.GetBytes(methodName);
			byte[] data = new byte[methodNameBytes.Length + 1 + input.Length];
			methodNameBytes.CopyTo(data, 0);
			data[methodNameBytes.Length] = 0; //separator
			input.CopyTo(data, methodNameBytes.Length + 1);

			// Console.WriteLine("Sending callmethod RPC request to Unity");
			var response = _callMethodRpc.RemoteRequest(data);
			if (!response.Success) {
				Console.WriteLine($"Warning: Unity failed to return a value for callmethod ({methodName})");
				return null;
			}
			// no need to deserialize return value?
			return response.Data;
		}
	}
}