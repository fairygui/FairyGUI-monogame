using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace FairyGUI
{
	/// <summary>
	/// Global configs. These options should be set before any UI construction.
	/// </summary>
	public class UIConfig
	{
		/// <summary>
		/// Dynamic Font Support. 
		/// set defaultFont to system font name(or names joint with comma). e.g. defaultFont="Microsoft YaHei, SimHei"
		/// </summary>
		public static string defaultFont = "Arial";

		/// <summary>
		/// Resource using in Window.ShowModalWait for locking the window.
		/// </summary>
		public static string windowModalWaiting;

		/// <summary>
		/// Resource using in GRoot.ShowModalWait for locking the screen.
		/// </summary>
		public static string globalModalWaiting;

		/// <summary>
		/// When a modal window is in front, the background becomes dark.
		/// </summary>
		public static Color modalLayerColor = new Color(0f, 0f, 0f, 0.4f);

		/// <summary>
		/// Default button click sound.
		/// </summary>
		public static SoundEffectInstance buttonSound;

		/// <summary>
		/// Default button click sound volume.
		/// </summary>
		public static float buttonSoundVolumeScale = 1f;

		/// <summary>
		/// Resource url of horizontal scrollbar
		/// </summary>
		public static string horizontalScrollBar;

		/// <summary>
		/// Resource url of vertical scrollbar
		/// </summary>
		public static string verticalScrollBar;

		/// <summary>
		/// Scrolling step in pixels
		/// 当调用ScrollPane.scrollUp/Down/Left/Right时，或者点击滚动条的上下箭头时，滑动的距离。
		/// 鼠标滚轮触发一次滚动的距离设定为defaultScrollStep*2
		/// </summary>
		public static float defaultScrollStep = 25;

		/// <summary>
		/// Deceleration ratio of scrollpane when its in touch dragging.
		/// 当手指拖动并释放滚动区域后，内容会滑动一定距离后停下，这个速率就是减速的速率。
		/// 越接近1，减速越慢，意味着滑动的时间和距离更长。
		/// 这个是全局设置，也可以通过ScrollPane.decelerationRate进行个性设置。
		/// </summary>
		public static float defaultScrollDecelerationRate = 0.967f;

		/// <summary>
		/// Scrollbar display mode. Recommended 'Auto' for mobile and 'Visible' for web.
		/// </summary>
		public static ScrollBarDisplayType defaultScrollBarDisplay = ScrollBarDisplayType.Default;

		/// <summary>
		/// Allow dragging anywhere in container to scroll.
		/// </summary>
		public static bool defaultScrollTouchEffect = true;

		/// <summary>
		/// The "rebound" effect in the scolling container.
		/// </summary> 
		public static bool defaultScrollBounceEffect = true;

		/// <summary>
		/// Resources url of PopupMenu.
		/// </summary>
		public static string popupMenu;

		/// <summary>
		/// Resource url of menu seperator.
		/// </summary>
		public static string popupMenu_seperator;

		/// <summary>
		/// In case of failure of loading content for GLoader, use this sign to indicate an error.
		/// </summary>
		public static string loaderErrorSign;

		/// <summary>
		/// Resource url of tooltips.
		/// </summary>
		public static string tooltipsWin;

		/// <summary>
		/// The number of visible items in ComboBox.
		/// </summary>
		public static int defaultComboBoxVisibleItemCount = 10;

		/// <summary>
		/// Pixel offsets of finger to trigger scrolling
		/// </summary>
		public static int touchScrollSensitivity = 20;

		/// <summary>
		/// Pixel offsets of finger to trigger dragging
		/// </summary>
		public static int touchDragSensitivity = 10;

		/// <summary>
		/// Pixel offsets of mouse pointer to trigger dragging.
		/// </summary>
		public static int clickDragSensitivity = 2;

		/// <summary>
		/// Allow softness on top or left side for scrollpane.
		/// </summary>
		public static bool allowSoftnessOnTopOrLeftSide = true;

		/// <summary>
		/// When click the window, brings to front automatically.
		/// </summary>
		public static bool bringWindowToFrontOnClick = true;

		/// <summary>
		/// 
		/// </summary>
		public static int inputCaretSize = 1;

		/// <summary>
		/// 
		/// </summary>
		public static Color inputHighlightColor = new Color(1, 0.875f, 0.553f, 0.5f);

		/// <summary>
		/// 
		/// </summary>
		public static float frameTimeForAsyncUIConstruction = 0.002f;

		/// <summary>
		/// if RenderTexture using in paiting mode has depth support.
		/// </summary>
		public static bool depthSupportForPaintingMode = false;
	}
}
