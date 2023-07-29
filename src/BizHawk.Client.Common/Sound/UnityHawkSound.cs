using System;

using BizHawk.Emulation.Common;
using Plunderludics.UnityHawk.SharedBuffers;

// this should probably be in a different namespace/dll actually
namespace BizHawk.Client.Common
{
	public class UnityHawkSound
	{
		SharedAudioRpc _audioRpc;
		SoundOutputProvider _bufferedSoundProvider;
		public UnityHawkSound(string audioBufferName, ISoundProvider soundSource, Func<double> getVsyncRate) {
			soundSource.SetSyncMode(SyncSoundMode.Sync);
			_bufferedSoundProvider = new SoundOutputProvider(getVsyncRate, standaloneMode: true); // [idk why but standalone mode seems to be less distorted]
			_bufferedSoundProvider.BaseSoundProvider = soundSource;

			_audioRpc = new(audioBufferName, GetSamples);
		}

		private short[] GetSamples(int nSamples) {
			short[] buf = new short[nSamples];
			_bufferedSoundProvider.GetSamples(buf);
			return buf;
		}
	}
}
