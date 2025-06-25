// This is for unity to call write-only api commands that don't require a return value
#nullable enable

using System;
using System.Text;
using System.Runtime.InteropServices;

using SharedMemory;

using Plunderludics.UnityHawk.Shared;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class ApiCommandBuffer {
		private CircularBuffer _buffer;
		private const int _defaultNodeCount = 2048; // size of input buffer, should be plenty
		private int _bufferItemSize = Marshal.SizeOf(typeof(MethodCall));
		public ApiCommandBuffer(string bufferName, int nodeCount = _defaultNodeCount) {
			Console.WriteLine($"Init api call buffer {bufferName}");
			_buffer = new CircularBuffer(bufferName, nodeCount, _bufferItemSize);
		}

		public MethodCall? Read() {
			// Read raw bytes from queue and deserialize to InputEvent type	
			byte[] bytes = new byte[_bufferItemSize];
			int amount = _buffer.Read(bytes, timeout: 0);
			if (amount > 0) {
				return Serialization.RawDeserialize<MethodCall>(bytes);
			} else {
				return null;
			}
		}
	}
}