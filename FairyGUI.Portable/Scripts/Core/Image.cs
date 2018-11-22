using System;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum FlipType
	{
		None,
		Horizontal,
		Vertical,
		Both
	}

	/// <summary>
	/// 
	/// </summary>
	public class Image : DisplayObject
	{
		protected NTexture _texture;
		protected Color _color;
		protected FlipType _flip;
		protected Rectangle? _scale9Grid;
		protected bool _scaleByTile;
		protected int _tileGridIndice;
		protected FillMethod _fillMethod;
		protected int _fillOrigin;
		protected float _fillAmount;
		protected bool _fillClockwise;

		public Image() : this(null)
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public Image(NTexture texture)
		{
			_touchDisabled = true;
			graphics = new NGraphics();

			_color = Color.White;
			if (texture != null)
				UpdateTexture(texture);
		}

		void Create(NTexture texture)
		{

		}

		/// <summary>
		/// 
		/// </summary>
		public NTexture texture
		{
			get { return _texture; }
			set
			{
				UpdateTexture(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _color; }
			set
			{
				_color = value;
				_requireUpdateMesh = true;
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
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FillMethod fillMethod
		{
			get { return _fillMethod; }
			set
			{
				if (_fillMethod != value)
				{
					_fillMethod = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int fillOrigin
		{
			get { return _fillOrigin; }
			set
			{
				if (_fillOrigin != value)
				{
					_fillOrigin = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool fillClockwise
		{
			get { return _fillClockwise; }
			set
			{
				if (_fillClockwise != value)
				{
					_fillClockwise = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float fillAmount
		{
			get { return _fillAmount; }
			set
			{
				if (_fillAmount != value)
				{
					_fillAmount = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Rectangle? scale9Grid
		{
			get { return _scale9Grid; }
			set
			{
				if (_scale9Grid == null && value == null)
					return;

				_scale9Grid = value;
				_requireUpdateMesh = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool scaleByTile
		{
			get { return _scaleByTile; }
			set
			{
				if (_scaleByTile != value)
				{
					_scaleByTile = value;
					_requireUpdateMesh = true;
				}
			}
		}

		public int tileGridIndice
		{
			get { return _tileGridIndice; }
			set
			{
				if (_tileGridIndice != value)
				{
					_tileGridIndice = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetNativeSize()
		{
			float oldWidth = _contentRect.Width;
			float oldHeight = _contentRect.Height;
			if (_texture != null)
			{
				_contentRect.Width = _texture.width;
				_contentRect.Height = _texture.height;
			}
			else
			{
				_contentRect.Width = 0;
				_contentRect.Height = 0;
			}
			if (oldWidth != _contentRect.Width || oldHeight != _contentRect.Height)
				OnSizeChanged(true, true);
		}

		public override void Update()
		{
			if (_requireUpdateMesh)
				Rebuild();

			base.Update();
		}

		virtual protected void UpdateTexture(NTexture value)
		{
			if (value == _texture)
				return;

			_requireUpdateMesh = true;
			_texture = value;
			if (_contentRect.Width == 0)
				SetNativeSize();
			graphics.texture = _texture;
		}

		static int[] gridTileIndice = new int[] { -1, 0, -1, 2, 4, 3, -1, 1, -1 };
		static float[] gridX = new float[4];
		static float[] gridY = new float[4];
		static float[] gridTexX = new float[4];
		static float[] gridTexY = new float[4];

		void GenerateGrids(Rectangle gridRect, Rectangle uvRect)
		{
			float sx = uvRect.Width / (float)_texture.width;
			float sy = uvRect.Height / (float)_texture.height;
			gridTexX[0] = uvRect.X;
			gridTexX[1] = uvRect.X + gridRect.X * sx;
			gridTexX[2] = uvRect.X + (gridRect.X + gridRect.Width) * sx;
			gridTexX[3] = uvRect.X + uvRect.Width;
			gridTexY[0] = uvRect.Y;
			gridTexY[1] = uvRect.Y + gridRect.Y * sy;
			gridTexY[2] = uvRect.Y + (gridRect.Y + gridRect.Height) * sy;
			gridTexY[3] = uvRect.Y + uvRect.Height;

			if (_contentRect.Width >= (_texture.width - gridRect.Width))
			{
				gridX[1] = gridRect.X;
				gridX[2] = _contentRect.Width - (_texture.width - (gridRect.X + gridRect.Width));
				gridX[3] = _contentRect.Width;
			}
			else
			{
				float tmp = gridRect.X / (_texture.width - (gridRect.X + gridRect.Width));
				tmp = _contentRect.Width * tmp / (1 + tmp);
				gridX[1] = tmp;
				gridX[2] = tmp;
				gridX[3] = _contentRect.Width;
			}

			if (_contentRect.Height >= (_texture.height - gridRect.Height))
			{
				gridY[1] = gridRect.Y;
				gridY[2] = _contentRect.Height - (_texture.height - (gridRect.Y + gridRect.Height));
				gridY[3] = _contentRect.Height;
			}
			else
			{
				float tmp = gridRect.Y / (_texture.height - (gridRect.Y + gridRect.Height));
				tmp = _contentRect.Height * tmp / (1 + tmp);
				gridY[1] = tmp;
				gridY[2] = tmp;
				gridY[3] = _contentRect.Height;
			}
		}

		int TileFill(Rectangle destRect, Rectangle uvRect, float sourceW, float sourceH, int vertIndex)
		{
			int hc = (int)Math.Ceiling(destRect.Width / sourceW);
			int vc = (int)Math.Ceiling(destRect.Height / sourceH);
			float tailWidth = destRect.Width - (hc - 1) * sourceW;
			float tailHeight = destRect.Height - (vc - 1) * sourceH;

			if (vertIndex == -1)
			{
				graphics.Alloc(hc * vc * 4);
				vertIndex = 0;
			}

			for (int i = 0; i < hc; i++)
			{
				for (int j = 0; j < vc; j++)
				{
					graphics.FillVerts(vertIndex, new Rectangle(destRect.X + i * sourceW, destRect.Y + j * sourceH,
							i == (hc - 1) ? tailWidth : sourceW, j == (vc - 1) ? tailHeight : sourceH));
					Rectangle uvTmp = uvRect;
					if (i == hc - 1)
						uvTmp.Width = MathHelper.Lerp(uvRect.X, uvRect.Right, tailWidth / sourceW) - uvTmp.X;
					if (j == vc - 1)
						uvTmp.Height = MathHelper.Lerp(uvRect.Y, uvRect.Bottom, tailHeight / sourceH) - uvTmp.Y;

					graphics.FillUV(vertIndex, uvTmp);
					vertIndex += 4;
				}
			}

			return vertIndex;
		}

		virtual protected void Rebuild()
		{
			_requireUpdateMesh = false;
			if (_texture == null)
			{
				graphics.ClearMesh();
				return;
			}

			Rectangle uvRect = _texture.uvRect;
			if (_flip != FlipType.None)
				ToolSet.FlipRect(ref uvRect, _flip);

			if (_fillMethod != FillMethod.None)
			{
				graphics.DrawRectWithFillMethod(_contentRect, uvRect, _color, _fillMethod, _fillAmount, _fillOrigin, _fillClockwise);
			}
			else if (_texture.width == _contentRect.Width && _texture.height == _contentRect.Height)
			{
				graphics.DrawRect(_contentRect, uvRect, _color);
			}
			else if (_scaleByTile)
			{
				//如果纹理是repeat模式，而且单独占满一张纹理，那么可以用repeat的模式优化显示
				/*if (_texture.nativeTexture != null && _texture.nativeTexture.wrapMode == TextureWrapMode.Repeat
					&& uvRect.X == 0 && uvRect.Y == 0 && uvRect.Width == 1 && uvRect.Height == 1)
				{
					uvRect.Width *= _contentRect.Width / _texture.width;
					uvRect.Height *= _contentRect.Height / _texture.height;
					graphics.DrawRect(_contentRect, uvRect, _color);
				}
				else*/
				{
					TileFill(_contentRect, uvRect, _texture.width, _texture.height, -1);
					graphics.FillColors(_color);
					graphics.FillTriangles();
				}
			}
			else if (_scale9Grid != null)
			{
				Rectangle gridRect = (Rectangle)_scale9Grid;

				if (_flip != FlipType.None)
					ToolSet.FlipInnerRect(_texture.width, _texture.height, ref gridRect, _flip);

				GenerateGrids(gridRect, uvRect);

				if (_tileGridIndice == 0)
				{
					graphics.Alloc(16);

					int k = 0;
					for (int cy = 0; cy < 4; cy++)
					{
						for (int cx = 0; cx < 4; cx++)
						{
							graphics.uv[k] = new Vector2(gridTexX[cx], gridTexY[cy]);
							graphics.vertices[k] = new Vector3(gridX[cx], gridY[cy], 0);
							k++;
						}
					}
					graphics.FillTriangles(NGraphics.TRIANGLES_9_GRID);
				}
				else
				{
					int hc, vc;
					Rectangle drawRect;
					Rectangle texRect;
					int row, col;
					int part;

					//先计算需要的顶点数量
					int vertCount = 0;
					for (int pi = 0; pi < 9; pi++)
					{
						col = pi % 3;
						row = pi / 3;
						part = gridTileIndice[pi];

						if (part != -1 && (_tileGridIndice & (1 << part)) != 0)
						{
							if (part == 0 || part == 1 || part == 4)
								hc = (int)Math.Ceiling((gridX[col + 1] - gridX[col]) / gridRect.Width);
							else
								hc = 1;
							if (part == 2 || part == 3 || part == 4)
								vc = (int)Math.Ceiling((gridY[row + 1] - gridY[row]) / gridRect.Height);
							else
								vc = 1;
							vertCount += hc * vc * 4;
						}
						else
							vertCount += 4;
					}

					graphics.Alloc(vertCount);

					int k = 0;

					for (int pi = 0; pi < 9; pi++)
					{
						col = pi % 3;
						row = pi / 3;
						part = gridTileIndice[pi];
						drawRect = new Rectangle(gridX[col], gridY[row], gridX[col + 1] - gridX[col], gridY[row + 1] - gridY[row]);
						texRect = new Rectangle(gridTexX[col], gridTexY[row], gridTexX[col + 1] - gridTexX[col], gridTexY[row + 1] - gridTexY[row]);

						if (part != -1 && (_tileGridIndice & (1 << part)) != 0)
						{
							k = TileFill(drawRect, texRect,
								(part == 0 || part == 1 || part == 4) ? gridRect.Width : drawRect.Width,
								(part == 2 || part == 3 || part == 4) ? gridRect.Height : drawRect.Height,
								k);
						}
						else
						{
							graphics.FillVerts(k, drawRect);
							graphics.FillUV(k, texRect);
							k += 4;
						}
					}

					graphics.FillTriangles();
				}

				graphics.FillColors(_color);
			}
			else
			{
				graphics.DrawRect(_contentRect, uvRect, _color);
			}

			if (_texture.rotated)
				NGraphics.RotateUV(graphics.uv, ref uvRect);
			graphics.UpdateMesh();
		}
	}
}
