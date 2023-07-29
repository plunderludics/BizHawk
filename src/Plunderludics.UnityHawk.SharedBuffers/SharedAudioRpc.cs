#nullable enable

using System;

using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedAudioRpc {
		private RpcBuffer _rpcBuffer;
		private Func<int, short[]> _getSamples; // [this is a bit messy, get samples from the emulator via a callback to avoid a dependency on BizHawk.Client.Common]
		public SharedAudioRpc(string bufferName, Func<int, short[]> getSamplesCallback) {
			Console.WriteLine($"Init audio rpc buffer {bufferName}");
			_getSamples = getSamplesCallback;
			_rpcBuffer = new RpcBuffer(bufferName, (msgId, payload) => GetAudioBuffer(payload));
		}

		private byte[] GetAudioBuffer(byte[] payload) {
			// Payload should be a 4-byte int (endianness according to cpu) representing the number of samples requested
			if (payload.Length != 4) {
				throw new ArgumentException($"GetAudioBuffer: payload had {payload.Length} bytes instead of 4");
			}
			int samplesNeeded = BitConverter.ToInt32(payload, 0);
			Console.WriteLine($"{samplesNeeded} samples requested");
			// TODO: get resampled audio buffer from provider and convert to bytes
			short[] shortbuf = _getSamples(samplesNeeded);

			// convert shorts to bytes
			byte[] bytes = new byte[shortbuf.Length*2];
			Buffer.BlockCopy(shortbuf, 0, bytes, 0, bytes.Length);

			return bytes;
		}
	}
}