// https://www.geeks3d.com/20141201/how-to-rotate-a-vertex-by-a-quaternion-in-glsl/


#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif


#define PI 3.14159265359f
#define ToDegrees 57.295779513f;
#define ToRadians 0.0174532925f;

//typedef enum D3DCUBEMAP_FACES {
//    D3DCUBEMAP_FACE_POSITIVE_X = 0,
//    D3DCUBEMAP_FACE_NEGATIVE_X = 1,
//    D3DCUBEMAP_FACE_POSITIVE_Y = 2,
//    D3DCUBEMAP_FACE_NEGATIVE_Y = 3,
//    D3DCUBEMAP_FACE_POSITIVE_Z = 4,
//    D3DCUBEMAP_FACE_NEGATIVE_Z = 5,
//    D3DCUBEMAP_FACE_FORCE_DWORD = 0xffffffff
//} D3DCUBEMAP_FACES, * LPD3DCUBEMAP_FACES;

int FaceToMap;
float4x4 CubeMapFaceNormal;

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

//____________________________________
// structs
//____________________________________

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

struct FaceStruct
{
    float3 PositionNormal;
    float3 FaceNormal;
    float3 FaceUp;
};

//____________________________________
// functions
//____________________________________

//var fuv = new Vector2(x, y) / faceWh;
//var v = UvFaceToCubeMapVector(fuv, index);
//var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);

// I made this up to do this tranform because i couldn't find the code to do it anywere.
// Ok so what people are doing regularly is like a matrix view transform i think im not sure that is actually any better.
FaceStruct UvFaceToCubeMapVector(float2 pos, int faceIndex)
{
    FaceStruct output = (FaceStruct)0;
    float u = pos.x ;
    float v = pos.y ;
    switch (abs(faceIndex))
    {
        case 1: //FACE_LEFT: CubeMapFace.NegativeX
            output.PositionNormal = float3(-1.0f, v, u);
            output.FaceNormal = float3(-1.0f, 0, 0);
            output.FaceUp = float3(0, 1, 0);
            break;
        case 5: // FACE_FORWARD: CubeMapFace.NegativeZ
            output.PositionNormal = float3(-u, v, -1.0f);
            output.FaceNormal = float3(0, 0, -1.0f);
            output.FaceUp = float3(0, 1, 0);
            break;
        case 0: //FACE_RIGHT: CubeMapFace.PositiveX
            output.PositionNormal = float3(1.0f, v, -u);
            output.FaceNormal = float3(1.0f, 0, 0);
            output.FaceUp = float3(0, 1, 0);
            break;
        case 4: //FACE_BACK: CubeMapFace.PositiveZ
            output.PositionNormal = float3(u, v, 1.0f);
            output.FaceNormal = float3(0, 0, 1.0f);
            output.FaceUp = float3(0, 1, 0);
            break;

        case 2: //FACE_TOP: CubeMapFace.PositiveY
            output.PositionNormal = float3(u, 1.0f, -v);
            output.FaceNormal = float3(0, 1.0f, 0);
            output.FaceUp = float3(0, 0, 1);
            break;
        case 3: //FACE_BOTTOM : CubeMapFace.NegativeY
            output.PositionNormal = float3(u, -1.0f, v);   // dir = float3(v, -1.0f, u);
            output.FaceNormal = float3(0, -1.0f, 0);
            output.FaceUp = float3(0, 0, -1);
            break;

        default:
            output.PositionNormal = float3(-1.0f, v, u); // na
            output.FaceNormal = float3(-1.0f, 0, 0);
            output.FaceUp = float3(0, 1, 0);
            break;
    }
    //output.PositionNormal = new Vector3(output.PositionNormal.z, -output.PositionNormal.y, output.PositionNormal.x); // invert
    output.PositionNormal = normalize(output.PositionNormal);
    return output;
}

//// I made this up to do this tranform because i couldn't find the code to do it anywere.
//// Ok so what people are doing regularly is like a matrix view transform i think im not sure that is actually any better.
//float3 UvFaceToCubeMapVector(float2 pos, int faceIndex)
//{
//    float u = pos.x;
//    float v = pos.y;
//    float3 dir = float3(0.0f, 0.0f, 1.0f);
//    switch (abs(faceIndex))
//    {
//    case 1: //FACE_LEFT: CubeMapFace.NegativeX
//        dir = float3(-1.0f, v, u);
//        break;
//    case 5: // FACE_FORWARD: CubeMapFace.NegativeZ
//        dir = float3(-u, v, -1.0f);
//        break;
//    case 0: //FACE_RIGHT: CubeMapFace.PositiveX
//        dir = float3(1.0f, v, -u);
//        break;
//    case 4: //FACE_BACK: CubeMapFace.PositiveZ
//        dir = float3(u, v, 1.0f);
//        break;
//
//    case 2: //FACE_TOP: CubeMapFace.PositiveY
//        dir = float3(u, 1.0f, -v);
//        break;
//    case 3: //FACE_BOTTOM : CubeMapFace.NegativeY
//        dir = float3(u, -1.0f, v);   // dir = float3(v, -1.0f, u);
//        break;
//
//    default:
//        dir = float3(v, -1.0f, u); // na
//        break;
//    }
//    //dir = new Vector3(dir.z, -dir.y, dir.x); // invert
//    dir = normalize(dir);
//    return dir;
//}

float2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(float3 normal)
{
    float2 uv = float2((float)atan2(-normal.z, normal.x), (float)asin(normal.y));
    float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
	uv = uv * INVERT_ATAN + float2(0.5f, 0.5f);
	return uv;
}

float2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(float3 a_coords)
{
    float pi = 3.141592653589793f;
    float3 a_coords_n = normalize(a_coords);
    float lon = atan2(a_coords_n.z, a_coords_n.x);
    float lat = acos(a_coords_n.y);
    float2 sphereCoords = float2(lon, lat) * (1.0f / pi);
    return float2(sphereCoords.x * 0.5f + 0.5f, 1.0f - sphereCoords.y);
}

float4 CubeToFaceCopy(float3 pixelpos, int face) 
{
    FaceStruct input = UvFaceToCubeMapVector(pixelpos, face);
    float3 n = input.PositionNormal;
    return  float4(texCUBElod(CubeMapSampler, float4(n, 0.0f)).rgb, 1.0f);
}


// http://www.codinglabs.net/article_physically_based_rendering.aspx
//
// F  needs to be tweeked.
//
float4 GetIrradiance(float2 pixelpos, int faceToMap)
{
    //float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);

    //float3 up = float3(normal.z, normal.x, normal.y);// float3(0,1,0);
    //float3 right = normalize(cross(up, normal));
    //up = cross(normal, right);

    //CubeMapFaceNormal

    FaceStruct input = UvFaceToCubeMapVector(pixelpos, faceToMap);
    float3 normal = input.PositionNormal;
    float3 up = input.FaceUp;
    float3 right = normalize(cross(up, input.FaceNormal));
    up = cross(input.FaceNormal, right);

    float3 accumulatedColor = float3(0, 0, 0);
    float totalWeight = 0;
    float3 averagedColor = float3(0, 0, 0);
    float totalSampleCount = 0;

    // to radians from degrees 
    float numberOfSamplesHemisphere = 25; //16.0f; //6.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
    float numberOfSamplesAround = 4; //16.0f; // 32.0f;//12.0f;
    float hemisphereMaxAngle = 45.0f; // 30.0f; //50.0f;
    float minimumAdjustment = 2.1f; //3.5f; //2.1f;
    float mipSampleLevel = 4;
    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // computed
    //float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //computed
    //float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //computed  // z roll

    //for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y 
    //{
    //    float earlyOut = (hemisphereMaxAngleTheta / (theta + 0.01f));
    //    float stepPhi = min( (360.0f / numberOfSamplesAround) * 0.0174532925f * earlyOut, minimumAdjustment); // we step out of our inner loop fast when theta is low, i.e. we are close to the normal.
    //    for (float phi = 0.05; phi < 6.283; phi += stepPhi) // z rot
    //    {

    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians - 0.05f;
    float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians - 0.05f;

    for (float phi = 0.01; phi < 6.283; phi += stepPhi) // z rot.
    {
        for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y 
        {
            float3 temp = cos(phi) * right + sin(phi) * up;
            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp, mipSampleLevel);
            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;
            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
            float NdotS = saturate(dot(normal, sampleVector.rgb));
            float phiMuliplier = 1.0f - ( phi / (5.283f + 1.0f));

            //accumulatedColor += sampledColor;
            //totalWeight++;

            //accumulatedColor += sampledColor * phiMuliplier;
            //totalWeight++;

            accumulatedColor += sampledColor * phiMuliplier;
            totalWeight += phiMuliplier;

            //accumulatedColor += sampledColor * (7.0f - phi);
            //totalWeight += (7.0f - phi);

            //accumulatedColor += sampledColor * NdotS;
            //totalWeight += NdotS;

            //accumulatedColor += sampledColor sin(theta);
            //totalWeight += 1.0f;

            //accumulatedColor += sampledColor * (cos(theta) * sin(theta));
            //totalWeight += cos(theta) * sin(theta);

            //accumulatedColor += sampledColor * (1.0f - NdotS);
            //totalWeight += (1.0f - NdotS);

            //accumulatedColor += sampledColor * (cos(theta) * sin(theta)) * NdotS;
            //totalWeight += cos(theta) * sin(theta) * NdotS;

            //// this is completely blured in fact its so way way too too smoothly blured.
            //accumulatedColor += sampledColor;
            //totalWeight += avg;

            ////// getting abit closer
            //NdotS = NdotS * NdotS ;
            //accumulatedColor += sampledColor * NdotS;
            //totalWeight += avg * NdotS * 3.0f;

            //NdotS = NdotS * NdotS * NdotS * NdotS;
            //float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * NdotS;
            //accumulatedColor += sampledColor * avg;
            //totalWeight += avg ;

            //averagedColor += avg;
            //totalSampleCount++;
        }
        //accumulatedColor += averagedColor;
        //totalWeight += totalSampleCount;
        //averagedColor =0;
        //totalSampleCount =0;
    }
    float3 directColor = texCUBElod(CubeMapSampler, float4(normal, 0)).rgb;
    directColor.rgb = (directColor.r + directColor.g + directColor.b) / 3;
    //float4 final = float4(accumulatedColor, 1.0f);
    float4 final = float4(accumulatedColor / totalWeight, 1.0f);
    //float4 final = float4(PI * accumulatedColor / totalWeight, 1.0f );
    //final.rgb = final.rgb * 0.90f + directColor.brg * 0.10f;
    return final;
}










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
    FaceStruct face = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    float3 v = face.PositionNormal;
    //float3 v = UvFaceToCubeMapVector(input.Position3D, FaceToMap);
    //float3 v = SphericalUvFaceToCubeMapVector(input.Position3D, FaceToMap);
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
// https://dominium.maksw.com/articles/physically-based-rendering-pbr/pbr-part-one/ LOTS of code.
// https://docs.imgtec.com/PBR_with_IBL_for_PVR/topics/Assets/pbr_ibl__the_prefiltered_map.html  according to this its super easy and im just retareded.
// https://youtu.be/j-A0mwsJRmk
// https://github.com/TheCherno/Sparky  sparky https://github.com/TheCherno/Sparky/tree/master/Sparky-core/src/sp/graphics
// https://www.tobias-franke.eu/log/2014/03/30/notes_on_importance_sampling.html
// https://www.tobias-franke.eu/log/tags.html#ImportanceSampling
// https://learnopengl.com/PBR/IBL/Diffuse-irradiance
// https://github.com/Nadrin/PBR/blob/master/data/shaders/hlsl/pbr.hlsl
// https://github.com/Nadrin/PBR/blob/master/data/shaders/hlsl/irmap.hlsl
// https://developer.nvidia.com/gpugems/gpugems2/part-ii-shading-lighting-and-shadows/chapter-10-real-time-computation-dynamic

//Precompute as much as possible
//The previous change had the unfortunate side effect of adding lots of math to the inner loop. So lets focus the attention there and see if anything can be pre-computed. First, it should be easy to see that we don’t need to be computing the Hammersley sequence for every single texel in the output, they are the same for every pixel. With this knowledge let’s see how the sequence random value is used:
//

//
//// I made up to do this tranform because i couldn't find the code to do it anywere.
//// Ok so what people are doing regularly is like a matrix view transform i think im not sure that is actually any better.
//float3 UvFaceToCubeMapVector(float2 uv, int faceIndex)
//{
//    float u = uv.x ;
//    float v = uv.y ;
//    float3 dir = float3(0.0f, 0.0f, 1.0f);
//    switch (abs(faceIndex))
//    {
//        case 1: //FACE_LEFT: CubeMapFace.NegativeX
//            dir = float3(-1.0f, v, u);
//            break;
//        case 5: // FACE_FORWARD: CubeMapFace.NegativeZ
//            dir = float3(-u, v, -1.0f);
//            break;
//        case 0: //FACE_RIGHT: CubeMapFace.PositiveX
//            dir = float3(1.0f, v, -u);
//            break;
//        case 4: //FACE_BACK: CubeMapFace.PositiveZ
//            dir = float3(u, v, 1.0f);
//            break;
//
//        case 2: //FACE_TOP: CubeMapFace.PositiveY
//            dir = float3(-v, 1.0f, -u);
//            break;
//        case 3: //FACE_BOTTOM : CubeMapFace.NegativeY
//            dir = float3(v, -1.0f, -u);   // dir = float3(v, -1.0f, u);
//            break;
//
//        default:
//            dir = float3(v, -1.0f, u); // na
//            break;
//    }
//    //dir = new Vector3(dir.z, -dir.y, dir.x); // invert
//    dir = normalize(dir);
//    return dir;
//}

//float3 vHalf = ImportanceSampleGGX(vXi, fRoughness, vNormal);
//
//...
//
//float3 ImportanceSampleGGX(float2 vXi, float fRoughness, float3 vNoral)
//{
//    // Compute the local half vector
//    float fA = fRoughness * fRoughness;
//    float fPhi = 2.0f * fPI * vXi.x;
//    float fCosTheta = sqrt((1.0f - vXi.y) / (1.0f + (fA*fA - 1.0f) * vXi.y));
//    float fSinTheta = sqrt(1.0f - fCosTheta * fCosTheta);
//    float3 vHalf;
//    vHalf.x = fSinTheta * cos(fPhi);
//    vHalf.y = fSinTheta * sin(fPhi);
//    vHalf.z = fCosTheta;
//
//    // Compute a tangent frame and rotate the half vector to world space
//    float3 vUp = abs(vNormal.z) < 0.999f ? float3(0.0f, 0.0f, 1.0f) : float3(1.0f, 0.0f, 0.0f);
//    float3 vTangentX = normalize(cross(vUp, vNormal));
//    float3 vTangentY = cross(vNormal, vTangentX);
//    // Tangent to world space
//    return vTangentX * vHalf.x + vTangentY * vHalf.y + vNormal * vHalf.z;
//}
//
//Joe DeVries
//    // the sample direction equals the hemisphere's orientation
//    vec3 normal = normalize(localPos);
//
//    vec3 irradiance = vec3(0.0);
//
//    [...] // convolution code
//
//
//
//vec3 irradiance = vec3(0.0);
//
//vec3 up    = vec3(0.0, 1.0, 0.0);
//vec3 right = cross(up, normal);
//up         = cross(normal, right);
//
//float sampleDelta = 0.025;
//float nrSamples = 0.0;
//for(float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
//{
//    for(float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
//    {
//        // spherical to cartesian (in tangent space)
//        vec3 tangentSample = vec3(sin(theta) * cos(phi),  sin(theta) * sin(phi), cos(theta));
//        // tangent space to world
//        vec3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;
//
//        irradiance += texture(environmentMap, sampleVec).rgb * cos(theta) * sin(theta);
//        nrSamples++;
//    }
//}
//irradiance = PI * irradiance * (1.0 / float(nrSamples));
//
//
//    FragColor = vec4(irradiance, 1.0);



//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//    //float3 normal = normalize(input.position.xyz);
//
//    float3 irradiance = float3(0.0f, 0.0f, 0.0f);
//
//    float3 up = float3(normal.z, normal.x, normal.y);// float3(0,1,0);
//    //float3 up = float3(0, 1, 0); //g_upVectorVal;
//    float3 right = cross(up, normal);
//    up = cross(normal, right);
//
//    float sampleDelta = 0.55f; //0.025f;
//    float nrSamples = 0.0f;
//    float hpi = 3.14159265359f * 0.5f;
//    float fpi = 3.14159265359f * 4.0f;
//    for (float phi = 0; phi < fpi; phi += sampleDelta)
//    {
//        for (float theta = 0; theta < hpi; theta += sampleDelta)
//        {
//            float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
//            float3 sampleVec = (tangentSample.x * right) + (tangentSample.y * up) + (tangentSample.z * normal);
//
//            irradiance += texCUBElod(CubeMapSampler, float4(sampleVec, 3)).rgb * cos(theta) * sin(theta);
//            nrSamples += 1.0f * cos(theta)* sin(theta);
//        }
//    }
//    irradiance = irradiance * (1.0f / nrSamples);
//    /*irradiance = PI * irradiance * (1.0f / nrSamples);*/
//    return float4(irradiance, 1.0f);
//}

    // some dudes version of joes open gl shader  https://computergraphics.stackexchange.com/questions/8612/weirdly-looking-diffuse-irradiance-map


//    // A sort of works somewhat.
//    // http://www.codinglabs.net/article_physically_based_rendering.aspx
//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    float3 up = float3(0,1,0);
//    float3 right = normalize(cross(up,normal));
//    up = cross(normal,right);
//
//    float3 sampledColour = float3(0,0,0);
//    float index = 0;
//    float thetaMaxAngle = 0.50f; // 1.57f;
//    float stepTheta = 0.05f;  //0.1f;
//    float stepPhi = 0.050f;  //0.025f;
//
//    for (float theta = 0; theta < thetaMaxAngle; theta += stepTheta) // y
//    {
//        for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
//        {
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp , 0);
//            sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb * sin(theta);  //* cos(theta); // *sin(theta);
//            index+= 1.0f;
//        }
//    }
//    return float4(PI * sampledColour / index, 1.0f ); //  float4(PI * sampledColour / index, 1.0f );
//}

//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    //float3 normal = normalize(float3(pixelpos.xy, 1));
//    //if (cubeFace == 2)
//    //    normal = normalize(float3(pixelpos.x,  1, -pixelpos.y));
//    //else if (cubeFace == 3)
//    //    normal = normalize(float3(pixelpos.x, -1, pixelpos.y));
//    //else if (cubeFace == 0)
//    //    normal = normalize(float3(1, pixelpos.y,-pixelpos.x));
//    //else if (cubeFace == 1)
//    //    normal = normalize(float3(-1, input.InterpolatedPosition.y, input.InterpolatedPosition.x));
//    //else if (cubeFace == 5)
//    //    normal = normalize(float3(-input.InterpolatedPosition.x, input.InterpolatedPosition.y, -1));
//
//    float3 up = float3(0,1,0);
//    float3 right = normalize(cross(up,normal));
//    up = cross(normal,right);
//
//    float3 sampledColour = float3(0,0,0);
//    float index = 0;
//    float stepPhi = 0.25f; //0.025f;
//    float stepTheta = 0.1f; //0.1f;
//    float thetaAngle = .4f; // 1.57f
//    for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
//    {
//        for (float theta = 0; theta < thetaAngle; theta += stepTheta) // y
//        {
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp , 0);
//            sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb * cos(theta) * sin(theta);
//            index++;
//            //stepTheta = stepTheta * 1.2f; //this causes banding i wouldn't think it would but it does.
//        }
//    }
//    return float4(PI * sampledColour / index, 1.0f );
//}





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

//// Frozen
//// http://www.codinglabs.net/article_physically_based_rendering.aspx
////
////  This isn't right at all needs to be tweeked or i need a better algorithm.
//// A
////
//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    float3 up = float3(0, 1, 0);
//    float3 right = normalize(cross(up, normal));
//    up = cross(normal, right);
//
//    float3 sampledColour = float3(0, 0, 0);
//    float index = 0;
//
//    // to radians from degrees
//    float numberOfSamplesHemisphere = 24.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
//    float numberOfSamplesAround = 16.0f;
//    float hemisphereMaxAngle = 45.0f;
//    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // 30;  1.57f // hemisphere
//    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //2.5f * ToRadians;  // 2.5f     // y dist
//    float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //2.85f * ToRadians; // 2.85f  // z roll
//
//    for (float theta = 0; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y
//    {
//        for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
//        {
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp, 0);
//            //sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb; // *sin(theta);   // * cos(theta) * sin(theta);  // * sin(theta);  //* cos(theta); // *sin(theta);
//            sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb * sin(theta);
//            //sampledColour += texCUBElod(CubeMapSampler, sampleVector).rgb * (cos(theta) * sin(theta));
//            index += 1.0f;
//        }
//    }
//    //return float4( sampledColour / index, 1.0f);
//    return float4(PI * sampledColour / index, 1.0f);
//}

//// http://www.codinglabs.net/article_physically_based_rendering.aspx
////
//// B  needs to be tweeked.
////
//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    float3 up = float3(0, 1, 0);
//    float3 right = normalize(cross(up, normal));
//    up = cross(normal, right);
//
//    float3 accumulatedColour = float3(0, 0, 0);
//    float totalWeight = 0;
//
//    // to radians from degrees
//    float numberOfSamplesHemisphere = 16.0f;// 16.0f; //6.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
//    float numberOfSamplesAround = 16.0f; // 32.0f;//12.0f;
//    float hemisphereMaxAngle = 45.0f; // 30.0f; //50.0f;
//    float minimumAdjustment = 3.5f; //2.1f;
//    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // 30;  1.57f // hemisphere
//    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //2.5f * ToRadians;  // 2.5f     // y dist
//    //float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //2.85f * ToRadians; // 2.85f  // z roll
//
//    for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y
//    {
//        float earlyOut = (hemisphereMaxAngleTheta / (theta + 0.01f));
//        float stepPhi = min((360.0f / numberOfSamplesAround) * 0.0174532925f * earlyOut, minimumAdjustment); // we step out of our inner loop fast when theta is low i.e. we are close to the normal.
//        for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
//        {
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp, 0);
//
//            float NdotS = saturate(dot(normal, sampleVector.rgb));
//            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;
//
//            accumulatedColour += sampledColor * NdotS;
//            totalWeight += NdotS;
//
//            //sampledColour = texCUBElod(CubeMapSampler, sampleVector).rgb * sin(theta);
//            //sampledColour = texCUBElod(CubeMapSampler, sampleVector).rgb * (cos(theta) * sin(theta));
//            //accumulatedColour += sampledColor;
//            //totalWeight += 1.0f;
//
////            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
////            accumulatedColour += sampledColor * avg;
////            totalWeight += avg;
//
//            //  Ok i believe the reason this is imbalanced is because of the geometry of the samples themselves.
//            //  When we sample close to the normal in a circle around it there are a lot of samples in the same place basically this makes the number of them combined actually too similar and compounded.
//            //  Such that curve of the samples near the normal are actually far higher near the normal then just 1.
//            //  So i need a function to balance close samples towards one or i need less samples near the normal.
//
//
//        }
//    }
//    //return float4(accumulatedColour, 1.0f);
//    return float4(accumulatedColour / totalWeight, 1.0f);
//    //return float4(PI * accumulatedColour / totalWeight, 1.0f );
//}

//// planetary haze
//
//// http://www.codinglabs.net/article_physically_based_rendering.aspx
////
//// C  needs to be tweeked.
////
//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    float3 up = float3(0,1,0);
//    float3 right = normalize(cross(up,normal));
//    up = cross(normal,right);
//
//    float3 accumulatedColour = float3(0, 0, 0);
//    float totalWeight = 0;
//
//    // to radians from degrees
//    float numberOfSamplesHemisphere = 16.0f;// 16.0f; //6.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
//    float numberOfSamplesAround = 16.0f; // 32.0f;//12.0f;
//    float hemisphereMaxAngle = 85.0f; // 30.0f; //50.0f;
//    float minimumAdjustment = 3.5f; //2.1f;
//    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // 30;  1.57f // hemisphere
//    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //2.5f * ToRadians;  // 2.5f     // y dist
//    //float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //2.85f * ToRadians; // 2.85f  // z roll
//
//    for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y
//    {
//        float earlyOut = (hemisphereMaxAngleTheta / (theta + 0.01f));
//        float stepPhi = min( (360.0f / numberOfSamplesAround) * 0.0174532925f * earlyOut, minimumAdjustment); // we step out of our inner loop fast when theta is low i.e. we are close to the normal.
//        for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
//        {
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp , 0);
//
//            float NdotS = saturate( dot(normal, sampleVector.rgb) );
//            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;
//
//            //accumulatedColour += sampledColor * NdotS;
//            //totalWeight += NdotS;
//
//            //sampledColour = texCUBElod(CubeMapSampler, sampleVector).rgb * sin(theta);
//            //sampledColour = texCUBElod(CubeMapSampler, sampleVector).rgb * (cos(theta) * sin(theta));
//            //accumulatedColour += sampledColor;
//            //totalWeight += 1.0f;
//
//            //float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
//            //accumulatedColour += sampledColor * avg;
//            //totalWeight += avg;
//
//
//            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
//            accumulatedColour += sampledColor * avg;
//            totalWeight += avg;
//
//            //  Ok i believe the reason this is imbalanced is because of the geometry of the samples themselves.
//            //  When we sample close to the normal in a circle around it there are a lot of samples in the same place basically this makes the number of them combined actually too similar and compounded.
//            //  Such that curve of the samples near the normal are actually far higher near the normal then just 1.
//            //  So i need a function to balance close samples towards one or i need less samples near the normal.
//
//
//        }
//    }
//    //return float4(accumulatedColour, 1.0f);
//    return float4( accumulatedColour / totalWeight, 1.0f);
//    //return float4(PI * accumulatedColour / totalWeight, 1.0f );
//}
//
//
//// http://www.codinglabs.net/article_physically_based_rendering.aspx
////
//// D  SUPER BLURRED
////
//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    float3 up = float3(0,1,0);
//    float3 right = normalize(cross(up,normal));
//    up = cross(normal,right);
//
//    float3 accumulatedColour = float3(0, 0, 0);
//    float totalWeight = 0;
//
//    // to radians from degrees
//    float numberOfSamplesHemisphere = 16.0f;// 16.0f; //6.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
//    float numberOfSamplesAround = 16.0f; // 32.0f;//12.0f;
//    float hemisphereMaxAngle = 85.0f; // 30.0f; //50.0f;
//    float minimumAdjustment = 3.5f; //2.1f;
//    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // 30;  1.57f // hemisphere
//    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //2.5f * ToRadians;  // 2.5f     // y dist
//    //float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //2.85f * ToRadians; // 2.85f  // z roll
//
//    for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y
//    {
//        float earlyOut = (hemisphereMaxAngleTheta / (theta + 0.01f));
//        float stepPhi = min( (360.0f / numberOfSamplesAround) * 0.0174532925f * earlyOut, minimumAdjustment); // we step out of our inner loop fast when theta is low i.e. we are close to the normal.
//        for (float phi = 0; phi < 6.283; phi += stepPhi) // z rot
//        {
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp , 0);
//
//            float NdotS = saturate( dot(normal, sampleVector.rgb) );
//            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;
//
//            //accumulatedColour += sampledColor * NdotS;
//            //totalWeight += NdotS;
//
//            //sampledColour = texCUBElod(CubeMapSampler, sampleVector).rgb * sin(theta);
//            //sampledColour = texCUBElod(CubeMapSampler, sampleVector).rgb * (cos(theta) * sin(theta));
//            //accumulatedColour += sampledColor;
//            //totalWeight += 1.0f;
//
//            //float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
//            //accumulatedColour += sampledColor * avg;
//            //totalWeight += avg;
//
//            // this is completely blured in fact its so way way too too smoothly blured.
//            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
//            accumulatedColour += sampledColor;
//            totalWeight += avg;
//
//            //float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
//            //accumulatedColour += sampledColor;
//            //totalWeight += avg;
//
//            //  Ok i believe the reason this is imbalanced is because of the geometry of the samples themselves.
//            //  When we sample close to the normal in a circle around it there are a lot of samples in the same place basically this makes the number of them combined actually too similar and compounded.
//            //  Such that curve of the samples near the normal are actually far higher near the normal then just 1.
//            //  So i need a function to balance close samples towards one or i need less samples near the normal.
//
//
//        }
//    }
//    //return float4(accumulatedColour, 1.0f);
//    return float4( accumulatedColour / totalWeight, 1.0f);
//    //return float4(PI * accumulatedColour / totalWeight, 1.0f );
//}
//
//// http://www.codinglabs.net/article_physically_based_rendering.aspx
////
//// E  needs to be tweeked.
////
//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    float3 up = float3(normal.z, normal.x, normal.y);// float3(0,1,0);
//    float3 right = normalize(cross(up,normal));
//    up = cross(normal,right);
//
//    float3 accumulatedColor = float3(0, 0, 0);
//    float totalWeight = 0;
//    float3 averagedColor = float3(0, 0, 0);
//    float totalSampleCount = 0;
//
//    // to radians from degrees
//    float numberOfSamplesHemisphere = 40; //16.0f;// 16.0f; //6.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
//    float numberOfSamplesAround = 4; //16.0f; // 32.0f;//12.0f;
//    float hemisphereMaxAngle = 20.0f; // 30.0f; //50.0f;
//    float minimumAdjustment = 0.1f; //3.5f; //2.1f;
//    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // computed
//    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //computed
//    //float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //computed  // z roll
//
//    for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y
//    {
//        float earlyOut = (hemisphereMaxAngleTheta / (theta + 0.01f));
//        float stepPhi = min( (360.0f / numberOfSamplesAround) * 0.0174532925f * earlyOut, minimumAdjustment); // we step out of our inner loop fast when theta is low i.e. we are close to the normal.
//        for (float phi = 0.05; phi < 6.283; phi += stepPhi) // z rot
//        {
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp , 0);
//            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;
//            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
//            float NdotS = saturate( dot(normal, sampleVector.rgb) );
//            float phiMuliplier =  phi / 6.283f + 1.0f;
//
//            //accumulatedColor += sampledColor * (7.0f - phi);
//            //totalWeight += (7.0f - phi);
//
//            //accumulatedColor += sampledColor * NdotS;
//            //totalWeight += NdotS;
//
//            //accumulatedColor += sampledColor sin(theta);
//            //totalWeight += 1.0f;
//
//            accumulatedColor += sampledColor * (cos(theta) * sin(theta));
//            totalWeight += cos(theta) * sin(theta);
//
//            //accumulatedColor += sampledColor * (1.0f - NdotS);
//            //totalWeight += (1.0f - NdotS);
//
//
//            //accumulatedColor += sampledColor * (cos(theta) * sin(theta)) * NdotS;
//            //totalWeight += cos(theta) * sin(theta) * NdotS;
//
//            //// this is completely blured in fact its so way way too too smoothly blured.
//            //accumulatedColor += sampledColor;
//            //totalWeight += avg;
//
//            //// getting abit closer
//            NdotS = NdotS * NdotS ;
//            accumulatedColor += sampledColor * NdotS;
//            totalWeight += avg * NdotS * 3.0f;
//
//
//            //NdotS = NdotS * NdotS * NdotS * NdotS;
//            //float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * NdotS;
//            //accumulatedColor += sampledColor * avg;
//            //totalWeight += avg ;
//
//            //  Ok i believe the reason this is imbalanced is because of the geometry of the samples themselves.
//            //  When we sample close to the normal in a circle around it there are a lot of samples in the same place basically this makes the number of them combined actually too similar and compounded.
//            //  Such that curve of the samples near the normal are actually far higher near the normal then just 1.
//            //  So i need a function to balance close samples towards one or i need less samples near the normal.
//
//            //averagedColor += avg;
//            //totalSampleCount++;
//        }
//        //accumulatedColor += averagedColor;
//        //totalWeight += totalSampleCount;
//        //averagedColor =0;
//        //totalSampleCount =0;
//    }
//    float3 directColor = texCUBElod(CubeMapSampler, float4(normal, 0)).rgb;
//    directColor.rgb = (directColor.r + directColor.g + directColor.b) / 3;
//    //float4 final = float4(accumulatedColor, 1.0f);
//    float4 final = float4( accumulatedColor / totalWeight, 1.0f);
//    //float4 final = float4(PI * accumulatedColor / totalWeight, 1.0f );
//    final.rgb = final.rgb * 0.75f + directColor.brg * 0.25f;
//    return final;
//}
//
//// http://www.codinglabs.net/article_physically_based_rendering.aspx
////
//// E  needs to be tweeked.
////
//float4 GetIrradiance(float2 pixelpos, int faceToMap)
//{
//    float3 normal = UvFaceToCubeMapVector(pixelpos, faceToMap);
//
//    float3 up = float3(normal.z, normal.x, normal.y);// float3(0,1,0);
//    float3 right = normalize(cross(up, normal));
//    up = cross(normal, right);
//
//    float3 accumulatedColor = float3(0, 0, 0);
//    float totalWeight = 0;
//    float3 averagedColor = float3(0, 0, 0);
//    float totalSampleCount = 0;
//
//    // to radians from degrees
//    float numberOfSamplesHemisphere = 15; //16.0f;// 16.0f; //6.0f;  // seems to be a ratio that affects quality between the hemisphere and circular sampling direction, maybe i should try to use a spiral like on the golden ratio.
//    float numberOfSamplesAround = 30; //16.0f; // 32.0f;//12.0f;
//    float hemisphereMaxAngle = 45.0f; // 30.0f; //50.0f;
//    float minimumAdjustment = 2.1f; //3.5f; //2.1f;
//    float mipSampleLevel = 4;
//    float hemisphereMaxAngleTheta = hemisphereMaxAngle * ToRadians; // computed
//    //float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians;   //computed
//    //float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians;    //computed  // z roll
//
//    //for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y
//    //{
//    //    float earlyOut = (hemisphereMaxAngleTheta / (theta + 0.01f));
//    //    float stepPhi = min( (360.0f / numberOfSamplesAround) * 0.0174532925f * earlyOut, minimumAdjustment); // we step out of our inner loop fast when theta is low i.e. we are close to the normal.
//    //    for (float phi = 0.05; phi < 6.283; phi += stepPhi) // z rot
//    //    {
//
//    float stepTheta = (hemisphereMaxAngle / numberOfSamplesHemisphere) * ToRadians - 0.05f;
//    float stepPhi = (360.0f / numberOfSamplesAround) * ToRadians - 0.05f;
//    for (float phi = 0.01; phi < 6.283; phi += stepPhi) // z rot.
//    {
//        for (float theta = 0.01f; theta < hemisphereMaxAngleTheta; theta += stepTheta) // y
//        {
//
//            float3 temp = cos(phi) * right + sin(phi) * up;
//            float4 sampleVector = float4(cos(theta) * normal + sin(theta) * temp, mipSampleLevel);
//            float3 sampledColor = texCUBElod(CubeMapSampler, sampleVector).rgb;
//            float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * 0.33333f;
//            float NdotS = saturate(dot(normal, sampleVector.rgb));
//            float phiMuliplier = 1.0f - ( phi / (5.283f + 1.0f));
//
//            //accumulatedColor += sampledColor;
//            //totalWeight++;
//
//            //accumulatedColor += sampledColor * phiMuliplier;
//            //totalWeight++;
//
//            accumulatedColor += sampledColor * phiMuliplier;
//            totalWeight += phiMuliplier;
//
//            //accumulatedColor += sampledColor * (7.0f - phi);
//            //totalWeight += (7.0f - phi);
//
//            //accumulatedColor += sampledColor * NdotS;
//            //totalWeight += NdotS;
//
//            //accumulatedColor += sampledColor sin(theta);
//            //totalWeight += 1.0f;
//
//            //accumulatedColor += sampledColor * (cos(theta) * sin(theta));
//            //totalWeight += cos(theta) * sin(theta);
//
//            //accumulatedColor += sampledColor * (1.0f - NdotS);
//            //totalWeight += (1.0f - NdotS);
//
//
//            //accumulatedColor += sampledColor * (cos(theta) * sin(theta)) * NdotS;
//            //totalWeight += cos(theta) * sin(theta) * NdotS;
//
//            //// this is completely blured in fact its so way way too too smoothly blured.
//            //accumulatedColor += sampledColor;
//            //totalWeight += avg;
//
//            ////// getting abit closer
//            //NdotS = NdotS * NdotS ;
//            //accumulatedColor += sampledColor * NdotS;
//            //totalWeight += avg * NdotS * 3.0f;
//
//
//            //NdotS = NdotS * NdotS * NdotS * NdotS;
//            //float avg = (sampledColor.r + sampledColor.b + sampledColor.g) * NdotS;
//            //accumulatedColor += sampledColor * avg;
//            //totalWeight += avg ;
//
//            //  Ok i believe the reason this is imbalanced is because of the geometry of the samples themselves.
//            //  When we sample close to the normal in a circle around it there are a lot of samples in the same place basically this makes the number of them combined actually too similar and compounded.
//            //  Such that curve of the samples near the normal are actually far higher near the normal then just 1.
//            //  So i need a function to balance close samples towards one or i need less samples near the normal.
//
//            //averagedColor += avg;
//            //totalSampleCount++;
//        }
//        //accumulatedColor += averagedColor;
//        //totalWeight += totalSampleCount;
//        //averagedColor =0;
//        //totalSampleCount =0;
//    }
//    float3 directColor = texCUBElod(CubeMapSampler, float4(normal, 0)).rgb;
//    directColor.rgb = (directColor.r + directColor.g + directColor.b) / 3;
//    //float4 final = float4(accumulatedColor, 1.0f);
//    float4 final = float4(accumulatedColor / totalWeight, 1.0f);
//    //float4 final = float4(PI * accumulatedColor / totalWeight, 1.0f );
//    //final.rgb = final.rgb * 0.90f + directColor.brg * 0.10f;
//    return final;
//}

