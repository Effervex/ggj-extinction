//The view-projection matrix for the camera.
uniform extern float4x4 xViewProjection;

//Added color.
uniform extern float3 xColor;

//Animation
uniform extern float4x3 xBones[72];

// texture
texture xDiffuseTexture;
sampler2D mySampler = sampler_state {
    Texture = (xDiffuseTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

//Struct used to transfer data from the program to the vertex-shader.
struct ProgramToVertex
{
	float4 Pos			: POSITION0;
	float2 TexCoords	: TEXCOORD0;
	float3 Normal		: NORMAL0;
};

struct ProgramToVertexUsingAnimation
{
	float4 Pos			: POSITION0;
	float2 TexCoords	: TEXCOORD0;
	float3 Normal		: NORMAL0;
	int4   boneIndex	: BLENDINDICES0;
    float4 boneWeight	: BLENDWEIGHT0;
};

//Struct used to transfer data from the vertex-shader to the pixel-shader.
struct VertexToPixel
{
	float4 Position			: POSITION;
	float2 rim              : TEXCOORD0;
};

//Struct used to transfer data from the pixel-shader to the rester unit.
struct PixelToFrame
{
	float4 Color 			: COLOR0;
};

VertexToPixel MyVertexAnimatedShader(ProgramToVertexUsingAnimation In)
{
	VertexToPixel Output = (VertexToPixel)0;
	
	// Calculate the final bone transformation matrix
	float4x3 matTransform = xBones[In.boneIndex.x] * In.boneWeight.x;
	matTransform += xBones[In.boneIndex.y] * In.boneWeight.y;
	matTransform += xBones[In.boneIndex.z] * In.boneWeight.z;
	float finalWeight = 1.0f - (In.boneWeight.x + In.boneWeight.y + In.boneWeight.z);
	matTransform += xBones[In.boneIndex.w] * finalWeight;	
	
	Output.Position = float4(mul(In.Pos, matTransform),1);		
	Output.Position = mul(Output.Position, xViewProjection);
	
	half3x3 rotMatrix = (half3x3)matTransform;
	float3 world_Normal = mul(In.Normal,rotMatrix ).xyz;
	float3 normalWorld = normalize(world_Normal);

	float3 camera0 = float3(xViewProjection._14,xViewProjection._24,xViewProjection._34);

	Output.rim = In.TexCoords;

	return Output;
}

//Scene deferred pixel-shader.
PixelToFrame MyPixelShader(VertexToPixel In)
{
	PixelToFrame Output = (PixelToFrame)0;

	//Output.Color = float4(xColor * In.rim,1);
	Output.Color = tex2D(mySampler, In.rim);
	Output.Color.w = 1;

	return Output;
}


technique Animated
{
	pass Pass0
    {
    	VertexShader = compile vs_2_0 MyVertexAnimatedShader();
        PixelShader  = compile ps_2_0 MyPixelShader();
    }
}