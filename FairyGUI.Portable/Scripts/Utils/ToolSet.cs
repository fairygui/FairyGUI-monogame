using System;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI.Utils
{
	/// <summary>
	/// 
	/// </summary>
	public static class ToolSet
	{
		public static Color ConvertFromHtmlColor(string str)
		{
			if (str.Length < 7 || str[0] != '#')
				return Color.Black;

			if (str.Length == 9)
			{
				return new Color((CharToHex(str[3]) * 16 + CharToHex(str[4])),
					(CharToHex(str[5]) * 16 + CharToHex(str[6])),
					(CharToHex(str[7]) * 16 + CharToHex(str[8])),
					(CharToHex(str[1]) * 16 + CharToHex(str[2])));
			}
			else
			{
				return new Color((CharToHex(str[1]) * 16 + CharToHex(str[2])),
					(CharToHex(str[3]) * 16 + CharToHex(str[4])),
					(CharToHex(str[5]) * 16 + CharToHex(str[6])),
					255);
			}
		}

		public static Color ColorFromRGB(int value)
		{
			return new Color(((value >> 16) & 0xFF), ((value >> 8) & 0xFF), (value & 0xFF), 255);
		}

		public static Color ColorFromRGBA(int value)
		{
			return new Color(((value >> 16) & 0xFF), ((value >> 8) & 0xFF), (value & 0xFF), ((value >> 24) & 0xFF));
		}

		public static System.Drawing.Color ToSystemColor(ref Color color)
		{
			return System.Drawing.Color.FromArgb((int)(color.A), (int)(color.R), (int)(color.G),
				(int)(color.B));
		}

		public static int CharToHex(char c)
		{
			if (c >= '0' && c <= '9')
				return (int)c - 48;
			if (c >= 'A' && c <= 'F')
				return 10 + (int)c - 65;
			else if (c >= 'a' && c <= 'f')
				return 10 + (int)c - 97;
			else
				return 0;
		}

		public static void FlipInnerRect(float sourceWidth, float sourceHeight, ref Rectangle rect, FlipType flip)
		{
			if (flip == FlipType.Horizontal || flip == FlipType.Both)
			{
				rect.X = sourceWidth - rect.X - rect.Width;
			}

			if (flip == FlipType.Vertical || flip == FlipType.Both)
			{
				rect.Y = sourceHeight - rect.Y - rect.Height;
			}
		}

		public static void FlipRect(ref Rectangle rect, FlipType flip)
		{
			if (flip == FlipType.Horizontal || flip == FlipType.Both)
			{
				float tmp = rect.X;
				rect.X = rect.X + rect.Width;
				rect.Width = tmp - rect.X;
			}
			if (flip == FlipType.Vertical || flip == FlipType.Both)
			{
				float tmp = rect.Y;
				rect.Y = rect.Y + rect.Height;
				rect.Height = tmp - rect.Y;
			}
		}

		public static Rectangle Intersection(ref Rectangle rect1, ref Rectangle rect2)
		{
			if (rect1.Width == 0 || rect1.Height == 0 || rect2.Width == 0 || rect2.Height == 0)
				return new Rectangle(0, 0, 0, 0);

			float left = rect1.X > rect2.X ? rect1.X : rect2.X;
			float right = (rect1.X + rect1.Width) < (rect2.X + rect2.Width) ? (rect1.X + rect1.Width) : (rect2.X + rect2.Width);
			float top = rect1.Y > rect2.Y ? rect1.Y : rect2.Y;
			float bottom = (rect1.Y + rect1.Height) < (rect2.Y + rect2.Height) ? (rect1.Y + rect1.Height) : (rect2.Y + rect2.Height);

			if (left > right || top > bottom)
				return new Rectangle(0, 0, 0, 0);
			else
				return new Rectangle(left, top, right - left, bottom - top);
		}

		public static Rectangle Union(ref Rectangle rect1, ref Rectangle rect2)
		{
			if (rect2.Width == 0 || rect2.Height == 0)
				return rect1;

			if (rect1.Width == 0 || rect1.Height == 0)
				return rect2;

			float x = MathHelper.Min(rect1.X, rect2.X);
			float y = MathHelper.Min(rect1.Y, rect2.Y);
			return new Rectangle(x, y, MathHelper.Max(rect1.X + rect1.Width, rect2.X + rect2.Width) - x,
				MathHelper.Max(rect1.Y + rect1.Height, rect2.Y + rect2.Height) - y);
		}

		static Vector2[] sHelperPoints = new Vector2[4];
		public static void TransformRect(ref Rectangle rect, ref Matrix localToWorld, ref Matrix worldToLocal)
		{
			sHelperPoints[0] = new Vector2(rect.X, rect.Y);
			sHelperPoints[1] = new Vector2(rect.X + rect.Width, rect.Y);
			sHelperPoints[2] = new Vector2(rect.X, rect.Y + rect.Height);
			sHelperPoints[3] = new Vector2(rect.X + rect.Width, rect.Y + rect.Height);

			rect.X = int.MaxValue;
			rect.Y = int.MaxValue;
			float maxX = float.MinValue;
			float maxY = float.MinValue;
			Vector2 v, v1;

			for (int i = 0; i < 4; i++)
			{
				Vector2.Transform(ref sHelperPoints[i], ref localToWorld, out v1);
				Vector2.Transform(ref v1, ref worldToLocal, out v);

				if (v.X < rect.X) rect.X = v.X;
				if (v.X > maxX) maxX = v.X;
				if (v.Y < rect.Y) rect.Y = v.Y;
				if (v.Y > maxY) maxY = v.Y;
			}

			rect.Width = maxX - rect.X;
			rect.Height = maxY - rect.Y;
		}

		public static void uvLerp(Vector2[] uvSrc, Vector2[] uvDest, float min, float max)
		{
			float uMin = float.MaxValue;
			float uMax = float.MinValue;
			float vMin = float.MaxValue;
			float vMax = float.MinValue;
			int len = uvSrc.Length;
			for (int i = 0; i < len; i++)
			{
				Vector2 v = uvSrc[i];
				if (v.X < uMin)
					uMin = v.X;
				if (v.X > uMax)
					uMax = v.X;
				if (v.Y < vMin)
					vMin = v.Y;
				if (v.Y > vMax)
					vMax = v.Y;
			}
			float uLen = uMax - uMin;
			float vLen = vMax - vMin;
			for (int i = 0; i < len; i++)
			{
				Vector2 v = uvSrc[i];
				v.X = (v.X - uMin) / uLen;
				v.Y = (v.Y - vMin) / vLen;
				uvDest[i] = v;
			}
		}

		//格式化回车符，使只出现\n
		public static string FormatCRLF(string source)
		{
			int pos = source.IndexOf("\r");
			if (pos != -1)
			{
				int len = source.Length;
				StringBuilder buffer = new StringBuilder();
				int lastPos = 0;
				while (pos != -1)
				{
					buffer.Append(source, lastPos, pos);
					if (pos == len - 1 || source[pos + 1] != '\n')
						buffer.Append('\n');

					lastPos = pos + 1;
					if (lastPos >= len)
						break;

					pos = source.IndexOf("\r", lastPos);
				}
				if (lastPos < len)
					buffer.Append(source, lastPos, len - lastPos);

				source = buffer.ToString();
			}

			return source;
		}

		//From Starling
		public static bool IsPointInTriangle(ref Vector2 p, ref Vector2 a, ref Vector2 b, ref Vector2 c)
		{
			// This algorithm is described well in this article:
			// http://www.blackpawn.com/texts/pointinpoly/default.html

			float v0x = c.X - a.X;
			float v0y = c.Y - a.Y;
			float v1x = b.X - a.X;
			float v1y = b.Y - a.Y;
			float v2x = p.X - a.X;
			float v2y = p.Y - a.Y;

			float dot00 = v0x * v0x + v0y * v0y;
			float dot01 = v0x * v1x + v0y * v1y;
			float dot02 = v0x * v2x + v0y * v2y;
			float dot11 = v1x * v1x + v1y * v1y;
			float dot12 = v1x * v2x + v1y * v2y;

			float invDen = 1.0f / (dot00 * dot11 - dot01 * dot01);
			float u = (dot11 * dot02 - dot01 * dot12) * invDen;
			float v = (dot00 * dot12 - dot01 * dot02) * invDen;

			return (u >= 0) && (v >= 0) && (u + v < 1);
		}

		public static bool EqualColor(ref Color c1, ref Color c2)
		{
			return c1.A == c2.A
				&& c1.R == c2.R
				&& c1.G == c2.G
				&& c1.B == c2.B;
		}

		public static class EpsilonData
		{
			// The float.Epsilon can be flushed to 0 in certain cases. That's why a slightly higher value is used instead in those cases.
			internal const float MagicNumber = 1.175494E-37f;
			internal const float FloatMinVal = float.Epsilon;

			/// <summary>
			/// Determines whether floats are normalized to zero. If the minimal value is flushed to zero we use a slightly bigger number for Epsilon instead.
			/// </summary>
			public static bool IsDeNormalizedFloatEnabled = FloatMinVal == 0.0d;
		}

		static readonly float Epsilon = EpsilonData.IsDeNormalizedFloatEnabled ? EpsilonData.MagicNumber : EpsilonData.FloatMinVal;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Approximately(float lhs, float rhs)
		{
			return Math.Abs(lhs - rhs) <= Epsilon;
		}

		private static readonly System.Random _random = new System.Random();
		private static readonly object _syncLock = new object();

		public static float Random(float minValue, float maxValue)
		{
			if (minValue > maxValue)
			{
				throw new System.ArgumentOutOfRangeException(nameof(maxValue),
					$"The value of {nameof(minValue)} is greater than the value of {nameof(maxValue)}!");
			}
			float value;
			lock (_syncLock)
			{
				value = (float)_random.NextDouble();
			}
			return (maxValue - minValue) * value + minValue;
		}

		public static int Random(int minValue, int maxValue)
		{
			if (minValue > maxValue)
			{
				throw new System.ArgumentOutOfRangeException(nameof(maxValue),
					$"The value of {nameof(minValue)} is greater than the value of {nameof(maxValue)}!");
			}
			float value;
			lock (_syncLock)
			{
				value = (float)_random.NextDouble();
			}
			return (int)Math.Floor((maxValue - minValue) * value + minValue);
		}

		public static Matrix CreateMatrix(Vector3 trans, Vector3 euler, Vector3 scale, Vector2 skew)
		{
			Matrix matrix = Matrix.CreateRotationY(MathHelper.ToRadians(euler.Y))
							* Matrix.CreateRotationX(MathHelper.ToRadians(euler.X))
							* Matrix.CreateRotationZ(MathHelper.ToRadians(euler.Z))
							* Matrix.CreateScale(scale);

			if (skew.X != 0 || skew.Y != 0)
			{
				float skewX = MathHelper.ToRadians(skew.X);
				float skewY = MathHelper.ToRadians(skew.Y);
				float sinX, cosX;
				sinX = (float)Math.Sin(skewX);
				cosX = (float)Math.Cos(skewX);
				float sinY, cosY;
				sinY = (float)Math.Sin(skewY);
				cosY = (float)Math.Cos(skewY);

				float M11 = matrix[0, 0] * cosY - matrix[0, 1] * sinX;
				float M12 = matrix[0, 0] * sinY + matrix[0, 1] * cosX;
				float M21 = matrix[1, 0] * cosY - matrix[1, 1] * sinX;
				float M22 = matrix[1, 0] * sinY + matrix[1, 1] * cosX;
				float M31 = matrix[2, 0] * cosY - matrix[2, 1] * sinX;
				float M32 = matrix[2, 0] * sinY + matrix[2, 1] * cosX;

				matrix[0, 0] = M11;
				matrix[0, 1] = M12;
				matrix[1, 0] = M21;
				matrix[1, 1] = M22;
				matrix[2, 0] = M31;
				matrix[2, 1] = M32;
			}

			matrix = matrix * Matrix.CreateTranslation(trans);

			return matrix;
		}
	}
}
