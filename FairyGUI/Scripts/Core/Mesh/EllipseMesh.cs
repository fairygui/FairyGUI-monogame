using System;
using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class EllipseMesh : IMeshFactory, IHitTest
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
		public Color? centerColor;

		/// <summary>
		/// 
		/// </summary>
		public Color? fillColor;

		/// <summary>
		/// 
		/// </summary>
		public float startDegree;

		/// <summary>
		/// 
		/// </summary>
		public float endDegreee;

		static int[] SECTOR_CENTER_TRIANGLES = new int[] {
			0, 4, 1,
			0, 3, 4,
			0, 2, 3,
			0, 8, 5,
			0, 7, 8,
			0, 6, 7,
			6, 5, 2,
			2, 1, 6
		};

		public EllipseMesh()
		{
			lineColor = Color.Black;
			startDegree = 0;
			endDegreee = 360;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Rectangle rect = drawRect != null ? (Rectangle)drawRect : vb.contentRect;
			Color color = fillColor != null ? (Color)fillColor : vb.vertexColor;

			float sectionStart = MathHelper.Clamp(startDegree, 0, 360);
			float sectionEnd = MathHelper.Clamp(endDegreee, 0, 360);
			bool clipped = sectionStart > 0 || sectionEnd < 360;
			sectionStart = MathHelper.ToRadians(sectionStart);
			sectionEnd = MathHelper.ToRadians(sectionEnd);
			Color centerColor2 = centerColor == null ? color : (Color)centerColor;

			float radiusX = rect.Width / 2;
			float radiusY = rect.Height / 2;
			int sides = (int)Math.Ceiling(Math.PI * (radiusX + radiusY) / 4);
			if (sides < 6)
				sides = 6;
			float angleDelta = (float)(2 * Math.PI / sides);
			float angle = 0;
			float lineAngle = 0;

			if (lineWidth > 0 && clipped)
			{
				lineAngle = lineWidth / Math.Max(radiusX, radiusY);
				sectionStart += lineAngle;
				sectionEnd -= lineAngle;
			}

			int vpos = vb.currentVertCount;
			float centerX = rect.X + radiusX;
			float centerY = rect.Y + radiusY;
			vb.AddVert(new Vector3(centerX, centerY, 0), centerColor2);
			for (int i = 0; i < sides; i++)
			{
				if (angle < sectionStart)
					angle = sectionStart;
				else if (angle > sectionEnd)
					angle = sectionEnd;
				Vector3 vec = new Vector3((float)Math.Cos(angle) * (radiusX - lineWidth) + centerX, (float)Math.Sin(angle) * (radiusY - lineWidth) + centerY, 0);
				vb.AddVert(vec, color);
				if (lineWidth > 0)
				{
					vb.AddVert(vec, lineColor);
					vb.AddVert(new Vector3((float)Math.Cos(angle) * radiusX + centerX, (float)Math.Sin(angle) * radiusY + centerY, 0), lineColor);
				}
				angle += angleDelta;
			}

			if (lineWidth > 0)
			{
				int cnt = sides * 3;
				for (int i = 0; i < cnt; i += 3)
				{
					if (i != cnt - 3)
					{
						vb.AddTriangle(0, i + 1, i + 4);
						vb.AddTriangle(i + 5, i + 2, i + 3);
						vb.AddTriangle(i + 3, i + 6, i + 5);
					}
					else if (!clipped)
					{
						vb.AddTriangle(0, i + 1, 1);
						vb.AddTriangle(2, i + 2, i + 3);
						vb.AddTriangle(i + 3, 3, 2);
					}
					else
					{
						vb.AddTriangle(0, i + 1, i + 1);
						vb.AddTriangle(i + 2, i + 2, i + 3);
						vb.AddTriangle(i + 3, i + 3, i + 2);
					}
				}
			}
			else
			{
				for (int i = 0; i < sides; i++)
				{
					if (i != sides - 1)
						vb.AddTriangle(0, i + 1, i + 2);
					else if (!clipped)
						vb.AddTriangle(0, i + 1, 1);
					else
						vb.AddTriangle(0, i + 1, i + 1);
				}
			}

			if (lineWidth > 0 && clipped)
			{
				//扇形内边缘的线条

				vb.AddVert(new Vector3(radiusX, radiusY, 0), lineColor);
				float centerRadius = lineWidth * 0.5f;

				sectionStart -= lineAngle;
				angle = sectionStart + lineAngle * 0.5f + (float)Math.PI * 0.5f;
				vb.AddVert(new Vector3((float)Math.Cos(angle) * centerRadius + radiusX, (float)Math.Sin(angle) * centerRadius + radiusY, 0), lineColor);
				angle -= (float)Math.PI;
				vb.AddVert(new Vector3((float)Math.Cos(angle) * centerRadius + radiusX, (float)Math.Sin(angle) * centerRadius + radiusY, 0), lineColor);
				vb.AddVert(new Vector3((float)Math.Cos(sectionStart) * radiusX + radiusX, (float)Math.Sin(sectionStart) * radiusY + radiusY, 0), lineColor);
				vb.AddVert(vb.GetPosition(vpos + 3), lineColor);

				sectionEnd += lineAngle;
				angle = sectionEnd - lineAngle * 0.5f + (float)Math.PI * 0.5f;
				vb.AddVert(new Vector3((float)Math.Cos(angle) * centerRadius + radiusX, (float)Math.Sin(angle) * centerRadius + radiusY, 0), lineColor);
				angle -= (float)Math.PI;
				vb.AddVert(new Vector3((float)Math.Cos(angle) * centerRadius + radiusX, (float)Math.Sin(angle) * centerRadius + radiusY, 0), lineColor);
				vb.AddVert(vb.GetPosition(vpos + sides * 3), lineColor);
				vb.AddVert(new Vector3((float)Math.Cos(sectionEnd) * radiusX + radiusX, (float)Math.Sin(sectionEnd) * radiusY + radiusY, 0), lineColor);

				vb.AddTriangles(SECTOR_CENTER_TRIANGLES, sides * 3 + 1);
			}
		}

		public bool HitTest(Rectangle contentRect, Vector2 point)
		{
			Rectangle rect = drawRect != null ? (Rectangle)drawRect : contentRect;
			if (!rect.Contains(point.X, point.Y))
				return false;

			float radiusX = rect.Width * 0.5f;
			float raduisY = rect.Height * 0.5f;
			float xx = point.X - radiusX - rect.X;
			float yy = point.Y - raduisY - rect.Y;
			if (Math.Pow(xx / radiusX, 2) + Math.Pow(yy / raduisY, 2) < 1)
			{
				if (startDegree != 0 || endDegreee != 360)
				{
					float deg = MathHelper.ToDegrees((float)Math.Atan2(yy, xx));
					if (deg < 0)
						deg += 360;
					return deg >= startDegree && deg <= endDegreee;
				}
				else
					return true;
			}

			return false;
		}
	}
}
