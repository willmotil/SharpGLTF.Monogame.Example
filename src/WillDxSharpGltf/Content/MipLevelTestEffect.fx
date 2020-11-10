﻿#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0 //_level_9_1
	#define PS_SHADERMODEL ps_4_0 //_level_9_1
#endif


matrix World;
matrix View;
matrix Projection;
float3 CameraPosition;
int testValue1;
//int testValue2;


//Texture2D TextureA; // primary texture.
//sampler2D TextureSamplerDiffuse = sampler_state
//{
//    texture = <TextureA>;
//};

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
    float4x4 vp = mul(View, Projection);
    float4 pos = mul(input.Position, World);
    float4 norm = mul(input.Normal, World);
    output.Position = mul(pos, vp);
    output.Position3D = mul(pos.xyz, View);
    output.Normal3D = norm.xyz;
    output.TexureCoordinate = input.TexureCoordinate;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    //float4 baseColor = tex2D(TextureSamplerDiffuse, input.TexureCoordinate); 
    ////clip(baseColor.a - .01f); // just straight clip super low alpha.
    //float3 P = input.Position3D;
    float3 N = normalize(input.Normal3D.xyz);
    //float3 V = normalize(CameraPosition - input.Position3D);
    /*float NdotV = max(0.0, dot(N, V));
    float3 R = 2.0 * NdotV * N - V; 
    float4 envMapColor = texCUBElod(CubeMapSampler, float4(R, testValue1));*/
    float4 envMapColor = texCUBElod(CubeMapSampler, float4(N, testValue1));
    return float4(envMapColor.rgb, 1.0f);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL 
            MainVS();
		PixelShader = compile PS_SHADERMODEL 
            MainPS();
	}
};