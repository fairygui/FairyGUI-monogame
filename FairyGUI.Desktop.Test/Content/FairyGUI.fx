#if SM4

#define PS_PROFILE ps_4_0_level_9_1
#define VS_PROFILE vs_4_0_level_9_1

#else

#define PS_PROFILE ps_2_0
#define VS_PROFILE vs_2_0

#endif

sampler s0;

float4 ColorMatrix0;
float4 ColorMatrix1;
float4 ColorMatrix2;
float4 ColorMatrix3;
float4 ColorMatrix4;

float4 PSDefault( float4 inPosition : SV_Position,
			    float4 inColor : COLOR0,
			    float2 coords : TEXCOORD0 ) : COLOR0
{
    return tex2D(s0, coords) * inColor;
}

float4 PSGrayed( float4 inPosition : SV_Position,
			    float4 inColor : COLOR0,
			    float2 coords : TEXCOORD0 ) : COLOR0
{
    float4 color = tex2D(s0, coords) * inColor;
    float greyscale = dot(color.rgb, float3(0.299, 0.587, 0.114));
		color.rgb = float3(greyscale, greyscale, greyscale);

    return color;
}

float4 PixelShaderFunction( float4 inPosition : SV_Position,
			    float4 inColor : COLOR0,
			    float2 coords : TEXCOORD0 ) : COLOR0
{
    float4 color = tex2D(s0, coords) * inColor;
    float greyscale = dot(color.rgb, float3(0.299, 0.587, 0.114));
		color.rgb = float3(greyscale, greyscale, greyscale);

    return color;
}

float4 PSColorFilter( float4 inPosition : SV_Position,
			    float4 inColor : COLOR0,
			    float2 coords : TEXCOORD0 ) : COLOR0
{
    float4 color = tex2D(s0, coords) * inColor;
    
    float4 col2 = color;
    col2.r = dot(color, ColorMatrix0) + ColorMatrix4.x;
    col2.g = dot(color, ColorMatrix1) + ColorMatrix4.y;
    col2.b = dot(color, ColorMatrix2) + ColorMatrix4.z;
    col2.a = dot(color, ColorMatrix3) + ColorMatrix4.w;

    return col2;
}

technique Default
{
    pass Pass1
    {
        PixelShader = compile PS_PROFILE PSDefault();
    }
}

technique Grayed
{
    pass Pass1
    {
        PixelShader = compile PS_PROFILE PSGrayed();
    }
}

technique ColorFilter
{
    pass Pass1
    {
        PixelShader = compile PS_PROFILE PSColorFilter();
    }
}