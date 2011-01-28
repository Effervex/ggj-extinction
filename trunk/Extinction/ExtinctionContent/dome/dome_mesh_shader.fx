#include "..\shader_shared.fx"

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : NORMAL;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Normal : TEXCOORD1;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float phase = Time + World._41 + World._43 + (input.Position.x + input.Position.z) * 0.1f;
	float radius = 0.5f;
	float2 xz = clamp(input.Position.y + 0.5, 0, radius) * float2(cos(phase), sin(phase));
	input.Position.xz += xz;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(input.Position, View);
    output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = input.Normal;
	
    // TODO: add your vertex shader code here.

    return output;
}
 
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 sample = tex2D(color, input.UV);
	float4 result = DoLighting( sample, input.Normal);

	result.w = sample.w;
	return result;
//	return tex2D(color, input.UV) * diffuse;
    //return float4(1, 0, 0, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
