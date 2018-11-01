using Microsoft.Xna.Framework;

namespace FairyGUI
{
	public class UIContentScaler
	{
		/// <summary>
		/// 
		/// </summary>
		public enum ScreenMatchMode
		{
			MatchWidthOrHeight,
			MatchWidth,
			MatchHeight
		}

		/// <summary>
		/// 
		/// </summary>
		public static float scaleFactor = 1;

		/// <summary>
		/// 
		/// </summary>
		static ScreenMatchMode screenMatchMode;

		/// <summary>
		/// 
		/// </summary>
		static int designResolutionX;

		/// <summary>
		/// 
		/// </summary>
		static int designResolutionY;

		static bool constantScaleFactor = true;

		public static void SetContentScaleFactor(float scaleFactor)
		{
			UIContentScaler.scaleFactor = scaleFactor;
			constantScaleFactor = true;

			ApplyChange();
		}

		/// <summary>
		/// Set content scale factor.
		/// </summary>
		/// <param name="designResolutionX">Design resolution of x axis.</param>
		/// <param name="designResolutionY">Design resolution of y axis.</param>
		public static void SetContentScaleFactor(int designResolutionX, int designResolutionY)
		{
			SetContentScaleFactor(designResolutionX, designResolutionY, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
		}

		/// <summary>
		/// Set content scale factor.
		/// </summary>
		/// <param name="designResolutionX">Design resolution of x axis.</param>
		/// <param name="designResolutionY">Design resolution of y axis.</param>
		/// <param name="screenMatchMode">Match mode.</param>
		public static void SetContentScaleFactor(int designResolutionX, int designResolutionY, UIContentScaler.ScreenMatchMode screenMatchMode)
		{
			UIContentScaler.designResolutionX = designResolutionX;
			UIContentScaler.designResolutionY = designResolutionY;
			UIContentScaler.screenMatchMode = screenMatchMode;
			constantScaleFactor = false;

			ApplyChange();
		}


		/// <summary>
		/// 
		/// </summary>
		internal static void ApplyChange()
		{
			if (!constantScaleFactor)
			{
				if (designResolutionX == 0 || designResolutionY == 0)
					return;

				int dx = designResolutionX;
				int dy = designResolutionY;

				if (screenMatchMode == ScreenMatchMode.MatchWidthOrHeight)
				{
					float s1 = (float)Stage.inst.width / dx;
					float s2 = (float)Stage.inst.height / dy;
					scaleFactor = MathHelper.Min(s1, s2);
				}
				else if (screenMatchMode == ScreenMatchMode.MatchWidth)
					scaleFactor = (float)Stage.inst.width / dx;
				else
					scaleFactor = (float)Stage.inst.height / dy;

				if (scaleFactor > 10)
					scaleFactor = 10;
			}

			int cnt = Stage.inst.numChildren;
			for (int i = 0; i < cnt; i++)
			{
				DisplayObject obj = Stage.inst.GetChildAt(i);
				if (obj.gOwner is GRoot)
					((GRoot)obj.gOwner).ApplyContentScaleFactor();
			}
		}
	}
}
