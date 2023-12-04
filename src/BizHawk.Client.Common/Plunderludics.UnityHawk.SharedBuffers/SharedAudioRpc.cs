#nullable enable

using System;

using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedAudioRpc {
		private RpcBuffer _audioBufferRpc; // for unity to request sample buffer from bizhawk
		private RpcBuffer _nSamplesRpc; // for bizhawk to ask unity how many samples it needs for each emulator frame
		private Func<short[]> _getSamples; // [this is a bit messy, get samples from the emulator via a callback to avoid a dependency on BizHawk.Client.Common]
		public SharedAudioRpc(string audioBufferName, string nSamplesBufferName, Func<short[]> getSamplesCallback) {
			Console.WriteLine($"Init audio rpc buffer {audioBufferName}, nSamples rpc buffer {nSamplesBufferName}");
			_getSamples = getSamplesCallback;

			// This function runs on the bizhawk side and is called by unity
			_audioBufferRpc = new RpcBuffer(
				audioBufferName,
				(msgId, payload) => GetAudioBuffer(),
				bufferCapacity: 50000,
				bufferNodeCount: 10
			);

			// This function runs on the unity side and is called by bizhawk
			_nSamplesRpc = new RpcBuffer(nSamplesBufferName);
		}

		// Ask Unity over RPC how many samples it wants
		public int GetSamplesNeeded() {
			var response = _nSamplesRpc.RemoteRequest();
			if (!response.Success) {
				throw new Exception("Unity failed to return a value for n samples needed");
			}

			byte[] bytes = response.Data;
			// Data should be a 4-byte int (endianness according to cpu) representing the number of samples requested
			if (bytes.Length != 4) {
				throw new ArgumentException($"GetSamplesNeeded: data had {bytes.Length} bytes instead of 4");
			}
			int samplesNeeded = BitConverter.ToInt32(bytes, 0);

			return samplesNeeded;
		}

		private byte[] GetAudioBuffer() {
			short[] samples = _getSamples();

			// Console.WriteLine($"Returning {samples.Length} samples over RPC");
			// Console.WriteLine($"first = {samples[0]}; last = {samples[samples.Length-1]}");

			// convert shorts to bytes
			byte[] bytes = new byte[samples.Length*2];
			Buffer.BlockCopy(samples, 0, bytes, 0, bytes.Length);

			return bytes;
		}
	}
}