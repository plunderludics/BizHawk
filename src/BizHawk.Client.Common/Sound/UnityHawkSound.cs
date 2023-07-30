using System;

using BizHawk.Emulation.Common;
using Plunderludics.UnityHawk.SharedBuffers;

// this should probably be in a different namespace/dll actually
namespace BizHawk.Client.Common
{
	public class UnityHawkSound
	{
		// [This has to match the one in Unity - this sucks change it]
		static readonly string samplesNeededSuffix = "-samples-needed";

		SharedAudioRpc _audioRpc;
		SoundOutputProvider _bufferedSoundProvider;

		// Running buffer
		short[] buffer;
		int currentBufferSize;
		static readonly int maxBufferSize = 44100*5; // 5 seconds should be plenty

		public UnityHawkSound(string audioBufferName, ISoundProvider soundSource, Func<double> getVsyncRate) {
			soundSource.SetSyncMode(SyncSoundMode.Sync);
			_bufferedSoundProvider = new SoundOutputProvider(getVsyncRate, standaloneMode: true); // [idk why but standalone mode seems to be less distorted]
			_bufferedSoundProvider.BaseSoundProvider = soundSource;

			_audioRpc = new(audioBufferName, audioBufferName+samplesNeededSuffix, GetSamples);

			currentBufferSize = 0;
			buffer = new short[maxBufferSize];
		}

		// Must be run once per emulated frame
		public void Update() {
			// Ask Unity via RPC how many audio samples it's waiting for
			int nSamples = _audioRpc.GetSamplesNeeded();
			// Get samples from soundProvider and append to running buffer
			short[] framebuf = new short[nSamples];
			_bufferedSoundProvider.GetSamples(framebuf);

			if (currentBufferSize + nSamples > maxBufferSize) {
				Console.WriteLine("UnityHawkSound - audio buffer overflowed");
				nSamples = maxBufferSize - currentBufferSize;
			}
			Array.Copy(framebuf, 0, buffer, currentBufferSize, nSamples);
			currentBufferSize += nSamples;
		}

		private short[] GetSamples() {
			// Get all accumulated samples so far
			short[] truncatedBuf = new short[currentBufferSize];
			Array.Copy(buffer, truncatedBuf, currentBufferSize);
			// Empty buffer
			currentBufferSize = 0;
			return truncatedBuf;
		}
	}
}
