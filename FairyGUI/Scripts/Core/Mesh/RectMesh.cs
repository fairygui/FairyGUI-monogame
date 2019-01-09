using Microsoft.Xna.Framework;
#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class RectMesh : IMeshFactory, IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public Rectangle? drawRect;

		/// <summary>
		/// 
		/// </summary>
		public float lineWidth;

		/// <summary>
		/// 
		/// </summary>
		public Color lineColor;

		/// <summary>
		/// 
		/// </summary>
		public Color? fillColor;

		/// <summary>
		/// 
		/// </summary>
		public Color[] colors;

		public RectMesh()
		{
			lineColor = Color.Black;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Rectangle rect = drawRect != null ? (Rectangle)drawRect : vb.contentRect;
			Color color = fillColor != null ? (Color)fillColor : vb.vertexColor;
			if (lineWidth == 0)
			{
				if (color.A != 0)//optimized
					vb.AddQuad(rect, color);
			}
			else
			{
				Rectangle part;

				//left,right
				part = Rectangle.FromLTRB(rect.X, rect.Y, lineWidth, rect.Height);
				vb.AddQuad(part, lineColor);
				part = Rectangle.FromLTRB(rect.Right - lineWidth, 0, rect.Right, rect.Bottom);
				vb.AddQuad(part, lineColor);

				//top, bottom
				part = Rectangle.FromLTRB(lineWidth, rect.X, rect.Right - lineWidth, lineWidth);
				vb.AddQuad(part, lineColor);
				part = Rectangle.FromLTRB(lineWidth, rect.Bottom - lineWidth, rect.Right - lineWidth, rect.Bottom);
				vb.AddQuad(part, lineColor);

				//middle
				if (color.A != 0)//optimized
				{
					part = Rectangle.FromLTRB(lineWidth, lineWidth, rect.Right - lineWidth, rect.Bottom - lineWidth);
					vb.AddQuad(part, color);
				}
			}

			if (colors != null)
				vb.RepeatColors(colors, 0, vb.currentVertCount);

			vb.AddTriangles();
		}

		public bool HitTest(Rectangle contentRect, Vector2 point)
		{
			if (drawRect != null)
				return ((Rectangle)drawRect).Contains(point.X, point.Y);
			else
				return contentRect.Contains(point.X, point.Y);
		}
	}
}
