using System;

using BizHawk.Emulation.Common;
using Plunderludics.UnityHawk.SharedBuffers;

// this should be merged into SharedAudioBuffer.cs
namespace BizHawk.Client.Common
{
	public class UnityHawkSound
	{
		private readonly SharedAudioBuffer _buffer;
		private ISoundProvider _soundSource;

		public UnityHawkSound(string audioBufferName, ISoundProvider soundSource) {
			_soundSource = soundSource;
			_soundSource.SetSyncMode(SyncSoundMode.Sync); // ?

			_buffer = new(audioBufferName);
		}

		// Must be run once per emulated frame
		public void Update() {
			// Get latest samples from soundProvider and append to running buffer
			short[] samples;
			int nSamples;
			_soundSource.GetSamplesSync(out samples, out nSamples);
			
			Console.WriteLine($"UnityHawkSound: writing {samples.Length} samples to buffer");
			_buffer.Write(samples);
		}
	}
}
