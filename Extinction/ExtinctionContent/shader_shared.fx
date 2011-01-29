
float4x4 World;
float4x4 View;
float4x4 Projection;
float Time;
sampler	color	: register( s0 );

float4 LightDir = float4(1,-1,1,0);
float3 ambient =float3(.1,.2,0.3);
float4 DoLighting(float4 color, float3 normal) {
	
	float diffuse = max(0, dot( normalize(normal),  normalize(-LightDir)));
	
	color.rgb *= lerp((float3)1, ambient, diffuse);
	return color;
}