using System.Collections.Generic;
using Microsoft.Xna.Framework;
#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PolygonMesh : IMeshFactory, IHitTest
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly List<Vector2> points;

		/// <summary>
		/// 
		/// </summary>
		public Color? fillColor;

		/// <summary>
		/// 
		/// </summary>
		public Color[] colors;

		/// <summary>
		/// 
		/// </summary>
		public bool usePercentPositions;

		static List<int> sRestIndices = new List<int>();

		public PolygonMesh()
		{
			points = new List<Vector2>();
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			int numVertices = points.Count;
			if (numVertices < 3)
				return;

			int restIndexPos, numRestIndices;

			Color color = fillColor != null ? (Color)fillColor : vb.vertexColor;
			for (int i = 0; i < numVertices; i++)
			{
				Vector3 vec = new Vector3(points[i].X, points[i].Y, 0);
				if (usePercentPositions)
				{
					vec.X *= vb.contentRect.Width;
					vec.Y *= vb.contentRect.Height;
				}
				vb.AddVert(vec, color);
			}

			// Algorithm "Ear clipping method" described here:
			// -> https://en.wikipedia.org/wiki/Polygon_triangulation
			//
			// Implementation inspired by:
			// -> http://polyk.ivank.net
			// -> Starling

			sRestIndices.Clear();
			for (int i = 0; i < numVertices; ++i)
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
					for (int i = 3; i < numRestIndices; ++i)
					{
						otherIndex = sRestIndices[(restIndexPos + i) % numRestIndices];
						p = points[otherIndex];

						if (IsPointInTriangle(ref p, ref a, ref b, ref c))
						{
							earFound = false;
							break;
						}
					}
				}

				if (earFound)
				{
					vb.AddTriangle(i0, i1, i2);
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
			vb.AddTriangle(sRestIndices[0], sRestIndices[1], sRestIndices[2]);

			if (colors != null)
				vb.RepeatColors(colors, 0, vb.currentVertCount);
		}

		bool IsPointInTriangle(ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			// From Starling
			// This algorithm is described well in this article:
			// http://www.blackpawn.com/texts/pointinpoly/default.html

			float v0x = c.X - a.X;
			float v0y = c.Y - a.Y;
			float v1x = b.X - a.X;
			float v1y = b.Y - a.Y;
			float v2x = p.X - a.X;
			float v2y = p.Y - a.Y;

			float dot00 = v0x * v0x + v0y * v0y;
			float dot01 = v0x * v1x + v0y * v1y;
			float dot02 = v0x * v2x + v0y * v2y;
			float dot11 = v1x * v1x + v1y * v1y;
			float dot12 = v1x * v2x + v1y * v2y;

			float invDen = 1.0f / (dot00 * dot11 - dot01 * dot01);
			float u = (dot11 * dot02 - dot01 * dot12) * invDen;
			float v = (dot00 * dot12 - dot01 * dot02) * invDen;

			return (u >= 0) && (v >= 0) && (u + v < 1);
		}

		public bool HitTest(Rectangle contentRect, Vector2 point)
		{
			if (!contentRect.Contains(point.X, point.Y))
				return false;

			// Algorithm & implementation thankfully taken from:
			// -> http://alienryderflex.com/polygon/
			// inspired by Starling
			int len = points.Count;
			int i;
			int j = len - 1;
			bool oddNodes = false;

			for (i = 0; i < len; ++i)
			{
				float ix = points[i].X;
				float iy = points[i].Y;
				float jx = points[j].X;
				float jy = points[j].Y;

				if ((iy < point.Y && jy >= point.Y || jy < point.Y && iy >= point.Y) && (ix <= point.X || jx <= point.X))
				{
					if (ix + (point.Y - iy) / (jy - iy) * (jx - ix) < point.X)
						oddNodes = !oddNodes;
				}

				j = i;
			}

			return oddNodes;
		}
	}
}
