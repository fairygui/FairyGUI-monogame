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
	public class NGraphics : IMeshFactory
	{
		/// <summary>
		/// 不参与剪裁
		/// </summary>
		public bool dontClip;

		public bool enabled;
		public bool pixelSnapping;

		/// <summary>
		/// 
		/// </summary>
		public delegate void MeshModifier();

		/// <summary>
		/// 当Mesh更新时被调用
		/// </summary>
		public MeshModifier meshModifier;

		NTexture _texture;
		IMeshFactory _meshFactory;
		
		Color _color;
		bool _meshDirty;
		Rectangle _contentRect;
		FlipType _flip;

		public List<Vector3> _vertices;
		public List<Vector2> _uv0;
		public List<Color> _colors;
		public List<int> _triangles;

		/// <summary>
		/// 
		/// </summary>
		public NGraphics()
		{
			enabled = true;

			_color = Color.White;
			_meshFactory = this;

			_vertices = new List<Vector3>();
			_uv0 = new List<Vector2>();
			_colors = new List<Color>();
			_triangles = new List<int>();

			Stats.LatestGraphicsCreation++;
		}

		/// <summary>
		/// 
		/// </summary>
		public IMeshFactory meshFactory
		{
			get { return _meshFactory; }
			set
			{
				_meshFactory = value;
				_meshDirty = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T GetMeshFactory<T>() where T : IMeshFactory, new()
		{
			if (!(_meshFactory is T))
			{
				_meshFactory = new T();
				_meshDirty = true;
			}
			return (T)_meshFactory;
		}

		/// <summary>
		/// 
		/// </summary>
		public Rectangle contentRect
		{
			get { return _contentRect; }
			set
			{
				_contentRect = value;
				_meshDirty = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FlipType flip
		{
			get { return _flip; }
			set
			{
				if (_flip != value)
				{
					_flip = value;
					_meshDirty = true;
				}
			}
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
					_meshDirty = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public Color color
		{
			get { return _color; }
			set
			{
				_color = value;
				_meshDirty = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetMeshDirty()
		{
			_meshDirty = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool UpdateMesh()
		{
			if (_meshDirty)
			{
				UpdateMeshNow();
				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
		}

		void UpdateMeshNow()
		{
			_meshDirty = false;

			if (_texture == null || _meshFactory == null)
			{
				if (_vertices.Count > 0)
				{
					_vertices.Clear();
					_uv0.Clear();
					_colors.Clear();
					_triangles.Clear();

					if (meshModifier != null)
						meshModifier();
				}
				return;
			}

			VertexBuffer vb = VertexBuffer.Begin();
			vb.contentRect = _contentRect;
			vb.uvRect = _texture.uvRect;
			if (_flip != FlipType.None)
			{
				if (_flip == FlipType.Horizontal || _flip == FlipType.Both)
				{
					float tmp = vb.uvRect.X;
					vb.uvRect.X = vb.uvRect.X + vb.uvRect.Width;
					vb.uvRect.Width = tmp - vb.uvRect.X;
				}
				if (_flip == FlipType.Vertical || _flip == FlipType.Both)
				{
					float tmp = vb.uvRect.Y;
					vb.uvRect.Y = vb.uvRect.Y + vb.uvRect.Height;
					vb.uvRect.Height = tmp - vb.uvRect.Y;
				}
			}
			vb.vertexColor = _color;
			_meshFactory.OnPopulateMesh(vb);

			int vertCount = vb.currentVertCount;
			if (vertCount == 0)
			{
				if (_vertices.Count > 0)
				{
					_vertices.Clear();
					_uv0.Clear();
					_colors.Clear();
					_triangles.Clear();

					if (meshModifier != null)
						meshModifier();
				}
				vb.End();
				return;
			}

			if (_texture.rotated)
			{
				float xMin = _texture.uvRect.X;
				float yMin = _texture.uvRect.Y;
				float yMax = _texture.uvRect.Bottom;
				float tmp;
				for (int i = 0; i < vertCount; i++)
				{
					Vector2 vec = vb.uv0[i];
					tmp = vec.Y;
					vec.Y = yMin + vec.X - xMin;
					vec.X = xMin + yMax - tmp;
					vb.uv0[i] = vec;
				}
			}

			_vertices.Clear();
			_uv0.Clear();
			_colors.Clear();
			_triangles.Clear();
			_vertices.AddRange(vb.vertices);
			_uv0.AddRange(vb.uv0);
			_colors.AddRange(vb.colors);
			_triangles.AddRange(vb.triangles);

			vb.End();

			if (meshModifier != null)
				meshModifier();
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Rectangle rect = texture.GetDrawRect(vb.contentRect);

			vb.AddQuad(rect, vb.vertexColor, vb.uvRect);
			vb.AddTriangles();
		}
	}
}
