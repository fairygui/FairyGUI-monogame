using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public class HtmlParseOptions
	{
		/// <summary>
		/// 
		/// </summary>
		public bool linkUnderline;

		/// <summary>
		/// 
		/// </summary>
		public Color linkColor;

		/// <summary>
		/// 
		/// </summary>
		public Color linkBgColor;

		/// <summary>
		/// 
		/// </summary>
		public Color linkHoverBgColor;

		/// <summary>
		/// 
		/// </summary>
		public bool ignoreWhiteSpace;

		/// <summary>
		/// 
		/// </summary>
		public static bool DefaultLinkUnderline = true;

		/// <summary>
		/// 
		/// </summary>
		public static Color DefaultLinkColor = new Color(0.227f, 0.404f, 0.8f, 1);

		/// <summary>
		/// 
		/// </summary>
		public static Color DefaultLinkBgColor = Color.Transparent;

		/// <summary>
		/// 
		/// </summary>
		public static Color DefaultLinkHoverBgColor = Color.Transparent;

		public HtmlParseOptions()
		{
			linkUnderline = DefaultLinkUnderline;
			linkColor = DefaultLinkColor;
			linkBgColor = DefaultLinkBgColor;
			linkHoverBgColor = DefaultLinkHoverBgColor;
		}
	}
}
