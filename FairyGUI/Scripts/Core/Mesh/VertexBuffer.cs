using System;
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
	public sealed class VertexBuffer
	{
		/// <summary>
		/// 
		/// </summary>
		public Rectangle contentRect;

		/// <summary>
		/// 
		/// </summary>
		public Rectangle uvRect;

		/// <summary>
		/// 
		/// </summary>
		public Color vertexColor;

		/// <summary>
		/// 
		/// </summary>
		public readonly List<Vector3> vertices;

		/// <summary>
		/// 
		/// </summary>
		public readonly List<Color> colors;

		/// <summary>
		/// 
		/// </summary>
		public readonly List<Vector2> uv0;

		/// <summary>
		/// 
		/// </summary>
		public readonly List<int> triangles;

		internal bool _alphaInVertexColor;

		static Stack<VertexBuffer> _pool = new Stack<VertexBuffer>();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static VertexBuffer Begin()
		{
			if (_pool.Count > 0)
			{
				VertexBuffer inst = _pool.Pop();
				inst.Clear();
				return inst;
			}
			else
				return new VertexBuffer();
		}

		private VertexBuffer()
		{
			vertices = new List<Vector3>();
			colors = new List<Color>();
			uv0 = new List<Vector2>();
			triangles = new List<int>();
		}

		/// <summary>
		/// 
		/// </summary>
		public void End()
		{
			_pool.Push(this);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			vertices.Clear();
			colors.Clear();
			uv0.Clear();
			triangles.Clear();
			_alphaInVertexColor = false;
		}

		/// <summary>
		/// 
		/// </summary>
		public int currentVertCount
		{
			get
			{
				return vertices.Count;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		public void AddVert(Vector3 position)
		{
			vertices.Add(position);
			colors.Add(vertexColor);
			uv0.Add(new Vector2(MathHelper.Lerp(uvRect.X, uvRect.Right, (position.X - contentRect.X) / contentRect.Width),
					MathHelper.Lerp(uvRect.Bottom, uvRect.Y, (position.Y - contentRect.Y) / contentRect.Height)));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="color"></param>
		public void AddVert(Vector3 position, Color color)
		{
			vertices.Add(position);
			colors.Add(color);
			if (color.A != 255)
				_alphaInVertexColor = true;
			uv0.Add(new Vector2(
					MathHelper.Lerp(uvRect.X, uvRect.Right, (position.X - contentRect.X) / contentRect.Width),
					MathHelper.Lerp(uvRect.Bottom, uvRect.Y, (position.Y - contentRect.Y) / contentRect.Height))
				);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="color"></param>
		/// <param name="uv"></param>
		public void AddVert(Vector3 position, Color color, Vector2 uv)
		{
			vertices.Add(position);
			uv0.Add(uv);
			colors.Add(color);
			if (color.A != 255)
				_alphaInVertexColor = true;
		}

		/// <summary>
		/// 
		/// 1---2
		/// | / |
		/// 0---3
		/// </summary>
		/// <param name="vertRect"></param>
		public void AddQuad(Rectangle vertRect)
		{
			AddVert(new Vector3(vertRect.X, vertRect.Bottom, 0f));
			AddVert(new Vector3(vertRect.X, vertRect.Y, 0f));
			AddVert(new Vector3(vertRect.Right, vertRect.Y, 0f));
			AddVert(new Vector3(vertRect.Right, vertRect.Bottom, 0f));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="color"></param>
		public void AddQuad(Rectangle vertRect, Color color)
		{
			AddVert(new Vector3(vertRect.X, vertRect.Bottom, 0f), color);
			AddVert(new Vector3(vertRect.X, vertRect.Y, 0f), color);
			AddVert(new Vector3(vertRect.Right, vertRect.Y, 0f), color);
			AddVert(new Vector3(vertRect.Right, vertRect.Bottom, 0f), color);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vertRect"></param>
		/// <param name="color"></param>
		/// <param name="uvRect"></param>
		public void AddQuad(Rectangle vertRect, Color color, Rectangle uvRect)
		{
			vertices.Add(new Vector3(vertRect.X, vertRect.Bottom, 0f));
			vertices.Add(new Vector3(vertRect.X, vertRect.Y, 0f));
			vertices.Add(new Vector3(vertRect.Right, vertRect.Y, 0f));
			vertices.Add(new Vector3(vertRect.Right, vertRect.Bottom, 0f));

			uv0.Add(new Vector2(uvRect.X, uvRect.Y));
			uv0.Add(new Vector2(uvRect.X, uvRect.Bottom));
			uv0.Add(new Vector2(uvRect.Right, uvRect.Bottom));
			uv0.Add(new Vector2(uvRect.Right, uvRect.Y));

			colors.Add(color);
			colors.Add(color);
			colors.Add(color);
			colors.Add(color);

			if (color.A != 255)
				_alphaInVertexColor = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="startIndex"></param>
		/// <param name="count"></param>
		public void RepeatColors(Color[] value, int startIndex, int count)
		{
			int len = Math.Min(startIndex + count, vertices.Count);
			int colorCount = value.Length;
			int k = 0;
			for (int i = startIndex; i < len; i++)
			{
				Color c = value[(k++) % colorCount];
				colors[i] = c;
				if (c.A != 255)
					_alphaInVertexColor = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="idx0"></param>
		/// <param name="idx1"></param>
		/// <param name="idx2"></param>
		public void AddTriangle(int idx0, int idx1, int idx2)
		{
			triangles.Add(idx0);
			triangles.Add(idx1);
			triangles.Add(idx2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="idxList"></param>
		/// <param name="startVertexIndex"></param>
		public void AddTriangles(int[] idxList, int startVertexIndex = 0)
		{
			if (startVertexIndex != 0)
			{
				if (startVertexIndex < 0)
					startVertexIndex = vertices.Count + startVertexIndex;

				int cnt = idxList.Length;
				for (int i = 0; i < cnt; i++)
					triangles.Add(idxList[i] + startVertexIndex);
			}
			else
				triangles.AddRange(idxList);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="startVertexIndex"></param>
		public void AddTriangles(int startVertexIndex = 0)
		{
			int cnt = vertices.Count;
			if (startVertexIndex < 0)
				startVertexIndex = cnt + startVertexIndex;

			for (int i = startVertexIndex; i < cnt; i += 4)
			{
				triangles.Add(i);
				triangles.Add(i + 1);
				triangles.Add(i + 2);

				triangles.Add(i + 2);
				triangles.Add(i + 3);
				triangles.Add(i);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Vector3 GetPosition(int index)
		{
			if (index < 0)
				index = vertices.Count + index;

			return vertices[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vb"></param>
		public void Append(VertexBuffer vb)
		{
			int len = vertices.Count;
			vertices.AddRange(vb.vertices);
			uv0.AddRange(vb.uv0);
			colors.AddRange(vb.colors);
			if (len != 0)
			{
				int len1 = vb.triangles.Count;
				for (int i = 0; i < len1; i++)
					triangles.Add(vb.triangles[i] + len);
			}
			else
				triangles.AddRange(vb.triangles);

			if (vb._alphaInVertexColor)
				_alphaInVertexColor = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="vb"></param>
		public void Insert(VertexBuffer vb)
		{
			vertices.InsertRange(0, vb.vertices);
			uv0.InsertRange(0, vb.uv0);
			colors.InsertRange(0, vb.colors);
			int len = triangles.Count;
			if (len != 0)
			{
				int len1 = vb.vertices.Count;
				for (int i = 0; i < len; i++)
					triangles[i] += len1;
			}
			triangles.InsertRange(0, vb.triangles);

			if (vb._alphaInVertexColor)
				_alphaInVertexColor = true;
		}
	}
}
