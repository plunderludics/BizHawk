// Read input events from circular buffer in shared memory
// (shared from Unity to EmuHawk)

#nullable enable

using System.Runtime.InteropServices;

using SharedMemory;

using Plunderludics.UnityHawk.Shared;

namespace Plunderludics.UnityHawk.SharedBuffers
{
public class SharedInputBuffer {
	public const int DEFAULT_NODE_COUNT = 2048; // size of input buffer, should be plenty
	
	private readonly CircularBuffer _inputBuffer;
	private readonly int _bufferItemSize = Marshal.SizeOf(typeof(InputEvent));
	
	public SharedInputBuffer(string bufferName, int nodeCount = DEFAULT_NODE_COUNT) {
		Console.WriteLine($"Init input buffer {bufferName}");
		_inputBuffer = new CircularBuffer(bufferName, nodeCount, _bufferItemSize);
	}

	public InputEvent? Read() {
		// Read raw bytes from queue and deserialize to InputEvent type	
		var bytes = new byte[_bufferItemSize];
		var amount = _inputBuffer.Read(bytes, timeout: 0);
		return amount <= 0 ? null : Serialization.RawDeserialize<InputEvent>(bytes);
	}
}
}