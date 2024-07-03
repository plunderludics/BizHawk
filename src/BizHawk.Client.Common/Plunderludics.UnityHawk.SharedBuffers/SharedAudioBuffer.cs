#nullable enable

using System;
using System.Diagnostics;
using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedAudioBuffer {
		private CircularBuffer _buffer;

		private const int _nodeCount = (int)(2*44100*0.05); // Number of samples in buffer = 0.05s
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
				// Console.WriteLine($"buffersize: {_buffer.BufferSize}, nodecount: {_buffer.NodeCount}");
				int amount = _buffer.Write<short>(ref samples[i], timeout: 0);
				if (amount <= 0) {
					// Console.WriteLine("Warning: SharedAudioBuffer failed to write sample");
				}
			}
		}
	}
}