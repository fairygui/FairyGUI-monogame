using System;
using System.Collections.Generic;
using FairyGUI.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if Windows || DesktopGL
using RectangleF = System.Drawing.RectangleF;
#endif

namespace FairyGUI
{
	/// <summary>
	/// FairyBatch is for internal use.
	/// </summary>
	public class FairyBatch
	{
		struct RenderTarget
		{
			public RenderTarget2D target;
			public Vector2 origin;
		}

		public float alpha;
		public bool grayed;

		Stack<RectangleF> _clipStack;
		bool _clipped;
		RectangleF _clipRect;

		Stack<RenderTarget> _renderTargets;
		bool _hasRenderTarget;
		Vector2 _renderOffset;

		VertexPositionColorTexture[] _vertexCache;
		int[] _indexCache;
		int _vertexPtr;
		int _indexPtr;

		BlendMode _blendMode;
		bool _grayed;

		GraphicsDevice _device;
		Viewport _originalViewPort;
		Texture2D _texture;
		RasterizerState _scissorTestEnabled;

		SpriteEffect _spriteEffect;
		EffectPass _spritePass;

		Effect _defaultEffect;
		EffectPass _defaultPass;
		EffectPass _grayedPass;

		public FairyBatch()
		{
			_clipStack = new Stack<RectangleF>();
			_renderTargets = new Stack<RenderTarget>();
			_vertexCache = new VertexPositionColorTexture[1024];
			_indexCache = new int[1024];

			_device = Stage.game.GraphicsDevice;

			_scissorTestEnabled = new RasterizerState();
			_scissorTestEnabled.CullMode = CullMode.None;
			_scissorTestEnabled.ScissorTestEnable = true;

			_defaultEffect = Stage.game.Content.Load<Effect>("FairyGUI");
			_defaultPass = _defaultEffect.Techniques["Default"].Passes[0];
			_grayedPass = _defaultEffect.Techniques["Grayed"].Passes[0];

			_spriteEffect = new SpriteEffect(_device);
			_spritePass = _spriteEffect.CurrentTechnique.Passes[0];
		}

		/// <summary>
		/// 
		/// </summary>
		public void Dispose()
		{
			_scissorTestEnabled.Dispose();
			_defaultEffect.Dispose();
			_spriteEffect.Dispose();
		}

		/// <summary>
		/// 
		/// </summary>
		public Effect defaultEffect
		{
			get { return _defaultEffect; }
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
			_grayed = false;
			_clipped = false;
			_clipStack.Clear();
			_renderTargets.Clear();
			_hasRenderTarget = false;

			Stats.ObjectCount = 0;
			Stats.GraphicsCount = 0;

			_originalViewPort = _device.Viewport;
			_device.BlendState = BlendState.NonPremultiplied;
			_device.DepthStencilState = DepthStencilState.None;
			_device.RasterizerState = RasterizerState.CullNone;
			_device.SamplerStates[0] = SamplerState.LinearClamp;

			_spritePass.Apply();
			_defaultPass.Apply();
		}

		/// <summary>
		/// 
		/// </summary>
		public void End()
		{
			Flush();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="clipRect"></param>
		public void EnterClipping(RectangleF clipRect)
		{
			Flush();

			_clipStack.Push(_clipRect);

			if (_clipped)
				clipRect = ToolSet.Intersection(ref _clipRect, ref clipRect);

			_device.RasterizerState = _scissorTestEnabled;

			_clipped = true;
			_clipRect = clipRect;
			SetScissor();
		}

		/// <summary>
		/// 
		/// </summary>
		public void LeaveClipping()
		{
			Flush();

			_clipRect = _clipStack.Pop();
			_clipped = _clipStack.Count > 0;

			if (_clipped)
				SetScissor();
			else
				_device.RasterizerState = RasterizerState.CullNone;
		}

		//TODO: not worked
		public void PushRenderTarget(NTexture texture, Vector2 origin)
		{
			Flush();

			RenderTarget rt = new RenderTarget() { target = (RenderTarget2D)texture.nativeTexture, origin = origin };
			_renderTargets.Push(rt);
			//_device.SetRenderTarget(rt.target);

			//_device.Viewport = new Viewport(0, 0, rt.target.Width, rt.target.Height);
			//_spritePass.Apply();

			//_hasRenderTarget = true;
			_renderOffset = origin;
		}

		public void PopRenderTarget()
		{
			Flush();

			_renderTargets.Pop();
			if (_renderTargets.Count > 0)
			{
				RenderTarget rt = _renderTargets.Peek();

				//_device.Viewport = new Viewport(0, 0, rt.target.Width, rt.target.Height);
				//_device.SetRenderTarget(rt.target);

				//_hasRenderTarget = true;
				_renderOffset = rt.origin;
			}
			else
			{
				//_device.Viewport = _originalViewPort;
				//_device.SetRenderTarget(null);
				//_hasRenderTarget = false;
			}
			//_spritePass.Apply();
		}

		public void Draw(NGraphics graphics, float alpha, bool grayed, BlendMode blendMode, ref Matrix localToWorldMatrix, IFilter filter)
		{
			if (graphics.texture == null || !graphics.enabled)
				return;

			int vertCount = graphics.vertCount;
			if (vertCount == 0)
				return;

			if (_blendMode != blendMode)
			{
				Flush();

				_blendMode = blendMode;
				_device.BlendState = BlendModeUtils.blendStates[(int)_blendMode];
			}

			grayed |= this.grayed;
			if (_grayed != grayed)
			{
				Flush();

				_grayed = grayed;
				if (_grayed)
					_grayedPass.Apply();
				else
					_defaultPass.Apply();
			}

			Texture2D texture = graphics.texture.nativeTexture;
			if (texture != _texture)
			{
				Flush();
				_texture = texture;
			}

			if (filter != null)
			{
				if (_vertexPtr > 0)
					Flush();

				filter.Apply(this);
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
				if (_hasRenderTarget)
				{
					vpct.Position.X -= _renderOffset.X;
					vpct.Position.Y -= _renderOffset.Y;
				}
				vpct.TextureCoordinate = uv[i];
				vpct.Color = colors[i] * this.alpha * alpha;
				_vertexCache[_vertexPtr++] = vpct;
			}

			if (filter != null)
			{
				if (_vertexPtr > 0)
					Flush();

				_defaultPass.Apply();
			}
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

		void SetScissor()
		{
			int rectX = (int)Math.Floor(_clipRect.X);
			int rectY = (int)Math.Floor(_clipRect.Y);
			_device.ScissorRectangle = new Rectangle(
				rectX,
				rectY,
				(int)Math.Ceiling(_clipRect.X + _clipRect.Width) - rectX,
				(int)Math.Ceiling(_clipRect.Y + _clipRect.Height) - rectY);
		}
	}
}
