using System.Collections.Generic;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;

#if Windows || DesktopGL
using Rectangle = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class SelectionShape : DisplayObject, IMeshFactory
	{
		public readonly List<Rectangle> rects;

		public SelectionShape()
		{
			graphics = new NGraphics();
			graphics.texture = NTexture.Empty;
			graphics.meshFactory = this;

			rects = new List<Rectangle>();
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get
			{
				return graphics.color;
			}
			set
			{
				graphics.color = value;
			}
		}

		public void Refresh()
		{
			int count = rects.Count;
			if (count > 0)
			{
				Rectangle rect = new Rectangle();
				rect = rects[0];
				Rectangle tmp;
				for (int i = 1; i < count; i++)
				{
					tmp = rects[i];
					rect = ToolSet.Union(ref rect, ref tmp);
				}
				SetSize(rect.Right, rect.Bottom);
			}
			else
				SetSize(0, 0);
			graphics.SetMeshDirty();
		}

		public void Clear()
		{
			rects.Clear();
			graphics.SetMeshDirty();
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			int count = rects.Count;
			if (count == 0)
				return;

			for (int i = 0; i < count; i++)
				vb.AddQuad(rects[i]);
			vb.AddTriangles();
		}

		protected override DisplayObject HitTest()
		{
			Vector2 localPoint = GlobalToLocal(HitTestContext.screenPoint);

			if (_contentRect.Contains(localPoint.X, localPoint.Y))
			{
				int count = rects.Count;
				for (int i = 0; i < count; i++)
				{
					if (rects[i].Contains(localPoint.X, localPoint.Y))
						return this;
				}
			}

			return null;
		}
	}
}
