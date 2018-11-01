using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FairyGUI.Utils
{
    public static class BitmapExtentions
    {
        public static int GetBPP(this Bitmap bmp)
        {
            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppArgb:
                    return 4;
                default:
                    return 0;
            }
        }

        public static byte[] GetPixels(this Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bitmapdata = bmp.LockBits(rect, ImageLockMode.ReadOnly, bmp.PixelFormat);
            int length = Math.Abs(bitmapdata.Stride) * bmp.Height;
            byte[] pix = new byte[length];
            Marshal.Copy(bitmapdata.Scan0, pix, 0, length);
            bmp.UnlockBits(bitmapdata);
            int bpp = bmp.GetBPP();
            Action<int> action1 = (Action<int>)(ofs =>
            {
                byte num = pix[ofs];
                pix[ofs] = pix[ofs + 2];
                pix[ofs + 2] = num;
            });
            Action<int> action2 = bpp != 4 ? (Action<int>)null : action1;
            if (action2 == null)
                throw new Exception("Unsupported Pixelformat");
            int num1 = 0;
            while (num1 < length)
            {
                action2(num1);
                num1 += bpp;
            }
            return pix;
        }
    }
}