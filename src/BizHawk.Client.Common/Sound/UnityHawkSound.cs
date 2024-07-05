using System;

using BizHawk.Emulation.Common;
using Plunderludics.UnityHawk.SharedBuffers;

// this should probably be in a different namespace/dll actually
namespace BizHawk.Client.Common
{
	public class UnityHawkSound
	{
		// [This has to match the one in Unity - this sucks change it]
		private const string SAMPLES_NEEDED_SUFFIX = "-samples-needed";

		private readonly SharedAudioRpc _audioRpc;
		private readonly SoundOutputProvider _bufferedSoundProvider;

		// Running buffer
		private const int MAX_BUFFER_SIZE = 44100 * 5; // 5 seconds should be plenty
		private readonly short[] _buffer;
		private int _currentBufferSize;

		public UnityHawkSound(string audioBufferName, ISoundProvider soundSource, Func<double> getVsyncRate) {
			soundSource.SetSyncMode(SyncSoundMode.Sync);
			_bufferedSoundProvider = new (getVsyncRate, standaloneMode: true)
				{
					BaseSoundProvider = soundSource
				}; // [idk why but standalone mode seems to be less distorted]

			_audioRpc = new(audioBufferName, audioBufferName+SAMPLES_NEEDED_SUFFIX, GetSamples);

			_currentBufferSize = 0;
			_buffer = new short[MAX_BUFFER_SIZE];
		}

		// Must be run once per emulated frame
		public void Update() {
			// Ask Unity via RPC how many audio samples it's waiting for
			var nSamples = _audioRpc.GetSamplesNeeded();
			// Get samples from soundProvider and append to running buffer
			var frameBuf = new short[nSamples];
			_bufferedSoundProvider.GetSamples(frameBuf);

			if (_currentBufferSize + nSamples > MAX_BUFFER_SIZE) {
				Console.WriteLine("UnityHawkSound - audio buffer overflowed");
				nSamples = MAX_BUFFER_SIZE - _currentBufferSize;
			}
			Array.Copy(frameBuf, 0, _buffer, _currentBufferSize, nSamples);
			_currentBufferSize += nSamples;
		}

		private short[] GetSamples() {
			// Get all accumulated samples so far
			var truncatedBuf = new short[_currentBufferSize];
			Array.Copy(_buffer, truncatedBuf, _currentBufferSize);
			// Empty buffer
			_currentBufferSize = 0;
			return truncatedBuf;
		}
	}
}
