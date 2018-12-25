#if DesktopGL

using System;
using System.Runtime.InteropServices;

namespace FairyGUI.SDL
{
	/// <summary>
	/// 
	/// </summary>
	public static class SDLNative
	{
		private const string nativeLibName = "x86/SDL2";

#region SDL_keyboard.h
		/// <summary>
		/// joystick 是指SDL_Joystick *
		/// 此功能仅适用于2.0.6或更高版本
		/// </summary>
		/// <param name="joystick"></param>
		/// <param name="axis"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_JoystickGetAxisInitialState(
			IntPtr joystick,
			int axis,
			out ushort state
		);

		/// <summary>
		/// 开始接受Unicode文本输入事件，显示键盘
		/// </summary>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_StartTextInput();

		/// <summary>
		/// 检查是否启用了unicode输入事件
		/// </summary>
		/// <returns></returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IsTextInputActive();

		/// <summary>
		/// 停止接收任何文本输入事件，隐藏屏幕上的kbd
		/// </summary>

		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_StopTextInput();

		/// <summary>
		/// 设置用于文本输入的矩形，提示IME
		/// </summary>
		/// <param name="rect"></param>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SDL_SetTextInputRect(ref SDL_Rect rect);

		/// <summary>
		/// 平台是否支持屏幕键盘？
		/// </summary>
		/// <returns></returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_HasScreenKeyboardSupport();

		/// <summary>
		/// 是否为给定窗口显示了屏幕键盘？
		/// window是一个SDL_Window指针
		/// </summary>
		/// <param name="window"></param>
		/// <returns></returns>
		[DllImport(nativeLibName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SDL_bool SDL_IsScreenKeyboardShown(IntPtr window);

		#endregion

		#region SDL_stdinc.h
		/// <summary>
		/// 
		/// </summary>
		public enum SDL_bool
		{
			/// <summary>
			/// 
			/// </summary>
			SDL_FALSE = 0,
			/// <summary>
			/// 
			/// </summary>
			SDL_TRUE = 1
		}
		#endregion

		#region SDL_rect.h
		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct SDL_Rect
		{
			/// <summary>
			/// 
			/// </summary>
			public int x;
			/// <summary>
			/// 
			/// </summary>
			public int y;
			/// <summary>
			/// 
			/// </summary>
			public int w;
			/// <summary>
			/// 
			/// </summary>
			public int h;
		}
		#endregion
	}



}

#endif