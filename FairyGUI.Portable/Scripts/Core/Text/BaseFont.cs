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
	public class GlyphInfo
	{
		public Rectangle vert;
		public Vector2[] uv = new Vector2[4];
		public float width;
		public float height;
		public int channel;
	}
}
