using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace FairyGUI.Scripts.Core.Text
{
	public sealed class WindowInputCapturer : NativeWindow, IDisposable, IInputHandler
	{
		public List<ICharacter> myCharacters { get; set; }

		private Game game;

		private const int DLGC_WANTCHARS = 0x0080;

		private const int DLGC_WANTALLKEYS = 0x0004;

		IntPtr context = IntPtr.Zero;

		private enum WindowMessages : int
		{
			WM_GETDLGCODE = 0x0087,
			WM_CHAR = 0x0102,
		}

		public WindowInputCapturer(IntPtr windowHandle, Game game)
		{
			AssignHandle(windowHandle);
			this.game = game;

			myCharacters = new List<ICharacter>();
		}

		protected override void WndProc(ref Message message)
		{
			if (message.Msg == WindowMessage.InputLanguageChange)
			{
				return;
			}

			if (message.Msg == WindowMessage.ImeSetContext)
			{
				if (message.WParam.ToInt32() == 1)
				{
					IntPtr imeContext = IMM.ImmGetContext(this.Handle);
					if (context == IntPtr.Zero)
						context = imeContext;
					IMM.ImmAssociateContext(this.Handle, context);
				}
			}
			base.WndProc(ref message);

			switch (message.Msg)
			{
				case (int)WindowMessages.WM_GETDLGCODE:
					{
						if (Is32Bit)
						{
							int returnCode = message.Result.ToInt32();
							returnCode |= (DLGC_WANTALLKEYS | DLGC_WANTCHARS);
							message.Result = new IntPtr(returnCode);
						}
						else
						{
							long returnCode = message.Result.ToInt64();
							returnCode |= (DLGC_WANTALLKEYS | DLGC_WANTCHARS);
							message.Result = new IntPtr(returnCode);
						}

						break;
					}
				case (int)WindowMessages.WM_CHAR:
					{
						int charInt = message.WParam.ToInt32();
						Character myCharacter = new Character();
						myCharacter.IsUsed = false;
						myCharacter.Chars = (char)charInt;
						//汉字的unicode编码范围是4e00-9fa5（19968至40869）
						//全/半角标点可以查看charInt输出
						switch (charInt)
						{
							case 8:
								myCharacter.CharacterType = (int)CharacterType.BackSpace;
								break;
							case 9:
								myCharacter.CharacterType = (int)CharacterType.Tab;
								break;
							case 13:
								myCharacter.CharacterType = (int)CharacterType.Enter;
								break;
							case 27:
								myCharacter.CharacterType = (int)CharacterType.Esc;
								break;
							default:
								myCharacter.CharacterType = (int)CharacterType.Char;
								break;
						}
						myCharacters.Add(myCharacter);
						break;
					}


			}

		}

		private static bool Is32Bit
		{
			get { return (IntPtr.Size == 4); }
		}
		private bool disposed;

		public void Dispose()
		{

			if (!this.disposed)
			{
				ReleaseHandle();
				this.disposed = true;
			}
		}
	}
}