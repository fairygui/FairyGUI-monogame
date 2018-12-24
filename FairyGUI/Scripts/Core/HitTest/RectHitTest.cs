using Microsoft.Xna.Framework;

#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class RectHitTest : IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public Rectangle rect { get; set; }

		public void SetEnabled(bool value)
		{
		}

		public bool HitTest(Container container, ref Vector2 localPoint)
		{
			localPoint = container.GlobalToLocal(HitTestContext.screenPoint);
			return rect.Contains(localPoint.X, localPoint.Y);
		}
	}
}
