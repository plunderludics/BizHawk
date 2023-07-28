// Added for UnityHawk support
// implemented by Input.cs (original OS input)
// and UnityHawkInput (get input from Unity via shared memory)

using System.Collections.Generic;

namespace BizHawk.Client.Common
{
	public interface IInput
	{
		public void Update();
		public InputEvent DequeueEvent();
		public IDictionary<string, int> GetAxisValues();
	}
}
