using Microsoft.Xna.Framework;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public static class IMEAdapter
	{
		/// <summary>
		/// 
		/// </summary>
		public enum CompositionMode
		{
#if !Windows
			/// <summary>
			/// 
			/// </summary>
			Auto = 0,
#endif
			/// <summary>
			/// 
			/// </summary>
			On = 1,
			/// <summary>
			/// 
			/// </summary>
			Off = 2
		}

		/// <summary>
		/// 
		/// </summary>
		public static Vector2 compositionCursorPos
		{
			get; set;
		}

		private static CompositionMode _compositionMode;
		/// <summary>
		/// 
		/// </summary>
		public static CompositionMode compositionMode
		{
			get { return _compositionMode; }
			set
			{
				_compositionMode = value;
//#if Windows
//				if (compositionMode == CompositionMode.On)
//					Stage.handler.Enabled = true;
//				else if (compositionMode == CompositionMode.Off)
//					Stage.handler.Enabled = false;
//#endif
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static string compositionString
		{
			get; set;
		}

        public static string candidataString { get; set; }


		static IMEAdapter()
		{
			compositionString = string.Empty;
		}
	}
}
