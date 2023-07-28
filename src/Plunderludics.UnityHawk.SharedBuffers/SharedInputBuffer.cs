// Shared memory buffer for sharing live emulator texture from EmuHawk to Unity

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedInputBuffer {
		private CircularBuffer _inputBuffer;
		private static int _nodeCount = 2048; // size of input buffer, should be plenty
		private int _bufferItemSize = Marshal.SizeOf(typeof(Plunderludics.UnityHawk.InputEvent));
		public SharedInputBuffer(string inputBufferName) {
			Console.WriteLine($"Init input buffer {inputBufferName}");
			_inputBuffer = new CircularBuffer(inputBufferName, _nodeCount, _bufferItemSize);
		}

		public Plunderludics.UnityHawk.InputEvent? Read() {
			// Read raw bytes from queue and deserialize to InputEvent type	
			byte[] bytes = new byte[_bufferItemSize];
			int amount = _inputBuffer.Read(bytes, timeout: 0);
			if (amount > 0) {
				return RawDeserialize<Plunderludics.UnityHawk.InputEvent>(bytes);
			} else {
				return null;
			}
		}

		private static T RawDeserialize<T>(byte[] rawData, int position = 0)
		{
			int rawsize = Marshal.SizeOf(typeof(T));
			if (rawsize > rawData.Length - position)
				throw new ArgumentException("Not enough data to fill struct. Array length from position: "+(rawData.Length-position) + ", Struct length: "+rawsize);
			IntPtr buffer = Marshal.AllocHGlobal(rawsize);
			Marshal.Copy(rawData, position, buffer, rawsize);
			T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			return retobj;
		}
	}
}