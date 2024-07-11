using System;

using BizHawk.Emulation.Common;
using Plunderludics.UnityHawk.SharedBuffers;

namespace BizHawk.Client.Common
{
	public class UnityHawkSound
	{
		private readonly SharedAudioRpc _rpc;
		private ISoundProvider _soundProvider;

		public UnityHawkSound(string audioBufferName, ISoundProvider soundProvider) {
			SetSoundProvider(soundProvider);

			_rpc = new(audioBufferName);
		}

		// Must be run once per emulated frame
		public void Update() {
			// Get latest samples from soundProvider and send directly to unity via rpc
			short[] samples;
			int nSamples;
			_soundProvider.GetSamplesSync(out samples, out nSamples);
			// Confusing, only the first nSamples*2 shorts are meaningful (*2 because stereo)
			_rpc.SendSamples(samples, nSamples*2);
		}

		public void SetSoundProvider(ISoundProvider soundProvider) {
			_soundProvider = soundProvider;
			_soundProvider.SetSyncMode(SyncSoundMode.Sync); // ?
		}
	}
}
