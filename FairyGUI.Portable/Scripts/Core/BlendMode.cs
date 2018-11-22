using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public enum BlendMode
	{
		Normal,
		None,
		Add,
		Multiply,
		Screen,
		Erase,
		Mask,
		Below,
		Off,
		Custom1,
		Custom2,
		Custom3
	}

	/// <summary>
	/// 
	/// </summary>
	public class BlendModeUtils
	{
		//Source指的是被计算的颜色，Destination是已经在屏幕上的颜色。
		//混合结果=Source * factor1 + Destination * factor2
		public static BlendState[] blendStates = new BlendState[] {
			//Normal
			BlendState.NonPremultiplied,

			//None
			new BlendState()
			{
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
				ColorDestinationBlend = Blend.One,
				AlphaDestinationBlend = Blend.One,
			},

			//Add
			BlendState.Additive,

			//Multiply
			new BlendState()
			{
				ColorSourceBlend = Blend.DestinationColor,
				AlphaSourceBlend = Blend.DestinationColor,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaDestinationBlend = Blend.InverseSourceAlpha,
			},

			//Screen
			new BlendState()
			{
				ColorSourceBlend = Blend.One,
				AlphaSourceBlend = Blend.One,
				ColorDestinationBlend = Blend.InverseSourceColor,
				AlphaDestinationBlend = Blend.InverseSourceColor,
			},

			//Erase
			new BlendState()
			{
				ColorSourceBlend = Blend.Zero,
				AlphaSourceBlend = Blend.Zero,
				ColorDestinationBlend = Blend.InverseSourceAlpha,
				AlphaDestinationBlend = Blend.InverseSourceAlpha,
			},

			//Mask,
			new BlendState()
			{
				ColorSourceBlend = Blend.Zero,
				AlphaSourceBlend = Blend.Zero,
				ColorDestinationBlend = Blend.SourceAlpha,
				AlphaDestinationBlend = Blend.SourceAlpha,
			},

			//Below
			new BlendState()
			{
				ColorSourceBlend = Blend.InverseDestinationAlpha,
				AlphaSourceBlend = Blend.InverseDestinationAlpha,
				ColorDestinationBlend = Blend.DestinationAlpha,
				AlphaDestinationBlend = Blend.DestinationAlpha,
			},

			//Off
			BlendState.Opaque,
			//Custom1
			BlendState.AlphaBlend,
			//Custom2
			BlendState.AlphaBlend,
			//Custom3
			BlendState.AlphaBlend,
		};

		/// <summary>
		/// 
		/// </summary>
		/// <param name="blendMode"></param>
		/// <param name="srcFactor"></param>
		/// <param name="dstFactor"></param>
		public static void Override(BlendMode blendMode, Blend source, Blend dst)
		{
			blendStates[(int)blendMode] = new BlendState()
			{
				ColorSourceBlend = source,
				AlphaSourceBlend = source,
				ColorDestinationBlend = dst,
				AlphaDestinationBlend = dst,
			};
		}
	}
}
