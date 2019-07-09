using System;
using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class FillMesh : IMeshFactory
	{
		/// <summary>
		/// 
		/// </summary>
		public FillMethod method;

		/// <summary>
		/// 
		/// </summary>
		public int origin;

		/// <summary>
		/// 
		/// </summary>
		public float amount;

		/// <summary>
		/// 
		/// </summary>
		public bool clockwise;

		public FillMesh()
		{
			clockwise = true;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			float amount = MathHelper.Clamp(this.amount, 0, 1);
			switch (method)
			{
				case FillMethod.Horizontal:
					FillHorizontal(vb, vb.contentRect, origin, amount);
					break;

				case FillMethod.Vertical:
					FillVertical(vb, vb.contentRect, origin, amount);
					break;

				case FillMethod.Radial90:
					FillRadial90(vb, vb.contentRect, (Origin90)origin, amount, clockwise);
					break;

				case FillMethod.Radial180:
					FillRadial180(vb, vb.contentRect, (Origin180)origin, amount, clockwise);
					break;

				case FillMethod.Radial360:
					FillRadial360(vb, vb.contentRect, (Origin360)origin, amount, clockwise);
					break;
			}
		}

		static void FillHorizontal(VertexBuffer vb, Rectangle vertRect, int origin, float amount)
		{
			float a = vertRect.Width * amount;
			if ((OriginHorizontal)origin == OriginHorizontal.Right || (OriginVertical)origin == OriginVertical.Bottom)
				vertRect.X += (vertRect.Width - a);
			vertRect.Width = a;

			vb.AddQuad(vertRect);
			vb.AddTriangles();
		}

		static void FillVertical(VertexBuffer vb, Rectangle vertRect, int origin, float amount)
		{
			float a = vertRect.Height * amount;
			if ((OriginHorizontal)origin == OriginHorizontal.Right || (OriginVertical)origin == OriginVertical.Bottom)
				vertRect.Y += (vertRect.Height - a);
			vertRect.Height = a;

			vb.AddQuad(vertRect);
			vb.AddTriangles();
		}

		//4 vertex
		static void FillRadial90(VertexBuffer vb, Rectangle vertRect, Origin90 origin, float amount, bool clockwise)
		{
            bool flipX = origin == Origin90.TopRight || origin == Origin90.BottomRight;
            bool flipY = origin == Origin90.BottomLeft || origin == Origin90.BottomRight;
            if (flipX != flipY)
                clockwise = !clockwise;

            float ratio = clockwise ? amount : (1 - amount);
            float tan = (float)Math.Tan(Math.PI * 0.5f * ratio);
            bool thresold = false;
            if (ratio != 1)
                thresold = (vertRect.Height / vertRect.Width - tan) > 0;
            if (!clockwise)
                thresold = !thresold;
            float x = vertRect.X + (ratio == 0 ? float.MaxValue : (vertRect.Height / tan));
            float y = vertRect.Y + (ratio == 1 ? float.MaxValue : (vertRect.Width * tan));
            float x2 = x;
            float y2 = y;
            if (flipX)
                x2 = vertRect.Width - x;
            if (flipY)
                y2 = vertRect.Height - y;
            float xMin = flipX ? (vertRect.Width - vertRect.X) : vertRect.X;
            float yMin = flipY ? (vertRect.Height - vertRect.Y) : vertRect.Y;
            float xMax = flipX ? -vertRect.X : vertRect.Right;
            float yMax = flipY ? -vertRect.Y : vertRect.Bottom;

            vb.AddVert(new Vector3(xMin, yMin, 0));

            if (clockwise)
                vb.AddVert(new Vector3(xMax, yMin, 0));

            if (y > vertRect.Bottom)
            {
                if (thresold)
                    vb.AddVert(new Vector3(x2, yMax, 0));
                else
                    vb.AddVert(new Vector3(xMax, yMax, 0));
            }
            else
                vb.AddVert(new Vector3(xMax, y2, 0));

            if (x > vertRect.Right)
            {
                if (thresold)
                    vb.AddVert(new Vector3(xMax, y2, 0));
                else
                    vb.AddVert(new Vector3(xMax, yMax, 0));
            }
            else
                vb.AddVert(new Vector3(x2, yMax, 0));

            if (!clockwise)
                vb.AddVert(new Vector3(xMin, yMax, 0));

            if (flipX == flipY)
            {
                vb.AddTriangle(0, 1, 2);
                vb.AddTriangle(0, 2, 3);
            }
            else
            {
                vb.AddTriangle(2, 1, 0);
                vb.AddTriangle(3, 2, 0);
            }
        }

		//8 vertex
		static void FillRadial180(VertexBuffer vb, Rectangle vertRect, Origin180 origin, float amount, bool clockwise)
		{
			switch (origin)
			{
				case Origin180.Top:
					if (amount <= 0.5f)
					{
						vertRect.Width /= 2;
						if (clockwise)
							vertRect.X += vertRect.Width;

						FillRadial90(vb, vertRect, clockwise ? Origin90.TopLeft : Origin90.TopRight, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-4);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Width /= 2;
						if (!clockwise)
							vertRect.X += vertRect.Width;

						FillRadial90(vb, vertRect, clockwise ? Origin90.TopRight : Origin90.TopLeft, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.X += vertRect.Width;
						else
							vertRect.X -= vertRect.Width;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}
					break;

				case Origin180.Bottom:
					if (amount <= 0.5f)
					{
						vertRect.Width /= 2;
						if (!clockwise)
							vertRect.X += vertRect.Width;

						FillRadial90(vb, vertRect, clockwise ? Origin90.BottomRight : Origin90.BottomLeft, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-4);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Width /= 2;
						if (clockwise)
							vertRect.X += vertRect.Width;

						FillRadial90(vb, vertRect, clockwise ? Origin90.BottomLeft : Origin90.BottomRight, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.X -= vertRect.Width;
						else
							vertRect.X += vertRect.Width;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}
					break;

				case Origin180.Left:
					if (amount <= 0.5f)
					{
						vertRect.Height /= 2;
						if (!clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial90(vb, vertRect, clockwise ? Origin90.BottomLeft : Origin90.TopLeft, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-4);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Height /= 2;
						if (clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial90(vb, vertRect, clockwise ? Origin90.TopLeft : Origin90.BottomLeft, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.Y -= vertRect.Height;
						else
							vertRect.Y += vertRect.Height;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}
					break;

				case Origin180.Right:
					if (amount <= 0.5f)
					{
						vertRect.Height /= 2;
						if (clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial90(vb, vertRect, clockwise ? Origin90.TopRight : Origin90.BottomRight, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-4);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Height /= 2;
						if (!clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial90(vb, vertRect, clockwise ? Origin90.BottomRight : Origin90.TopRight, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.Y += vertRect.Height;
						else
							vertRect.Y -= vertRect.Height;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}
					break;
			}
		}

		//12 vertex
		static void FillRadial360(VertexBuffer vb, Rectangle vertRect, Origin360 origin, float amount, bool clockwise)
		{
			switch (origin)
			{
				case Origin360.Top:
					if (amount < 0.5f)
					{
						vertRect.Width /= 2;
						if (clockwise)
							vertRect.X += vertRect.Width;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Left : Origin180.Right, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-8);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Width /= 2;
						if (!clockwise)
							vertRect.X += vertRect.Width;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Right : Origin180.Left, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.X += vertRect.Width;
						else
							vertRect.X -= vertRect.Width;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}

					break;

				case Origin360.Bottom:
					if (amount < 0.5f)
					{
						vertRect.Width /= 2;
						if (!clockwise)
							vertRect.X += vertRect.Width;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Right : Origin180.Left, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-8);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Width /= 2;
						if (clockwise)
							vertRect.X += vertRect.Width;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Left : Origin180.Right, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.X -= vertRect.Width;
						else
							vertRect.X += vertRect.Width;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}
					break;

				case Origin360.Left:
					if (amount < 0.5f)
					{
						vertRect.Height /= 2;
						if (!clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Bottom : Origin180.Top, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-8);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Height /= 2;
						if (clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Top : Origin180.Bottom, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.Y -= vertRect.Height;
						else
							vertRect.Y += vertRect.Height;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}
					break;

				case Origin360.Right:
					if (amount < 0.5f)
					{
						vertRect.Height /= 2;
						if (clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Top : Origin180.Bottom, amount / 0.5f, clockwise);
						Vector3 vec = vb.GetPosition(-8);
						vb.AddQuad(new Rectangle(vec.X, vec.Y, 0, 0));
						vb.AddTriangles(-4);
					}
					else
					{
						vertRect.Height /= 2;
						if (!clockwise)
							vertRect.Y += vertRect.Height;

						FillRadial180(vb, vertRect, clockwise ? Origin180.Bottom : Origin180.Top, (amount - 0.5f) / 0.5f, clockwise);

						if (clockwise)
							vertRect.Y += vertRect.Height;
						else
							vertRect.Y -= vertRect.Height;
						vb.AddQuad(vertRect);
						vb.AddTriangles(-4);
					}
					break;
			}
		}
	}
}