using Microsoft.Xna.Framework.Graphics;

#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

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
		public NTexture(Texture2D texture)
		{
			_root = this;
			_nativeTexture = texture;
			uvRect = new Rectangle(0, 0, 1, 1);
			_region = new Rectangle(0, 0, texture.Width, texture.Height);
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
			_region = new Rectangle(0, 0, texture.Width, texture.Height);
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
			uvRect = new Rectangle(region.X / _nativeTexture.Width, region.Y / _nativeTexture.Height,
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
			uvRect = new Rectangle(region.X * root.uvRect.Width / root.width, region.Y * root.uvRect.Height / root.height,
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
