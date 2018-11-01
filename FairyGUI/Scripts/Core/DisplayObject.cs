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
	public class DisplayObject : EventDispatcher
	{
		/// <summary>
		/// 
		/// </summary>
		public string name;

		/// <summary>
		/// 
		/// </summary>
		public Container parent { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public NGraphics graphics { get; protected set; }

		/// <summary>
		/// 
		/// </summary>
		public NGraphics paintingGraphics { get; protected set; }

		/// <summary>
		/// 
		/// </summary>
		public GObject gOwner;

		/// <summary>
		/// 
		/// </summary>
		public uint id;

		/// <summary>
		/// 
		/// </summary>
		public EventListener onClick { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRightClick { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchBegin { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchMove { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onTouchEnd { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRollOver { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRollOut { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onMouseWheel { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onAddedToStage { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onRemovedFromStage { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onKeyDown { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onClickLink { get; private set; }

		protected Vector3 _rotation;
		protected Vector3 _position;
		protected Vector2 _scale;

		bool _visible;
		bool _touchable;
		Vector2 _pivot;
		Vector3 _pivotOffset;
		Vector2 _skew;
		float _alpha;
		bool _grayed;
		IFilter _filter;
		BlendMode _blendMode;

		protected int _paintingMode; //1-滤镜，2-blendMode，4-transformMatrix, 8-cacheAsBitmap
		protected Margin _paintingMargin;
		protected int _paintingFlag;
		protected Effect _paintingMaterial;
		protected bool _cacheAsBitmap;

		protected Rectangle _contentRect;
		protected bool _requireUpdateMesh;
		protected bool _outlineChanged;

		protected Matrix _localToWorldMatrix;
		private uint _matrixVersion;
		private uint _parentMatrixVersion;

		internal bool _disposed;
		internal protected bool _touchDisabled;

		internal static uint _gInstanceCounter;

		public DisplayObject()
		{
			_alpha = 1;
			_visible = true;
			_touchable = true;
			id = _gInstanceCounter++;
			_scale = new Vector2(1, 1);
			_blendMode = BlendMode.Normal;

			_matrixVersion = _parentMatrixVersion = 0;
			_localToWorldMatrix = Matrix.Identity;
			_outlineChanged = true;

			onClick = new EventListener(this, "onClick");
			onRightClick = new EventListener(this, "onRightClick");
			onTouchBegin = new EventListener(this, "onTouchBegin");
			onTouchMove = new EventListener(this, "onTouchMove");
			onTouchEnd = new EventListener(this, "onTouchEnd");
			onRollOver = new EventListener(this, "onRollOver");
			onRollOut = new EventListener(this, "onRollOut");
			onMouseWheel = new EventListener(this, "onMouseWheel");
			onAddedToStage = new EventListener(this, "onAddedToStage");
			onRemovedFromStage = new EventListener(this, "onRemovedFromStage");
			onKeyDown = new EventListener(this, "onKeyDown");
			onClickLink = new EventListener(this, "onClickLink");
		}

		/// <summary>
		/// 
		/// </summary>
		public float alpha
		{
			get { return _alpha; }
			set { _alpha = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool grayed
		{
			get { return _grayed; }
			set { _grayed = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool visible
		{
			get { return _visible; }
			set { _visible = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float x
		{
			get { return _position.X; }
			set
			{
				_position.X = value;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float y
		{
			get { return _position.Y; }
			set
			{
				_position.Y = value;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float z
		{
			get { return _position.Z; }
			set
			{
				_position.Z = value;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector3 position
		{
			get { return _position; }
			set { SetPosition(value.X, value.Y, value.Z); }
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 xy
		{
			get { return new Vector2(_position.X, _position.Y); }
			set { SetPosition(value.X, value.Y); }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		/// <param name="zv"></param>
		public void SetPosition(float xv, float yv, float zv)
		{
			_position.X = xv;
			_position.Y = yv;
			_position.Z = zv;
			_outlineChanged = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		public void SetPosition(float xv, float yv)
		{
			_position.X = xv;
			_position.Y = yv;
			_outlineChanged = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public float width
		{
			get
			{
				EnsureSizeCorrect();
				return _contentRect.Width;
			}
			set
			{
				if (!ToolSet.Approximately(value, _contentRect.Width))
				{
					_contentRect.Width = value;
					OnSizeChanged(true, false);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float height
		{
			get
			{
				EnsureSizeCorrect();
				return _contentRect.Height;
			}
			set
			{
				if (!ToolSet.Approximately(value, _contentRect.Height))
				{
					_contentRect.Height = value;
					OnSizeChanged(false, true);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 size
		{
			get
			{
				EnsureSizeCorrect();
				return new Vector2(_contentRect.Width, _contentRect.Height);
			}
			set
			{
				SetSize(value.X, value.Y);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wv"></param>
		/// <param name="hv"></param>
		public void SetSize(float wv, float hv)
		{
			bool wc = !ToolSet.Approximately(wv, _contentRect.Width);
			bool hc = !ToolSet.Approximately(hv, _contentRect.Height);

			if (wc || hc)
			{
				_contentRect.Width = wv;
				_contentRect.Height = hv;
				OnSizeChanged(wc, hc);
			}
		}

		virtual public void EnsureSizeCorrect()
		{
		}

		virtual protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			ApplyPivot();
			_paintingFlag = 1;
			if (graphics != null)
				_requireUpdateMesh = true;
			_outlineChanged = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public float scaleX
		{
			get { return _scale.X; }
			set
			{
				_scale.X = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float scaleY
		{
			get { return _scale.Y; }
			set
			{
				_scale.Y = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xv"></param>
		/// <param name="yv"></param>
		public void SetScale(float xv, float yv)
		{
			_scale.X = xv;
			_scale.Y = yv;
			_outlineChanged = true;
			ApplyPivot();
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 scale
		{
			get { return _scale; }
			set
			{
				SetScale(value.X, value.Y);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotation
		{
			get
			{
				return _rotation.Z;
			}
			set
			{
				_rotation.Z = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotationX
		{
			get
			{
				return _rotation.X;
			}
			set
			{
				_rotation.X = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float rotationY
		{
			get
			{
				return _rotation.Y;
			}
			set
			{
				_rotation.Y = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 skew
		{
			get { return _skew; }
			set
			{
				_skew = value;
				_outlineChanged = true;
				ApplyPivot();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 pivot
		{
			get { return _pivot; }
			set
			{
				Vector3 deltaPivot = new Vector3((value.X - _pivot.X) * _contentRect.Width, (value.Y - _pivot.Y) * _contentRect.Height, 0);
				Vector3 oldOffset = _pivotOffset;

				_pivot = value;
				UpdatePivotOffset();
				_position += oldOffset - _pivotOffset + deltaPivot;
				_outlineChanged = true;
			}
		}

		void UpdatePivotOffset()
		{
			float px = _pivot.X * _contentRect.Width;
			float py = _pivot.Y * _contentRect.Height;

			Matrix matrix = ToolSet.CreateMatrix(Vector3.Zero, _rotation, new Vector3(_scale.X, _scale.Y, 1), _skew);
			Vector3 offset = new Vector3(px, py, 0);
			Vector3.Transform(ref offset, ref matrix, out _pivotOffset);
		}

		void ApplyPivot()
		{
			if (_pivot.X != 0 || _pivot.Y != 0)
			{
				Vector3 oldOffset = _pivotOffset;

				UpdatePivotOffset();
				_position += oldOffset - _pivotOffset;
				_outlineChanged = true;
			}
		}

		/// <summary>
		/// This is the pivot position
		/// </summary>
		public Vector3 location
		{
			get
			{
				Vector3 pos = _position;
				pos.X += _pivotOffset.X;
				pos.Y += _pivotOffset.Y;
				pos.Z += _pivotOffset.Z;
				return pos;
			}

			set
			{
				this.SetPosition(value.X - _pivotOffset.X, value.Y - _pivotOffset.Y, value.Z - _pivotOffset.Z);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool isDisposed
		{
			get { return _disposed; }
		}

		internal void InternalSetParent(Container value)
		{
			if (parent != value)
			{
				parent = value;
				_parentMatrixVersion = 0;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Container topmost
		{
			get
			{
				DisplayObject currentObject = this;
				while (currentObject.parent != null)
					currentObject = currentObject.parent;
				return currentObject as Container;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Stage stage
		{
			get
			{
				return topmost as Stage;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		virtual public bool touchable
		{
			get { return _touchable; }
			set { _touchable = value; }
		}

		void ValidateMatrix(bool checkParent)
		{
			if (parent != null)
			{
				if (checkParent)
					parent.ValidateMatrix(checkParent);
				if (_parentMatrixVersion != parent._matrixVersion)
				{
					_outlineChanged = true;
					_parentMatrixVersion = parent._matrixVersion;
				}
			}

			if (_outlineChanged)
			{
				_outlineChanged = false;
				_matrixVersion++;

				Matrix mat = ToolSet.CreateMatrix(_position, _rotation, new Vector3(_scale.X, _scale.Y, 1), _skew);
				if (parent != null)
					_localToWorldMatrix = mat * parent._localToWorldMatrix;
				else
					_localToWorldMatrix = mat;
			}
		}

		public Matrix transformMatrix
		{
			get
			{
				ValidateMatrix(true);
				return _localToWorldMatrix;
			}
		}

		/// <summary>
		/// 进入绘画模式，整个对象将画到一张RenderTexture上，然后这种贴图将代替原有的显示内容。
		/// 可以在onPaint回调里对这张纹理进行进一步操作，实现特殊效果。
		/// 可能有多个地方要求进入绘画模式，这里用requestorId加以区别，取值是1、2、4、8、16以此类推。1024内内部保留。用户自定义的id从1024开始。
		/// </summary>
		/// <param name="requestId">请求者id</param>
		/// <param name="margin">纹理四周的留空。如果特殊处理后的内容大于原内容，那么这里的设置可以使纹理扩大。</param>
		public void EnterPaintingMode(int requestorId, Margin? margin)
		{
			bool first = _paintingMode == 0;
			_paintingMode |= requestorId;
			if (first)
			{
				if (paintingGraphics == null)
					paintingGraphics = new NGraphics();
				else
					paintingGraphics.enabled = true;

				_paintingMargin = new Margin();
				_outlineChanged = true;
			}
			if (margin != null)
				_paintingMargin = (Margin)margin;
			_paintingFlag = 1;
		}

		/// <summary>
		/// 离开绘画模式
		/// </summary>
		/// <param name="requestId"></param>
		public void LeavePaintingMode(int requestorId)
		{
			if (_paintingMode == 0 || _disposed)
				return;

			_paintingMode ^= requestorId;
			if (_paintingMode == 0)
			{
				paintingGraphics.ClearMesh();
				paintingGraphics.enabled = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool paintingMode
		{
			get { return _paintingMode > 0; }
		}

		/// <summary>
		/// 将整个显示对象（如果是容器，则容器包含的整个显示列表）静态化，所有内容被缓冲到一张纹理上。
		/// DC将保持为1。CPU消耗将降到最低。但对象的任何变化不会更新。
		/// 当cacheAsBitmap已经为true时，再次调用cacheAsBitmap=true将会刷新一次。
		/// </summary>
		public bool cacheAsBitmap
		{
			get { return _cacheAsBitmap; }
			set
			{
				_cacheAsBitmap = value;
				if (value)
					EnterPaintingMode(8, null);
				else
					LeavePaintingMode(8);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public IFilter filter
		{
			get
			{
				return _filter;
			}

			set
			{
				if (value == _filter)
					return;

				if (_filter != null)
					_filter.Dispose();

				if (value != null && value.target != null)
					value.target.filter = null;

				_filter = value;
				if (_filter != null)
					_filter.target = this;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public BlendMode blendMode
		{
			get { return _blendMode; }
			set
			{
				_blendMode = value;

				if (this is Container)
				{
					if (_blendMode != BlendMode.Normal)
					{
						EnterPaintingMode(2, null);
					}
					else
						LeavePaintingMode(2);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="targetSpace"></param>
		/// <returns></returns>
		virtual public Rectangle GetBounds(DisplayObject targetSpace)
		{
			EnsureSizeCorrect();

			if (targetSpace == this || _contentRect.Width == 0 || _contentRect.Height == 0) // optimization
			{
				return _contentRect;
			}
			else if (targetSpace == parent && _rotation.Z == 0)
			{
				float sx = this.scaleX;
				float sy = this.scaleY;
				return new Rectangle(this.x, this.y, _contentRect.Width * sx, _contentRect.Height * sy);
			}
			else
				return TransformRect(_contentRect, targetSpace);
		}

		protected internal DisplayObject InternalHitTest()
		{
			if (!_visible || (HitTestContext.forTouch && (!_touchable || _touchDisabled)))
				return null;

			return HitTest();
		}

		protected internal DisplayObject InternalHitTestMask()
		{
			if (_visible)
				return HitTest();
			else
				return null;
		}

		virtual protected DisplayObject HitTest()
		{
			Rectangle rect = GetBounds(this);
			if (rect.Width == 0 || rect.Height == 0)
				return null;

			Vector2 localPoint = GlobalToLocal(HitTestContext.screenPoint);
			if (rect.Contains(localPoint.X, localPoint.Y))
				return this;
			else
				return null;
		}

		/// <summary>
		/// 将舞台坐标转换为本地坐标
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vector2 GlobalToLocal(Vector2 point)
		{
			Matrix mat = Matrix.Invert(transformMatrix);
			return Vector2.Transform(point, mat);
		}

		/// <summary>
		/// 将本地坐标转换为舞台坐标
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Vector2 LocalToGlobal(Vector2 point)
		{
			return Vector2.Transform(point, transformMatrix);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="point"></param>
		/// <param name="targetSpace">null if to world space</param>
		/// <returns></returns>
		public Vector2 TransformPoint(Vector2 point, DisplayObject targetSpace)
		{
			if (targetSpace == null)
				targetSpace = Stage.inst;

			if (targetSpace == this)
				return point;

			ValidateMatrix(true);

			Vector2 vec2;
			Vector2.Transform(ref point, ref _localToWorldMatrix, out vec2);
			return Vector2.Transform(vec2, Matrix.Invert(targetSpace.transformMatrix));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="rect"></param>
		/// <param name="targetSpace">null if to world space</param>
		/// <returns></returns>
		public Rectangle TransformRect(Rectangle rect, DisplayObject targetSpace)
		{
			if (targetSpace == null)
				targetSpace = Stage.inst;

			if (targetSpace == this)
				return rect;

			if (targetSpace == parent && _rotation.Z == 0) // optimization
			{
				return new Rectangle((this.x + rect.X) * _scale.X, (this.y + rect.Y) * _scale.Y,
					rect.Width * _scale.X, rect.Height * _scale.Y);
			}
			else
			{
				ValidateMatrix(true);

				Matrix mat = Matrix.Invert(targetSpace.transformMatrix);
				ToolSet.TransformRect(ref rect, ref _localToWorldMatrix, ref mat);

				return rect;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveFromParent()
		{
			if (parent != null)
				parent.RemoveChild(this);
		}

		virtual public void Update()
		{
			ValidateMatrix(false);

			if (_paintingMode != 0)
			{
				NTexture paintingTexture = paintingGraphics.texture;
				if (paintingTexture != null && paintingTexture.disposed) //Texture可能已被Stage.MonitorTexture销毁
				{
					paintingTexture = null;
					_paintingFlag = 1;
				}
				if (_paintingFlag == 1)
				{
					_paintingFlag = 0;

					//从优化考虑，决定使用绘画模式的容器都需要明确指定大小，而不是自动计算包围。这在UI使用上并没有问题，因为组件总是有固定大小的
					int textureWidth = (int)Math.Round((float)_contentRect.Width + _paintingMargin.left + _paintingMargin.right);
					int textureHeight = (int)Math.Round((float)_contentRect.Height + _paintingMargin.top + _paintingMargin.bottom);
					if (paintingTexture == null || paintingTexture.width != textureWidth || paintingTexture.height != textureHeight)
					{
						if (paintingTexture != null)
							paintingTexture.Dispose();
						if (textureWidth > 0 && textureHeight > 0)
						{
							var texture = new Texture2D(Stage.game.GraphicsDevice, textureWidth, textureHeight);
							texture.SetData(new byte[textureWidth * textureHeight * 4]);
							paintingTexture = new NTexture(texture);
							Stage.inst.MonitorTexture(paintingTexture);
						}
						else
							paintingTexture = null;
						paintingGraphics.texture = paintingTexture;
					}

					if (paintingTexture != null)
					{
						paintingGraphics.DrawRect(new Rectangle(-_paintingMargin.left, -_paintingMargin.top, paintingTexture.width, paintingTexture.height),
							new Rectangle(0, 0, 1, 1), Color.White);
						paintingGraphics.UpdateMesh();
					}
					else
						paintingGraphics.ClearMesh();
				}

				if (paintingTexture != null)
					paintingTexture.lastActive = Timers.time;
			}

			Stats.ObjectCount++;
		}

		virtual public void Draw(FairyBatch batch)
		{
			ValidateMatrix(false);

			if (graphics != null)
				batch.Draw(graphics, _alpha, _grayed, _blendMode, _localToWorldMatrix);

			if (_filter != null)
				_filter.Update();
		}

		virtual public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;
			RemoveFromParent();
			RemoveEventListeners();
			if (_filter != null)
				_filter.Dispose();
		}
	}
}
