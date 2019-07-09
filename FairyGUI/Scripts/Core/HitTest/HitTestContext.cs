using Microsoft.Xna.Framework;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class HitTestContext
	{
		//set before hit test
		public static Vector2 screenPoint;
		public static bool raycastDone;
		public static uint hitEntityId;
		public static Vector2 hitUV;
		public static bool forTouch;
        public static int displayIndex;
    }
}
