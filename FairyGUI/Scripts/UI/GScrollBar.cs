using System;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;

namespace FairyGUI
{
	/// <summary>
	/// GScrollBar class.
	/// </summary>
	public class GScrollBar : GComponent
	{
		GObject _grip;
		GObject _arrowButton1;
		GObject _arrowButton2;
		GObject _bar;
		ScrollPane _target;

		bool _vertical;
		float _scrollPerc;
		bool _fixedGripSize;

		Vector2 _dragOffset;

		public GScrollBar()
		{
			_scrollPerc = 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="vertical"></param>
		public void SetScrollPane(ScrollPane target, bool vertical)
		{
			_target = target;
			_vertical = vertical;
		}

		/// <summary>
		/// 
		/// </summary>
		public float displayPerc
		{
			set
			{
				if (_vertical)
				{
					if (!_fixedGripSize)
						_grip.height = (int)Math.Floor(value * _bar.height);
					_grip.y = (int)Math.Round(_bar.y + (_bar.height - _grip.height) * _scrollPerc);
				}
				else
				{
					if (!_fixedGripSize)
						_grip.width = (int)Math.Floor(value * _bar.width);
					_grip.x = (int)Math.Round(_bar.x + (_bar.width - _grip.width) * _scrollPerc);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float scrollPerc
		{
			set
			{
				_scrollPerc = value;
				if (_vertical)
					_grip.y = (int)Math.Round(_bar.y + (_bar.height - _grip.height) * _scrollPerc);
				else
					_grip.x = (int)Math.Round(_bar.x + (_bar.width - _grip.width) * _scrollPerc);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float minSize
		{
			get
			{
				if (_vertical)
					return (_arrowButton1 != null ? _arrowButton1.height : 0) + (_arrowButton2 != null ? _arrowButton2.height : 0);
				else
					return (_arrowButton1 != null ? _arrowButton1.width : 0) + (_arrowButton2 != null ? _arrowButton2.width : 0);
			}
		}

		override protected void ConstructExtension(ByteBuffer buffer)
		{
			buffer.Seek(0, 6);

			_fixedGripSize = buffer.ReadBool();

			_grip = GetChild("grip");
			if (_grip == null)
			{
				Log.Warning("FairyGUI: " + this.resourceURL + " should define grip");
				return;
			}

			_bar = GetChild("bar");
			if (_bar == null)
			{
				Log.Warning("FairyGUI: " + this.resourceURL + " should define bar");
				return;
			}

			_arrowButton1 = GetChild("arrow1");
			_arrowButton2 = GetChild("arrow2");

			_grip.onTouchBegin.Add(__gripTouchBegin);
			_grip.onTouchMove.Add(__gripTouchMove);

			this.onTouchBegin.Add(__touchBegin);
			if (_arrowButton1 != null)
				_arrowButton1.onTouchBegin.Add(__arrowButton1Click);
			if (_arrowButton2 != null)
				_arrowButton2.onTouchBegin.Add(__arrowButton2Click);
		}

		void __gripTouchBegin(EventContext context)
		{
			if (_bar == null)
				return;

			context.StopPropagation();

			InputEvent evt = context.inputEvent;
			if (evt.button != 0)
				return;

			context.CaptureTouch();

			_dragOffset = this.GlobalToLocal(new Vector2(evt.x, evt.y)) - _grip.xy;
		}

		void __gripTouchMove(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			Vector2 pt = this.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (float.IsNaN(pt.X))
				return;

			if (_vertical)
			{
				float curY = pt.Y - _dragOffset.Y;
				float diff = _bar.height - _grip.height;
				if (diff == 0)
					_target.percY = 0;
				else
					_target.percY = (curY - _bar.y) / diff;
			}
			else
			{
				float curX = pt.X - _dragOffset.X;
				float diff = _bar.width - _grip.width;
				if (diff == 0)
					_target.percX = 0;
				else
					_target.percX = (curX - _bar.x) / diff;
			}
		}

		void __arrowButton1Click(EventContext context)
		{
			context.StopPropagation();

			if (_vertical)
				_target.ScrollUp();
			else
				_target.ScrollLeft();
		}

		void __arrowButton2Click(EventContext context)
		{
			context.StopPropagation();

			if (_vertical)
				_target.ScrollDown();
			else
				_target.ScrollRight();
		}

		void __touchBegin(EventContext context)
		{
			context.StopPropagation();

			InputEvent evt = context.inputEvent;
			Vector2 pt = _grip.GlobalToLocal(new Vector2(evt.x, evt.y));
			if (_vertical)
			{
				if (pt.Y < 0)
					_target.ScrollUp(4, false);
				else
					_target.ScrollDown(4, false);
			}
			else
			{
				if (pt.X < 0)
					_target.ScrollLeft(4, false);
				else
					_target.ScrollRight(4, false);
			}
		}
	}
}
