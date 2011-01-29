#include "..\shader_shared.fx"
			
float4 PixelShaderFunction(float4 position : POSITION0, 
	float3 aux : TEXCOORD1,
	float2 uv : TEXCOORD0) : COLOR0
{
	float alpha = aux.x;
	float4 samp = tex2D(color, uv);

	samp.w *= alpha;
	samp = 0.0f;
	return samp;
 }

void VertexShaderFunction(float4 position : POSITION0, 
	float3 normal : NORMAL,
	 float2 texCoord : TEXCOORD0, 
	out float4 opos : POSITION0,
	out float2 otexCoord : TEXCOORD0,
	out float3 oaux: TEXCOORD1) 
{
	
    float4 worldPosition = position;
	
	float2 coord = texCoord;
	
	float angle = 0; 
	float2x2 rotate = { cos(angle), -sin(angle),
						sin(angle), cos(angle) };

	coord = mul(coord - 0.5f, rotate) * 5;
	
	float3 haxis = View._m00_m10_m20 * coord.x;
	float3 vaxis = View._m01_m11_m21 * coord.y;
	
	float3 offset = haxis + vaxis;
	

	worldPosition.xyz += offset;

    float4 viewPosition = mul(worldPosition, View);
    opos= mul(viewPosition, Projection);
	oaux = normal;
	otexCoord = texCoord * 0.5 + 0.5; 
	otexCoord.y = 1 - otexCoord.y;
}

technique Techniq
{
	pass P0
	{
		vertexShader = compile vs_2_0 VertexShaderFunction();
		pixelShader = compile ps_2_0 PixelShaderFunction();
	}
}    
