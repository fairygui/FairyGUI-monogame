using System;
using Microsoft.Xna.Framework;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class PlaneMesh : IMeshFactory
	{
		public int gridSize = 30;

		public void OnPopulateMesh(VertexBuffer vb)
		{
			float w = vb.contentRect.Width;
			float h = vb.contentRect.Height;
			float xMax = vb.contentRect.Right;
			float yMax = vb.contentRect.Bottom;
			int hc = (int)MathHelper.Min((int)Math.Ceiling(w / gridSize), 9);
			int vc = (int)MathHelper.Min((int)Math.Ceiling(h / gridSize), 9);
			int eachPartX = (int)Math.Floor(w / hc);
			int eachPartY = (int)Math.Floor(h / vc);
			float x, y;
			for (int i = 0; i <= vc; i++)
			{
				if (i == vc)
					y = yMax;
				else
					y = vb.contentRect.Y + i * eachPartY;
				for (int j = 0; j <= hc; j++)
				{
					if (j == hc)
						x = xMax;
					else
						x = vb.contentRect.X + j * eachPartX;
					vb.AddVert(new Vector3(x, y, 0));
				}
			}

			for (int i = 0; i < vc; i++)
			{
				int k = i * (hc + 1);
				for (int j = 1; j <= hc; j++)
				{
					int m = k + j;
					vb.AddTriangle(m - 1, m, m + hc);
					vb.AddTriangle(m, m + hc + 1, m + hc);
				}
			}
		}
	}
}