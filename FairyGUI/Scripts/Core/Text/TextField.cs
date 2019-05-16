using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using System.Drawing;
using Rectangle = System.Drawing.RectangleF;
using System.Drawing.Drawing2D;
using Region = System.Drawing.Region;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class TextField : DisplayObject, IMeshFactory
	{
		VertAlignType _verticalAlign;
		TextFormat _textFormat;
		bool _input;
		string _text;
		AutoSizeType _autoSize;
		bool _wordWrap;
		bool _singleLine;
		bool _html;
		int _maxWidth;

		int _stroke;
		Color _strokeColor;
		Vector2 _shadowOffset;

		List<HtmlElement> _elements;
		List<LineInfo> _lines;
		List<RenderElement> _renderElements;
		List<CharPosition> _charPositions;

		BaseFont _font;
		float _textWidth;
		float _textHeight;
		float _minHeight;
		bool _textChanged;
		int _yOffset;
		float _fontSizeScale;
		float _globalScale;
        Bitmap _canvas;
        NTexture _texture;

		RichTextField _richTextField;

		const int GUTTER_X = 2;
		const int GUTTER_Y = 2;
		static float[] STROKE_OFFSET = new float[]
		{
			 -1, 0, 1, 0,
			0, -1, 0, 1,
			-1, -1, 1, -1,
			-1, 1, 1, 1
		};

		public TextField()
		{
			_touchDisabled = true;

			_textFormat = new TextFormat();
			_strokeColor = Color.Black;
			_fontSizeScale = 1;
			_globalScale = UIContentScaler.scaleFactor;

			_wordWrap = false;
			_text = string.Empty;

			_elements = new List<HtmlElement>(0);
			_lines = new List<LineInfo>(1);
			_renderElements = new List<RenderElement>(1);

			graphics = new NGraphics();
			graphics.pixelSnapping = true;
			graphics.meshFactory = this;
		}

		public override void Dispose()
		{
			base.Dispose();

            if (_canvas != null)
				_canvas.Dispose();
        }

        internal void EnableRichSupport(RichTextField richTextField)
		{
			_richTextField = richTextField;
			if (_richTextField is InputTextField)
			{
				_input = true;
				_charPositions = new List<CharPosition>();
				_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public TextFormat textFormat
		{
			get { return _textFormat; }
			set
			{
				_textFormat = value;
				ResolveFont();
				if (!string.IsNullOrEmpty(_text))
					_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AlignType align
		{
			get { return _textFormat.align; }
			set
			{
				if (_textFormat.align != value)
				{
					_textFormat.align = value;
					if (!string.IsNullOrEmpty(_text))
						_textChanged = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public VertAlignType verticalAlign
		{
			get
			{
				return _verticalAlign;
			}
			set
			{
				if (_verticalAlign != value)
				{
					_verticalAlign = value;
					ApplyVertAlign();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string text
		{
			get { return _text; }
			set
			{
				_text = ToolSet.FormatCRLF(value);
				_textChanged = true;
				_html = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string htmlText
		{
			get { return _text; }
			set
			{
				_text = value;
				_textChanged = true;
				_html = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AutoSizeType autoSize
		{
			get { return _autoSize; }
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					_textChanged = true;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool wordWrap
		{
			get { return _wordWrap; }
			set
			{
				_wordWrap = value;
				_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool singleLine
		{
			get { return _singleLine; }
			set
			{
				_singleLine = value;
				_textChanged = true;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int stroke
		{
			get
			{
				return _stroke;
			}
			set
			{
				if (_stroke != value)
				{
					_stroke = value;
					graphics.SetMeshDirty();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color strokeColor
		{
			get
			{
				return _strokeColor;
			}
			set
			{
				_strokeColor = value;
				graphics.SetMeshDirty();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Vector2 shadowOffset
		{
			get
			{
				return _shadowOffset;
			}
			set
			{
				_shadowOffset = value;
				graphics.SetMeshDirty();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float textWidth
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textWidth;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float textHeight
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textHeight;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int maxWidth
		{
			get { return _maxWidth; }
			set
			{
				if (_maxWidth != value)
				{
					_maxWidth = value;
					_textChanged = true;
				}
			}
		}

		public List<HtmlElement> htmlElements
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _elements;
			}
		}

		public List<LineInfo> lines
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _lines;
			}
		}

		public List<CharPosition> charPositions
		{
			get
			{
				if (_textChanged)
					BuildLines();

				graphics.UpdateMesh();

				return _charPositions;
			}
		}

		public RichTextField richTextField
		{
			get { return _richTextField; }
		}

		/// <summary>
		/// 立刻重建文本
		/// </summary>
		public bool Rebuild()
		{
			if (_globalScale != UIContentScaler.scaleFactor)
				_textChanged = true;

			if (_textChanged)
				BuildLines();

			return graphics.UpdateMesh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="startLine"></param>
		/// <param name="startCharX"></param>
		/// <param name="endLine"></param>
		/// <param name="endCharX"></param>
		/// <param name="clipped"></param>
		/// <param name="resultRects"></param>
		public void GetLinesShape(int startLine, float startCharX, int endLine, float endCharX,
			bool clipped,
			List<Rectangle> resultRects)
		{
			LineInfo line1 = _lines[startLine];
			LineInfo line2 = _lines[endLine];
			if (startLine == endLine)
			{
				Rectangle r = new Rectangle(startCharX, line1.y, endCharX - startCharX, line1.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
			}
			else if (startLine == endLine - 1)
			{
				Rectangle r = new Rectangle(startCharX, line1.y, GUTTER_X + line1.width - startCharX, line1.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
				r = new Rectangle(GUTTER_X, line1.y + line1.height, endCharX - GUTTER_X, line2.y + line2.height - line1.y - line1.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
			}
			else
			{
				Rectangle r = new Rectangle(startCharX, line1.y, GUTTER_X + line1.width - startCharX, line1.height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
				for (int i = startLine + 1; i < endLine; i++)
				{
					LineInfo line = _lines[i];
					r = new Rectangle(GUTTER_X, r.Y + r.Height, (int)line.width, (int)line.y + (int)line.height - r.Y - r.Height);
					if (clipped)
						resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
					else
						resultRects.Add(r);
				}
				r = new Rectangle(GUTTER_X, r.Y + r.Height, (int)endCharX - GUTTER_X, (int)line2.y + (int)line2.height - r.Y - r.Height);
				if (clipped)
					resultRects.Add(ToolSet.Intersection(ref r, ref _contentRect));
				else
					resultRects.Add(r);
			}
		}

		override protected void OnSizeChanged(bool widthChanged, bool heightChanged)
		{
			if (!_updatingSize)
			{
				_minHeight = _contentRect.Height;

				if (_wordWrap && widthChanged)
					_textChanged = true;
				else if (_autoSize != AutoSizeType.None)
					graphics.SetMeshDirty();

				if (_verticalAlign != VertAlignType.Top)
					ApplyVertAlign();
			}

			base.OnSizeChanged(widthChanged, heightChanged);
		}

		public override void EnsureSizeCorrect()
		{
			if (_textChanged && _autoSize != AutoSizeType.None)
				BuildLines();
		}

		public override void Update()
		{
			base.Update();

			if (_richTextField == null) //如果是richTextField，会在update前主动调用了Rebuild
				Rebuild();
		}

		void ResolveFont()
		{
			string fontName = _textFormat.font;
			if (string.IsNullOrEmpty(fontName))
				fontName = UIConfig.defaultFont;
			if (_font == null || _font.name != fontName)
			{
				_font = FontManager.GetFont(fontName);
				if (_font is DynamicFont)
					graphics.texture = _texture;
				else
					graphics.texture = ((BitmapFont)_font).mainTexture;
			}
		}

		void BuildLines()
		{
			_textChanged = false;
			graphics.SetMeshDirty();
			_globalScale = UIContentScaler.scaleFactor;

			if (_font == null)
				ResolveFont();

			Cleanup();

			if (_html && _text.Length > 0)
				HtmlParser.inst.Parse(_text, _textFormat, _elements,
					_richTextField != null ? _richTextField.htmlParseOptions : null);

			if (_text.Length == 0 || _html && _elements.Count == 0)
			{
				LineInfo emptyLine = LineInfo.Borrow();
				emptyLine.width = emptyLine.height = 0;
				emptyLine.y = emptyLine.y2 = GUTTER_Y;
				_lines.Add(emptyLine);

				_textWidth = _textHeight = 0;
				_fontSizeScale = 1;

				BuildLinesFinal();

				return;
			}

			int letterSpacing = _textFormat.letterSpacing;
			int lineSpacing = _textFormat.lineSpacing - 1;
			float rectWidth = _contentRect.Width - GUTTER_X * 2;
			float glyphWidth = 0, glyphHeight = 0;
			short wordChars = 0;
			float wordStart = 0;
			bool wordPossible = false;
			int supSpace = 0, subSpace = 0;

			bool systemFont = _font is DynamicFont;
			TextFormat format = _textFormat;
			_font.SetFormat(format, _fontSizeScale);
			bool wrap = _wordWrap && !_singleLine;
			if (_input)
			{
				letterSpacing++;
				wrap = !_singleLine;
			}
			else
			{
				wrap = _wordWrap && !_singleLine;
				if (_maxWidth > 0)
				{
					wrap = true;
					rectWidth = _maxWidth - GUTTER_X * 2;
				}
			}
			_fontSizeScale = 1;

			int elementCount = _elements.Count;
			int elementIndex = 0;
			HtmlElement element = null;
			if (elementCount > 0)
				element = _elements[0];

			Graphics nativeGraphics = Graphics.FromHwnd(IntPtr.Zero);
			StringFormat stringFormat = new StringFormat();
			stringFormat.FormatFlags = StringFormatFlags.NoClip;
			if (!wrap)
				stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
			if (_input)
				stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
			int charactersFitte;
			int linesFilled;

			int lineIndex = 0;
			LineInfo line = LineInfo.Borrow();
			_lines.Add(line);
			line.y = line.y2 = GUTTER_Y;

			float lastLineHeight = 0;
			EventCallback0 startNewLine = () =>
			{
				if (line.width > _textWidth)
					_textWidth = line.width;

				if (line.textHeight == 0)
				{
					if (line.height == 0)
					{
						if (lastLineHeight == 0)
							line.height = format.size;
						else
							line.height = lastLineHeight;
					}
					line.textHeight = line.height;
				}
				lastLineHeight = line.height;

				if (supSpace > 0)
					line.height = Math.Max(line.textHeight + supSpace, line.height);
				if (subSpace > lineSpacing)
					supSpace = subSpace - lineSpacing;
				subSpace = 0;

				wordChars = 0;
				wordPossible = false;

				LineInfo newLine = LineInfo.Borrow();
				newLine.y = newLine.y2 = line.y + (line.height + lineSpacing);
				_lines.Add(newLine);
				line = newLine;
				lineIndex++;
			};

			string textBlock = _html ? null : _text;
			while (true)
			{
				if (element != null)
				{
					wordChars = 0;
					wordPossible = false;

					if (element.type == HtmlElementType.Text)
					{
						format = element.format;
						_font.SetFormat(format, _fontSizeScale);
						textBlock = element.text;

						RenderElement re = RenderElement.Borrow();
						re.lineIndex = lineIndex;
						re.element = element;
						_renderElements.Add(re);
					}
					else
					{
						IHtmlObject htmlObject = null;
						if (_richTextField != null)
						{
							element.space = (int)(rectWidth - line.width - 4);
							htmlObject = _richTextField.htmlPageContext.CreateObject(_richTextField, element);
							element.htmlObject = htmlObject;
						}
						if (htmlObject != null)
						{
							glyphWidth = (int)htmlObject.width;
							glyphHeight = (int)htmlObject.height;
						}
						else
							glyphWidth = 0;

						if (glyphWidth > 0)
						{
							glyphWidth += 3;

							if (line.width != 0)
								line.width += letterSpacing;
							line.width += glyphWidth;

							if (wrap && line.width > rectWidth && line.width > glyphWidth)
							{
								line.width -= (glyphWidth + letterSpacing);
								startNewLine();
								line.width = glyphWidth;
								line.height = glyphHeight;
							}
							else
							{
								if (glyphHeight > line.height)
									line.height = glyphHeight;
							}
						}

						RenderElement re = RenderElement.Borrow();
						re.lineIndex = lineIndex;
						re.element = element;
						_renderElements.Add(re);

						elementIndex++;
						if (elementIndex >= elementCount)
							break;

						element = _elements[elementIndex];
						continue;
					}
				}

				if (systemFont)
				{
					SizeF measureRect = new SizeF(wrap ? (_contentRect.Width - line.width) : int.MaxValue, format.size);
					Font measureFont = ((DynamicFont)_font).GetNativeFont(false);
					while (true)
					{
						SizeF measureRect2 = nativeGraphics.MeasureString(textBlock, measureFont, measureRect, stringFormat, out charactersFitte, out linesFilled);
						if (measureRect2.Width > measureRect.Width && charactersFitte == 1)
						{
							measureRect.Width = wrap ? _contentRect.Width : int.MaxValue;

							startNewLine();
							continue;
						}

						measureRect = measureRect2;
						measureRect.Width -= GUTTER_X * 2;
						measureRect.Height -= GUTTER_Y;

						if (measureRect.Height > line.textHeight)
							line.textHeight = measureRect.Height;

						if (measureRect.Height > line.height)
							line.height = measureRect.Height;

						if (line.width != 0)
							line.width += letterSpacing;
						line.width += (wrap ? Math.Min(rectWidth - line.width, measureRect.Width) : measureRect.Width);

						RenderElement re = RenderElement.Borrow();
						re.lineIndex = lineIndex;
						re.measureWidth = measureRect.Width;
						re.measureHeight = measureRect.Height;
						_renderElements.Add(re);

						if (charactersFitte == textBlock.Length)
						{
							if (textBlock[charactersFitte - 1] == '\n')
							{
								re.text = textBlock.Substring(0, charactersFitte - 1);
								startNewLine();
							}
							else
								re.text = textBlock;
							break;
						}

						if (textBlock[charactersFitte - 1] == '\n')
							re.text = textBlock.Substring(0, charactersFitte - 1);
						else
							re.text = textBlock.Substring(0, charactersFitte);
						textBlock = textBlock.Substring(charactersFitte);
						measureRect.Width = wrap ? _contentRect.Width : int.MaxValue;

						startNewLine();
					}
				}
				else
				{
					while (true)
					{
						int textLength = textBlock.Length;
						if (textLength == 0)
							break;

						RenderElement re = RenderElement.Borrow();
						re.lineIndex = lineIndex;
						re.text = textBlock;
						_renderElements.Add(re);

						int j = 0;
						for (; j < textLength; j++)
						{
							char ch = textBlock[j];
							if (ch == '\n')
							{
								wordChars = 0;
								wordPossible = false;

								re.text = textBlock.Substring(0, j);
								j++;
								startNewLine();
								break;
							}

							if (ch == ' ')
							{
								wordChars = 0;
								wordPossible = true;
							}
							else if (wordPossible && (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z'))
							{
								if (wordChars == 0)
									wordStart = line.width;
								else if (wordChars > 10)
									wordChars = short.MinValue;

								wordChars++;
							}
							else
							{
								wordChars = 0;
								wordPossible = false;
							}

							if (((BitmapFont)_font).GetGlyphSize(ch, out glyphWidth, out glyphHeight))
							{
								if (glyphHeight > line.textHeight)
									line.textHeight = glyphHeight;

								if (glyphHeight > line.height)
									line.height = glyphHeight;

								if (line.width != 0)
									line.width += letterSpacing;
								line.width += glyphWidth;

								if (format.specialStyle == TextFormat.SpecialStyle.Subscript)
									subSpace = (int)(glyphHeight * 0.333f);
								else if (format.specialStyle == TextFormat.SpecialStyle.Superscript)
									supSpace = (int)(glyphHeight * 0.333f);
							}

							if (wrap && line.width > rectWidth && format.specialStyle == TextFormat.SpecialStyle.None
								&& line.width > glyphWidth)
							{
								if (wordChars > 0 && wordStart > 0) //if word had broken, move it to new line
								{
									j -= wordChars;
									re.text = textBlock.Substring(0, j);
									line.width = wordStart;
									startNewLine();
								}
								else
								{
									re.text = textBlock.Substring(0, j);
									line.width -= (glyphWidth + letterSpacing);
									startNewLine();
									line.height = line.textHeight = glyphHeight;
									line.width = glyphWidth;
								}
								break;
							}
						}

						if (j < textLength)
							textBlock = textBlock.Substring(j);
						else
							break;
					}
				}

				elementIndex++;
				if (elementIndex >= elementCount)
					break;

				element = _elements[elementIndex];
			}

			nativeGraphics.Dispose();

			if (subSpace > 0)
				_lines[_lines.Count - 1].height += subSpace;

			if (line.width > _textWidth)
				_textWidth = line.width;
			if (_textWidth > 0)
				_textWidth += GUTTER_X * 2;

			line = _lines[_lines.Count - 1];
			_textHeight = line.y + line.height + GUTTER_Y;

			_textWidth = (int)Math.Ceiling(_textWidth);
			_textHeight = (int)Math.Ceiling(_textHeight);
			if (_autoSize == AutoSizeType.Shrink && _textWidth > rectWidth)
			{
				_fontSizeScale = rectWidth / _textWidth;
				_textWidth = rectWidth;
				_textHeight = (int)Math.Ceiling(_textHeight * _fontSizeScale);

				//调整各行的大小
				int lineCount = _lines.Count;
				for (int i = 0; i < lineCount; ++i)
				{
					line = _lines[i];
					line.y *= _fontSizeScale;
					line.y2 *= _fontSizeScale;
					line.height *= _fontSizeScale;
					line.width *= _fontSizeScale;
					line.textHeight *= _fontSizeScale;
				}
			}
			else
				_fontSizeScale = 1;

			BuildLinesFinal();
		}

		bool _updatingSize; //防止重复调用BuildLines
		void BuildLinesFinal()
		{
			if (!_input && _autoSize == AutoSizeType.Both)
			{
				_updatingSize = true;
				if (_richTextField != null)
					_richTextField.SetSize(_textWidth, _textHeight);
				else
					SetSize(_textWidth, _textHeight);
				_updatingSize = false;
			}
			else if (_autoSize == AutoSizeType.Height)
			{
				_updatingSize = true;
				float h = _textHeight;
				if (_input && h < _minHeight)
					h = _minHeight;
				if (_richTextField != null)
					_richTextField.height = h;
				else
					this.height = h;
				_updatingSize = false;
			}

			_yOffset = 0;
			ApplyVertAlign();

			if (_font is DynamicFont)
			{
				if (_textWidth > 0 && _textHeight > 0)
				{
					int canvasWidth = 0, canvasHeight = 0;
					canvasWidth = (int)Math.Ceiling(Math.Max(_textWidth, _contentRect.Width) * _globalScale);
					canvasHeight = (int)Math.Ceiling(Math.Max(_textHeight, _contentRect.Height) * _globalScale);
					if (_canvas == null || _canvas.Width < canvasWidth || _canvas.Height < canvasHeight)
					{
						if (_canvas != null)
							_canvas.Dispose();

						_canvas = new Bitmap(canvasWidth, canvasHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						if (_texture != null)
							_texture.Dispose();

						_texture = new NTexture(new Texture2D(Stage.game.GraphicsDevice, _canvas.Width, _canvas.Height, false, SurfaceFormat.Color));
						graphics.texture = _texture;
					}
				}
				else if (_texture == null)
				{
					if (_charPositions != null)
					{
						_charPositions.Clear();
						_charPositions.Add(new CharPosition());
					}

					if (_richTextField != null)
						_richTextField.RefreshObjects();
				}
			}
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			if (_textWidth == 0 && _lines.Count == 1)
			{
				if (_charPositions != null)
				{
					_charPositions.Clear();
					_charPositions.Add(new CharPosition());
				}

				if (_richTextField != null)
					_richTextField.RefreshObjects();

				return;
			}

			if (_font is DynamicFont)
				BuildMesh_systemFont(vb);
			else
				BuildMesh_bitmapFont(vb);

			if (_richTextField != null)
				_richTextField.RefreshObjects();
		}

		void BuildMesh_systemFont(VertexBuffer vb)
		{
			int letterSpacing = _textFormat.letterSpacing;
			float rectWidth = _contentRect.Width - GUTTER_X * 2;
			TextFormat format = _textFormat;
			_font.SetFormat(format, _fontSizeScale);
			Color color = format.color;
			if (_input)
				letterSpacing++;
			if (_charPositions != null)
				_charPositions.Clear();
			int charIndex = 0;

			HtmlLink currentLink = null;
			float linkStartX = 0;
			int linkStartLine = 0;

			float charX = 0;
			float xIndent;
			int yIndent = 0;
			bool clipped = !_input && _autoSize == AutoSizeType.None;
			bool lineClipped = false;
			AlignType lineAlign;

			Graphics nativeGraphics = null;
			StringFormat stringFormat = null;
			SolidBrush brush = null;
			SolidBrush shadowBrush = null;
			Pen outlinePen = null;
			GraphicsPath outlinePath = null;

			nativeGraphics = Graphics.FromImage(_canvas);
			nativeGraphics.Clear(System.Drawing.Color.Transparent);
			nativeGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

			stringFormat = new StringFormat();
			stringFormat.FormatFlags = StringFormatFlags.NoClip | StringFormatFlags.NoWrap;
			if (_input)
				stringFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

			brush = new SolidBrush(ToolSet.ToSystemColor(ref format.color));
			if (_stroke != 0)
			{
				nativeGraphics.SmoothingMode = SmoothingMode.AntiAlias;
				outlinePen = new Pen(ToolSet.ToSystemColor(ref _strokeColor), _stroke);
				outlinePath = new GraphicsPath();
			}

			if (_shadowOffset.X != 0 || _shadowOffset.Y != 0)
				shadowBrush = new SolidBrush(ToolSet.ToSystemColor(ref _strokeColor));

			int renderElementCount = _renderElements.Count;
			int lastLineIndex = -1;
			LineInfo line = null;
			for (int i = 0; i < renderElementCount; i++)
			{
				RenderElement re = _renderElements[i];
				HtmlElement element = re.element;

				if (re.lineIndex != lastLineIndex)
				{
					line = _lines[re.lineIndex];
					lastLineIndex = re.lineIndex;

					lineClipped = clipped && re.lineIndex != 0 && line.y + line.height > _contentRect.Height; //超出区域，剪裁
					lineAlign = format.align;
					if (element != null)
						lineAlign = element.format.align;
					else
						lineAlign = format.align;
					if (lineAlign == AlignType.Center)
						xIndent = (int)((rectWidth - line.width) / 2);
					else
					{
						if (lineAlign == AlignType.Right)
							xIndent = rectWidth - line.width;
						else
							xIndent = 0;
					}
					if (_input && xIndent < 0)
						xIndent = 0;
					charX = GUTTER_X + xIndent;

					if (re.lineIndex != 0 && _charPositions != null) //\n
					{
						CharPosition cp = new CharPosition();
						cp.charIndex = charIndex++;
						cp.lineIndex = (short)re.lineIndex;
						cp.offsetX = (int)charX;
						_charPositions.Add(cp);
					}
				}

				if (element != null)
				{
					if (element.type == HtmlElementType.Text)
					{
						format = element.format;
						_font.SetFormat(format, _fontSizeScale);
						color = format.color;
					}
					else if (element.type == HtmlElementType.Link)
					{
						currentLink = (HtmlLink)element.htmlObject;
						if (currentLink != null)
						{
							element.position = Vector2.Zero;
							currentLink.SetPosition(0, 0);
							linkStartX = charX;
							linkStartLine = re.lineIndex;
						}
					}
					else if (element.type == HtmlElementType.LinkEnd)
					{
						if (currentLink != null)
						{
							currentLink.SetArea(linkStartLine, linkStartX, re.lineIndex, charX);
							currentLink = null;
						}
					}
					else
					{
						IHtmlObject htmlObj = element.htmlObject;
						if (htmlObj != null)
						{
							element.position = new Vector2(charX + 1, line.y + (int)((line.height - htmlObj.height) / 2));
							htmlObj.SetPosition(element.position.X, element.position.Y);
							if (lineClipped || clipped && (element.position.X < GUTTER_X || element.position.X + htmlObj.width > _contentRect.Width - GUTTER_X))
								element.status |= 1;
							else
								element.status &= 254;
							charX += htmlObj.width + letterSpacing + 2;
						}
					}
				}
				else
				{
					if (!lineClipped)
					{
						Font measureFont = ((DynamicFont)_font).GetNativeFont(false);

						yIndent = (int)((line.height + line.textHeight) / 2 - re.measureHeight);
						float charX2 = charX - GUTTER_X;

						if (_charPositions != null)
						{
							int textLength = re.text.Length;
							List<CharacterRange> characterRanges = new List<CharacterRange>();
							for (int ti = 0; ti < textLength; ti++)
							{
								characterRanges.Add(new CharacterRange(ti, 1));
								if (characterRanges.Count == 32 || ti == textLength - 1)
								{
									stringFormat.SetMeasurableCharacterRanges(characterRanges.ToArray());
									Region[] regions = nativeGraphics.MeasureCharacterRanges(re.text, measureFont, new RectangleF(0, 0, int.MaxValue, int.MaxValue), stringFormat);
									foreach (Region region in regions)
									{
										RectangleF rectF = region.GetBounds(nativeGraphics);
										CharPosition cp = new CharPosition();
										cp.charIndex = charIndex++;
										cp.lineIndex = (short)re.lineIndex;
										cp.offsetX = (int)(charX2 + rectF.X);
										_charPositions.Add(cp);
									}
									characterRanges.Clear();
								}
							}
						}

						Font drawFont = ((DynamicFont)_font).GetNativeFont(true);
						brush.Color = ToolSet.ToSystemColor(ref format.color);
						if (shadowBrush != null)
							nativeGraphics.DrawString(re.text, drawFont, shadowBrush,
								(float)Math.Floor((charX2 + _shadowOffset.X) * _globalScale),
								(float)Math.Floor((line.y + yIndent + _shadowOffset.Y) * _globalScale),
								stringFormat);

						if (outlinePen == null)
						{
							nativeGraphics.DrawString(re.text, drawFont, brush,
								(float)Math.Floor(charX2 * _globalScale),
								(float)Math.Floor((line.y + yIndent) * _globalScale),
								stringFormat);
						}
						else
						{
							outlinePath.Reset();
							outlinePath.AddString(re.text, drawFont.FontFamily, (int)drawFont.Style, drawFont.Size,
								new PointF((float)Math.Floor(charX2 * _globalScale), (float)Math.Floor((line.y + yIndent) * _globalScale)),
								stringFormat);

							nativeGraphics.FillPath(brush, outlinePath);
							nativeGraphics.DrawPath(outlinePen, outlinePath);
						}
					}

					charX += letterSpacing + (int)Math.Ceiling(re.measureWidth);
				}
			}

			if (_charPositions != null)
			{
				CharPosition cp = new CharPosition();
				cp.charIndex = charIndex++;
				cp.lineIndex = (short)(_lines.Count - 1);
				cp.offsetX = (int)charX;
				_charPositions.Add(cp);
			}

			nativeGraphics.Dispose();
			brush.Dispose();
			if (outlinePen != null)
				outlinePen.Dispose();
			if (shadowBrush != null)
				shadowBrush.Dispose();

			byte[] data = _canvas.GetPixels();
			_texture.nativeTexture.SetData(data);

			int canvasWidth = 0, canvasHeight = 0;
			canvasWidth = (int)Math.Ceiling(Math.Max(_textWidth, _contentRect.Width) * _globalScale);
			canvasHeight = (int)Math.Ceiling(Math.Max(_textHeight, _contentRect.Height) * _globalScale);

			float h = (float)canvasWidth / _canvas.Height;
			vb.AddQuad(new Rectangle(0, 0, canvasWidth / _globalScale, canvasWidth / _globalScale), Color.White,
				new Rectangle(0, 1 - h, (float)canvasWidth / _canvas.Width, h));
			vb.AddTriangles();
		}

		void BuildMesh_bitmapFont(VertexBuffer vb)
		{
			int letterSpacing = _textFormat.letterSpacing;
			float rectWidth = _contentRect.Width - GUTTER_X * 2;
			TextFormat format = _textFormat;
			_font.SetFormat(format, _fontSizeScale);
			Color color = format.color;
			if (_input)
				letterSpacing++;
			if (_charPositions != null)
				_charPositions.Clear();
			int charIndex = 0;

			HtmlLink currentLink = null;
			float linkStartX = 0;
			int linkStartLine = 0;

			float charX = 0;
			float xIndent = 0;
			int yIndent = 0;
			bool clipped = !_input && _autoSize == AutoSizeType.None;
			bool lineClipped = false;
			AlignType lineAlign;
			float lastGlyphHeight = 0;
			Vector3 v0 = Vector3.Zero, v1 = Vector3.Zero;
			Vector2 u0, u1, u2, u3;
			bool canTint = ((BitmapFont)_font).colorEnabled;

			List<Vector3> vertList = vb.vertices;
			List<Vector2> uvList = vb.uv0;
			List<Color> colList = vb.colors;

			GlyphInfo glyph = new GlyphInfo();
			GlyphInfo lineGlyph = new GlyphInfo();

			if (_canvas != null)
			{
				_canvas.Dispose();
				_canvas = null;
			}
			if (_texture != null)
			{
				_texture.Dispose();
				_texture = null;
			}

			int renderElementCount = _renderElements.Count;
			int lastLineIndex = -1;
			LineInfo line = null;
			for (int i = 0; i < renderElementCount; i++)
			{
				RenderElement re = _renderElements[i];
				HtmlElement element = re.element;

				if (re.lineIndex != lastLineIndex)
				{
					line = _lines[re.lineIndex];
					lastLineIndex = re.lineIndex;

					lineClipped = clipped && re.lineIndex != 0 && line.y + line.height > _contentRect.Height; //超出区域，剪裁
					lineAlign = format.align;
					if (element != null)
						lineAlign = element.format.align;
					else
						lineAlign = format.align;
					if (lineAlign == AlignType.Center)
						xIndent = (int)((rectWidth - line.width) / 2);
					else
					{
						if (lineAlign == AlignType.Right)
							xIndent = rectWidth - line.width;
						else
							xIndent = 0;
					}
					if (_input && xIndent < 0)
						xIndent = 0;
					charX = GUTTER_X + xIndent;

					if (re.lineIndex != 0 && _charPositions != null) //\n
					{
						CharPosition cp = new CharPosition();
						cp.charIndex = charIndex++;
						cp.lineIndex = (short)re.lineIndex;
						cp.offsetX = (int)charX;
						_charPositions.Add(cp);
					}
				}

				if (element != null)
				{
					if (element.type == HtmlElementType.Text)
					{
						format = element.format;
						_font.SetFormat(format, _fontSizeScale);
						color = format.color;
					}
					else if (element.type == HtmlElementType.Link)
					{
						currentLink = (HtmlLink)element.htmlObject;
						if (currentLink != null)
						{
							element.position = Vector2.Zero;
							currentLink.SetPosition(0, 0);
							linkStartX = charX;
							linkStartLine = re.lineIndex;
						}
					}
					else if (element.type == HtmlElementType.LinkEnd)
					{
						if (currentLink != null)
						{
							currentLink.SetArea(linkStartLine, linkStartX, re.lineIndex, charX);
							currentLink = null;
						}
					}
					else
					{
						IHtmlObject htmlObj = element.htmlObject;
						if (htmlObj != null)
						{
							element.position = new Vector2(charX + 1, line.y + (int)((line.height - htmlObj.height) / 2));
							htmlObj.SetPosition(element.position.X, element.position.Y);
							if (lineClipped || clipped && (element.position.X < GUTTER_X || element.position.X + htmlObj.width > _contentRect.Width - GUTTER_X))
								element.status |= 1;
							else
								element.status &= 254;
							charX += htmlObj.width + letterSpacing + 2;
						}
					}
				}
				else
				{
					int textLength = re.text.Length;
					for (int j = 0; j < textLength; j++)
					{
						char ch = re.text[j];

						if (_charPositions != null)
						{
							CharPosition cp = new CharPosition();
							cp.charIndex = charIndex++;
							cp.lineIndex = (short)re.lineIndex;
							cp.offsetX = (int)charX;
							_charPositions.Add(cp);
						}

						if (((BitmapFont)_font).GetGlyph(ch, ref glyph))
						{
							if (lineClipped || clipped && (rectWidth < 7 || charX != (GUTTER_X + xIndent)) && charX + glyph.width > _contentRect.Width - GUTTER_X + 0.5f) //超出区域，剪裁
							{
								charX += letterSpacing + glyph.width;
								continue;
							}

							yIndent = (int)((line.height + line.textHeight) / 2 - glyph.height);
							if (format.specialStyle == TextFormat.SpecialStyle.Subscript)
								yIndent += (int)(glyph.height * 0.333f);
							else if (format.specialStyle == TextFormat.SpecialStyle.Superscript)
								yIndent -= (int)(lastGlyphHeight - glyph.height * 0.667f);
							else
								lastGlyphHeight = glyph.height;

							v0.X = charX + glyph.vertMin.X;
							v0.Y = line.y + yIndent + glyph.vertMin.Y;
							v1.X = charX + glyph.vertMax.X;
							v1.Y = line.y + yIndent + glyph.vertMax.Y;
							u0 = glyph.uvBottomLeft;
							u1 = glyph.uvTopLeft;
							u2 = glyph.uvTopRight;
							u3 = glyph.uvBottomRight;

							uvList.Add(u0);
							uvList.Add(u1);
							uvList.Add(u2);
							uvList.Add(u3);

							vertList.Add(new Vector3(v0.X, v1.Y, 0));
							vertList.Add(new Vector3(v0.X, v0.Y, 0));
							vertList.Add(new Vector3(v1.X, v0.Y, 0));
							vertList.Add(new Vector3(v1.X, v1.Y, 0));

							if (canTint)
							{
								colList.Add(color);
								colList.Add(color);
								colList.Add(color);
								colList.Add(color);
							}
							else
							{
								colList.Add(Color.White);
								colList.Add(Color.White);
								colList.Add(Color.White);
								colList.Add(Color.White);
							}

							if (format.underline)
							{
								if (((BitmapFont)_font).GetGlyph('_', ref lineGlyph))
								{
									//取中点的UV
									if (lineGlyph.uvBottomLeft.X != lineGlyph.uvBottomRight.X)
										u0.X = (lineGlyph.uvBottomLeft.X + lineGlyph.uvBottomRight.X) * 0.5f;
									else
										u0.X = (lineGlyph.uvBottomLeft.X + lineGlyph.uvTopLeft.X) * 0.5f;

									if (lineGlyph.uvBottomLeft.Y != lineGlyph.uvTopLeft.Y)
										u0.Y = (lineGlyph.uvBottomLeft.Y + lineGlyph.uvTopLeft.Y) * 0.5f;
									else
										u0.Y = (lineGlyph.uvBottomLeft.Y + lineGlyph.uvBottomRight.Y) * 0.5f;

									uvList.Add(u0);
									uvList.Add(u0);
									uvList.Add(u0);
									uvList.Add(u0);

									v0.Y = line.y + yIndent - lineGlyph.vertMin.Y + 1;
									v1.Y = line.y + yIndent - lineGlyph.vertMax.Y + 1;
									if (v0.Y - v1.Y > 2)
										v0.Y = v1.Y + 2;

									float tmpX = charX + letterSpacing + glyph.width;

									vertList.Add(new Vector3(charX, v0.Y, 0));
									vertList.Add(new Vector3(charX, v1.Y, 0));
									vertList.Add(new Vector3(tmpX, v1.Y, 0));
									vertList.Add(new Vector3(tmpX, v0.Y, 0));

									if (canTint)
									{
										colList.Add(color);
										colList.Add(color);
										colList.Add(color);
										colList.Add(color);
									}
									else
									{
										colList.Add(Color.White);
										colList.Add(Color.White);
										colList.Add(Color.White);
										colList.Add(Color.White);
									}
								}
								else
									format.underline = false;
							}

							if (_charPositions != null)
							{
								CharPosition cp = new CharPosition();
								cp.lineIndex = (short)i;
								cp.charIndex = _charPositions.Count;
								cp.offsetX = (int)charX;
								_charPositions.Add(cp);
							}

							charX += letterSpacing + glyph.width;
						}
						else //if GetGlyph failed
						{
							charX += letterSpacing;
						}
					}
				}
			}

			if (_charPositions != null)
			{
				CharPosition cp = new CharPosition();
				cp.charIndex = charIndex++;
				cp.lineIndex = (short)(_lines.Count - 1);
				cp.offsetX = (int)charX;
				_charPositions.Add(cp);
			}

			int count = vertList.Count;
			bool hasShadow = _shadowOffset.X != 0 || _shadowOffset.Y != 0;
			int allocCount = count;
			int drawDirs = 0;
			if (_stroke != 0)
			{
				drawDirs = 4;
				allocCount += count * drawDirs;
			}
			if (hasShadow)
				allocCount += count;

			if (allocCount != count)
			{
				VertexBuffer vb2 = VertexBuffer.Begin();
				List<Vector3> vertList2 = vb2.vertices;
				List<Vector2> uvList2 = vb2.uv0;
				List<Color> colList2 = vb2.colors;

				Color strokeColor = _strokeColor;
				if (_stroke != 0)
				{
					for (int j = 0; j < drawDirs; j++)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vert = vertList[i];
							Vector2 u = uvList[i];

							uvList2.Add(u);
							vertList2.Add(new Vector3(vert.X + STROKE_OFFSET[j * 2] * _stroke, vert.Y + STROKE_OFFSET[j * 2 + 1] * _stroke, 0));
							colList2.Add(strokeColor);
						}
					}
				}

				if (hasShadow)
				{
					for (int i = 0; i < count; i++)
					{
						Vector3 vert = vertList[i];
						Vector2 u = uvList[i];

						uvList2.Add(u);
						vertList2.Add(new Vector3(vert.X + _shadowOffset.X, vert.Y - _shadowOffset.Y, 0));
						colList2.Add(strokeColor);
					}
				}

				vb.Insert(vb2);
				vb2.End();
			}

			vb.AddTriangles();
		}

		void Cleanup()
		{
			if (_richTextField != null)
				_richTextField.CleanupObjects();

			HtmlElement.ReturnElements(_elements);
			LineInfo.Return(_lines);
			RenderElement.Return(_renderElements);
			if (_charPositions != null)
				_charPositions.Clear();

			_textWidth = 0;
			_textHeight = 0;
		}

		void ApplyVertAlign()
		{
			int oldOffset = _yOffset;
			if (_autoSize == AutoSizeType.Both || _autoSize == AutoSizeType.Height
				|| _verticalAlign == VertAlignType.Top)
				_yOffset = 0;
			else
			{
				float dh;
				if (_textHeight == 0)
					dh = _contentRect.Height - this.textFormat.size;
				else
					dh = _contentRect.Height - _textHeight;
				if (dh < 0)
					dh = 0;
				if (_verticalAlign == VertAlignType.Middle)
					_yOffset = (int)(dh / 2);
				else
					_yOffset = (int)dh;
			}

			if (oldOffset != _yOffset)
			{
				int cnt = _lines.Count;
				for (int i = 0; i < cnt; i++)
					_lines[i].y = _lines[i].y2 + _yOffset;

				graphics.SetMeshDirty();
			}
		}

		public class LineInfo
		{
			/// <summary>
			/// 行的宽度
			/// </summary>
			public float width;

			/// <summary>
			/// 行的高度
			/// </summary>
			public float height;

			/// <summary>
			/// 行内文本的高度
			/// </summary>
			public float textHeight;

			/// <summary>
			/// 行的y轴位置
			/// </summary>
			public float y;

			/// <summary>
			/// 行的y轴位置的备份
			/// </summary>
			public float y2;

			static Stack<LineInfo> pool = new Stack<LineInfo>();

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public static LineInfo Borrow()
			{
				if (pool.Count > 0)
				{
					LineInfo ret = pool.Pop();
					ret.width = 0;
					ret.height = 0;
					ret.textHeight = 0;
					ret.y = 0;
					ret.y2 = 0;
					return ret;
				}
				else
					return new LineInfo();
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="value"></param>
			public static void Return(LineInfo value)
			{
				pool.Push(value);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="values"></param>
			public static void Return(List<LineInfo> values)
			{
				int cnt = values.Count;
				for (int i = 0; i < cnt; i++)
					pool.Push(values[i]);

				values.Clear();
			}
		}

		public class RenderElement
		{
			public int lineIndex;

			//text block
			public string text;
			public float measureWidth;
			public float measureHeight;

			//html element
			public HtmlElement element;

			static Stack<RenderElement> pool = new Stack<RenderElement>();

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public static RenderElement Borrow()
			{
				if (pool.Count > 0)
					return pool.Pop();
				else
					return new RenderElement();
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="value"></param>
			public static void Return(RenderElement value)
			{
				value.lineIndex = 0;
				value.measureWidth = value.measureHeight = 0;
				value.text = null;
				value.element = null;
				pool.Push(value);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="values"></param>
			public static void Return(List<RenderElement> values)
			{
				int cnt = values.Count;
				for (int i = 0; i < cnt; i++)
				{
					RenderElement value = values[i];
					value.lineIndex = 0;
					value.measureWidth = value.measureHeight = 0;
					value.text = null;
					value.element = null;
					pool.Push(value);
				}

				values.Clear();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public struct CharPosition
		{
			/// <summary>
			/// 字符索引
			/// </summary>
			public int charIndex;

			/// <summary>
			/// 字符所在的行索引
			/// </summary>
			public short lineIndex;

			/// <summary>
			/// 字符的x偏移
			/// </summary>
			public int offsetX;
		}
	}
}
