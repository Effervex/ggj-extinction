#include "..\shader_shared.fx"
			
float4 PixelShaderFunction(float4 position : POSITION0, 
	float3 aux : TEXCOORD1,
	float3 col : TEXCOORD2,
	float2 uv : TEXCOORD0) : COLOR0
{
	float alpha = aux.z;
	float4 samp = tex2D(color, uv);// * alpha;
	samp.rgb *= col;
	//samp.w = aux.z;//(samp.w* saturate( alpha - 0.1));
	//samp.w=0;
	//samp = 1;
	return samp;
 }

void VertexShaderFunction(float4 position : POSITION0, 
	float3 normal : NORMAL,
	 float2 texCoord : TEXCOORD0, 
	out float4 opos : POSITION0,
	out float2 otexCoord : TEXCOORD0,
	out float3 oaux: TEXCOORD1,
	out float3 col : TEXCOORD2) 
{
	oaux = normal;

	col = lerp( 
		lerp (
		float3(1,.6,0),
		float3(.4,1,0), 
		normal.x * 0.5),
		float3(0,0,1),
		saturate( normal.x - 0.5) * 0.5);


    float4 worldPosition = position;
	
	float2 coord = texCoord;
	
	float angle = 0; 
	float2x2 rotate = { cos(angle), -sin(angle),
						sin(angle), cos(angle) };

	
	coord = mul(coord - 0.5f, rotate) * normal.y;
	
	float3 haxis = View._m00_m10_m20 * coord.x;
	float3 vaxis = View._m01_m11_m21 * coord.y;
	
	float3 offset = haxis + vaxis;
	

	worldPosition.xyz += offset;

    float4 viewPosition = mul(worldPosition, View);
    opos= mul(viewPosition, Projection);
	//oaux = normal;
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