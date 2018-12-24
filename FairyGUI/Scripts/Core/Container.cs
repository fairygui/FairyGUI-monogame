using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;
#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Container : DisplayObject
	{
		/// <summary>
		/// 
		/// </summary>
		public bool opaque;

		/// <summary>
		/// 
		/// </summary>
		public IHitTest hitArea;

		/// <summary>
		/// 
		/// </summary>
		public bool touchChildren;

		List<DisplayObject> _children;
		DisplayObject _mask;
		Rectangle? _clipRect;

		/// <summary>
		/// 
		/// </summary>
		public Container()
			: base()
		{
			_children = new List<DisplayObject>();
			touchChildren = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public int numChildren
		{
			get { return _children.Count; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public DisplayObject AddChild(DisplayObject child)
		{
			AddChildAt(child, _children.Count);
			return child;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public DisplayObject AddChildAt(DisplayObject child, int index)
		{
			int count = _children.Count;
			if (index >= 0 && index <= count)
			{
				if (child.parent == this)
				{
					SetChildIndex(child, index);
				}
				else
				{
					child.RemoveFromParent();
					if (index == count)
						_children.Add(child);
					else
						_children.Insert(index, child);
					child.InternalSetParent(this);

					if (stage != null)
					{
						if (child is Container)
							child.onAddedToStage.BroadcastCall();
						else
							child.onAddedToStage.Call();
					}
				}
				return child;
			}
			else
			{
				throw new Exception("Invalid child index");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public bool Contains(DisplayObject child)
		{
			return _children.Contains(child);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public DisplayObject GetChildAt(int index)
		{
			return _children[index];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public DisplayObject GetChild(string name)
		{
			int cnt = _children.Count;
			for (int i = 0; i < cnt; ++i)
			{
				if (_children[i].name == name)
					return _children[i];
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public int GetChildIndex(DisplayObject child)
		{
			return _children.IndexOf(child);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		public DisplayObject RemoveChild(DisplayObject child)
		{
			return RemoveChild(child, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <param name="dispose"></param>
		/// <returns></returns>
		public DisplayObject RemoveChild(DisplayObject child, bool dispose)
		{
			if (child.parent != this)
				throw new Exception("obj is not a child");

			int i = _children.IndexOf(child);
			if (i >= 0)
				return RemoveChildAt(i, dispose);
			else
				return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public DisplayObject RemoveChildAt(int index)
		{
			return RemoveChildAt(index, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="dispose"></param>
		/// <returns></returns>
		public DisplayObject RemoveChildAt(int index, bool dispose)
		{
			if (index >= 0 && index < _children.Count)
			{
				DisplayObject child = _children[index];

				if (stage != null && !child._disposed)
				{
					if (child is Container)
						child.onRemovedFromStage.BroadcastCall();
					else
						child.onRemovedFromStage.Call();
				}
				_children.Remove(child);
				if (!dispose)
					child.InternalSetParent(null);
				else
					child.Dispose();

				return child;
			}
			else
				throw new Exception("Invalid child index");
		}

		/// <summary>
		/// 
		/// </summary>
		public void RemoveChildren()
		{
			RemoveChildren(0, int.MaxValue, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="beginIndex"></param>
		/// <param name="endIndex"></param>
		/// <param name="dispose"></param>
		public void RemoveChildren(int beginIndex, int endIndex, bool dispose)
		{
			if (endIndex < 0 || endIndex >= numChildren)
				endIndex = numChildren - 1;

			for (int i = beginIndex; i <= endIndex; ++i)
				RemoveChildAt(beginIndex, dispose);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child"></param>
		/// <param name="index"></param>
		public void SetChildIndex(DisplayObject child, int index)
		{
			int oldIndex = _children.IndexOf(child);
			if (oldIndex == index) return;
			if (oldIndex == -1) throw new ArgumentException("Not a child of this container");
			_children.RemoveAt(oldIndex);
			if (index >= _children.Count)
				_children.Add(child);
			else
				_children.Insert(index, child);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="child1"></param>
		/// <param name="child2"></param>
		public void SwapChildren(DisplayObject child1, DisplayObject child2)
		{
			int index1 = _children.IndexOf(child1);
			int index2 = _children.IndexOf(child2);
			if (index1 == -1 || index2 == -1)
				throw new Exception("Not a child of this container");
			SwapChildrenAt(index1, index2);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="index1"></param>
		/// <param name="index2"></param>
		public void SwapChildrenAt(int index1, int index2)
		{
			DisplayObject obj1 = _children[index1];
			DisplayObject obj2 = _children[index2];
			_children[index1] = obj2;
			_children[index2] = obj1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indice"></param>
		/// <param name="objs"></param>
		public void ChangeChildrenOrder(List<int> indice, List<DisplayObject> objs)
		{
			int cnt = indice.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject obj = objs[i];
				if (obj.parent != this)
					throw new Exception("Not a child of this container");

				_children[indice[i]] = obj;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Rectangle? clipRect
		{
			get { return _clipRect; }
			set { _clipRect = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject mask
		{
			get { return _mask; }
			set
			{
				_mask = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		override public bool touchable
		{
			get { return base.touchable; }
			set
			{
				base.touchable = value;
				if (hitArea != null)
					hitArea.SetEnabled(value);
			}
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
				OnSizeChanged(true, true);
			}
		}

		public override Rectangle GetBounds(DisplayObject targetSpace)
		{
			if (_clipRect != null)
				return TransformRect((Rectangle)_clipRect, targetSpace);

			int count = _children.Count;

			Rectangle rect;
			if (count == 0)
			{
				Vector2 v = TransformPoint(Vector2.Zero, targetSpace);
				rect = new Rectangle(v.X, v.Y, 0, 0);
			}
			else if (count == 1)
			{
				rect = _children[0].GetBounds(targetSpace);
			}
			else
			{
				float minX = float.MaxValue, maxX = float.MinValue;
				float minY = float.MaxValue, maxY = float.MinValue;

				for (int i = 0; i < count; ++i)
				{
					rect = _children[i].GetBounds(targetSpace);
					minX = minX < rect.X ? minX : rect.X;
					maxX = maxX > (rect.X + rect.Width) ? maxX : (rect.X + rect.Width);
					minY = minY < rect.Y ? minY : rect.Y;
					maxY = maxY > (rect.Y + rect.Height) ? maxY : (rect.Y + rect.Height);
				}

				rect = new Rectangle(minX, minY, maxX - minX, maxY - minY);
			}

			return rect;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stagePoint"></param>
		/// <returns></returns>
		public DisplayObject HitTest(Vector2 stagePoint, bool forTouch)
		{
			HitTestContext.screenPoint = stagePoint;
			HitTestContext.forTouch = forTouch;
			HitTestContext.raycastDone = false;

			DisplayObject ret = HitTest();
			if (ret != null)
				return ret;
			else if (this is Stage)
				return this;
			else
				return null;
		}

		override protected DisplayObject HitTest()
		{
			if (_scale.X == 0 || _scale.Y == 0)
				return null;

			Vector2 localPoint = new Vector2();
			Vector2 savedScreenPoint = HitTestContext.screenPoint;

			if (hitArea != null)
			{
				if (!hitArea.HitTest(this, ref localPoint))
					return null;
			}
			else
			{
				localPoint = GlobalToLocal(HitTestContext.screenPoint);
				if (_clipRect != null && !((Rectangle)_clipRect).Contains(localPoint.X, localPoint.Y))
					return null;
			}

			DisplayObject target = null;
			if (touchChildren)
			{
				int count = _children.Count;
				for (int i = count - 1; i >= 0; --i) // front to back!
				{
					DisplayObject child = _children[i];
					if (child == _mask)
						continue;

					target = child.InternalHitTest();
					if (target != null)
						break;
				}
			}

			if (target == null && opaque && (hitArea != null || _contentRect.Contains(localPoint.X, localPoint.Y)))
				target = this;

			HitTestContext.screenPoint = savedScreenPoint;

			return target;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsAncestorOf(DisplayObject obj)
		{
			if (obj == null)
				return false;

			Container p = obj.parent;
			while (p != null)
			{
				if (p == this)
					return true;

				p = p.parent;
			}
			return false;
		}

		public override void Update()
		{
			base.Update();

			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject child = _children[i];
				if (child.visible)
					child.Update();
			}

			if (gOwner != null && (gOwner is GComponent))
				((GComponent)gOwner).OnUpdate();
		}

		override public void Draw(FairyBatch batch)
		{
			base.Draw(batch);

			if (_paintingMode != 0 && paintingGraphics.texture != null)
			{
				batch.PushRenderTarget(paintingGraphics.texture, 
					Vector2.Transform(Vector2.Zero, _localToWorldMatrix) + new Vector2(_paintingMargin.left, _paintingMargin.top));
			}

			if (_clipRect != null)
			{
				//在这里可以直接使用 _localToWorldMatrix， 因为已经更新
				Rectangle clipRect = (Rectangle)_clipRect;
				Matrix mat = Matrix.Identity;
				ToolSet.TransformRect(ref clipRect, ref _localToWorldMatrix, ref mat);
				batch.EnterClipping(clipRect);
			}

			float savedAlpha = batch.alpha;
			batch.alpha *= this.alpha;
			bool savedGrayed = batch.grayed;
			batch.grayed = batch.grayed || this.grayed;

			int cnt = _children.Count;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject child = _children[i];
				if (child.visible)
					child.Draw(batch);
			}

			if (_clipRect != null)
				batch.LeaveClipping();

			if (_paintingMode != 0 && paintingGraphics.texture != null)
			{
				batch.PopRenderTarget();
				batch.Draw(paintingGraphics, 1, false, _blendMode, ref _localToWorldMatrix, _filter);
			}

			batch.alpha = savedAlpha;
			batch.grayed = savedGrayed;
		}

		public override void Dispose()
		{
			base.Dispose(); //Destroy GameObject tree first, avoid destroying each seperately;

			int numChildren = _children.Count;
			for (int i = numChildren - 1; i >= 0; --i)
			{
				DisplayObject obj = _children[i];
				obj.InternalSetParent(null); //Avoid RemoveParent call
				obj.Dispose();
			}
		}
	}
}
