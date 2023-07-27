// Added for UnityHawk support
// implemented by Input.cs (original OS input)
// and UnityHawkInput (get input from Unity via shared memory)

using System;
using System.Collections.Generic;

using BizHawk.Client.Common;

using System.Runtime.InteropServices;
using SharedMemory;

namespace BizHawk.Client.EmuHawk
{
	public class UnityHawkInput : IInput
	{
		private string _inputBufferName; // a SharedMemory buffer
		private CircularBuffer _inputBuffer;

		private Queue<InputEvent> _inputEvents;

		private int bufferItemSize = Marshal.SizeOf(typeof(BizHawk.UnityHawk.InputEvent));
		public UnityHawkInput(string inputBufferName) {
			_inputBufferName = inputBufferName;
			int nodeCount = 2048; // size of input buffer, should be plenty
			_inputBuffer = new CircularBuffer(_inputBufferName, nodeCount, bufferItemSize);

			_inputEvents = new();
		}

		public void Update() {
			byte[] serialized = new byte[bufferItemSize];
			while (_inputBuffer.Read(serialized, timeout: 0) > 0) {
				BizHawk.UnityHawk.InputEvent uie = RawDeserialize<BizHawk.UnityHawk.InputEvent>(serialized);
				// convert BizHawk.UnityHawk.KeyEvent to BizHawk.Client.Common.KeyEvent
				uint mods = 0; // ignore modifier keys for now
				List<string> emptyList = new();
				BizHawk.Client.Common.InputEvent ie = new BizHawk.Client.Common.InputEvent {
					EventType = (InputEventType)uie.EventType,
					LogicalButton = new(uie.ButtonName, mods, () => emptyList),
					Source = (ClientInputFocus)uie.Source
				};
				// Console.WriteLine($"Read inputevent: {uie}");
				_inputEvents.Enqueue(ie);
			}
		}
		public InputEvent DequeueEvent() {
			return _inputEvents.Count == 0 ? null : _inputEvents.Dequeue();
		}
		public IDictionary<string, int> GetAxisValues() {
			// TODO: for gamepad & mouse support
			return new Dictionary<string, int>();
		}

		private static T RawDeserialize<T>(byte[] rawData, int position = 0)
		{
			int rawsize = Marshal.SizeOf(typeof(T));
			if (rawsize > rawData.Length - position)
				throw new ArgumentException("Not enough data to fill struct. Array length from position: "+(rawData.Length-position) + ", Struct length: "+rawsize);
			IntPtr buffer = Marshal.AllocHGlobal(rawsize);
			Marshal.Copy(rawData, position, buffer, rawsize);
			T retobj = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			return retobj;
		}
	}
}
