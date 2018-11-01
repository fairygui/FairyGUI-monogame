using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FairyGUI
{
	[Flags]
	public enum InputModifierFlags
	{
		None = 0,
		LCtrl = 1,
		LShift = 2,
		LAlt = 4,
		LWin = 8,
		RCtrl = 16,
		Ctrl = 17,
		RShift = 32,
		Shift = 34,
		RAlt = 64,
		Alt = 68,
		RWin = 128,
		Win = 136,
		Modifiers = 255,
		NumLock = 256,
		CapsLock = 512,
		ScrollLock = 1024,
		LockKeys = 1792

	}
	/// <summary>
	/// 
	/// </summary>
	public class InputEvent
	{
		/// <summary>
		/// x position in stage coordinates.
		/// </summary>
		public float x { get; internal set; }

		/// <summary>
		/// y position in stage coordinates.
		/// </summary>
		public float y { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public Keys keyCode { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public string KeyName { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public InputModifierFlags modifiers { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public int mouseWheelDelta { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		public int touchId { get; internal set; }

		/// <summary>
		/// -1-none,0-left,1-right,2-middle
		/// </summary>
		public int button { get; internal set; }

		internal int clickCount;

		public InputEvent()
		{
			touchId = -1;
			x = 0;
			y = 0;
			clickCount = 0;
			keyCode = Keys.None;
			KeyName = null;
			modifiers = 0;
			mouseWheelDelta = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 position
		{
			get { return new Vector2(x, y); }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDoubleClick
		{
			get { return clickCount > 1; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool ctrl
		{
			get
			{
				return ((modifiers & InputModifierFlags.LCtrl) != 0) ||
					((modifiers & InputModifierFlags.RCtrl) != 0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool shift
		{
			get
			{
				return ((modifiers & InputModifierFlags.LShift) != 0) ||
					((modifiers & InputModifierFlags.RShift) != 0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool alt
		{
			get
			{
				return ((modifiers & InputModifierFlags.LAlt) != 0) ||
					((modifiers & InputModifierFlags.RAlt) != 0);
			}
		}
	}
}
