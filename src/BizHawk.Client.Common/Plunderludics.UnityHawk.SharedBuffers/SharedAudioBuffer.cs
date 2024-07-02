#nullable enable

using System;

using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedAudioBuffer {
		private CircularBuffer _buffer;

		private const int _nodeCount = 44100*5; // Number of samples in buffer
		private const int _nodeBufferSize = sizeof(short);

		public SharedAudioBuffer(string name) {
			Console.WriteLine($"Init audio rpc buffer {name}");
			_buffer = new(name, _nodeCount, _nodeBufferSize);
		}

		public void Write(short[] samples, int nSamples) {
			Console.WriteLine($"Writing {nSamples*2} samples to shared buffer");
			// Console.WriteLine($"first = {samples[0]}; last = {samples[samples.Length-1]}");

			// Write samples one at a time, which seems inefficient
			for (int i = 0; i < nSamples*2; i++) {
				int amount = _buffer.Write<short>(ref samples[i]);
				if (amount <= 0) {
					Console.WriteLine("Warning: SharedAudioBuffer failed to write sample");
				}
			}
		}
	}
}