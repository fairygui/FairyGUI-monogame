using System;
using System.Runtime.InteropServices;

namespace FairyGUI_IME
{
    internal sealed class IMM
    {
        [DllImport("imm32.dll", CharSet = CharSet.Auto)]
        public extern static IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll", CharSet = CharSet.Auto)]
        public extern static IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);
    }

    public static class WindowMessage
    {
        public const int ImeSetContext = 0x0281;
        public const int InputLanguageChange = 0x0051;
    }
}