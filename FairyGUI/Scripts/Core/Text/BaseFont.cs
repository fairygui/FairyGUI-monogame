using Microsoft.Xna.Framework;

namespace FairyGUI
{
	/// <summary>
	/// Base class for all kind of fonts. 
	/// </summary>
	public class BaseFont
	{
		/// <summary>
		/// The name of this font object.
		/// </summary>
		public string name { get; protected set; }

		virtual public void SetFormat(TextFormat format, float fontSizeScale)
		{
		}

		public BaseFont()
		{
		}
	}

	/// <summary>
	/// Character info.
	/// </summary>
	public struct GlyphInfo
	{
		public Vector2 vertMin;
		public Vector2 vertMax;
		public Vector2 uvBottomLeft;
		public Vector2 uvTopLeft;
		public Vector2 uvTopRight;
		public Vector2 uvBottomRight;
		public float width;
		public float height;
		public int channel;
	}
}
