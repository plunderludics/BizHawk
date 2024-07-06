#nullable enable

using System;
using System.Diagnostics;
using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedAudioBuffer {
		private CircularBuffer _buffer;
		private const int _nodeCount = (int)(2*44100*5); // Number of samples in buffer = 5s
		private const int _nodeBufferSize = sizeof(short)*2048;

		public SharedAudioBuffer(string name) {
			Console.WriteLine($"Init audio rpc buffer {name}");
			_buffer = new(name, _nodeCount, _nodeBufferSize);
		}

		public void Write(short[] samples, int nSamples) {
			// Console.WriteLine($"first = {samples[0]}; last = {samples[samples.Length-1]}");

			short[] samplesToWrite = new short[nSamples*2];
			Array.Copy(samples, samplesToWrite, samplesToWrite.Length);

			int amount = _buffer.Write<short>(samplesToWrite, timeout: 0);			
			if (amount <= 0) {
				Console.WriteLine("Warning: SharedAudioBuffer failed to write sample");
			}
			Console.WriteLine($"Attempted to write {samplesToWrite.Length} samples to shared buffer, wrote {amount}");
		}
	}
}