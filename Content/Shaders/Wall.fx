#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;
 
float Brightness = 1.0f;
 
texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (WallTexture);
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};
 
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};
 
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 Distance : TEXCOORD1;
};
 
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    
    float4 cameraPosition = mul(input.Position, World);
    cameraPosition = mul(cameraPosition, View);
	
	VertexShaderOutput output;
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;
    output.Distance = float2(cameraPosition.z, 0);
	
    return output;
}
 
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 result = tex2D(textureSampler, input.TextureCoordinate) * input.Color;
    
	if (Brightness < 1)
	{
		result.r *= Brightness;
		result.g *= Brightness;
		result.b *= Brightness;
	}
	
    if (input.Distance.x <= -30)
    {
        float t = (-input.Distance.x - 30.0f) / 40.f;
        clamp(t, 0, 1);
        return lerp(result, float4(0, 0, 0, 1), t);
        
    }
	
    return result;
}
 
technique Textured
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}