// Read input events from circular buffer in shared memory
// (shared from Unity to EmuHawk)

#nullable enable

using System;
using System.Runtime.InteropServices;

using SharedMemory;

using Plunderludics.UnityHawk;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedKeyInputBuffer {
		CircularBuffer _inputBuffer;
		const int _defaultNodeCount = 2048; // size of input buffer, should be plenty
		int _bufferItemSize = Marshal.SizeOf(typeof(Plunderludics.UnityHawk.InputEvent));
		public SharedKeyInputBuffer(string bufferName, int nodeCount = _defaultNodeCount) {
			Console.WriteLine($"Init input buffer {bufferName}");
			_inputBuffer = new CircularBuffer(bufferName, nodeCount, _bufferItemSize);
		}

		public InputEvent? Read() {
			// Read raw bytes from queue and deserialize to InputEvent type	
			byte[] bytes = new byte[_bufferItemSize];
			int amount = _inputBuffer.Read(bytes, timeout: 0);
			if (amount > 0) {
				return Serialization.RawDeserialize<InputEvent>(bytes);
			} else {
				return null;
			}
		}
	}
}