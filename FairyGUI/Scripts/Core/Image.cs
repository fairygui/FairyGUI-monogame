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
	public class Image : DisplayObject, IMeshFactory
	{
		protected Rectangle? _scale9Grid;
		protected bool _scaleByTile;
		protected int _tileGridIndice;
		protected FillMesh _fillMesh;

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
			graphics.meshFactory = this;

			if (texture != null)
				UpdateTexture(texture);
		}

		/// </summary>
		public NTexture texture
		{
			get { return graphics.texture; }
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
			get
			{
				return graphics.color;
			}
			set
			{
				graphics.color = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public FillMethod fillMethod
		{
			get { return _fillMesh != null ? _fillMesh.method : FillMethod.None; }
			set
			{
				if (_fillMesh == null)
				{
					if (value == FillMethod.None)
						return;

					_fillMesh = new FillMesh();
				}
				if (_fillMesh.method != value)
				{
					_fillMesh.method = value;
					graphics.SetMeshDirty();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int fillOrigin
		{
			get { return _fillMesh != null ? _fillMesh.origin : 0; }
			set
			{
				if (_fillMesh == null)
					_fillMesh = new FillMesh();

				if (_fillMesh.origin != value)
				{
					_fillMesh.origin = value;
					graphics.SetMeshDirty();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool fillClockwise
		{
			get { return _fillMesh != null ? _fillMesh.clockwise : true; }
			set
			{
				if (_fillMesh == null)
					_fillMesh = new FillMesh();

				if (_fillMesh.clockwise != value)
				{
					_fillMesh.clockwise = value;
					graphics.SetMeshDirty();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float fillAmount
		{
			get { return _fillMesh != null ? _fillMesh.amount : 0; }
			set
			{
				if (_fillMesh.amount != value)
				{
					_fillMesh.amount = value;
					graphics.SetMeshDirty();
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
				if (_scale9Grid != value)
				{
					_scale9Grid = value;
					graphics.SetMeshDirty();
				}
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
					graphics.SetMeshDirty();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int tileGridIndice
		{
			get { return _tileGridIndice; }
			set
			{
				if (_tileGridIndice != value)
				{
					_tileGridIndice = value;
					graphics.SetMeshDirty();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void SetNativeSize()
		{
			if (graphics.texture != null)
				SetSize(graphics.texture.width, graphics.texture.height);
			else
				SetSize(0, 0);
		}

		virtual protected void UpdateTexture(NTexture value)
		{
			if (value == graphics.texture)
				return;

			graphics.texture = value;
			if (_contentRect.Width == 0)
				SetNativeSize();
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			if (_fillMesh != null && _fillMesh.method != FillMethod.None)
			{
				_fillMesh.OnPopulateMesh(vb);
			}
			else if (_scaleByTile)
			{
				float sourceW = texture.width;
				float sourceH = texture.height;

				//if (texture.root == texture
				//			&& texture.nativeTexture != null
				//			&& texture.nativeTexture.wrapMode == TextureWrapMode.Repeat)
				//{
				//	Rectangle uvRect = vb.uvRect;
				//	uvRect.Width *= vb.contentRect.Width / sourceW;
				//	uvRect.Height *= vb.contentRect.Height / sourceH;

				//	vb.AddQuad(vb.contentRect, vb.vertexColor, uvRect);
				//	vb.AddTriangles();
				//}
				//else
				{
					TileFill(vb, vb.contentRect, vb.uvRect, sourceW, sourceH);
					vb.AddTriangles();
				}
			}
			else if (_scale9Grid != null)
			{
				SliceFill(vb);
			}
			else
				graphics.OnPopulateMesh(vb);
		}

		static int[] TRIANGLES_9_GRID = new int[] {
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

		static int[] gridTileIndice = new int[] { -1, 0, -1, 2, 4, 3, -1, 1, -1 };
		static float[] gridX = new float[4];
		static float[] gridY = new float[4];
		static float[] gridTexX = new float[4];
		static float[] gridTexY = new float[4];

		public void SliceFill(VertexBuffer vb)
		{
			Rectangle gridRect = (Rectangle)_scale9Grid;
			Rectangle contentRect = vb.contentRect;
			Rectangle uvRect = vb.uvRect;

			float sourceW = texture.width;
			float sourceH = texture.height;

			if (graphics.flip != FlipType.None)
			{
				if (graphics.flip == FlipType.Horizontal || graphics.flip == FlipType.Both)
					gridRect.X = sourceW - gridRect.Right;

				if (graphics.flip == FlipType.Vertical || graphics.flip == FlipType.Both)
					gridRect.Y = sourceH - gridRect.Bottom;
			}

			float sx = uvRect.Width / sourceW;
			float sy = uvRect.Height / sourceH;
			float xMax = uvRect.Right;
			float yMax = uvRect.Bottom;
			float xMax2 = gridRect.Right;
			float yMax2 = gridRect.Bottom;

			gridTexX[0] = uvRect.X;
			gridTexX[1] = uvRect.X + gridRect.X * sx;
			gridTexX[2] = uvRect.X + xMax2 * sx;
			gridTexX[3] = xMax;
			gridTexY[0] = yMax;
			gridTexY[1] = yMax - gridRect.Y * sy;
			gridTexY[2] = yMax - yMax2 * sy;
			gridTexY[3] = uvRect.Y;

			if (contentRect.Width >= (sourceW - gridRect.Width))
			{
				gridX[1] = gridRect.X;
				gridX[2] = contentRect.Width - (sourceW - xMax2);
				gridX[3] = contentRect.Width;
			}
			else
			{
				float tmp = gridRect.X / (sourceW - xMax2);
				tmp = contentRect.Width * tmp / (1 + tmp);
				gridX[1] = tmp;
				gridX[2] = tmp;
				gridX[3] = contentRect.Width;
			}

			if (contentRect.Height >= (sourceH - gridRect.Height))
			{
				gridY[1] = gridRect.Y;
				gridY[2] = contentRect.Height - (sourceH - yMax2);
				gridY[3] = contentRect.Height;
			}
			else
			{
				float tmp = gridRect.Y / (sourceH - yMax2);
				tmp = contentRect.Height * tmp / (1 + tmp);
				gridY[1] = tmp;
				gridY[2] = tmp;
				gridY[3] = contentRect.Height;
			}

			if (_tileGridIndice == 0)
			{
				for (int cy = 0; cy < 4; cy++)
				{
					for (int cx = 0; cx < 4; cx++)
						vb.AddVert(new Vector3(gridX[cx], gridY[cy], 0), vb.vertexColor, new Vector2(gridTexX[cx], gridTexY[cy]));
				}
				vb.AddTriangles(TRIANGLES_9_GRID);
			}
			else
			{
				Rectangle drawRect;
				Rectangle texRect;
				int row, col;
				int part;

				for (int pi = 0; pi < 9; pi++)
				{
					col = pi % 3;
					row = pi / 3;
					part = gridTileIndice[pi];
					drawRect = Rectangle.FromLTRB(gridX[col], gridY[row], gridX[col + 1], gridY[row + 1]);
					texRect = Rectangle.FromLTRB(gridTexX[col], gridTexY[row + 1], gridTexX[col + 1], gridTexY[row]);

					if (part != -1 && (_tileGridIndice & (1 << part)) != 0)
					{
						TileFill(vb, drawRect, texRect,
							(part == 0 || part == 1 || part == 4) ? gridRect.Width : drawRect.Width,
							(part == 2 || part == 3 || part == 4) ? gridRect.Height : drawRect.Height);
					}
					else
					{
						vb.AddQuad(drawRect, vb.vertexColor, texRect);
					}
				}

				vb.AddTriangles();
			}
		}

		void TileFill(VertexBuffer vb, Rectangle contentRect, Rectangle uvRect, float sourceW, float sourceH)
		{
			int hc = (int)Math.Ceiling(contentRect.Width / sourceW);
			int vc = (int)Math.Ceiling(contentRect.Height / sourceH);
			float tailWidth = contentRect.Width - (hc - 1) * sourceW;
			float tailHeight = contentRect.Height - (vc - 1) * sourceH;
			float xMax = uvRect.Right;
			float yMax = uvRect.Bottom;

			for (int i = 0; i < hc; i++)
			{
				for (int j = 0; j < vc; j++)
				{
					Rectangle uvTmp = uvRect;
					if (i == hc - 1)
						uvTmp.Width = MathHelper.Lerp(uvRect.X, xMax, tailWidth / sourceW) - uvTmp.X;
					if (j == vc - 1)
					{
						float tmp = MathHelper.Lerp(uvRect.Y, yMax, 1 - tailHeight / sourceH);
						uvTmp.Height -= (tmp - uvTmp.Y);
						uvTmp.Y = tmp;
					}

					vb.AddQuad(new Rectangle(contentRect.X + i * sourceW, contentRect.Y + j * sourceH,
							i == (hc - 1) ? tailWidth : sourceW, j == (vc - 1) ? tailHeight : sourceH), vb.vertexColor, uvTmp);
				}
			}
		}
	}
}
