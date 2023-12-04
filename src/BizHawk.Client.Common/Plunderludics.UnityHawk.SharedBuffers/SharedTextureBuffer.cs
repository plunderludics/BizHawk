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
		public SharedTextureBuffer(string name, int bufferSize) {
			Console.WriteLine($"Init texture buffer {name}");
			sharedArray = new (name, bufferSize+10); // for some reason have to make it a bit bigger
		}
		public void Write(int[] pixels, int width, int height) {
			sharedArray.Write(pixels, 0);
			sharedArray[sharedArray.Length - 2] = width;
			sharedArray[sharedArray.Length - 1] = height;
		}
	}
}