using Microsoft.Xna.Framework;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class TextFormat
	{
		public enum SpecialStyle
		{
			None,
			Superscript,
			Subscript
		}

		/// <summary>
		/// 
		/// </summary>
		public int size;

		/// <summary>
		/// 
		/// </summary>
		public string font;

		/// <summary>
		/// 
		/// </summary>
		public Color color;

		/// <summary>
		/// 
		/// </summary>
		public int lineSpacing;

		/// <summary>
		/// 
		/// </summary>
		public int letterSpacing;

		/// <summary>
		/// 
		/// </summary>
		public bool bold;

		/// <summary>
		/// 
		/// </summary>
		public bool underline;

		/// <summary>
		/// 
		/// </summary>
		public bool italic;

		/// <summary>
		/// 
		/// </summary>
		public AlignType align;

		/// <summary>
		/// 
		/// </summary>
		public SpecialStyle specialStyle;

		public TextFormat()
		{
			color = Color.Black;
			size = 12;
			lineSpacing = 3;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void SetColor(uint value)
		{
			int r = (int)((value >> 16) & 0x0000ff);
			int g = (int)((value >> 8) & 0x0000ff);
			int b = (int)(value & 0x0000ff);
			color = new Color(r, g, b, 255);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aFormat"></param>
		/// <returns></returns>
		public bool EqualStyle(TextFormat aFormat)
		{
			return size == aFormat.size && color.A == aFormat.color.A
				&& color.R == aFormat.color.R
				&& color.G == aFormat.color.G
				&& color.B == aFormat.color.B
				&& bold == aFormat.bold && underline == aFormat.underline
				&& italic == aFormat.italic
				&& align == aFormat.align
				&& specialStyle == aFormat.specialStyle;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		public void CopyFrom(TextFormat source)
		{
			this.size = source.size;
			this.font = source.font;
			this.color = source.color;
			this.lineSpacing = source.lineSpacing;
			this.letterSpacing = source.letterSpacing;
			this.bold = source.bold;
			this.underline = source.underline;
			this.italic = source.italic;
			this.align = source.align;
			this.specialStyle = source.specialStyle;
		}
	}
}
