using System;
using System.Runtime.InteropServices;
using System.Text;
using FairyGUI.Scripts.Core.Text;
using FairyGUI.Utils;
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

	    private static CompositionMode _compositionMode;

		public static CompositionMode compositionMode
		{
		    set
		    {
		        if (value == CompositionMode.On)
		            Stage.Handler.Enabled = true;
                else if (value == CompositionMode.Off)
		            Stage.Handler.Enabled = false;

		        _compositionMode = value;

		    }
		    get { return _compositionMode; }
		}

		public static string compositionString
		{
			get { return string.Empty; }
		}
	}
}
