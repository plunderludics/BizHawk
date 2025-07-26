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
		// We use a simple array for the texture buffer rather than a circular buffer
		// because the unity client is only interested in the latest frame - no point storing intermediates
		// And we also want the buffer size to be variable
		// It is a bit messy this way though because we store metadata alongside pixels
		private SharedArray<int> _sharedArray;
		private int _index;
		private string _name;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		// (_sharedArray is initialized within SetSize)
		public SharedTextureBuffer(string name, int bufferSize) {
			_index = 0;
			_name = name;
			SetSize(bufferSize);
		}
#pragma warning restore CS8618
		public void Write(int[] pixels, int width, int height, int frame) {
			// Very weird but we have to write the pixels before writing the metadata
			// or the metadata all gets overwritten (maybe some bug in SharedArray.Write when startIndex != 0?)
			_sharedArray.Write(pixels, TextureBufferLayout.PixelDataStartIndex);
			
			_sharedArray[TextureBufferLayout.WidthIndex] = width;
			_sharedArray[TextureBufferLayout.HeightIndex] = height;
			_sharedArray[TextureBufferLayout.FrameIndex] = frame;
		}

		public void SetSize(int bufferSize) {
			// Not allowed to re-open a shared buffer with same name as a previous one, so add a counter to the name
			// [Same logic happens in UnityHawk SharedTextureBuffer.cs - TODO this logic should be moved to shared code]
			string trueName = $"{_name}-{_index}";

			Console.WriteLine($"Init texture buffer {trueName}");
			_sharedArray = new (trueName, bufferSize+10); // for some reason have to make it a bit bigger
			_index++;
		}
	}
}