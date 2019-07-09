﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Rectangle = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// Inspired by kim ki won (http://mypi.ruliweb.daum.net/mypi.htm?id=newtypekorea)
	/// </summary>
	public class LineMesh : IMeshFactory
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly GPath path;

		/// <summary>
		/// 
		/// </summary>
		public float lineWidth;

		/// <summary>
		/// 
		/// </summary>
		public bool roundEdge;

		/// <summary>
		/// 
		/// </summary>
		public float fillStart;

		/// <summary>
		/// 
		/// </summary>
		public float fillEnd;

		/// <summary>
		/// 
		/// </summary>
		public float pointDensity;

		static List<Vector3> points = new List<Vector3>();
		static List<float> ts = new List<float>();

		public LineMesh()
		{
			path = new GPath();
			lineWidth = 2;
			fillStart = 0;
			fillEnd = 1;
			pointDensity = 0.1f;
		}

		public void OnPopulateMesh(VertexBuffer vb)
		{
			Vector2 uvMin = new Vector2(vb.uvRect.X, vb.uvRect.Y);
			Vector2 uvMax = new Vector2(vb.uvRect.Right, vb.uvRect.Bottom);

			int segCount = path.segmentCount;
			float t = 0;
			float lw = lineWidth;
			float u;
			for (int si = 0; si < segCount; si++)
			{
				float ratio = path.GetSegmentLength(si) / path.length;
				float t0 = MathHelper.Clamp(fillStart - t, 0, ratio) / ratio;
				float t1 = MathHelper.Clamp(fillEnd - t, 0, ratio) / ratio;
				if (t0 >= t1)
				{
					t += ratio;
					continue;
				}

				points.Clear();
				ts.Clear();
				path.GetPointsInSegment(si, t0, t1, points, ts, pointDensity);
				int cnt = points.Count;

				Color c0 = vb.vertexColor;
				Color c1 = vb.vertexColor;
				/*if (gradient != null)
					c0 = gradient.Evaluate(t);
				if (lineWidthCurve != null)
					lw = lineWidthCurve.Evaluate(t);*/

				if (roundEdge && si == 0 && t0 == 0)
					DrawRoundEdge(vb, points[0], points[1], lw, c0, uvMin);

				int vertCount = vb.currentVertCount;
				for (int i = 1; i < cnt; i++)
				{
					Vector3 p0 = points[i - 1];
					Vector3 p1 = points[i];
					int k = vertCount + (i - 1) * 2;
					float tc = t + ratio * ts[i];

					Vector3 lineVector = p1 - p0;
					Vector3 widthVector = Vector3.Cross(lineVector, new Vector3(0, 0, 1));
					widthVector.Normalize();

					if (i == 1)
					{
						u = MathHelper.Lerp(uvMin.X, uvMax.X, t + ratio * ts[i - 1]);
						vb.AddVert(p0 - widthVector * lw * 0.5f, c0, new Vector2(u, uvMax.Y));
						vb.AddVert(p0 + widthVector * lw * 0.5f, c0, new Vector2(u, uvMin.Y));

						if (si != 0) //joint
						{
							vb.AddTriangle(k - 2, k - 1, k + 1);
							vb.AddTriangle(k - 2, k + 1, k);
						}
					}
					//if (gradient != null)
					//	c1 = gradient.Evaluate(tc);

					//if (lineWidthCurve != null)
					//	lw = lineWidthCurve.Evaluate(tc);

					u = MathHelper.Lerp(uvMin.X, uvMax.X, tc);
					vb.AddVert(p1 - widthVector * lw * 0.5f, c1, new Vector2(u, uvMax.Y));
					vb.AddVert(p1 + widthVector * lw * 0.5f, c1, new Vector2(u, uvMin.Y));

					vb.AddTriangle(k, k + 1, k + 3);
					vb.AddTriangle(k, k + 3, k + 2);
				}

				if (roundEdge && si == segCount - 1 && t1 == 1)
					DrawRoundEdge(vb, points[cnt - 1], points[cnt - 2], lw, c1, uvMax);

				t += ratio;
			}
		}

		void DrawRoundEdge(VertexBuffer vb, Vector3 p0, Vector3 p1, float lw, Color color, Vector2 uv)
		{
			Vector3 tmp = p0 - p1;
			Vector3 widthVector = Vector3.Cross(tmp, new Vector3(0, 0, 1));
			widthVector.Normalize();
			widthVector = widthVector * lw / 2f;
			tmp.Normalize();
			Vector3 lineVector = tmp * lw / 2f;

			int sides = (int)Math.Ceiling(Math.PI * lw / 2);
			if (sides < 6)
				sides = 6;
			int current = vb.currentVertCount;
			float angleUnit = (float)Math.PI / (sides - 1);

			vb.AddVert(p0, color, uv);
			vb.AddVert(p0 + widthVector, color, uv);

			for (int n = 0; n < sides; n++)
			{
				vb.AddVert(p0 + (float)Math.Cos(angleUnit * n) * widthVector + (float)Math.Sin(angleUnit * n) * lineVector, color, uv);
				vb.AddTriangle(current, current + 1 + n, current + 2 + n);
			}
		}
	}
}
