using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Stage : Container, IGameComponent, IDrawable, IUpdateable
	{
		public static Game game;

		/// <summary>
		/// 
		/// </summary>
		public float soundVolume { get; set; }

		DisplayObject _touchTarget;
		DisplayObject _focused;
		InputTextField _lastInput;
		FairyBatch _batch;
		List<DisplayObject> _rollOutChain;
		List<DisplayObject> _rollOverChain;
		TouchInfo _touchInfo;
		Vector2 _touchPosition;
		EventCallback1 _focusRemovedDelegate;
		List<NTexture> _toCollectTextures = new List<NTexture>();
		bool _soundEnabled;
		Dictionary<Keys, float> _lastKeyDownTime;
		Keys[] _lastKeys;
		int _lastScrollWheelValue;

		/// <summary>
		/// 
		/// </summary>
		public static EventCallback0 beforeUpdate;
		/// <summary>
		/// 
		/// </summary>
		public static EventCallback0 afterUpdate;

		static Stage _inst;
		/// <summary>
		/// 
		/// </summary>
		public static Stage inst
		{
			get { return _inst; }
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool isTouchOnUI
		{
			get { return _inst != null && _inst.touchTarget != null; }
		}

		/// <summary>
		/// 
		/// </summary>
		public static bool touchScreen
		{
			get { return false; }
		}

        public IInputHandler _wic { get; }

        public static readonly char[] SPECIAL_CHARACTERS = { '\a', '\b', '\n', '\r', '\f', '\t', '\v' };

        /// <summary>
        /// 
        /// </summary>
        public Stage(Game game, IInputHandler handler)
        {
            _inst = this;
            _wic = handler;
            Stage.game = game;

            if (handler == null)
                game.Window.TextInput += WindowOnTextInput;

            soundVolume = 1;

			_batch = new FairyBatch();
			_soundEnabled = true;

			_touchInfo = new TouchInfo();
			_touchInfo.touchId = 0;
			_lastKeyDownTime = new Dictionary<Keys, float>();

			_rollOutChain = new List<DisplayObject>();
			_rollOverChain = new List<DisplayObject>();

			_focusRemovedDelegate = OnFocusRemoved;
		}

		private void WindowOnTextInput(object sender, TextInputEventArgs e)
        {
            if (!SPECIAL_CHARACTERS.Contains(e.Character))
            {
                IMEAdapter.compositionString += e.Character;
            }

        }


		/// <summary>
		/// 
		/// </summary>
		public void Initialize()
		{
			Timers.inst.Add(5, 0, RunTextureCollector);

			game.Window.ClientSizeChanged += OnResolutionChanged;
			OnResolutionChanged(null, null);

			GRoot._inst = new GRoot();
			GRoot._inst.ApplyContentScaleFactor();
			AddChild(GRoot._inst.displayObject);
		}

		/// <summary>
		/// 
		/// </summary>
		public override void Dispose()
		{
			_batch.Dispose();

			base.Dispose();
		}

		/// <summary>
		/// 绘制顺序
		/// </summary>
		public int DrawOrder
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// 可见性
		/// </summary>
		public bool Visible
		{
			get
			{
				return true;
			}
		}

#pragma warning disable 0067
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<EventArgs> DrawOrderChanged;
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<EventArgs> VisibleChanged;
#pragma warning restore 0067

		/// <summary>
		/// 是否启用
		/// </summary>
		public bool Enabled { get { return true; } }
		/// <summary>
		/// 更新顺序
		/// </summary>
		public int UpdateOrder { get { return 0; } }

#pragma warning disable 0067
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<EventArgs> EnabledChanged;
		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<EventArgs> UpdateOrderChanged;
#pragma warning restore 0067

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject touchTarget
		{
			get
			{
				if (_touchTarget == this)
					return null;
				else
					return _touchTarget;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public DisplayObject focus
		{
			get
			{
				if (_focused != null && _focused.isDisposed)
					_focused = null;
				return _focused;
			}
			set
			{
				if (_focused == value)
					return;

				DisplayObject oldFocus = _focused;
				_focused = value;
				if (_focused == this)
					_focused = null;

				if (oldFocus != null)
				{
					InputTextField field = oldFocus as InputTextField;
					if (field != null)
						field.DispatchEvent("onFocusOut", null);

					oldFocus.onRemovedFromStage.RemoveCapture(_focusRemovedDelegate);
				}

				if (_focused != null)
				{
					InputTextField field = _focused as InputTextField;
					if (field != null)
					{
						_lastInput = field;
						_lastInput.DispatchEvent("onFocusIn", null);
					}

					_focused.onRemovedFromStage.AddCapture(_focusRemovedDelegate);
				}
			}
		}

		void OnFocusRemoved(EventContext context)
		{
			if (context.sender == _focused)
			{
				if (_focused is InputTextField)
					_lastInput = null;
				this.focus = null;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 touchPosition
		{
			get
			{
				return _touchPosition;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		/// <returns></returns>
		public Vector2 GetTouchPosition(int touchId)
		{
			return _touchPosition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		public void CancelClick(int touchId)
		{
			_touchInfo.clickCancelled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void EnableSound()
		{
			_soundEnabled = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void DisableSound()
		{
			_soundEnabled = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="volumeScale"></param>
		public void PlayOneShotSound(SoundEffectInstance clip, float volumeScale)
		{
			if (_soundEnabled)
			{
				clip.Volume = volumeScale;
				clip.Play();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clip"></param>
		public void PlayOneShotSound(SoundEffectInstance clip)
		{
			if (_soundEnabled)
				clip.Play();
		}

		static EventCallback0 _tempDelegate;

		void OnResolutionChanged(object sender, EventArgs args)
		{
			SetSize(game.GraphicsDevice.PresentationParameters.BackBufferWidth, game.GraphicsDevice.PresentationParameters.BackBufferHeight);

			UIContentScaler.ApplyChange();
		}

		internal void HandleMouseEvents()
		{
			MouseState mouseState = Mouse.GetState();

			_touchPosition = new Vector2(mouseState.Position.X, mouseState.Position.Y);
			_touchTarget = HitTest(_touchPosition, true);
			_touchInfo.target = _touchTarget;

			if (Math.Abs(_touchInfo.x - _touchPosition.X) > 0 || Math.Abs(_touchInfo.y - _touchPosition.Y) > 0)
			{
				_touchInfo.x = _touchPosition.X;
				_touchInfo.y = _touchPosition.Y;
				_touchInfo.Move();
			}

			if (_touchInfo.lastRollOver != _touchInfo.target)
				HandleRollOver(_touchInfo);

			if (mouseState.LeftButton == ButtonState.Pressed ||
				mouseState.RightButton == ButtonState.Pressed ||
				mouseState.MiddleButton == ButtonState.Pressed)
			{
				if (!_touchInfo.began)
				{
					_touchInfo.Begin();
					_touchInfo.button =
						(mouseState.LeftButton == ButtonState.Pressed)
							? 0
							: (mouseState.RightButton == ButtonState.Pressed ? 1 : 2);
					this.focus = _touchInfo.target;

					_touchInfo.UpdateEvent();
					_touchInfo.target.BubbleEvent("onTouchBegin", _touchInfo.evt);
				}
			}
			else if (mouseState.LeftButton == ButtonState.Released ||
				mouseState.RightButton == ButtonState.Released ||
				mouseState.MiddleButton == ButtonState.Released)
			{
				if (_touchInfo.began)
				{
					_touchInfo.End();

					DisplayObject clickTarget = _touchInfo.ClickTest();
					if (clickTarget != null)
					{
						_touchInfo.UpdateEvent();

						if (_touchInfo.button == 1)
							clickTarget.BubbleEvent("onRightClick", _touchInfo.evt);
						else
							clickTarget.BubbleEvent("onClick", _touchInfo.evt);
					}

					_touchInfo.button = -1;
				}
			}

			int deltaWheel = mouseState.ScrollWheelValue - _lastScrollWheelValue;
			if (deltaWheel != 0)
			{
				if (_touchTarget != null)
				{
					_touchInfo.mouseWheelDelta = -deltaWheel;
					_touchInfo.UpdateEvent();
					_touchTarget.BubbleEvent("onMouseWheel", _touchInfo.evt);
					_touchInfo.mouseWheelDelta = 0;
				}

				_lastScrollWheelValue = mouseState.ScrollWheelValue;
			}
		}

		private static Dictionary<Keys, string> _charByScanCode1 = new Dictionary<Keys, string>
		{
			{ Keys.D1, "1"},{ Keys.D2, "2"},{ Keys.D3, "3"},{ Keys.D4, "4"},{ Keys.D5, "5"},
			{ Keys.D6, "6"},{ Keys.D7, "7"},{ Keys.D8, "8"},{ Keys.D9, "9"},{ Keys.D0, "0"},
			{ Keys.OemMinus, "-"},{ Keys.OemPlus, "="},{ Keys.OemOpenBrackets, "["},{ Keys.OemCloseBrackets, "]"},{ Keys.OemPipe, "\\"},
			{ Keys.OemSemicolon, ";"},{ Keys.OemQuotes, "'"},{ Keys.OemComma, ","},{ Keys.OemPeriod, "."},{ Keys.OemQuestion, "/"},
			{ Keys.Enter, "\n" }, {Keys.Tab, "\t" }, {Keys.Space, " " }, {Keys.OemTilde, "`" }
		};

		private static Dictionary<Keys, string> _charByScanCode2 = new Dictionary<Keys, string>
		{
			{ Keys.D1, "!"},{ Keys.D2, "@"},{ Keys.D3, "#"},{ Keys.D4, "$"},{ Keys.D5, "%"},
			{ Keys.D6, "^"},{ Keys.D7, "&"},{ Keys.D8, "*"},{ Keys.D9, "("},{ Keys.D0, ")"},
			{ Keys.OemMinus, "_"},{ Keys.OemPlus, "+"},{ Keys.OemOpenBrackets, "{"},{ Keys.OemCloseBrackets, "}"},{ Keys.OemPipe, "|"},
			{ Keys.OemSemicolon, ":"},{ Keys.OemQuotes, "\""},{ Keys.OemComma, "<"},{ Keys.OemPeriod, ">"},{ Keys.OemQuestion, "?"},
			{Keys.OemTilde, "~" }
		};

		private static Dictionary<Keys, string> _charByScanCode3 = new Dictionary<Keys, string>
		{
			{ Keys.NumPad1, "1"},{ Keys.NumPad2, "2"},{ Keys.NumPad3, "3"},{ Keys.NumPad4, "4"},{ Keys.NumPad5, "5"},
			{ Keys.NumPad6, "6"},{ Keys.NumPad7, "7"},{ Keys.NumPad8, "8"},{ Keys.NumPad9, "9"},{ Keys.NumPad0, "0"},
			{ Keys.Add, "+"},{ Keys.Divide, "/"},{ Keys.Enter, "\n"},{ Keys.Multiply, "*"},{ Keys.Decimal, "."},
			{ Keys.Subtract, "-"}
		};

		private void HandleKeyEvents()
		{
			Keys[] keys = Keyboard.GetState().GetPressedKeys();

			InputModifierFlags modifiers = 0;

			float currTime = Timers.time;
			int cnt = keys.Length;

			for (int i = 0; i < cnt; i++)
			{
				Keys key = keys[i];

                switch (key)
				{
					case Keys.LeftShift:
						modifiers |= InputModifierFlags.LShift;
						break;

					case Keys.RightShift:
						modifiers |= InputModifierFlags.RShift;
						break;

					case Keys.LeftAlt:
						modifiers |= InputModifierFlags.LAlt;
						break;

					case Keys.RightAlt:
						modifiers |= InputModifierFlags.RAlt;
						break;

					case Keys.LeftControl:
						modifiers |= InputModifierFlags.LCtrl;
						break;

					case Keys.RightControl:
						modifiers |= InputModifierFlags.RCtrl;
						break;

					case Keys.CapsLock:
						modifiers |= InputModifierFlags.CapsLock;
						break;

					case Keys.NumLock:
						modifiers |= InputModifierFlags.NumLock;
						break;
				}
			}

			if (_lastKeys != null)
			{
				cnt = _lastKeys.Length;
				for (int i = 0; i < cnt; i++)
				{
					Keys key = _lastKeys[i];
					if (Array.IndexOf(keys, key) == -1)
						_lastKeyDownTime.Remove(key);
				}
			}

			_lastKeys = keys;

			cnt = keys.Length;
			for (int i = 0; i < cnt; i++)
			{
				Keys key = keys[i];

				float lastDownTime;
				if (_lastKeyDownTime.TryGetValue(key, out lastDownTime))
				{
					if (currTime - lastDownTime < 0)
						continue;

					_lastKeyDownTime[key] = currTime + 0.05f;
				}
				else
					_lastKeyDownTime[key] = currTime + 0.5f;

				_touchInfo.keyCode = key;
				_touchInfo.keyName = null;
				_touchInfo.modifiers = modifiers;

				//evt.keyName is not reliable, I parse it myself.
				if (key >= Keys.A && key <= Keys.Z)
				{
					bool capsLock = (modifiers & InputModifierFlags.CapsLock) != 0;
					if ((modifiers & InputModifierFlags.Shift) != 0)
						capsLock = !capsLock;
					if (capsLock)
						_touchInfo.keyName = key.ToString();
					else
						_touchInfo.keyName = key.ToString().ToLower();
				}
				else
				{
					if (_charByScanCode3.TryGetValue(key, out _touchInfo.keyName))
					{
						if (key == Keys.NumLock)
							_touchInfo.keyName = null;
					}
					else if ((modifiers & InputModifierFlags.Shift) != 0)
					{
						if (!_charByScanCode2.TryGetValue(key, out _touchInfo.keyName))
							_charByScanCode1.TryGetValue(key, out _touchInfo.keyName);
					}
					else
						_charByScanCode1.TryGetValue(key, out _touchInfo.keyName);
				}

				_touchInfo.UpdateEvent();
				DisplayObject f = this.focus;
				if (f != null)
					f.BubbleEvent("onKeyDown", _touchInfo.evt);
				else
					DispatchEvent("onKeyDown", _touchInfo.evt);
			}
		}

		void HandleTextInput()
		{
			InputTextField textField = (InputTextField)_focused;
			if (!textField.editable)
				return;

			textField.CheckComposition();
		}

		void HandleRollOver(TouchInfo touch)
		{
			DisplayObject element;
			element = touch.lastRollOver;
			while (element != null)
			{
				_rollOutChain.Add(element);
				element = element.parent;
			}

			touch.lastRollOver = touch.target;

			element = touch.target;
			int i;
			while (element != null)
			{
				i = _rollOutChain.IndexOf(element);
				if (i != -1)
				{
					_rollOutChain.RemoveRange(i, _rollOutChain.Count - i);
					break;
				}
				_rollOverChain.Add(element);

				element = element.parent;
			}

			int cnt = _rollOutChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOutChain[i];
					if (element.stage != null)
						element.DispatchEvent("onRollOut", null);
				}
				_rollOutChain.Clear();
			}

			cnt = _rollOverChain.Count;
			if (cnt > 0)
			{
				for (i = 0; i < cnt; i++)
				{
					element = _rollOverChain[i];
					if (element.stage != null)
						element.DispatchEvent("onRollOver", null);
				}
				_rollOverChain.Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="texture"></param>
		public void MonitorTexture(NTexture texture)
		{
			if (_toCollectTextures.IndexOf(texture) == -1)
				_toCollectTextures.Add(texture);
		}

		void RunTextureCollector(object param)
		{
			int cnt = _toCollectTextures.Count;
			float curTime = Timers.time;
			int i = 0;
			while (i < cnt)
			{
				NTexture texture = _toCollectTextures[i];
				if (texture.disposed)
				{
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else if (curTime - texture.lastActive > 5)
				{
					texture.Dispose();
					_toCollectTextures.RemoveAt(i);
					cnt--;
				}
				else
					i++;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="touchId"></param>
		/// <param name="target"></param>
		public void AddTouchMonitor(int touchId, EventDispatcher target)
		{
			if (_touchInfo.touchMonitors.IndexOf(target) == -1)
				_touchInfo.touchMonitors.Add(target);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		public void RemoveTouchMonitor(EventDispatcher target)
		{
			int i = _touchInfo.touchMonitors.IndexOf(target);
			if (i != -1)
				_touchInfo.touchMonitors[i] = null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Update(GameTime gameTime)
		{
			Timers.inst.Update(gameTime);
			TweenManager.Update(gameTime);

            HandleInputCapturer();
            HandleKeyEvents();
			HandleMouseEvents();
            if (_focused is InputTextField)
				HandleTextInput();

			_tempDelegate = beforeUpdate;
			beforeUpdate = null;

			//允许beforeUpdate里再次Add，这里没有做死锁检查
			while (_tempDelegate != null)
			{
				_tempDelegate.Invoke();
				_tempDelegate = beforeUpdate;
				beforeUpdate = null;
			}
			_tempDelegate = null;

			Update();

			if (afterUpdate != null)
				afterUpdate.Invoke();

			afterUpdate = null;
		}

        public void HandleInputCapturer()
        {
            if (_wic == null)
                return;

            var getChars = _wic.myCharacters;
            foreach (var character in getChars)
            {
                if (!character.IsUsed && character.CharacterType == 0)
                {
                    IMEAdapter.compositionString += character.Chars;
                    character.IsUsed = true;
                }
            }
        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		public void Draw(GameTime gameTime)
		{
			_batch.Begin();
			Draw(_batch);
			_batch.End();
		}
	}

	class TouchInfo
	{
		public float x;
		public float y;
		public int touchId;
		public int clickCount;
		public Keys keyCode;
		public string keyName;
		public char character;
		public InputModifierFlags modifiers;
		public int mouseWheelDelta;
		public int button;

		public float downX;
		public float downY;
		public bool began;
		public bool clickCancelled;
		public float lastClickTime;
		public DisplayObject target;
		public List<DisplayObject> downTargets;
		public DisplayObject lastRollOver;
		public List<EventDispatcher> touchMonitors;

		public InputEvent evt;

		static List<EventBridge> sHelperChain = new List<EventBridge>();

		public TouchInfo()
		{
			evt = new InputEvent();
			downTargets = new List<DisplayObject>();
			touchMonitors = new List<EventDispatcher>();
			Reset();
		}

		public void Reset()
		{
			touchId = -1;
			x = 0;
			y = 0;
			clickCount = 0;
			button = -1;
			keyCode = Keys.None;
			keyName = null;
			character = '\0';
			modifiers = 0;
			mouseWheelDelta = 0;
			lastClickTime = 0;
			began = false;
			target = null;
			downTargets.Clear();
			lastRollOver = null;
			clickCancelled = false;
			touchMonitors.Clear();
		}

		public void UpdateEvent()
		{
			evt.touchId = this.touchId;
			evt.x = this.x;
			evt.y = this.y;
			evt.clickCount = this.clickCount;
			evt.keyCode = this.keyCode;
			evt.KeyName = this.keyName;
			evt.modifiers = this.modifiers;
			evt.mouseWheelDelta = this.mouseWheelDelta;
			evt.button = this.button;
		}

		public void Begin()
		{
			began = true;
			clickCancelled = false;
			downX = x;
			downY = y;

			downTargets.Clear();
			if (target != null)
			{
				downTargets.Add(target);
				DisplayObject obj = target;
				while (obj != null)
				{
					downTargets.Add(obj);
					obj = obj.parent;
				}
			}
		}

		public void Move()
		{
			UpdateEvent();

			if (Math.Abs(x - downX) > 50 || Math.Abs(y - downY) > 50) clickCancelled = true;

			if (touchMonitors.Count > 0)
			{
				int len = touchMonitors.Count;
				for (int i = 0; i < len; i++)
				{
					EventDispatcher e = touchMonitors[i];
					if (e != null)
					{
						if ((e is DisplayObject) && ((DisplayObject)e).stage == null)
							continue;
						if ((e is GObject) && !((GObject)e).onStage)
							continue;
						e.GetChainBridges("onTouchMove", sHelperChain, false);
					}
				}

				Stage.inst.BubbleEvent("onTouchMove", evt, sHelperChain);
				sHelperChain.Clear();
			}
			else
				Stage.inst.DispatchEvent("onTouchMove", evt);
		}

		public void End()
		{
			began = false;

			UpdateEvent();

			if (touchMonitors.Count > 0)
			{
				int len = touchMonitors.Count;
				for (int i = 0; i < len; i++)
				{
					EventDispatcher e = touchMonitors[i];
					if (e != null)
						e.GetChainBridges("onTouchEnd", sHelperChain, false);
				}
				target.BubbleEvent("onTouchEnd", evt, sHelperChain);

				touchMonitors.Clear();
				sHelperChain.Clear();
			}
			else
				target.BubbleEvent("onTouchEnd", evt);

			if (Timers.time - lastClickTime < 0.35f)
			{
				if (clickCount == 2)
					clickCount = 1;
				else
					clickCount++;
			}
			else
				clickCount = 1;
			lastClickTime = Timers.time;
		}

		public DisplayObject ClickTest()
		{
			if (downTargets.Count == 0
				|| clickCancelled
				|| Math.Abs(x - downX) > 50 || Math.Abs(y - downY) > 50)
				return null;

			DisplayObject obj = downTargets[0];
			if (obj.stage != null) //依然派发到原来的downTarget，虽然可能它已经偏离当前位置，主要是为了正确处理点击缩放的效果
				return obj;

			obj = target;
			while (obj != null)
			{
				int i = downTargets.IndexOf(obj);
				if (i != -1 && obj.stage != null)
					return obj;

				obj = obj.parent;
			}

			downTargets.Clear();

			return obj;
		}
	}
}