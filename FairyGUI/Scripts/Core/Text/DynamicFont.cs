using System;
using System.Collections.Generic;
using System.Drawing;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class DynamicFont : BaseFont
	{
		int _size;
		FontStyle _style;

		static Dictionary<int, Font> sFontCache = new Dictionary<int, Font>();

		public DynamicFont(string name)
		{
			this.name = name;
		}

		override public void SetFormat(TextFormat format, float fontSizeScale)
		{
			if (fontSizeScale == 1)
				_size = format.size;
			else
				_size = (int)Math.Floor((float)format.size * fontSizeScale);

			_style = FontStyle.Regular;
			if (format.bold)
				_style |= FontStyle.Bold;
			if (format.italic)
				_style |= FontStyle.Italic;
			if (format.underline)
				_style |= FontStyle.Underline;
		}

		public Font GetNativeFont(bool applyGlobalScale)
		{
			int size = applyGlobalScale ? (int)Math.Floor(_size * UIContentScaler.scaleFactor) : _size;

			int key = (size << 16) + (int)_style;
			Font result;
			if (!sFontCache.TryGetValue(key, out result))
			{
				result = new Font(this.name, size, _style, GraphicsUnit.Pixel);
				sFontCache[key] = result;
			}

			return result;
		}
	}
}
