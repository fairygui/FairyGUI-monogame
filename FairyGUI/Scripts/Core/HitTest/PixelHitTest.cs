﻿using System;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PixelHitTestData
	{
		public int pixelWidth;
		public float scale;
		public byte[] pixels;

		public void Load(ByteBuffer ba)
		{
			ba.ReadInt();
			pixelWidth = ba.ReadInt();
			scale = 1.0f / ba.ReadByte();
			int len = ba.ReadInt();
			pixels = new byte[len];
			for (int i = 0; i < len; i++)
				pixels[i] = ba.ReadByte();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class PixelHitTest : IHitTest
	{
		public int offsetX;
		public int offsetY;
		public float sourceWidth;
		public float sourceHeight;

		PixelHitTestData _data;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offsetX"></param>
		/// <param name="offsetY"></param>
		public PixelHitTest(PixelHitTestData data, int offsetX, int offsetY, float sourceWidth, float sourceHeight)
		{
			_data = data;
			this.offsetX = offsetX;
			this.offsetY = offsetY;
			this.sourceWidth = sourceWidth;
			this.sourceHeight = sourceHeight;
		}

		public bool HitTest(Rectangle contentRect, Vector2 localPoint)
		{
			int x = (int)Math.Floor((localPoint.X * sourceWidth / contentRect.Width - offsetX) * _data.scale);
			int y = (int)Math.Floor((localPoint.Y * sourceHeight / contentRect.Height - offsetY) * _data.scale);
			if (x < 0 || y < 0 || x >= _data.pixelWidth)
				return false;

			int pos = y * _data.pixelWidth + x;
			int pos2 = pos / 8;
			int pos3 = pos % 8;

			if (pos2 >= 0 && pos2 < _data.pixels.Length)
				return ((_data.pixels[pos2] >> pos3) & 0x1) > 0;
			else
				return false;
		}
	}
}
