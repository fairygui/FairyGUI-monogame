using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NGraphics
	{
		/// <summary>
		/// 
		/// </summary>
		public Vector3[] vertices { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Vector2[] uv { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public Color[] colors { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int[] triangles { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int vertCount { get; private set; }

		/// <summary>
		/// 不参与剪裁
		/// </summary>
		public bool dontClip;

		public bool enabled;
		public bool pixelSnapping;

		NTexture _texture;

		/// <summary>
		/// 写死的一些三角形顶点组合，避免每次new
		/// 1---2
		/// | / |
		/// 0---3
		/// </summary>
		public static int[] TRIANGLES = new int[] { 0, 1, 2, 2, 3, 0 };

		/// <summary>
		/// 
		/// </summary>
		public static int[] TRIANGLES_9_GRID = new int[] {
			4,0,1,1,5,4,
			5,1,2,2,6,5,
			6,2,3,3,7,6,
			8,4,5,5,9,8,
			9,5,6,6,10,9,
			10,6,7,7,11,10,
			12,8,9,9,13,12,
			13,9,10,10,14,13,
			14,10,11,
			11,15,14
		};

		/// <summary>
		/// 
		/// </summary>
		public static int[] TRIANGLES_4_GRID = new int[] {
			4, 0, 5,
			4, 5, 1,
			4, 1, 6,
			4, 6, 2,
			4, 2, 7,
			4, 7, 3,
			4, 3, 8,
			4, 8, 0
		};

		/// <summary>
		/// 
		/// </summary>
		public NGraphics()
		{
			enabled = true;

			Stats.LatestGraphicsCreation++;
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get { return _texture; }
			set
			{
				if (_texture != value)
				{
					_texture = value;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertCount"></param>
		public void Alloc(int vertCount)
		{
			if (vertices == null || vertices.Length != vertCount)
			{
				vertices = new Vector3[vertCount];
				uv = new Vector2[vertCount];
				colors = new Color[vertCount];
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void UpdateMesh()
		{
			vertCount = vertices.Length;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="color"></param>
		public void DrawRect(Rectangle vertRect, Rectangle uvRect, Color color)
		{
			Alloc(4);
			FillVerts(0, vertRect);
			FillUV(0, uvRect);
			this.triangles = TRIANGLES;

			FillColors(color);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="lineSize"></param>
		/// <param name="lineColor"></param>
		/// <param name="fillColor"></param>
		public void DrawRect(Rectangle vertRect, Rectangle uvRect, int lineSize, Color lineColor, Color fillColor)
		{
			if (lineSize == 0)
			{
				DrawRect(vertRect, uvRect, fillColor);
			}
			else
			{
				Alloc(20);

				Rectangle rect;
				//left,right
				rect = new Rectangle(0, 0, lineSize, vertRect.Height);
				FillVerts(0, rect);
				rect = new Rectangle(vertRect.Width - lineSize, 0, lineSize, vertRect.Height);
				FillVerts(4, rect);

				//top, bottom
				rect = new Rectangle(lineSize, 0, vertRect.Width - lineSize * 2, lineSize);
				FillVerts(8, rect);

				rect = new Rectangle(lineSize, vertRect.Height - lineSize, vertRect.Width - lineSize * 2, lineSize);
				FillVerts(12, rect);

				//middle
				rect = new Rectangle(lineSize, lineSize, vertRect.Width - lineSize * 2, vertRect.Height - lineSize * 2);
				FillVerts(16, rect);

				FillShapeUV(ref vertRect, ref uvRect);

				Color[] arr = this.colors;
				for (int i = 0; i < 16; i++)
					arr[i] = lineColor;

				for (int i = 16; i < 20; i++)
					arr[i] = fillColor;

				FillTriangles();
			}
		}

		static float[] sCornerRadius = new float[] { 0, 0, 0, 0 };

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="fillColor"></param>
		/// <param name="topLeftRadius"></param>
		/// <param name="topRightRadius"></param>
		/// <param name="bottomLeftRadius"></param>
		/// <param name="bottomRightRadius"></param>
		public void DrawRoundRect(Rectangle vertRect, Rectangle uvRect, Color fillColor,
			float topLeftRadius, float topRightRadius, float bottomLeftRadius, float bottomRightRadius)
		{
			sCornerRadius[0] = topRightRadius;
			sCornerRadius[1] = topLeftRadius;
			sCornerRadius[2] = bottomLeftRadius;
			sCornerRadius[3] = bottomRightRadius;

			int numSides = 0;
			for (int i = 0; i < 4; i++)
			{
				float radius = sCornerRadius[i];

				if (radius != 0)
				{
					float radiusX = MathHelper.Min(radius, vertRect.Width / 2);
					float radiusY = MathHelper.Min(radius, vertRect.Height / 2);
					numSides += MathHelper.Max(1, (int)Math.Ceiling(Math.PI * (radiusX + radiusY) / 4 / 4)) + 1;
				}
				else
					numSides++;
			}

			Alloc(numSides + 1);
			Vector3[] vertices = this.vertices;

			vertices[0] = new Vector3(vertRect.Width / 2, vertRect.Height / 2, 0);
			int k = 1;

			/*
			 2 - 3
			 | / |
			 1 - 0
			 */
			for (int i = 0; i < 4; i++)
			{
				float radius = sCornerRadius[i];

				float radiusX = MathHelper.Min(radius, vertRect.Width / 2);
				float radiusY = MathHelper.Min(radius, vertRect.Height / 2);

				float offsetX = 0;
				float offsetY = 0;
				
				if (i == 0 || i == 3)
					offsetX = vertRect.Width - radiusX * 2;
				if (i == 0 || i == 1)
					offsetY = vertRect.Height - radiusY * 2;

				if (radius != 0)
				{
					float partNumSides = MathHelper.Max(1, (float)Math.Ceiling(Math.PI * (radiusX + radiusY) / 4 / 4)) + 1;
					float angleDelta = (float)Math.PI / 2 / partNumSides;
					float angle = (float)Math.PI / 2 * i;
					float startAngle = angle;

					for (int j = 1; j <= partNumSides; j++)
					{
						if (j == partNumSides) //消除精度误差带来的不对齐
							angle = startAngle + (float)Math.PI / 2;
						vertices[k] = new Vector3(offsetX + (float)Math.Cos(angle) * radiusX + radiusX,
							offsetY + (float)Math.Sin(angle) * radiusY + radiusY, 0);
						angle += angleDelta;
						k++;
					}
				}
				else
				{
					vertices[k] = new Vector3(offsetX, offsetY, 0);
					k++;
				}
			}

			FillShapeUV(ref vertRect, ref uvRect);

			AllocTriangleArray(numSides * 3);
			int[] triangles = this.triangles;

			k = 0;
			for (int i = 1; i < numSides; i++)
			{
				triangles[k++] = 0;
				triangles[k++] = i;
				triangles[k++] = i + 1;
			}
			triangles[k++] = 0;
			triangles[k++] = numSides;
			triangles[k++] = 1;

			FillColors(fillColor);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="fillColor"></param>
		public void DrawEllipse(Rectangle vertRect, Rectangle uvRect, Color fillColor)
		{
			float radiusX = vertRect.Width / 2;
			float radiusY = vertRect.Height / 2;
			int numSides = (int)Math.Ceiling(Math.PI * (radiusX + radiusY) / 4);
			if (numSides < 6) numSides = 6;

			Alloc(numSides + 1);
			Vector3[] vertices = this.vertices;

			float angleDelta = 2 * (float)Math.PI / numSides;
			float angle = 0;

			vertices[0] = new Vector3(radiusX, radiusY, 0);
			for (int i = 1; i <= numSides; i++)
			{
				vertices[i] = new Vector3((float)Math.Cos(angle) * radiusX + radiusX,
					(float)Math.Sin(angle) * radiusY + radiusY, 0);
				angle += angleDelta;
			}

			FillShapeUV(ref vertRect, ref uvRect);

			AllocTriangleArray(numSides * 3);
			int[] triangles = this.triangles;

			int k = 0;
			for (int i = 1; i < numSides; i++)
			{
				triangles[k++] = 0;
				triangles[k++] = i;
				triangles[k++] = i + 1;
			}
			triangles[k++] = 0;
			triangles[k++] = numSides;
			triangles[k++] = 1;

			FillColors(fillColor);
		}

		static List<int> sRestIndices = new List<int>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="points"></param>
		/// <param name="fillColor"></param>
		public void DrawPolygon(Rectangle vertRect, Rectangle uvRect, Vector2[] points, Color fillColor)
		{
			int numVertices = points.Length;
			if (numVertices < 3)
				return;

			int numTriangles = numVertices - 2;
			int i, restIndexPos, numRestIndices;
			int k = 0;

			Alloc(numVertices);
			Vector3[] vertices = this.vertices;

			for (i = 0; i < numVertices; i++)
				vertices[i] = new Vector3(points[i].X, -points[i].Y, 0);

			FillShapeUV(ref vertRect, ref uvRect);

			// Algorithm "Ear clipping method" described here:
			// -> https://en.wikipedia.org/wiki/Polygon_triangulation
			//
			// Implementation inspired by:
			// -> http://polyk.ivank.net
			// -> Starling

			AllocTriangleArray(numTriangles * 3);
			int[] triangles = this.triangles;

			sRestIndices.Clear();
			for (i = 0; i < numVertices; ++i)
				sRestIndices.Add(i);

			restIndexPos = 0;
			numRestIndices = numVertices;

			Vector2 a, b, c, p;
			int otherIndex;
			bool earFound;
			int i0, i1, i2;

			while (numRestIndices > 3)
			{
				earFound = false;
				i0 = sRestIndices[restIndexPos % numRestIndices];
				i1 = sRestIndices[(restIndexPos + 1) % numRestIndices];
				i2 = sRestIndices[(restIndexPos + 2) % numRestIndices];

				a = points[i0];
				b = points[i1];
				c = points[i2];

				if ((a.Y - b.Y) * (c.X - b.X) + (b.X - a.X) * (c.Y - b.Y) >= 0)
				{
					earFound = true;
					for (i = 3; i < numRestIndices; ++i)
					{
						otherIndex = sRestIndices[(restIndexPos + i) % numRestIndices];
						p = points[otherIndex];

						if (ToolSet.IsPointInTriangle(ref p, ref a, ref b, ref c))
						{
							earFound = false;
							break;
						}
					}
				}

				if (earFound)
				{
					triangles[k++] = i0;
					triangles[k++] = i1;
					triangles[k++] = i2;
					sRestIndices.RemoveAt((restIndexPos + 1) % numRestIndices);

					numRestIndices--;
					restIndexPos = 0;
				}
				else
				{
					restIndexPos++;
					if (restIndexPos == numRestIndices) break; // no more ears
				}
			}
			triangles[k++] = sRestIndices[0];
			triangles[k++] = sRestIndices[1];
			triangles[k++] = sRestIndices[2];

			FillColors(fillColor);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="uvRect"></param>
		/// <param name="method"></param>
		/// <param name="amount"></param>
		/// <param name="origin"></param>
		/// <param name="clockwise"></param>
		public void DrawRectWithFillMethod(Rectangle vertRect, Rectangle uvRect, Color fillColor,
			FillMethod method, float amount, int origin, bool clockwise)
		{
			amount = MathHelper.Clamp(amount, 0, 1);
			switch (method)
			{
				case FillMethod.Horizontal:
					Alloc(4);
					FillUtils.FillHorizontal((OriginHorizontal)origin, amount, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Vertical:
					Alloc(4);
					FillUtils.FillVertical((OriginVertical)origin, amount, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial90:
					Alloc(4);
					FillUtils.FillRadial90((Origin90)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial180:
					Alloc(8);
					FillUtils.FillRadial180((Origin180)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;

				case FillMethod.Radial360:
					Alloc(12);
					FillUtils.FillRadial360((Origin360)origin, amount, clockwise, vertRect, uvRect, vertices, uv);
					break;
			}

			FillColors(fillColor);
			FillTriangles();
		}

		void FillShapeUV(ref Rectangle vertRect, ref Rectangle uvRect)
		{
			Vector3[] vertices = this.vertices;
			Vector2[] uv = this.uv;

			int len = vertices.Length;
			for (int i = 0; i < len; i++)
			{
				uv[i] = new Vector2(MathHelper.Lerp(uvRect.X, uvRect.Right, (vertices[i].X - vertRect.X) / vertRect.Width),
					MathHelper.Lerp(uvRect.Y, uvRect.Bottom, (vertices[i].Y - vertRect.Y) / vertRect.Height));
			}
		}

		/// <summary>
		/// 从当前顶点缓冲区位置开始填入一个矩形的四个顶点
		/// </summary>
		/// <param name="index">填充位置顶点索引</param>
		/// <param name="rect"></param>
		public void FillVerts(int index, Rectangle rect)
		{
			vertices[index] = new Vector3(rect.X, rect.Bottom, 0f);
			vertices[index + 1] = new Vector3(rect.X, rect.Y, 0f);
			vertices[index + 2] = new Vector3(rect.Right, rect.Y, 0f);
			vertices[index + 3] = new Vector3(rect.Right, rect.Bottom, 0f);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="rect"></param>
		public void FillUV(int index, Rectangle rect)
		{
			uv[index] = new Vector2(rect.X, rect.Bottom);
			uv[index + 1] = new Vector2(rect.X, rect.Y);
			uv[index + 2] = new Vector2(rect.Right, rect.Y);
			uv[index + 3] = new Vector2(rect.Right, rect.Bottom);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void FillColors(Color value)
		{
			Color[] arr = this.colors;
			int count = arr.Length;
			for (int i = 0; i < count; i++)
				arr[i] = value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void FillColors(Color[] value)
		{
			Color[] arr = this.colors;
			int count = arr.Length;
			int count2 = value.Length;
			for (int i = 0; i < count; i++)
				arr[i] = value[i % count2];
		}

		void AllocTriangleArray(int requestSize)
		{
			if (this.triangles == null
				|| this.triangles.Length != requestSize
				|| this.triangles == TRIANGLES
				|| this.triangles == TRIANGLES_9_GRID
				|| this.triangles == TRIANGLES_4_GRID)
				this.triangles = new int[requestSize];
		}

		/// <summary>
		/// 
		/// </summary>
		public void FillTriangles()
		{
			int vertCount = this.vertices.Length;
			AllocTriangleArray((vertCount >> 1) * 3);

			int k = 0;
			for (int i = 0; i < vertCount; i += 4)
			{
				triangles[k++] = i;
				triangles[k++] = i + 1;
				triangles[k++] = i + 2;

				triangles[k++] = i + 2;
				triangles[k++] = i + 3;
				triangles[k++] = i;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="triangles"></param>
		public void FillTriangles(int[] triangles)
		{
			this.triangles = triangles;
		}

		/// <summary>
		/// 
		/// </summary>
		public void ClearMesh()
		{
			vertCount = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void Tint(Color value)
		{
			if (this.colors == null || vertCount == 0)
				return;

			int count = this.colors.Length;
			for (int i = 0; i < count; i++)
				this.colors[i] = value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="verts"></param>
		/// <param name="index"></param>
		/// <param name="rect"></param>
		public static void FillVertsOfQuad(Vector3[] verts, int index, Rectangle rect)
		{
			verts[index] = new Vector3(rect.X, rect.Bottom, 0f);
			verts[index + 1] = new Vector3(rect.X, rect.Y, 0f);
			verts[index + 2] = new Vector3(rect.Right, rect.Y, 0f);
			verts[index + 3] = new Vector3(rect.Right, rect.Bottom, 0f);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uv"></param>
		/// <param name="index"></param>
		/// <param name="rect"></param>
		public static void FillUVOfQuad(Vector2[] uv, int index, Rectangle rect)
		{
			uv[index] = new Vector2(rect.X, rect.Bottom);
			uv[index + 1] = new Vector2(rect.X, rect.Y);
			uv[index + 2] = new Vector2(rect.Right, rect.Y);
			uv[index + 3] = new Vector2(rect.Right, rect.Bottom);
		}

		public static void RotateUV(Vector2[] uv, ref Rectangle baseUVRect)
		{
			int vertCount = uv.Length;
			float X = MathHelper.Min(baseUVRect.X, baseUVRect.Right);
			float Y = baseUVRect.Y;
			float Bottom = baseUVRect.Bottom;
			if (Y > Bottom)
			{
				Y = Bottom;
				Bottom = baseUVRect.Y;
			}

			float tmp;
			for (int i = 0; i < vertCount; i++)
			{
				Vector2 m = uv[i];
				tmp = m.Y;
				m.Y = Y + m.X - X;
				m.X = X + Bottom - tmp;
				uv[i] = m;
			}
		}
	}
}
