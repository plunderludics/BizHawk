using System;

using BizHawk.Emulation.Common;
using Plunderludics.UnityHawk.SharedBuffers;

// this should be merged into SharedAudioBuffer.cs
namespace BizHawk.Client.Common
{
	public class UnityHawkSound
	{
		private readonly SharedAudioRpc _rpc;
		private ISoundProvider _soundSource;

		public UnityHawkSound(string audioBufferName, ISoundProvider soundSource) {
			_soundSource = soundSource;
			_soundSource.SetSyncMode(SyncSoundMode.Sync); // ?

			_rpc = new(audioBufferName);
		}

		// Must be run once per emulated frame
		public void Update() {
			// Get latest samples from soundProvider and append to running buffer
			short[] samples;
			int nSamples;
			_soundSource.GetSamplesSync(out samples, out nSamples);
			// Confusing, only the first nSamples*2 shorts are meaningful (*2 because stereo)
			_rpc.SendSamples(samples, nSamples*2);
		}
	}
}
