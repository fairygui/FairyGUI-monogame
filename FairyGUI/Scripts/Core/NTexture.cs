using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class NTexture
	{
		/// <summary>
		/// 
		/// </summary>
		public Rectangle uvRect;

		/// <summary>
		/// 
		/// </summary>
		public bool rotated;

		/// <summary>
		/// 
		/// </summary>
		public int refCount;

		/// <summary>
		/// 
		/// </summary>
		public float lastActive;

		Texture2D _nativeTexture;
		Texture2D _alphaTexture;
		Rectangle _region;
		Vector2 _offset;
		Vector2 _originalSize;
		NTexture _root;

		static Texture2D CreateEmptyTexture(GraphicsDevice graphics)
		{
			var texture = new Texture2D(graphics, 1, 1);
			texture.SetData(new byte[]
			{
				0xFF, 0xFF, 0xFF, 0xFF,
			});

			return texture;
		}

		static NTexture _empty;

		/// <summary>
		/// 
		/// </summary>
		public static NTexture Empty
		{
			get
			{
				if (_empty == null)
					_empty = new NTexture(CreateEmptyTexture(Stage.game.GraphicsDevice));

				return _empty;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public static void DisposeEmpty()
		{
			if (_empty != null)
			{
				NTexture tmp = _empty;
				_empty = null;
				tmp.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="texture"></param>
		public NTexture(Texture2D texture) : this(texture, null, 1, 1)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// /// <param name="alphaTexture"></param>
		/// <param name="xScale"></param>
		/// <param name="yScale"></param>
		public NTexture(Texture2D texture, Texture2D alphaTexture, int xScale, int yScale)
		{
			_root = this;
			_nativeTexture = texture;
			_alphaTexture = alphaTexture;
			uvRect = new Rectangle(0, 0, xScale, yScale);
			_originalSize = new Vector2(texture.Width, texture.Height);
			_region = new Rectangle(0, 0, _originalSize.X, _originalSize.Y);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		/// <param name="region"></param>
		public NTexture(Texture2D texture, Rectangle region)
		{
			_root = this;
			_nativeTexture = texture;
			_region = region;
			_originalSize = new Vector2(_region.Width, _region.Height);
			uvRect = new Rectangle(region.X / _nativeTexture.Width, 1 - region.Bottom / _nativeTexture.Height,
				region.Width / _nativeTexture.Width, region.Height / _nativeTexture.Height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		public NTexture(NTexture root, Rectangle region, bool rotated)
		{
			_root = root;
			this.rotated = rotated;
			region.X += root._region.X;
			region.Y += root._region.Y;
			uvRect = new Rectangle(region.X * root.uvRect.Width / root.width, 1 - region.Bottom * root.uvRect.Height / root.height,
				region.Width * root.uvRect.Width / root.width, region.Height * root.uvRect.Height / root.height);
			if (rotated)
			{
				float tmp = region.Width;
				region.Width = region.Height;
				region.Height = tmp;

				tmp = uvRect.Width;
				uvRect.Width = uvRect.Height;
				uvRect.Height = tmp;
			}

			_region = region;
			_originalSize = new Vector2(_region.Width, _region.Height);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="root"></param>
		/// <param name="region"></param>
		/// <param name="rotated"></param>
		/// <param name="originalSize"></param>
		/// <param name="offset"></param>
		public NTexture(NTexture root, Rectangle region, bool rotated, Vector2 originalSize, Vector2 offset)
			: this(root, region, rotated)
		{
			_originalSize = originalSize;
			_offset = offset;
		}

		/// <summary>
		/// 
		/// </summary>
		public int width
		{
			get { return (int)_region.Width; }
		}

		/// <summary>
		/// 
		/// </summary>
		public int height
		{
			get { return (int)_region.Height; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 offset
		{
			get { return _offset; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 originalSize
		{
			get { return _originalSize; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="drawRect"></param>
		/// <returns></returns>
		public Rectangle GetDrawRect(Rectangle drawRect)
		{
			if (_originalSize.X == _region.Width && _originalSize.Y == _region.Height)
				return drawRect;

			float sx = drawRect.Width / _originalSize.X;
			float sy = drawRect.Height / _originalSize.Y;
			return new Rectangle(_offset.X * sx, _offset.Y * sy, _region.Width * sx, _region.Height * sy);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uv"></param>
		public void GetUV(Vector2[] uv)
		{
			uv[0] = new Vector2(uvRect.X, uvRect.Y);
			uv[1] = new Vector2(uvRect.X, uvRect.Bottom);
			uv[2] = new Vector2(uvRect.Right, uvRect.Bottom);
			uv[3] = new Vector2(uvRect.Right, uvRect.Y);
			if (rotated)
			{
				float xMin = uvRect.X;
				float yMin = uvRect.Y;
				float yMax = uvRect.Bottom;

				float tmp;
				for (int i = 0; i < 4; i++)
				{
					Vector2 m = uv[i];
					tmp = m.Y;
					m.Y = yMin + m.X - xMin;
					m.X = xMin + yMax - tmp;
					uv[i] = m;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture root
		{
			get { return _root; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool disposed
		{
			get { return _root == null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture2D nativeTexture
		{
			get { return _root != null ? _root._nativeTexture : null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Texture2D alphaTexture
		{
			get { return _root != null ? _root._alphaTexture : null; }
		}
		/// <summary>
		/// 
		/// </summary>
		public void Unload()
		{
			if (this == _empty)
				return;

			if (_root != this)
				throw new System.Exception("Unload is not allow to call on none root NTexture.");

			if (_nativeTexture != null)
			{
				_nativeTexture.Dispose();
				_nativeTexture = null;
			}

			if (_alphaTexture != null)
			{
				_alphaTexture.Dispose();
				_alphaTexture = null;
			}
		}

		public void Dispose()
		{
			if (this == _empty)
				return;

			if (_root == this)
				Unload();
			_root = null;
		}
	}
}
