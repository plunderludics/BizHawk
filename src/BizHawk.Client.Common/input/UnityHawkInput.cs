using System;
using System.Collections.Generic;

using Plunderludics.UnityHawk.SharedBuffers;

// this should probably be in a different namespace/dll actually
namespace BizHawk.Client.Common
{
	public class UnityHawkInput : IInput
	{
		private Queue<InputEvent> _inputEvents;
		private SharedKeyInputBuffer _keyInputBuffer;
		private SharedAnalogInputBuffer _analogInputBuffer;
		public UnityHawkInput(string keyInputBufferName, string analogInputBufferName) {
			// TODO: this should not crash if either of the strings given is null
			_keyInputBuffer = new(keyInputBufferName);
			_analogInputBuffer = new(analogInputBufferName);
			_inputEvents = new();
		}

		public void Update() {	
			Plunderludics.UnityHawk.InputEvent? uie;
			while ((uie = _keyInputBuffer.Read()).HasValue) {
				// convert Plunderludics.UnityHawk.InputEvent to BizHawk.Client.Common.InputEvent
				uint mods = 0; // ignore modifier keys for now
				List<string> emptyList = new();
				BizHawk.Client.Common.InputEvent ie = new BizHawk.Client.Common.InputEvent {
					EventType = (InputEventType)uie?.EventType,
					LogicalButton = new(uie?.ButtonName, mods, () => emptyList),
					Source = (ClientInputFocus)uie?.Source
				};
				// Console.WriteLine($"Read inputevent: {uie}");
				_inputEvents.Enqueue(ie);
			}
		}
		public InputEvent DequeueEvent() {
			return _inputEvents.Count == 0 ? null : _inputEvents.Dequeue();
		}
		public IDictionary<string, int> GetAxisValues() {
			// For analog inputs (gamepad & mouse)
			return _analogInputBuffer.Read();
		}
	}
}
