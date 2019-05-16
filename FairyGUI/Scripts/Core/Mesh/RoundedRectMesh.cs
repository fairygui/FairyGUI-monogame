using System;
using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	public class RoundedRectMesh : IMeshFactory, IHitTest
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
		public float topLeftRadius;

		/// <summary>
		/// 
		/// </summary>
		public float topRightRadius;

		/// <summary>
		/// 
		/// </summary>
		public float bottomLeftRadius;

		/// <summary>
		/// 
		/// </summary>
		public float bottomRightRadius;

		public RoundedRectMesh()
		{
			lineColor = Color.Black;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Rectangle rect = drawRect != null ? (Rectangle)drawRect : vb.contentRect;
			Color color = fillColor != null ? (Color)fillColor : vb.vertexColor;

			float radiusX = rect.Width / 2;
			float radiusY = rect.Height / 2;
			float cornerMaxRadius = Math.Min(radiusX, radiusY);
			float centerX = radiusX + rect.X;
			float centerY = radiusY + rect.Y;

			vb.AddVert(new Vector3(centerX, centerY, 0), color);

			int cnt = vb.currentVertCount;
			for (int i = 0; i < 4; i++)
			{
				float radius = 0;
				switch (i)
				{
					case 0:
						radius = bottomRightRadius;
						break;

					case 1:
						radius = bottomLeftRadius;
						break;

					case 2:
						radius = topLeftRadius;
						break;

					case 3:
						radius = topRightRadius;
						break;
				}
				radius = Math.Min(cornerMaxRadius, radius);

				float offsetX = rect.X;
				float offsetY = rect.Y;

				if (i == 0 || i == 3)
					offsetX = rect.Right - radius * 2;
				if (i == 0 || i == 1)
					offsetY = rect.Bottom - radius * 2;

				if (radius != 0)
				{
					int partNumSides = (int)Math.Max(1, Math.Ceiling(Math.PI * radius / 8)) + 1;
					float angleDelta = (float)Math.PI / 2 / partNumSides;
					float angle = (float)Math.PI / 2 * i;
					float startAngle = angle;

					for (int j = 1; j <= partNumSides; j++)
					{
						if (j == partNumSides) //消除精度误差带来的不对齐
							angle = startAngle + (float)Math.PI / 2;
						Vector3 v1 = new Vector3(offsetX + (float)Math.Cos(angle) * (radius - lineWidth) + radius,
							offsetY + (float)Math.Sin(angle) * (radius - lineWidth) + radius, 0);
						vb.AddVert(v1, color);
						if (lineWidth != 0)
						{
							vb.AddVert(v1, lineColor);
							vb.AddVert(new Vector3(offsetX + (float)Math.Cos(angle) * radius + radius,
								offsetY + (float)Math.Sin(angle) * radius + radius, 0), lineColor);
						}
						angle += angleDelta;
					}
				}
				else
				{
					Vector3 v1 = new Vector3(offsetX, offsetY, 0);
					if (lineWidth != 0)
					{
						if (i == 0 || i == 3)
							offsetX -= lineWidth;
						else
							offsetX += lineWidth;
						if (i == 0 || i == 1)
							offsetY -= lineWidth;
						else
							offsetY += lineWidth;
						Vector3 v2 = new Vector3(offsetX, offsetY, 0);
						vb.AddVert(v2, color);
						vb.AddVert(v2, lineColor);
						vb.AddVert(v1, lineColor);
					}
					else
						vb.AddVert(v1, color);
				}
			}
			cnt = vb.currentVertCount - cnt;

			if (lineWidth > 0)
			{
				for (int i = 0; i < cnt; i += 3)
				{
					if (i != cnt - 3)
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
				for (int i = 0; i < cnt; i++)
					vb.AddTriangle(0, i + 1, (i == cnt - 1) ? 1 : i + 2);
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
