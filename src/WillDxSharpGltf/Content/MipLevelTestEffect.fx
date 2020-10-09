#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0 //_level_9_1
	#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif

matrix WorldViewProjection;

matrix World;
matrix View;
matrix Projection;
float3 CameraPosition;
int testValue1;
int testValue2;

Texture2D TextureA; // primary texture.
sampler2D TextureSamplerDiffuse = sampler_state
{
    texture = <TextureA>;
};
TextureCube CubeMap;
//sampler CubeMapSampler = sampler_state
samplerCUBE CubeMapSampler = sampler_state
//sampler CubeMapSampler = sampler_state
{
    texture = <CubeMap>;
    magfilter = Linear;
    minfilter = Linear;
    mipfilter = Linear;
    AddressU = clamp;
    AddressV = clamp;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float3 Position3D : TEXCOORD1;
    float3 Normal3D : TEXCOORD2;
    float2 TexureCoordinate : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 pos = mul(input.Position, World);
    output.TexureCoordinate = input.TexureCoordinate;
    output.Position3D = pos.xyz;
    output.Normal3D = normalize(mul(input.Normal, World));
    float4x4 vp = mul(View, Projection);
    output.Position = mul(pos, vp);
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 baseColor = tex2D(TextureSamplerDiffuse, input.TexureCoordinate); 
    clip(baseColor.a - .01f); // just straight clip super low alpha.
    float3 P = input.Position3D;
    float3 N = normalize(input.Normal3D.xyz);
    float3 V = normalize(CameraPosition - input.Position3D);
    float NdotV = max(0.0, dot(N, V));
    float3 R = 2.0 * NdotV * N - V; 
    float4 envMapColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
    switch (abs(testValue2))
    {
        case 0:
            envMapColor = texCUBEbias(CubeMapSampler, float4(R, testValue1));
            break;
        case 1:
            envMapColor = texCUBElod(CubeMapSampler, float4(R, testValue1));
            break;
        
        default:
            envMapColor = texCUBEbias(CubeMapSampler, float4(R, testValue1));
            break;
    }
    return float4(envMapColor.rgb, 1.0f);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};