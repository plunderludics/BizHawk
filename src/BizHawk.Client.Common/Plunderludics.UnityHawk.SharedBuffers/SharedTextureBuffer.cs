// Writing emulator texture to fixed buffer in shared memory
// (for sharing texture from EmuHawk to Unity)

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using SharedMemory;

namespace Plunderludics.UnityHawk.SharedBuffers
{
	public class SharedTextureBuffer {
		private SharedArray<int> sharedArray;
		int _index;
		string _name;
		public SharedTextureBuffer(string name, int bufferSize) {
			_index = 0;
			_name = name;
			SetSize(bufferSize);
		}
		public void Write(int[] pixels, int width, int height, int frame) {
			sharedArray.Write(pixels, 0);
			sharedArray[sharedArray.Length - 3] = width;
			sharedArray[sharedArray.Length - 2] = height;
			sharedArray[sharedArray.Length - 1] = frame;
		}

		public void SetSize(int bufferSize) {
			// Not allowed to re-open a shared buffer with same name as a previous one, so add a counter to the name
			// [Same logic happens in UnityHawk Emulator.cs - TODO this logic should be moved to shared code]
			string trueName = $"{_name}-{_index}";

			Console.WriteLine($"Init texture buffer {trueName}");
			sharedArray = new (trueName, bufferSize+10); // for some reason have to make it a bit bigger
			_index++;
		}
	}
}