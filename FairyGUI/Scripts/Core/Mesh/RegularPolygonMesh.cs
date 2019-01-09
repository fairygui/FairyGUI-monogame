using System;
using Microsoft.Xna.Framework;
#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class RegularPolygonMesh : IMeshFactory, IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public Rectangle? drawRect;

		/// <summary>
		/// 
		/// </summary>
		public int sides;

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
		public Color? centerColor;

		/// <summary>
		/// 
		/// </summary>
		public Color? fillColor;

		/// <summary>
		/// 
		/// </summary>
		public float[] distances;

		/// <summary>
		/// 
		/// </summary>
		public float rotation;

		public RegularPolygonMesh()
		{
			sides = 3;
			lineColor = Color.Black;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			if (distances != null && distances.Length != sides)
			{
				throw new System.Exception("distances.Length!=sides");
			}

			Rectangle rect = drawRect != null ? (Rectangle)drawRect : vb.contentRect;
			Color color = fillColor != null ? (Color)fillColor : vb.vertexColor;

			rotation = MathHelper.ToRadians(rotation);
			float angleDelta = 2 * (float)Math.PI / sides;
			float angle = rotation;
			float radius = Math.Min(rect.Width / 2, rect.Height / 2);

			float centerX = radius + rect.X;
			float centerY = radius + rect.Y;
			vb.AddVert(new Vector3(centerX, centerY, 0), centerColor == null ? color : (Color)centerColor);
			for (int i = 0; i < sides; i++)
			{
				float r = radius;
				if (distances != null)
					r *= distances[i];
				float xv = (float)Math.Cos(angle) * (r - lineWidth);
				float yv = (float)Math.Sin(angle) * (r - lineWidth);
				Vector3 vec = new Vector3(xv + centerX, yv + centerY, 0);
				vb.AddVert(vec, color);
				if (lineWidth > 0)
				{
					vb.AddVert(vec, lineColor);

					xv = (float)Math.Cos(angle) * r + centerX;
					yv = (float)Math.Sin(angle) * r + centerY;
					vb.AddVert(new Vector3(xv, yv, 0), lineColor);
				}
				angle += angleDelta;
			}

			if (lineWidth > 0)
			{
				int tmp = sides * 3;
				for (int i = 0; i < tmp; i += 3)
				{
					if (i != tmp - 3)
					{
						vb.AddTriangle(0, i + 1, i + 4);
						vb.AddTriangle(i + 5, i + 2, i + 3);
						vb.AddTriangle(i + 3, i + 6, i + 5);
					}
					else
					{
						vb.AddTriangle(0, i + 1, 1);
						vb.AddTriangle(2, i + 2, i + 3);
						vb.AddTriangle(i + 3, 3, 2);
					}
				}
			}
			else
			{
				for (int i = 0; i < sides; i++)
					vb.AddTriangle(0, i + 1, (i == sides - 1) ? 1 : i + 2);
			}
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
