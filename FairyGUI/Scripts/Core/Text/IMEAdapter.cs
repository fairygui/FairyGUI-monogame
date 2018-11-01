using System;
using System.Text;
using FairyGUI.Scripts.Core.Text;
using Microsoft.Xna.Framework;

namespace FairyGUI
{
	public static class IMEAdapter
	{
	    public enum CompositionMode
		{
			//Auto = 0, // 没有Auto属性
			On = 1,
			Off = 2
		}

		public static Vector2 compositionCursorPos
		{
			set { }
		}

		public static CompositionMode compositionMode
		{
		    set
		    {
		        if (value == CompositionMode.On)
		            Stage.Handler.Enabled = true;
                else if (value == CompositionMode.Off)
		            Stage.Handler.Enabled = false;

		    }
		}

		public static string compositionString
		{
			get { return string.Empty; }
		}

	    public static string content { get; set; }

	    /*
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);
        [DllImport("Imm32.dll")]
        public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);
        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        private static extern int ImmGetCompositionStringW(IntPtr hIMC, int dwIndex, byte[] lpBuf, int dwBufLen);
        [DllImport("imm32.dll", EntryPoint = "ImmGetConversionStatus")]
        public static extern bool ImmGetConversionStatus(
               IntPtr himc,
               ref IntPtr lpdw,
               ref IntPtr lpdw2
       );
        [DllImport("imm32.dll", EntryPoint = "ImmSetConversionStatus")]
        public static extern bool ImmSetConversionStatus(
                IntPtr himc,
                IntPtr dw1,
                IntPtr dw2
        );
        [DllImport("imm32.dll")]
        public static extern bool ImmSetOpenStatus(IntPtr himc, bool b);
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();

        public const int IME_CMODE_SOFTKBD = 0x80;

        public void OpenIME()
        {
            IntPtr hwndInput = ImmGetContext(GetActiveWindow());
            ImmSetOpenStatus(hwndInput, true);
            IntPtr dw1 = IntPtr.Zero;
            IntPtr dw2 = IntPtr.Zero;
            bool isSuccess = ImmGetConversionStatus(hwndInput, ref dw1, ref dw2);
            Log.Info("" + hwndInput + "," + isSuccess);
            if (isSuccess)
            {
                int intTemp = dw1.ToInt32() & IME_CMODE_SOFTKBD;
                if (intTemp > 0)
                    dw1 = (IntPtr)(dw1.ToInt32() ^ IME_CMODE_SOFTKBD);
                else
                    dw1 = (IntPtr)(dw1.ToInt32() | IME_CMODE_SOFTKBD);
            }
            isSuccess = ImmSetConversionStatus(hwndInput, dw1, dw2);
            ImmReleaseContext(GetActiveWindow(), hwndInput);
        }

    private const int GCS_COMPSTR = 8;

    /// IntPtr handle is the handle to the textbox
    public string CurrentCompStr(IntPtr handle)
    {
        int readType = GCS_COMPSTR;

        IntPtr hIMC = ImmGetContext(handle);
        try
        {
            int strLen = ImmGetCompositionStringW(hIMC, readType, null, 0);

            if (strLen > 0)
            {
                byte[] buffer = new byte[strLen];

                ImmGetCompositionStringW(hIMC, readType, buffer, strLen);

                return Encoding.Unicode.GetString(buffer);

            }
            else
            {
                return string.Empty;
            }
        }
        finally
        {
            ImmReleaseContext(handle, hIMC);
        }
    }*/
	}
}
