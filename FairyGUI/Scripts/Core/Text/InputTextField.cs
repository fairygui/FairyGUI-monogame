using FairyGUI.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 接收用户输入的文本控件。因为支持直接输入表情，所以从RichTextField派生。
	/// </summary>
	public class InputTextField : RichTextField
	{
		/// <summary>
		/// 
		/// </summary>
		public EventListener onFocusIn { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onFocusOut { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onSubmit { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public int maxLength { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool editable { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool hideInput { get; set; }

		string _text;
		string _restrict;
		Regex _restrictPattern;
		bool _displayAsPassword;
		string _promptText;
		string _decodedPromptText;

		bool _editing;
		int _caretPosition;
		int _selectionStart;
		int _composing;

		static Shape _caret;
		static SelectionShape _selectionShape;
		static float _nextBlink;

		const int GUTTER_X = 2;
		const int GUTTER_Y = 2;

		public InputTextField()
		{
			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");
			onSubmit = new EventListener(this, "onSubmit");

			_text = string.Empty;
			maxLength = 0;
			editable = true;
			_composing = 0;

			/* 因为InputTextField定义了ClipRect，而ClipRect是四周缩进了2个像素的（GUTTER)，默认的点击测试
			 * 是使用ClipRect的，那会造成无法点击四周的空白区域。所以这里自定义了一个HitArea
			 */
			this.hitArea = new RectHitTest();
			this.touchChildren = false;

			onFocusIn.Add(__focusIn);
			onFocusOut.AddCapture(__focusOut);
			onKeyDown.AddCapture(__keydown);
			onTouchBegin.AddCapture(__touchBegin);
			onTouchMove.AddCapture(__touchMove);
		}

		/// <summary>
		/// 
		/// </summary>
		public override string text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = ToolSet.FormatCRLF(value);
				ClearSelection();
				UpdateText();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override TextFormat textFormat
		{
			get
			{
				return base.textFormat;
			}
			set
			{
				base.textFormat = value;
				if (_editing)
				{
					_caret.height = textField.textFormat.size;
					_caret.DrawRect(0, Color.Transparent, textField.textFormat.color);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string restrict
		{
			get { return _restrict; }
			set
			{
				_restrict = value;
				if (string.IsNullOrEmpty(_restrict))
					_restrictPattern = null;
				else
					_restrictPattern = new Regex(value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int caretPosition
		{
			get
			{
				textField.Rebuild();
				return _caretPosition;
			}
			set
			{
				SetSelection(value, 0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string promptText
		{
			get
			{
				return _promptText;
			}
			set
			{
				_promptText = value;
				if (!string.IsNullOrEmpty(_promptText))
					_decodedPromptText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
				else
					_decodedPromptText = null;
				UpdateText();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool displayAsPassword
		{
			get { return _displayAsPassword; }
			set
			{
				if (_displayAsPassword != value)
				{
					_displayAsPassword = value;
					UpdateText();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="start"></param>
		/// <param name="length">-1 means the rest count from start</param>
		public void SetSelection(int start, int length)
		{
			if (!_editing)
				Stage.inst.focus = this;

			_selectionStart = start;
			_caretPosition = start + (length < 0 ? int.MaxValue : length);
			if (!textField.Rebuild())
			{
				int cnt = textField.charPositions.Count;
				if (_caretPosition >= cnt)
					_caretPosition = cnt - 1;
				if (_selectionStart >= cnt)
					_selectionStart = cnt - 1;
				UpdateCaret(false);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceSelection(string value)
		{
			if (!editable)
				throw new Exception("InputTextField is not editable.");

			if (!_editing)
				Stage.inst.focus = this;

			textField.Rebuild();
			int t0, t1;
			if (_selectionStart != _caretPosition)
			{
				if (_selectionStart < _caretPosition)
				{
					t0 = _selectionStart;
					t1 = _caretPosition;
					_caretPosition = _selectionStart;
				}
				else
				{
					t0 = _caretPosition;
					t1 = _selectionStart;
					_selectionStart = _caretPosition;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(value))
					return;

				t0 = t1 = _caretPosition;
			}

			StringBuilder buffer = new StringBuilder();
			buffer.Append(_text, 0, t0);
			if (!string.IsNullOrEmpty(value))
			{
				value = ToolSet.FormatCRLF(value);
				value = ValidateInput(value);
				buffer.Append(value);

				_caretPosition += value.Length;
			}
			if (IMEAdapter.compositionString.Length > 0)
			{
				if (textField.text.Length - t1 > _composing)
					buffer.Append(_text, t1 + _composing, textField.text.Length - t1 - _composing);
			}
			else
				buffer.Append(_text, t1 + _composing, _text.Length - t1 - _composing);
			
			if (maxLength > 0 && buffer.Length > maxLength)
				buffer.Length = maxLength;

			this.text = buffer.ToString();
			onChanged.Call();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceText(string value)
		{
			if (value == _text)
				return;

			value = ValidateInput(value);

			if (maxLength > 0 && value.Length > maxLength)
				value = value.Substring(0, maxLength);

			this.text = value;
			onChanged.Call();
		}

		string ValidateInput(string source)
		{
			if (_restrict != null)
			{
				StringBuilder sb = new StringBuilder();
				Match mc = _restrictPattern.Match(source);
				int lastPos = 0;
				string s;
				while (mc != Match.Empty)
				{
					if (mc.Index != lastPos)
					{
						//保留tab和回车
						for (int i = lastPos; i < mc.Index; i++)
						{
							if (source[i] == '\n' || source[i] == '\t')
								sb.Append(source[i]);
						}
					}

					s = mc.ToString();
					lastPos = mc.Index + s.Length;
					sb.Append(s);

					mc = mc.NextMatch();
				}
				for (int i = lastPos; i < source.Length; i++)
				{
					if (source[i] == '\n' || source[i] == '\t')
						sb.Append(source[i]);
				}

				return sb.ToString();
			}
			else
				return source;
		}

		void UpdateText()
		{
			int composing = _composing;
			_composing = 0;

			if (!_editing && _text.Length == 0 && !string.IsNullOrEmpty(_decodedPromptText))
				textField.htmlText = _decodedPromptText;
			else if (_displayAsPassword)
				textField.text = EncodePasswordText(_text);
			else if (IMEAdapter.compositionString.Length > 0 && _caretPosition == 0)
			{
				StringBuilder buffer = new StringBuilder();
				buffer.Append(_text, 0, _caretPosition);
				buffer.Append(IMEAdapter.compositionString);
				buffer.Append(_text, _caretPosition + composing, textField.text.Length - _caretPosition - composing);

				_composing = IMEAdapter.compositionString.Length;

				string newText = buffer.ToString();
				textField.text = newText;
			}
			else
			{
				if (_caretPosition == _text.Length)
					textField.text = _text;
			}
		}

		string EncodePasswordText(string value)
		{
			int textLen = value.Length;
			StringBuilder tmp = new StringBuilder(textLen);
			int i = 0;
			while (i < textLen)
			{
				char c = value[i];
				if (c == '\n')
					tmp.Append(c);
				else
				{
					if (char.IsHighSurrogate(c))
						i++;
					tmp.Append("*");
				}
				i++;
			}
			return tmp.ToString();
		}

		void ClearSelection()
		{
			if (_selectionStart != _caretPosition)
			{
				if (_selectionShape != null)
					_selectionShape.Clear();
				_selectionStart = _caretPosition;
			}
		}

		string GetSelection()
		{
			if (_selectionStart == _caretPosition)
				return string.Empty;

			if (_selectionStart < _caretPosition)
				return _text.Substring(_selectionStart, _caretPosition);
			else
				return _text.Substring(_caretPosition, _selectionStart - _caretPosition);
		}

		void AdjustCaret(TextField.CharPosition cp, bool moveSelectionHeader = false)
		{
			_caretPosition = cp.charIndex;
			if (moveSelectionHeader)
				_selectionStart = _caretPosition;

			UpdateCaret(false);
		}

		void UpdateCaret(bool forceUpdate = false)
		{
			TextField.CharPosition cp;
			if (_editing)
				cp = GetCharPosition(_caretPosition + IMEAdapter.compositionString.Length);
			else
				cp = GetCharPosition(_caretPosition);

			Vector2 pos = GetCharLocation(cp);
			TextField.LineInfo line = textField.lines[cp.lineIndex];
			pos.Y = line.y + textField.y;
			Vector2 newPos = pos;

			if (newPos.X < textField.textFormat.size)
				newPos.X += Math.Min(50, (int)(_contentRect.Width / 2));
			else if (newPos.X > _contentRect.Width - GUTTER_X - textField.textFormat.size)
				newPos.X -= Math.Min(50, (int)(_contentRect.Width / 2));

			if (newPos.X < GUTTER_X)
				newPos.X = GUTTER_X;
			else if (newPos.X > _contentRect.Width - GUTTER_X)
				newPos.X = Math.Max(GUTTER_X, _contentRect.Width - GUTTER_X);

			if (newPos.Y < GUTTER_Y)
				newPos.Y = GUTTER_Y;
			else if (newPos.Y + line.height >= _contentRect.Height - GUTTER_Y)
				newPos.Y = Math.Max(GUTTER_Y, _contentRect.Height - line.height - GUTTER_Y);

			pos += MoveContent(newPos - pos, forceUpdate);

			if (_editing)
			{
				if (line.height > 0) //将光标居中
					pos.Y += (int)(line.height - textField.textFormat.size) / 2;

				_caret.SetPosition(pos.X, pos.Y, 0);

				Vector2 cursorPos = _caret.LocalToGlobal(new Vector2(0, _caret.height));
				IMEAdapter.compositionCursorPos = cursorPos;

				_nextBlink = Timers.time + 0.5f;
				_caret.graphics.enabled = true;

				UpdateSelection(cp);
			}
		}

		Vector2 MoveContent(Vector2 delta, bool forceUpdate)
		{
			float ox = textField.x;
			float oy = textField.y;
			float nx = ox + delta.X;
			float ny = oy + delta.Y;
			if (_contentRect.Width - nx > textField.textWidth)
				nx = _contentRect.Width - textField.textWidth;
			if (_contentRect.Height - ny > textField.textHeight)
				ny = _contentRect.Height - textField.textHeight;
			if (nx > 0)
				nx = 0;
			if (ny > 0)
				ny = 0;
			nx = (int)nx;
			ny = (int)ny;

			if (nx != ox || ny != oy || forceUpdate)
			{
				textField.SetPosition(nx, ny, 0);

				List<HtmlElement> elements = textField.htmlElements;
				int count = elements.Count;
				for (int i = 0; i < count; i++)
				{
					HtmlElement element = elements[i];
					if (element.htmlObject != null)
						element.htmlObject.SetPosition(element.position.X + nx, element.position.Y + ny);
				}
			}

			delta.X = nx - ox;
			delta.Y = ny - oy;
			return delta;
		}

		void UpdateSelection(TextField.CharPosition cp)
		{
			if (_selectionStart == _caretPosition)
			{
				_selectionShape.Clear();
				return;
			}

			TextField.CharPosition start;
			if (_editing && IMEAdapter.compositionString.Length > 0)
			{
				if (_selectionStart < _caretPosition)
				{
					cp = GetCharPosition(_caretPosition);
					start = GetCharPosition(_selectionStart);
				}
				else
					start = GetCharPosition(_selectionStart + IMEAdapter.compositionString.Length);
			}
			else
				start = GetCharPosition(_selectionStart);
			if (start.charIndex > cp.charIndex)
			{
				TextField.CharPosition tmp = start;
				start = cp;
				cp = tmp;
			}

			Vector2 v1 = GetCharLocation(start);
			Vector2 v2 = GetCharLocation(cp);

			List<Rectangle> rects = _selectionShape.rects;
			if (rects == null)
				rects = new List<Rectangle>(2);
			else
				rects.Clear();
			textField.GetLinesShape(start.lineIndex, v1.X - textField.x, cp.lineIndex, v2.X - textField.x, false, rects);
			_selectionShape.rects = rects;
			_selectionShape.position = textField.position;
		}

		TextField.CharPosition GetCharPosition(int caretIndex)
		{
			if (caretIndex < 0)
				caretIndex = 0;
			else if (caretIndex >= textField.charPositions.Count)
				caretIndex = textField.charPositions.Count - 1;

			return textField.charPositions[caretIndex];
		}

		/// <summary>
		/// 通过本地坐标获得字符索引位置
		/// </summary>
		/// <param name="location">本地坐标</param>
		/// <returns></returns>
		TextField.CharPosition GetCharPosition(Vector2 location)
		{
			if (textField.charPositions.Count <= 1)
				return textField.charPositions[0];

			location.X -= textField.x;
			location.Y -= textField.y;

			List<TextField.LineInfo> lines = textField.lines;
			int len = lines.Count;
			TextField.LineInfo line;
			int i;
			for (i = 0; i < len; i++)
			{
				line = lines[i];
				if (line.y + line.height > location.Y)
					break;
			}
			if (i == len)
				i = len - 1;

			int lineIndex = i;

			len = textField.charPositions.Count;
			TextField.CharPosition v;
			int firstInLine = -1;
			for (i = 0; i < len; i++)
			{
				v = textField.charPositions[i];
				if (v.lineIndex == lineIndex)
				{
					if (firstInLine == -1)
						firstInLine = i;
					if (v.offsetX > location.X)
					{
						if (i > firstInLine)
						{
							//最后一个字符有点难点
							if (v.offsetX - location.X < 2)
								return v;
							else
								return textField.charPositions[i - 1];
						}
						else
							return textField.charPositions[firstInLine];
					}
				}
				else if (firstInLine != -1)
					break;
			}

			return textField.charPositions[i - 1];
		}

		/// <summary>
		/// 获得字符的坐标。这个坐标是容器的坐标，不是文本里的坐标。
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		Vector2 GetCharLocation(TextField.CharPosition cp)
		{
			TextField.LineInfo line = textField.lines[cp.lineIndex];
			Vector2 pos = new Vector2();
			if (line.width == 0 || textField.charPositions.Count == 0)
			{
				if (textField.align == AlignType.Center)
					pos.X = (int)(_contentRect.Width / 2);
				else
					pos.X = GUTTER_X;
			}
			else
			{
				TextField.CharPosition v = textField.charPositions[Math.Min(cp.charIndex, textField.charPositions.Count - 1)];
				pos.X = v.offsetX - 1;
			}
			pos.X += textField.x;
			pos.Y = textField.y + line.y;
			return pos;
		}

		override internal void RefreshObjects()
		{
			base.RefreshObjects();

			if (_editing)
			{
				SetChildIndex(_selectionShape, this.numChildren - 1);
				SetChildIndex(_caret, this.numChildren - 2);
			}

			int cnt = textField.charPositions.Count;
			if (_caretPosition >= cnt)
				_caretPosition = cnt - 1;
			if (_selectionStart >= cnt)
				_selectionStart = cnt - 1;

			UpdateCaret(true);
		}

		protected override void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			base.OnSizeChanged(widthChanged, heightChanged);

			Rectangle rect = _contentRect;
			rect.X += GUTTER_X;
			rect.Y += GUTTER_Y;
			rect.Width -= GUTTER_X * 2;
			//高度不减GUTTER_X * 2，因为怕高度不小心截断文字
			this.clipRect = rect;
			((RectHitTest)this.hitArea).rect = _contentRect;
		}

		public override void Update()
		{
			base.Update();

			if (_editing)
			{
				if (_nextBlink < Timers.time)
				{
					_nextBlink = Timers.time + 0.5f;
					_caret.graphics.enabled = !_caret.graphics.enabled;
				}
			}
		}

		public override void Dispose()
		{
			if (_editing)
			{
				_caret.RemoveFromParent();
				_selectionShape.RemoveFromParent();
			}
			base.Dispose();
		}

		void DoCopy(string value)
		{
		}

		void DoPaste()
		{
		}

		static void CreateCaret()
		{
			_caret = new Shape();
			_caret.touchable = false;
			_caret.graphics.dontClip = true;

			_selectionShape = new SelectionShape();
			_selectionShape.color = UIConfig.inputHighlightColor;
			_selectionShape.touchable = false;
		}

		void __touchBegin(EventContext context)
		{
			if (!_editing || textField.charPositions.Count <= 1)
				return;

			ClearSelection();

			Vector2 v = Stage.inst.touchPosition;
			v = this.GlobalToLocal(v);
			TextField.CharPosition cp = GetCharPosition(v);

			AdjustCaret(cp, true);

			context.CaptureTouch();
		}

		void __touchMove(EventContext context)
		{
			if (isDisposed)
				return;

			Vector2 v = Stage.inst.touchPosition;
			v = this.GlobalToLocal(v);
			if (float.IsNaN(v.X))
				return;

			TextField.CharPosition cp = GetCharPosition(v);
			if (cp.charIndex != _caretPosition)
				AdjustCaret(cp);
		}

		void __focusIn(EventContext context)
		{
			if (!editable)
				return;

			_editing = true;

			if (_caret == null)
				CreateCaret();

			if (!string.IsNullOrEmpty(_promptText))
				UpdateText();

			float caretSize = UIConfig.inputCaretSize;
			_caret.SetSize(caretSize, textField.textFormat.size);
			_caret.DrawRect(0, Color.Transparent, textField.textFormat.color);
			AddChild(_caret);

			_selectionShape.Clear();
			AddChild(_selectionShape);

			if (!textField.Rebuild())
			{
				TextField.CharPosition cp = GetCharPosition(_caretPosition);
				AdjustCaret(cp);
			}

			IMEAdapter.compositionMode = IMEAdapter.CompositionMode.On;
			_composing = 0;
		}

		void __focusOut(EventContext contxt)
		{
			if (!_editing)
				return;

			_editing = false;

			IMEAdapter.compositionMode = IMEAdapter.CompositionMode.Off;

			if (!string.IsNullOrEmpty(_promptText))
				UpdateText();

			_caret.RemoveFromParent();
			_selectionShape.RemoveFromParent();
		}

		void __keydown(EventContext context)
		{
			if (!_editing || context.isDefaultPrevented)
				return;

			InputEvent evt = context.inputEvent;

			switch (evt.keyCode)
			{
				case Keys.Back:
					{
						if (textField.text.Length > 0)
							IMEAdapter.compositionString = textField.text.Remove(textField.text.Length - 1, 1);

						context.PreventDefault();
						if (_selectionStart == _caretPosition && _caretPosition > 0)
							_selectionStart = _caretPosition - 1;

						ReplaceSelection(null);
						break;
					}

				case Keys.Delete:
					{
						context.PreventDefault();
						if (_selectionStart == _caretPosition && _caretPosition < textField.charPositions.Count - 1)
							_selectionStart = _caretPosition + 1;
						ReplaceSelection(null);
						break;
					}

				case Keys.Left:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();
						if (_caretPosition > 0)
						{
							TextField.CharPosition cp = GetCharPosition(_caretPosition - 1);
							AdjustCaret(cp, !evt.shift);
						}
						break;
					}

				case Keys.Right:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();
						if (_caretPosition < textField.charPositions.Count - 1)
						{
							TextField.CharPosition cp = GetCharPosition(_caretPosition + 1);
							AdjustCaret(cp, !evt.shift);
						}
						break;
					}

				case Keys.Up:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == 0)
							return;

						TextField.LineInfo line = textField.lines[cp.lineIndex - 1];
						cp = GetCharPosition(new Vector2(_caret.x, line.y + textField.y));
						AdjustCaret(cp, !evt.shift);
						break;
					}

				case Keys.Down:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == textField.lines.Count - 1)
							cp.charIndex = textField.charPositions.Count - 1;
						else
						{
							TextField.LineInfo line = textField.lines[cp.lineIndex + 1];
							cp = GetCharPosition(new Vector2(_caret.x, line.y + textField.y));
						}
						AdjustCaret(cp, !evt.shift);
						break;
					}

				case Keys.PageUp:
					{
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case Keys.PageDown:
					{
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case Keys.Home:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						TextField.LineInfo line = textField.lines[cp.lineIndex];
						cp = GetCharPosition(new Vector2(int.MinValue, line.y + textField.y));
						AdjustCaret(cp, !evt.shift);
						break;
					}

				case Keys.End:
					{
						context.PreventDefault();
						if (!evt.shift)
							ClearSelection();

						TextField.CharPosition cp = GetCharPosition(_caretPosition);
						TextField.LineInfo line = textField.lines[cp.lineIndex];
						cp = GetCharPosition(new Vector2(int.MaxValue, line.y + textField.y));
						AdjustCaret(cp, !evt.shift);

						break;
					}

				//Select All
				case Keys.A:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							_selectionStart = 0;
							AdjustCaret(GetCharPosition(int.MaxValue));
						}
						break;
					}

				//Copy
				case Keys.C:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
								DoCopy(s);
						}
						break;
					}

				//Paste
				case Keys.V:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							DoPaste();
						}
						break;
					}

				//Cut
				case Keys.X:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
							{
								DoCopy(s);
								ReplaceSelection(null);
							}
						}
						break;
					}

				case Keys.Enter:
					{
						if (textField.singleLine)
						{
							onSubmit.Call();
							return;
						}
						break;
					}
			}

			string str = evt.KeyName;
			if (str != null && str.Length > 0 && IMEAdapter.compositionMode == IMEAdapter.CompositionMode.Off)
			{
				if (evt.ctrl)
					return;

				if (textField.singleLine && str == "\n")
					return;

				ReplaceSelection(str);
			}
			else
			{
				if (IMEAdapter.compositionString.Length > 0)
					UpdateText();
			}
		}

		internal void CheckComposition()
		{
			if (_composing != 0 && IMEAdapter.compositionString.Length == 0)
				UpdateText();
		}
	}
}
