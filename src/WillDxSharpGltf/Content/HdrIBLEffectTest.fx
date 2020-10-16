#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif


#define PI 3.14159265359f

int FaceToMap;

Texture2D Texture; // primary texture.
sampler2D TextureSamplerDiffuse = sampler_state
{
    texture = <Texture>;
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

struct HdrToCubeMapVertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexureCoordinate : TEXCOORD0;
};

struct HdrToCubeMapVertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Position3D : TEXCOORD1;
    float2 TexureCoordinate : TEXCOORD0;
};

//____________________________________
// functions
//____________________________________

//var fuv = new Vector2(x, y) / faceWh;
//var v = UvFaceToCubeMapVector(fuv, index);
//var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);

// I made up to do this tranform because i couldn't find the code to do it anywere.
// Ok so what people are doing regularly is like a matrix view transform i think im not sure that is actually any better.
float3 UvFaceToCubeMapVector(float2 uv, int faceIndex)
{
    float u = uv.x ;
    float v = uv.y ;
    float3 dir = float3(0.0f, 0.0f, 1.0f);
    switch (abs(faceIndex))
    {
    case 0: // FACE_FORWARD: CubeMapFace.NegativeZ
        dir = float3(-1.0f, v, u);
        break;
    case 2: //FACE_LEFT: CubeMapFace.NegativeX
        dir = float3(u, v, 1.0f);
        break;
    case 3: //FACE_BACK: CubeMapFace.PositiveZ
        dir = float3(1.0f, v, -u);
        break;
    case 5: //FACE_RIGHT: CubeMapFace.PositiveX
        dir = float3(-u, v, -1.0f);
        break;

    case 1: //FACE_TOP: CubeMapFace.PositiveY
        dir = float3(-v, 1.0f, -u);
        break;
    case 4: //FACE_BOTTOM : CubeMapFace.NegativeY
        dir = float3(v, -1.0f, u);
        break;

    default:
        dir = float3(-1.0f, -1.0f, -1.0f); // na
        break;
    }
    //dir = new Vector3(dir.z, -dir.y, dir.x); // invert
    dir = normalize(dir);
    return dir;
}

float2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(float3 normal)
{
    float2 uv = float2((float)atan2(normal.z, normal.x), (float)asin(normal.y));
    float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
	uv = uv * INVERT_ATAN + float2(0.5f, 0.5f);
	return uv;
}

float4 CubeToFaceCopy(float3 pixelpos, int face) 
{
    float3 n = UvFaceToCubeMapVector(pixelpos, face);
    return  float4(texCUBElod(CubeMapSampler, float4(n, 0.0f)).rgb, 1.0f);
}


// http://www.codinglabs.net/article_physically_based_rendering.aspx
//
//  This isn't right at all needs to be tweeked or i need a better algorithm.
//
float4 GetIrradiance(float2 pixelpos, int faceToMap)
{
    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);

    //float3 normal = normalize(float3(pixelpos.xy, 1));
    //if (cubeFace == 2)
    //    normal = normalize(float3(pixelpos.x,  1, -pixelpos.y));
    //else if (cubeFace == 3)
    //    normal = normalize(float3(pixelpos.x, -1, pixelpos.y));
    //else if (cubeFace == 0)
    //    normal = normalize(float3(1, pixelpos.y,-pixelpos.x));
    //else if (cubeFace == 1)
    //    normal = normalize(float3(-1, input.InterpolatedPosition.y, input.InterpolatedPosition.x));
    //else if (cubeFace == 5)
    //    normal = normalize(float3(-input.InterpolatedPosition.x, input.InterpolatedPosition.y, -1));

    float3 up = float3(0,1,0);
    float3 right = normalize(cross(up,normal));
    up = cross(normal,right);

    float3 sampledColour = float3(0,0,0);
    float index = 0;
    float stepPhi = 0.25f;//0.025f;
    float stepTheta = 0.1f;//0.1f;
    float thetaAngle = .4f; // 1.57f
    for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
    {
        for (float theta = 0; theta < thetaAngle; theta += 0.1f) // y 
        {
            float3 temp = cos(phi) * right + sin(phi) * up;
            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp , 0);
            sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb * cos(theta) * sin(theta);
            index++;
            stepTheta = stepTheta * 1.2f;
        }
    }
    return float4(PI * sampledColour / index, 1.0f );
}

/*
            float falloff = 1.0f - (theta / thetaAngle);
            sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb * falloff; // *cos(theta)* sin(theta);
            index += falloff;
            stepTheta = stepTheta * 1.05f;
        }
    }
    return float4(sampledColour / index, 1.0f );
*/

//float4 GetIrradiance(float3 pixelpos, int face) 
//{
//    return irradiance;
//}

//____________________________________
// shaders
//____________________________________




// Copy 2d spherical hdr to enviromental cubemap

HdrToCubeMapVertexShaderOutput HdrToEnvCubeMapVS(in HdrToCubeMapVertexShaderInput input)
{
    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
	output.Position = input.Position;
    output.Position3D = input.Position;
	return output;
}

float4 HdrToEnvCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    float3 v = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    float2 uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
    float2 texcoords = float2(uv.x, 1.0f - uv.y);  // raw dx transform
	float4 color = float4( tex2D(TextureSamplerDiffuse, texcoords).rgb, 1.0f);
	return color;
}

technique HdrToEnvCubeMap
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL 
            HdrToEnvCubeMapVS();
		PixelShader = compile PS_SHADERMODEL 
            HdrToEnvCubeMapPS();
	}
};


// Generate diffuse illumination map from enviroment cubemap.

HdrToCubeMapVertexShaderOutput HdrToDiffuseIlluminationCubeMapVS(in HdrToCubeMapVertexShaderInput input)
{
    HdrToCubeMapVertexShaderOutput output = (HdrToCubeMapVertexShaderOutput)0;
    output.Position = input.Position;
    output.Position3D = input.Position;
    return output;
}

float4 HdrToDiffuseIlluminationCubeMapPS(HdrToCubeMapVertexShaderOutput input) : COLOR
{
    /*
    float3 v = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    float4 dirAndFace = float4(v, FaceToMap);
    float4 color = float4(texCUBElod(CubeMapSampler, dirAndFace).rgb, 1.0f);
    */

    float4 color = GetIrradiance(input.Position3D, FaceToMap);

    //float4 color = CubeToFaceCopy(input.Position3D, FaceToMap);
    return color;
}

technique EnvCubemapToDiffuseIlluminationCubeMap
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL
            HdrToDiffuseIlluminationCubeMapVS();
        PixelShader = compile PS_SHADERMODEL
            HdrToDiffuseIlluminationCubeMapPS();
    }
};

// Not much reference for shader implementation of a illumination map.
//
// references improvements on importance sampling and hammersly.
// https://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf  importance sampling as well.
// https://placeholderart.wordpress.com/2015/07/28/implementation-notes-runtime-environment-map-filtering-for-image-based-lighting/
// https://youtu.be/j-A0mwsJRmk
// https://github.com/TheCherno/Sparky  sparky https://github.com/TheCherno/Sparky/tree/master/Sparky-core/src/sp/graphics
// https://www.tobias-franke.eu/log/2014/03/30/notes_on_importance_sampling.html
// https://www.tobias-franke.eu/log/tags.html#ImportanceSampling
// https://learnopengl.com/PBR/IBL/Diffuse-irradiance
// https://github.com/Nadrin/PBR/blob/master/data/shaders/hlsl/pbr.hlsl
// https://github.com/Nadrin/PBR/blob/master/data/shaders/hlsl/irmap.hlsl
// https://developer.nvidia.com/gpugems/gpugems2/part-ii-shading-lighting-and-shadows/chapter-10-real-time-computation-dynamic
/*
Precompute as much as possible
The previous change had the unfortunate side effect of adding lots of math to the inner loop. So lets focus the attention there and see if anything can be pre-computed. First, it should be easy to see that we don’t need to be computing the Hammersley sequence for every single texel in the output, they are the same for every pixel. With this knowledge let’s see how the sequence random value is used:

float3 vHalf = ImportanceSampleGGX(vXi, fRoughness, vNormal);

...

float3 ImportanceSampleGGX(float2 vXi, float fRoughness, float3 vNoral)
{
    // Compute the local half vector
    float fA = fRoughness * fRoughness;
    float fPhi = 2.0f * fPI * vXi.x;
    float fCosTheta = sqrt((1.0f - vXi.y) / (1.0f + (fA*fA - 1.0f) * vXi.y));
    float fSinTheta = sqrt(1.0f - fCosTheta * fCosTheta);
    float3 vHalf;
    vHalf.x = fSinTheta * cos(fPhi);
    vHalf.y = fSinTheta * sin(fPhi);
    vHalf.z = fCosTheta;

    // Compute a tangent frame and rotate the half vector to world space
    float3 vUp = abs(vNormal.z) < 0.999f ? float3(0.0f, 0.0f, 1.0f) : float3(1.0f, 0.0f, 0.0f);
    float3 vTangentX = normalize(cross(vUp, vNormal));
    float3 vTangentY = cross(vNormal, vTangentX);
    // Tangent to world space
    return vTangentX * vHalf.x + vTangentY * vHalf.y + vNormal * vHalf.z;
}

Joe DeVries
    // the sample direction equals the hemisphere's orientation
    vec3 normal = normalize(localPos);

    vec3 irradiance = vec3(0.0);

    [...] // convolution code



vec3 irradiance = vec3(0.0);

vec3 up    = vec3(0.0, 1.0, 0.0);
vec3 right = cross(up, normal);
up         = cross(normal, right);

float sampleDelta = 0.025;
float nrSamples = 0.0;
for(float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
{
    for(float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
    {
        // spherical to cartesian (in tangent space)
        vec3 tangentSample = vec3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
        // tangent space to world
        vec3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;

        irradiance += texture(environmentMap, sampleVec).rgb * cos(theta) * sin(theta);
        nrSamples++;
    }
}
irradiance = PI * irradiance * (1.0 / float(nrSamples));


    FragColor = vec4(irradiance, 1.0);




    // sort of works somewhat.
    // http://www.codinglabs.net/article_physically_based_rendering.aspx
//float4 GetIrradiance(float2 pixelpos, int faceToMap) : COLOR
float4 GetIrradiance(float2 pixelpos, int faceToMap)
{
    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);

    //float3 normal = normalize(float3(pixelpos.xy, 1));
    //if (cubeFace == 2)
    //    normal = normalize(float3(pixelpos.x,  1, -pixelpos.y));
    //else if (cubeFace == 3)
    //    normal = normalize(float3(pixelpos.x, -1, pixelpos.y));
    //else if (cubeFace == 0)
    //    normal = normalize(float3(1, pixelpos.y,-pixelpos.x));
    //else if (cubeFace == 1)
    //    normal = normalize(float3(-1, input.InterpolatedPosition.y, input.InterpolatedPosition.x));
    //else if (cubeFace == 5)
    //    normal = normalize(float3(-input.InterpolatedPosition.x, input.InterpolatedPosition.y, -1));

    float3 up = float3(0,1,0);
    float3 right = normalize(cross(up,normal));
    up = cross(normal,right);

    float3 sampledColour = float3(0,0,0);
    float index = 0;
    float stepPhi = 0.25f;//0.025f;
    float stepTheta = 0.1f;//0.1f;
    float thetaAngle = .4f; // 1.57f
    for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
    {
        for (float theta = 0; theta < thetaAngle; theta += 0.1f) // y
        {
            float3 temp = cos(phi) * right + sin(phi) * up;
            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp , 0);
            sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb * cos(theta) * sin(theta);
            index++;
            stepTheta = stepTheta * 1.2f;
        }
    }
    return float4(PI * sampledColour / index, 1.0f );
}





//float3   rotatePointAboutYaxis(float3 p, float q)
//{
//    //z' = z*cos s - x*sin s , x' = z*sin s + x*cos s , y' = y
//    return float3( p.z * cos(q) - p.x * sin(q),  p.y  , p.z * sin(q) + p.x * cos(q) );
//}
//float3   rotatePointAboutZaxis(float3 p, float q)
//{
//    //x' = x*cos s - y*sin s , y' = x*sin s + y*cos s , z' = z
//    return float3 (p.x * cos(q) - p.y * sin(q), p.x * sin(q) + p.y * cos(q) , p.z);
//}

//// ah shit i can't do it like this unless i make a axis angle damnit ill go back and do it with a view matrix  prolly better off that way anyways now that im past the spherical map,
//float4 GetIrradiance(float3 pixelpos, int face) {
//
//    float3 n = UvFaceToCubeMapVector(pixelpos, face);
//    float4 irradiance = float4(0.0f, 0.0f, 0.0f, 1.0f);
//    float sampleDelta = 0.050f;
//    float sum = 1.0f;
//    float specularization = 0.01f;  //1.0f;
//    float HalfAngularArea = 0.24f * PI * specularization;
//    irradiance.rgb += texCUBElod(CubeMapSampler, float4(n, 0.0f)).rgb;
//    for (float theta = -HalfAngularArea; theta < HalfAngularArea; theta += sampleDelta) // spin up or right one of the two doesn't matter don't care.
//    {
//        for (float phi = -PI; phi < PI; phi += sampleDelta)  // roll z
//        {
//            float3 rotposA = rotatePointAboutYaxis(n, theta);
//            float3 rotposB = rotatePointAboutZaxis(rotposA, phi);
//            float4 normcoords = float4(rotposB, 0.0f);
//            irradiance.rgb += texCUBElod(CubeMapSampler, normcoords).rgb; // *((0.5f - theta) * 2.0f);
//            sum++; //=  ((0.5f - theta) *2.0f);
//        }
//    }
//    irradiance.rgb = irradiance.rgb / sum;
//    return irradiance;
//}

////// Hemispherical pixel accumulation at normal i believe this is the base version of what hammersly approximates ...  pulled from Joe DeVries https://learnopengl.com/PBR/IBL/Diffuse-irradiance
////// https://learnopengl.com/code_viewer_gh.php?code=src/6.pbr/2.1.1.ibl_irradiance_conversion/ibl_irradiance_conversion.cpp convolution.
///// http://www.codinglabs.net/article_physically_based_rendering.aspx
//float4 GetIrradianceModifyed(float3 pixelpos, int faceToMap, texture environmentMap)
//{
//    //// the sample direction equals the hemisphere's orientation
//    float3 normal = normalize(pixelpos);
//    float3 irradiance = float3(0.0f, 0.0f, 0.0f);
//
//    // ok if i understand this right then this will work but there will be singularitys at either pole.
//    // maybe he just doesn't care as i guess its not to important if one or two pixels at the poles are messed, or i misunderstand.
//    // on further thought i think its cause he's using the view to mult the pixel im not sure that would stop the singularity k lets test this first.
//    float3 up = float3(0.0f, 1.0f, 0.0f);
//    float3 right = cross(up, normal);
//    up = cross(normal, right);
//
//    float sampleDelta = 0.025f;
//    float nrSamples = 0.0f;
//    for (float phi = 0.0f; phi < 2.0f * PI; phi += sampleDelta)
//    {
//        for (float theta = 0.0f; theta < 0.5f * PI; theta += sampleDelta)
//        {
//            // spherical to cartesian (in tangent space)
//            float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
//            // tangent space to world
//            float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;  // k 2 normals ack ....
//
//
//            irradiance += texCUBElod(CubeMapSampler, sampleVec).rgb * cos(theta) * sin(theta);
//            nrSamples++;
//        }
//    }
//    irradiance = PI * irradiance * (1.0f / float(nrSamples));
//    return float4(irradiance , 1.0f);
//}
*/