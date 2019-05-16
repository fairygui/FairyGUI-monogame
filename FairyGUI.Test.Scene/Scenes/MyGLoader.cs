using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using FairyGUI.Utils;

namespace FairyGUI.Test.Scenes
{
	public class MyGLoader : GLoader
	{
		protected override void LoadExternal()
		{
			string file = Path.Combine("Icons", this.url);
			try
			{
				Texture2D tex = Stage.game.Content.Load<Texture2D>(file);
				onExternalLoadSuccess(new NTexture(tex));
			}
			catch (Exception e)
			{
				Log.Info("LoadExternal failed: " + file);
			}
		}

		protected override void FreeExternal(NTexture texture)
		{
		}
	}
}