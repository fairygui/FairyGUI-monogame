using Microsoft.Xna.Framework;
#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum HitTestMode
	{
		Default,
		Raycast
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="contentRect"></param>
		/// <param name="localPoint"></param>
		/// <returns></returns>
		bool HitTest(Rectangle contentRect, Vector2 localPoint);
	}
}
