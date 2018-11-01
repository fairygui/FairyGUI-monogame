using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RectangleF = System.Drawing.RectangleF;

namespace FairyGUI
{
	/// <summary>
	/// UpdateContext is for internal use.
	/// </summary>
	public class FairyBatch
	{
		public struct ClipInfo
		{
			public RectangleF rect;
			public uint clipId;
		}

		public bool clipped;
		public ClipInfo clipInfo;

		public float alpha;
		public bool grayed;

		Stack<ClipInfo> _clipStack;
		VertexPositionColorTexture[] _vertexCache;
		int[] _indexCache;
		int _vertexPtr;
		int _indexPtr;
		GraphicsDevice _device;
		SpriteBatch _batch;
		BlendMode _blendMode;
		Texture2D _texture;
		RasterizerState _scissorTestEnabled;

		public FairyBatch()
		{
			_clipStack = new Stack<ClipInfo>();
			_vertexCache = new VertexPositionColorTexture[1024];
			_indexCache = new int[1024];

			_device = Stage.game.GraphicsDevice;
			_batch = new SpriteBatch(_device);

			_scissorTestEnabled = new RasterizerState();
			_scissorTestEnabled.CullMode = CullMode.None;
			_scissorTestEnabled.ScissorTestEnable = true;
		}

		public void Dispose()
		{
			_batch.Dispose();
			_scissorTestEnabled.Dispose();
		}

		/// <summary>
		/// 
		/// </summary>
		public void Begin()
		{
			grayed = false;
			alpha = 1;
			_blendMode = BlendMode.Normal;
			_texture = null;

			clipped = false;
			_clipStack.Clear();

			Stats.ObjectCount = 0;
			Stats.GraphicsCount = 0;

			_batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
		}

		/// <summary>
		/// 
		/// </summary>
		public void End()
		{
			Flush();
			_batch.End();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clipId"></param>
		/// <param name="clipRect"></param>
		/// <param name="softness"></param>
		public void EnterClipping(uint clipId, RectangleF clipRect)
		{
			_clipStack.Push(clipInfo);

			if (clipped)
				clipRect = ToolSet.Intersection(ref clipInfo.rect, ref clipRect);

			clipped = true;
			clipInfo.rect = clipRect;
			clipInfo.clipId = clipId;

			ChangeState();

			SetScissor(clipInfo.rect);
		}

		/// <summary>
		/// 
		/// </summary>
		public void LeaveClipping()
		{
			clipInfo = _clipStack.Pop();
			clipped = _clipStack.Count > 0;

			ChangeState();

			if (clipped)
				SetScissor(clipInfo.rect);
		}

		public void Draw(NGraphics graphics, float alpha, bool grayed, BlendMode blendMode, Matrix localToWorldMatrix)
		{
			if (graphics.texture == null || !graphics.enabled)
				return;

			int vertCount = graphics.vertCount;
			if (vertCount == 0)
				return;

			if (_blendMode != blendMode)
			{
				_blendMode = blendMode;
				ChangeState();
			}

			Texture2D texture = graphics.texture.nativeTexture;
			if (texture != _texture)
			{
				Flush();
				_texture = texture;
			}

			Vector3[] vertices = graphics.vertices;
			Vector2[] uv = graphics.uv;
			Color[] colors = graphics.colors;
			int[] triangles = graphics.triangles;
			int indexCount = triangles.Length;

			if (_vertexPtr + vertCount > _vertexCache.Length)
			{
				VertexPositionColorTexture[] newArray = new VertexPositionColorTexture[_vertexCache.Length + vertCount + (int)Math.Ceiling(_vertexCache.Length * 0.5f)];
				_vertexCache.CopyTo(newArray, 0);
				_vertexCache = newArray;
			}

			if (_indexPtr + indexCount > _indexCache.Length)
			{
				int[] newArray = new int[_indexCache.Length + indexCount + (int)Math.Ceiling(_indexCache.Length * 0.5f)];
				_indexCache.CopyTo(newArray, 0);
				_indexCache = newArray;
			}

			VertexPositionColorTexture vpct;

			for (int i = 0; i < indexCount; i++)
			{
				_indexCache[_indexPtr++] = triangles[i] + _vertexPtr;
			}

			for (int i = 0; i < vertCount; i++)
			{
				Vector3.Transform(ref vertices[i], ref localToWorldMatrix, out vpct.Position);
				vpct.TextureCoordinate = uv[i];
				vpct.Color = colors[i] * this.alpha * alpha;
				_vertexCache[_vertexPtr++] = vpct;
			}
		}

		void ChangeState()
		{
			Flush();

			_batch.End();
			_batch.Begin(SpriteSortMode.Immediate, BlendModeUtils.blendStates[(int)_blendMode], 
				null, null, clipped ? _scissorTestEnabled : null);
		}

		void Flush()
		{
			if (_vertexPtr > 0)
			{
				_device.Textures[0] = _texture;
				_device.DrawUserIndexedPrimitives(
					PrimitiveType.TriangleList,
					_vertexCache,
					0,
					_vertexPtr,
					_indexCache,
					0,
					_indexPtr / 3,
					VertexPositionColorTexture.VertexDeclaration);

				_vertexPtr = _indexPtr = 0;
			}
		}

		void SetScissor(RectangleF rect)
		{
			int rectX = (int)Math.Floor(rect.X);
			int rectY = (int)Math.Floor(rect.Y);
			_device.ScissorRectangle = new Rectangle(rectX, rectY,
					(int)Math.Ceiling(rect.X + rect.Width) - rectX, (int)Math.Ceiling(rect.Y + rect.Height) - rectY);
		}
	}
}
