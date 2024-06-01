// Writing emulator texture to fixed buffer in shared memory
// (for sharing texture from EmuHawk to Unity)

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Plunderludics.UnityHawk;


using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedAnalogInputBuffer {
		private SharedArray<byte> _buffer;
		private int _bufferSize = Marshal.SizeOf(typeof(AxisValuesStruct));
		public SharedAnalogInputBuffer(string name) {
			_buffer = new (name, _bufferSize);
		}
		public Dictionary<string, int> Read() {
			// Read raw AxisValue structs from buffer and deserialize to Dictionary<string, int>
			byte[] bytes = new byte[_bufferSize];
			_buffer.CopyTo(bytes, startIndex: 0);

			AxisValuesStruct avStruct = Serialization.RawDeserialize<AxisValuesStruct>(bytes);

			// axisValues always has 256 values so we have to filter out the null ones:
			var actualValues = avStruct.axisValues.Where(av => !string.IsNullOrEmpty(av.Name));

			return actualValues.ToDictionary(
				av => av.Name,
				av => av.Value
			);
		}
	}
}