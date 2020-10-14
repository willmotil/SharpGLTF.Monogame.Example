#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif


int FaceToMap;

Texture2D Texture; // primary texture.
sampler2D TextureSamplerDiffuse = sampler_state
{
    texture = <Texture>;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Position3D : TEXCOORD1;
    float2 TexureCoordinate : TEXCOORD0;
};

//____________________________________
// functions
//____________________________________


// I only have this shaky method i made up to do this tranform because i couldn't find the code to do it anywere im doubtful this is entirely correct.
float3 UvFaceToCubeMapVector(float2 uv, int faceIndex)
{
    float u = uv.x * 2.0f - 1.0f;
    float v = uv.y * 2.0f - 1.0f;
    float3 dir = float3(0.0f, 0.0f, 1.0f);
    switch (abs(faceIndex))
    {
    case 2: // FACE_BACK:
        dir = float3(-1.0f, v, -u);
        break;
    case 4: //FACE_TOP:
        dir = float3(v, -1.0f, u);
        break;
    case 0: //FACE_LEFT:
        dir = float3(u, v, -1.0f);
        break;
    case 5: //FACE_FRONT:
        dir = float3(1.0f, v, u);
        break;
    case 1: //FACE_BOTTOM:
        dir = float3(-v, 1.0f, u);
        break;
    case 3: //FACE_RIGHT:
        dir = float3(-u, v, 1.0f);
        break;
    default:
        dir = float3(-1.0f, -1.0f, -1.0f); // na
        break;
    }
    //dir = new Vector3(dir.Z, -dir.Y, dir.X);
    dir = normalize(dir);
    return dir;
}

float2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(float3 normal)
{
    float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
    float2 uv = float2((float)atan2(normal.z, normal.x), (float)asin(normal.y));
	uv *= INVERT_ATAN;
	uv += float2(0.5f, 0.5f);
	return uv;
}


//____________________________________
// shaders
//____________________________________



VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = input.Position;
    output.Position3D = input.Position;
	//output.TexureCoordinate = input.TexureCoordinate;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 v = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    float2 uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
    float2 texcoords = float2(uv.x, 1.0f - uv.y);  // raw dx transform
	//float2 texcoords = float2(input.TexureCoordinate.x, 1.0f - input.TexureCoordinate.y);  // raw dx transform
	float4 color = float4( tex2D(TextureSamplerDiffuse, texcoords).rgb, 1.0f);
	return color;
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