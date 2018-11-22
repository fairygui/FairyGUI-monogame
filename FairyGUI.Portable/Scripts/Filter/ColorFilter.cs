using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class ColorFilter : IFilter
	{
		// Most of the color transformation math was taken from the excellent ColorMatrixFilter class in Starling Framework

		DisplayObject _target;
		float[] _matrix;
		EffectPass _pass;
		EffectParameter[] _params;

		const float LUMA_R = 0.299f;
		const float LUMA_G = 0.587f;
		const float LUMA_B = 0.114f;

		static float[] IDENTITY = new float[] { 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0 };
		static string[] FILTER_KEY = new string[] { "COLOR_FILTER" };

		public ColorFilter()
		{
			_matrix = new float[20];
			Array.Copy(IDENTITY, _matrix, _matrix.Length);
		}

		public DisplayObject target
		{
			get { return _target; }
			set
			{
				_target = value;

				if (_target != null && !(_target is Image) && !(_target is MovieClip))
				{
					_target.EnterPaintingMode(1, null);
				}
			}
		}

		public void Dispose()
		{
			if (_target != null && !(_target is Image) && !(_target is MovieClip))
			{
				_target.LeavePaintingMode(1);
			}
			_target = null;
		}

		public void Update()
		{

		}

		public void Apply(FairyBatch batch)
		{
			if (_pass == null)
			{
				Effect effect = batch.defaultEffect;
				_pass = effect.Techniques["ColorFilter"].Passes[0];
				_params = new EffectParameter[] {
					effect.Parameters["ColorMatrix0"],
					effect.Parameters["ColorMatrix1"],
					effect.Parameters["ColorMatrix2"],
					effect.Parameters["ColorMatrix3"],
					effect.Parameters["ColorMatrix4"]
				};
			}

			_params[0].SetValue(new Vector4(_matrix[0], _matrix[1], _matrix[2], _matrix[3]));
			_params[1].SetValue(new Vector4(_matrix[5], _matrix[6], _matrix[7], _matrix[8]));
			_params[2].SetValue(new Vector4(_matrix[10], _matrix[11], _matrix[12], _matrix[13]));
			_params[3].SetValue(new Vector4(_matrix[15], _matrix[16], _matrix[17], _matrix[18]));
			_params[4].SetValue(new Vector4(_matrix[4], _matrix[9], _matrix[14], _matrix[19]));

			_pass.Apply();
		}

		public void Invert()
		{
			ConcatValues(-1, 0, 0, 0, 1,
						  0, -1, 0, 0, 1,
						  0, 0, -1, 0, 1,
						  0, 0, 0, 1, 0);
		}

		/// <summary>
		/// Changes the saturation. Typical values are in the range (-1, 1).
		/// Values above zero will raise, values below zero will reduce the saturation.
		/// '-1' will produce a grayscale image. 
		/// </summary>
		/// <param name="sat"></param>
		public void AdjustSaturation(float sat)
		{
			sat += 1;

			float invSat = 1 - sat;
			float invLumR = invSat * LUMA_R;
			float invLumG = invSat * LUMA_G;
			float invLumB = invSat * LUMA_B;

			ConcatValues((invLumR + sat), invLumG, invLumB, 0, 0,
						  invLumR, (invLumG + sat), invLumB, 0, 0,
						  invLumR, invLumG, (invLumB + sat), 0, 0,
						  0, 0, 0, 1, 0);
		}

		/// <summary>
		/// Changes the contrast. Typical values are in the range (-1, 1).
		/// Values above zero will raise, values below zero will reduce the contrast.
		/// </summary>
		/// <param name="value"></param>
		public void AdjustContrast(float value)
		{
			float s = value + 1;
			float o = 128f / 255 * (1 - s);

			ConcatValues(s, 0, 0, 0, o,
						 0, s, 0, 0, o,
						 0, 0, s, 0, o,
						 0, 0, 0, 1, 0);
		}

		/// <summary>
		/// Changes the brightness. Typical values are in the range (-1, 1).
		/// Values above zero will make the image brighter, values below zero will make it darker.
		/// </summary>
		/// <param name="value"></param>
		public void AdjustBrightness(float value)
		{
			ConcatValues(1, 0, 0, 0, value,
						 0, 1, 0, 0, value,
						 0, 0, 1, 0, value,
						 0, 0, 0, 1, 0);
		}

		/// <summary>
		///Changes the hue of the image. Typical values are in the range (-1, 1).
		/// </summary>
		/// <param name="value"></param>
		public void AdjustHue(float value)
		{
			value *= (float)Math.PI;

			float cos = (float)Math.Cos(value);
			float sin = (float)Math.Sin(value);

			ConcatValues(
				((LUMA_R + (cos * (1 - LUMA_R))) + (sin * -(LUMA_R))), ((LUMA_G + (cos * -(LUMA_G))) + (sin * -(LUMA_G))), ((LUMA_B + (cos * -(LUMA_B))) + (sin * (1 - LUMA_B))), 0, 0,
				((LUMA_R + (cos * -(LUMA_R))) + (sin * 0.143f)), ((LUMA_G + (cos * (1 - LUMA_G))) + (sin * 0.14f)), ((LUMA_B + (cos * -(LUMA_B))) + (sin * -0.283f)), 0, 0,
				((LUMA_R + (cos * -(LUMA_R))) + (sin * -((1 - LUMA_R)))), ((LUMA_G + (cos * -(LUMA_G))) + (sin * LUMA_G)), ((LUMA_B + (cos * (1 - LUMA_B))) + (sin * LUMA_B)), 0, 0,
				0, 0, 0, 1, 0);
		}

		/// <summary>
		/// Tints the image in a certain color, analog to what can be done in Adobe Animate.
		/// </summary>
		/// <param name="color">the RGB color with which the image should be tinted.</param>
		/// <param name="amount">the intensity with which tinting should be applied. Range (0, 1).</param>
		public void Tint(Color color, float amount = 1.0f)
		{
			float q = 1 - amount;

			float rA = amount * color.R;
			float gA = amount * color.G;
			float bA = amount * color.B;

			ConcatValues(
				q + rA * LUMA_R, rA * LUMA_G, rA * LUMA_B, 0, 0,
				gA * LUMA_R, q + gA * LUMA_G, gA * LUMA_B, 0, 0,
				bA * LUMA_R, bA * LUMA_G, q + bA * LUMA_B, 0, 0,
				0, 0, 0, 1, 0);
		}

		/// <summary>
		/// Changes the filter matrix back to the identity matrix
		/// </summary>
		public void Reset()
		{
			Array.Copy(IDENTITY, _matrix, _matrix.Length);
		}

		static float[] tmp = new float[20];

		/// <summary>
		/// 
		/// </summary>
		/// <param name="values"></param>
		public void ConcatValues(params float[] values)
		{
			int i = 0;

			for (int y = 0; y < 4; ++y)
			{
				for (int x = 0; x < 5; ++x)
				{
					tmp[i + x] = values[i] * _matrix[x] +
							values[i + 1] * _matrix[x + 5] +
							values[i + 2] * _matrix[x + 10] +
							values[i + 3] * _matrix[x + 15] +
							(x == 4 ? values[i + 4] : 0);
				}
				i += 5;
			}
			Array.Copy(tmp, _matrix, tmp.Length);
		}
	}
}
