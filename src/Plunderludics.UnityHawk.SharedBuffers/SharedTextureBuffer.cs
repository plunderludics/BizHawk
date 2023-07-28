// Shared memory buffer for sharing live emulator texture from EmuHawk to Unity

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
			sharedArray = new (name, bufferSize+10); // for some reason have to make it a bit bigger
		}
		public void Write(int[] pixels, int width, int height) {
				sharedArray.Write(pixels, 0);
				sharedArray[sharedArray.Length - 2] = width;
				sharedArray[sharedArray.Length - 1] = height;
		}
	}
}