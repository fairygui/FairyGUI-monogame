using System;
using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Shape : DisplayObject
	{
		int _type;
		int _lineSize;
		Color _lineColor;
		Color _fillColor;
		Color[] _colors;
		Vector2[] _polygonPoints;
		float[] _cornerRadius;

		/// <summary>
		/// 
		/// </summary>
		public Shape()
		{
			graphics = new NGraphics();
			graphics.texture = NTexture.Empty;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool empty
		{
			get { return _type == 0; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get { return _fillColor; }
			set
			{
				if (_fillColor != value)
				{
					_fillColor = value;
					_requireUpdateMesh = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lineSize"></param>
		/// <param name="lineColor"></param>
		/// <param name="fillColor"></param>
		public void DrawRect(int lineSize, Color lineColor, Color fillColor)
		{
			_type = 1;
			_lineSize = lineSize;
			_lineColor = lineColor;
			_fillColor = fillColor;
			_colors = null;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="lineSize"></param>
		/// <param name="colors"></param>
		public void DrawRect(int lineSize, Color[] colors)
		{
			_type = 1;
			_lineSize = lineSize;
			_colors = colors;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		public void DrawRoundRect(Color fillColor, float[] cornerRadius)
		{
			_type = 4;
			_fillColor = fillColor;
			_cornerRadius = cornerRadius;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="fillColor"></param>
		public void DrawEllipse(Color fillColor)
		{
			_type = 2;
			_fillColor = fillColor;
			_colors = null;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="colors"></param>
		public void DrawEllipse(Color[] colors)
		{
			_type = 2;
			_colors = colors;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="fillColor"></param>
		public void DrawPolygon(Vector2[] points, Color fillColor)
		{
			_type = 3;
			_polygonPoints = points;
			_fillColor = fillColor;
			_colors = null;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="colors"></param>
		public void DrawPolygon(Vector2[] points, Color[] colors)
		{
			_type = 3;
			_polygonPoints = points;
			_colors = colors;

			_touchDisabled = false;
			_requireUpdateMesh = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Clear()
		{
			_type = 0;
			_touchDisabled = true;
			graphics.ClearMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public override void Update()
		{
			if (_requireUpdateMesh)
			{
				_requireUpdateMesh = false;
				if (_type != 0)
				{
					if (_contentRect.Width > 0 && _contentRect.Height > 0)
					{
						switch (_type)
						{
							case 1:
								graphics.DrawRect(_contentRect, new Rectangle(0, 0, 1, 1), _lineSize, _lineColor, _fillColor);
								if (_colors != null)
									graphics.FillColors(_colors);
								break;
							case 2:
								graphics.DrawEllipse(_contentRect, new Rectangle(0, 0, 1, 1), _fillColor);
								if (_colors != null)
									graphics.FillColors(_colors);
								break;
							case 3:
								graphics.DrawPolygon(_contentRect, new Rectangle(0, 0, 1, 1), _polygonPoints, _fillColor);
								if (_colors != null)
									graphics.FillColors(_colors);
								break;
							case 4:
								if (_cornerRadius.Length >= 4)
									graphics.DrawRoundRect(_contentRect, new Rectangle(0, 0, 1, 1), _fillColor, _cornerRadius[0], _cornerRadius[1], _cornerRadius[2], _cornerRadius[3]);
								else
									graphics.DrawRoundRect(_contentRect, new Rectangle(0, 0, 1, 1), _fillColor, _cornerRadius[0], _cornerRadius[0], _cornerRadius[0], _cornerRadius[0]);
								break;
							default:
								break;
						}

						graphics.UpdateMesh();
					}
					else
						graphics.ClearMesh();
				}
			}

			base.Update();
		}

		protected override DisplayObject HitTest()
		{
			if (_type == 2)
			{
				Vector2 localPoint = GlobalToLocal(HitTestContext.screenPoint);
				if (!_contentRect.Contains(localPoint.X, localPoint.Y))
					return null;

				//圆形加多一个在圆内的判断
				float xx = localPoint.X - _contentRect.Width * 0.5f;
				float yy = localPoint.Y - _contentRect.Height * 0.5f;
				if (Math.Pow((xx / (_contentRect.Width * 0.5f)), 2) + Math.Pow((yy / (_contentRect.Height * 0.5f)), 2) < 1)
					return this;
				else
					return null;
			}
			else if (_type == 3)
			{
				Vector2 localPoint = GlobalToLocal(HitTestContext.screenPoint);
				if (!_contentRect.Contains(localPoint.X, localPoint.Y))
					return null;

				// Algorithm & implementation thankfully taken from:
				// -> http://alienryderflex.com/polygon/
				// inspired by Starling
				int len = _polygonPoints.Length;
				int i;
				int j = len - 1;
				bool oddNodes = false;

				for (i = 0; i < len; ++i)
				{
					float ix = _polygonPoints[i].X;
					float iy = _polygonPoints[i].Y;
					float jx = _polygonPoints[j].X;
					float jy = _polygonPoints[j].Y;

					if ((iy < localPoint.Y && jy >= localPoint.Y || jy < localPoint.Y && iy >= localPoint.Y) && (ix <= localPoint.X || jx <= localPoint.X))
					{
						if (ix + (localPoint.Y - iy) / (jy - iy) * (jx - ix) < localPoint.X)
							oddNodes = !oddNodes;
					}

					j = i;
				}

				return oddNodes ? this : null;
			}

			return base.HitTest();
		}
	}
}
