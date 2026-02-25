#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_3_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float destroyInterval[5];
float flashInterval[5];
float4 flashColor[5];

sampler s0;
sampler noiseTexture = sampler_state
{
    Texture = <noise>;
    Filter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 PixelShaderFunction(float4 position : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
    int i = color1.a * 255;
	
    if (i > 4)
    {
        float4 color = tex2D(s0, texCoord) * color1;
        if (i <= 16) return color;
    }
	
    if (destroyInterval[i] <= tex2D(noiseTexture, texCoord).x)
    {
        return float4(0, 0, 0, 0);
    }

    float4 color = tex2D(s0, texCoord) * float4(color1.r, color1.g, color1.b, 1.0f);
	if (color.a <= 0) return color;
    
    color.a = lerp(color.a, flashColor[i].a, flashInterval[i]);
    color.r = lerp(color.r, flashColor[i].r, flashInterval[i]);
    color.g = lerp(color.g, flashColor[i].g, flashInterval[i]);
    color.b = lerp(color.b, flashColor[i].b, flashInterval[i]);

	return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}