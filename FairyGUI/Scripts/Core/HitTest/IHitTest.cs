using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

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
