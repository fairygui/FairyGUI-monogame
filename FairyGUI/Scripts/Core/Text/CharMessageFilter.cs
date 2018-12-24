using System;
using System.Runtime.InteropServices;

#if Windows || DesktopGL
using System.Windows.Forms;
#endif

namespace FairyGUI.Scripts.Core.Text
{
	public class CharMessageFilter
	{
		public static bool Added { get; private set; }
		public static int MouseWheel { get; private set; }

		public static void AddFilter()
		{
			if (!Added)
				Application.AddMessageFilter(new filter());
			Added = true;
		}

		private class filter : IMessageFilter
		{
			public bool PreFilterMessage(ref Message m)
			{
				switch (m.Msg)
				{
#if Windows
					case IMM.KeyDown:

						IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(m));
						Marshal.StructureToPtr(m, intPtr, true);
						IMM.TranslateMessage(intPtr);

						return false;
#endif
					case 0x020A:
						// Mouse wheel is not correct if the IME helper is used, thus it is needed to grab the value here.
						MouseWheel += (int)(short)((uint)(int)m.WParam >> 16);
						return false;
				}
				return false;
			}
		}
	}
}