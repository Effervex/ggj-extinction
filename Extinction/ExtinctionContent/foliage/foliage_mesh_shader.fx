#include "..\shader_shared.fx"

float3 ViewRight;

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
	float3 Color : TEXCOORD2;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 oworld = mul(input.Position, World);

	float phase = Time + World._41 + World._43 + (input.Position.x + input.Position.z+ input.Position.y) * 10.1f;
	float radius = 0.15f;
	float2 xz = clamp(input.Position.y, 0, radius) * float2(cos(phase), sin(phase));
	input.Position.xz += xz;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.UV = input.UV;
	output.Normal = mul( mul(float4(input.Normal,0), World), View).xyz;
	//output.UV.x += ((World._41 + World._43) % 4.0f) / 4.0f;
	
	output.Color  = lerp( float3(0.9,0.1,0.1), float3(0.1,0.97,0.11), 
		 (sin((worldPosition.x + worldPosition.z) * 1.5) + 1) * 0.5) * 1;

    // TODO: add your vertex shader code here.

    return output;
}
 
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	

    float4 sample = tex2D(color, input.UV);

	float4 result = sample;//DoLighting( sample, input.Normal);
	clip(sample.w - 0.75f);
	//result.w = sample.w;

	//float blend = saturate( abs( 1 - dot(input.Normal, float3(1,0,0)) ) * 5);
	//result.xyz = abs( dot(input.Normal, float3(0,0,1)) );
	//result = blend;
	//result.w = blend;
result.rgb =  lerp(result.rgb, result.rgb*0.2 + ambient * 0.8, (input.UV.y));//+= input.Color * 0.4;
	//result.w *= saturate (abs(dot(ViewRight, input.Normal)) * 5 );
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
