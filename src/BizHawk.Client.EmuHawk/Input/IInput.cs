// Added for UnityHawk support
// implemented by Input.cs (original OS input)
// and UnityHawkInput (get input from Unity via shared memory)

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using BizHawk.Bizware.DirectX;
using BizHawk.Bizware.OpenTK3;
using BizHawk.Common;
using BizHawk.Client.Common;
using BizHawk.Common.CollectionExtensions;

namespace BizHawk.Client.EmuHawk
{
	public interface IInput
	{
		public void Update();
		public InputEvent DequeueEvent();
		public IDictionary<string, int> GetAxisValues();
	}
}
