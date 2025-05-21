#nullable enable

using System;

using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedAudioRpc {
		private RpcBuffer _sendSamplesRpc; // for bizhawk to send sample buffer to unity
		public SharedAudioRpc(string name) {
			Console.WriteLine($"Init audio rpc buffer {name}");
			// This function runs on the unity side and is called by bizhawk
			_sendSamplesRpc = new RpcBuffer(name);
		}

		public void SendSamples(short[] samples, int sampleCount) {
			byte[] bytes = new byte[sampleCount*2]; // TODO should not initialize this every frame 
			Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);
			var response = _sendSamplesRpc.RemoteRequest(bytes);
			if (!response.Success) {
				Console.WriteLine("Warning: Unity failed to return a response when sending samples");
				return;
			}
		}
	}
}