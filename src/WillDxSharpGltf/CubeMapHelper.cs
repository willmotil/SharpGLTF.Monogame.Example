using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WillDxSharpGltf
{
    public static class CubeMapHelper
    {
        private const int FACE_LEFT = 0; // NegativeX
        private const int FACE_BOTTOM = 1; // NegativeY
        private const int FACE_BACK = 2; // NegativeZ
        private const int FACE_RIGHT = 3; // PositiveX
        private const int FACE_TOP = 4; // PositiveY
        private const int FACE_FRONT = 5; // PositiveZ

        /// <summary>
        /// Set faces to cubemap by name, neg xyz,  pos  xyz.
        /// </summary>
        public static TextureCube SetIndividualFacesToCubeMap(GraphicsDevice gd, int size, TextureCube map, Texture2D textureLeft, Texture2D textureBottom, Texture2D textureBack, Texture2D textureRight, Texture2D textureTop, Texture2D textureFront)
        {
            return SetIndividualCubeFacesToCubeMap(gd, size, map, textureLeft, textureBottom, textureBack, textureRight, textureTop, textureFront);
        }

        /// <summary>
        /// Set faces to cubemap, neg xyz,  pos  xyz.
        /// </summary>
        public static TextureCube SetIndividualCubeFacesToCubeMap(GraphicsDevice gd, int size, TextureCube map, Texture2D textureNegativeX, Texture2D textureNegativeY, Texture2D textureNegativeZ, Texture2D texturePositiveX, Texture2D texturePositiveY, Texture2D texturePositiveZ)
        {
            if (map == null)
                map = new TextureCube(gd, size, true, SurfaceFormat.Color);
            SetTextureToCubeMapFace(map, textureNegativeX, CubeMapFace.NegativeX);
            SetTextureToCubeMapFace(map, textureNegativeY, CubeMapFace.NegativeY);
            SetTextureToCubeMapFace(map, textureNegativeZ, CubeMapFace.NegativeZ);
            SetTextureToCubeMapFace(map, texturePositiveX, CubeMapFace.PositiveX);
            SetTextureToCubeMapFace(map, texturePositiveY, CubeMapFace.PositiveY);
            SetTextureToCubeMapFace(map, texturePositiveZ, CubeMapFace.PositiveZ);
            return map;
        }

        /// <summary>
        /// This sets a individual texture and its mipmaps to a cubemap.
        /// </summary>
        public static void SetTextureToCubeMapFace(TextureCube cubeMap, Texture2D textureFace, CubeMapFace faceId)
        {
            for (int level = 0; level < textureFace.LevelCount; level += 1)
            {
                // Allocations here so be mindful of when you load.
                var faceData = new Color[(textureFace.Width >> level) * (textureFace.Height >> level)];
                textureFace.GetData(level, null, faceData, 0, faceData.Length);
                cubeMap.SetData(faceId, level, null, faceData, 0, faceData.Length);
            }
        }

        /// <summary>
        /// Ok so this is a destination pixel version this version attempts to include mip levels.
        /// However lots of dimensional variables to account for here.
        /// </summary>
        public static TextureCube GetCubeMapFromEquaRectangularMap(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize)
        {
            TextureCube cubeMap;
            cubeMap = new TextureCube(gd, faceSize, true, SurfaceFormat.Color);
            var eqLevelCount = equaRectangularMap.LevelCount;
            for (int level = 0; level < eqLevelCount; level += 1)
            {
                int eqw = equaRectangularMap.Width >> level;
                int eqh = equaRectangularMap.Height >> level;
                var mapColorData = new Color[(equaRectangularMap.Width >> level) * (equaRectangularMap.Height >> level)];
                equaRectangularMap.GetData(level, null, mapColorData, 0, mapColorData.Length);
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    var adjFaceSize = faceSize >> level;
                    var wh = new Vector2(adjFaceSize - 1, adjFaceSize - 1);
                    var faceData = new Color[adjFaceSize * adjFaceSize];
                    for (int y = 0; y < adjFaceSize; y++)
                    {
                        for (int x = 0; x < adjFaceSize; x++)
                        {
                            var fuv = new Vector2(x, y) / wh;
                            var v = UvFaceToCubeMapVector(fuv, faceIndex);
                            var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                            var eqIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                            faceData[x + (y * adjFaceSize)] = mapColorData[eqIndex];
                        }
                    }
                    var cubeMapFace = GetFaceFromInt(faceIndex);
                    cubeMap.SetData(cubeMapFace, level, null, faceData, 0, faceData.Length);
                }
            }
            return cubeMap;
        }

        /// <summary>
        /// This doesn't handle mip maps it wouldn't make much sense for it to.
        /// </summary>
        public static Texture2D GetEquaRectangularMapFromSixImageFaces(GraphicsDevice gd, int outputWidth, int outputHeight, Texture2D textureLeft, Texture2D textureBottom, Texture2D textureBack, Texture2D textureRight, Texture2D textureTop, Texture2D textureFront)
        {
            Color[] mapColorData = new Color[outputWidth * outputHeight];
            Texture2D equaRectangularMap = new Texture2D(gd, outputWidth, outputHeight);
            var fw = textureLeft.Width;
            var fh = textureLeft.Height;
            var fwm = fw - 1;
            var fhm = fh - 1;
            Color[] leftColorData = new Color[fw * fh];
            textureLeft.GetData<Color>(leftColorData);
            Color[] bottomColorData = new Color[fw * fh];
            textureBottom.GetData<Color>(bottomColorData);
            Color[] backColorData = new Color[fw * fh];
            textureBack.GetData<Color>(backColorData);
            Color[] rightColorData = new Color[fw * fh];
            textureRight.GetData<Color>(rightColorData);
            Color[] topColorData = new Color[fw * fh];
            textureTop.GetData<Color>(topColorData);
            Color[] frontColorData = new Color[fw * fh];
            textureFront.GetData<Color>(frontColorData);
            for (int y = 0; y < outputHeight; y++)
            {
                for (int x = 0; x < outputWidth; x++)
                {
                    var eqIndex = (int)(x) + (int)(y * outputWidth);
                    var equv = new Vector2(x, y) / new Vector2(outputWidth, outputHeight);
                    var v = EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(equv);
                    int face = 0;
                    var fuv = CubeMapVectorToUvFace(v, out face);
                    int findex = (int)(fuv.X * fwm) + (int)(fuv.Y * fhm) * fw;
                    switch (face)
                    {
                        case 0:
                            mapColorData[eqIndex] = leftColorData[findex]; // NegativeX;
                            break;
                        case 1:
                            mapColorData[eqIndex] = bottomColorData[findex]; // NegativeY;
                            break;
                        case 2:
                            mapColorData[eqIndex] = backColorData[findex]; // NegativeZ;
                            break;
                        case 3:
                            mapColorData[eqIndex] = rightColorData[findex]; // PositiveX;
                            break;
                        case 4:
                            mapColorData[eqIndex] = topColorData[findex]; // PositiveY;
                            break;
                        case 5:
                            mapColorData[eqIndex] = frontColorData[findex]; // PositiveZ;
                            break;
                        default:
                            mapColorData[eqIndex] = leftColorData[findex]; // PositiveZ;
                            break;
                    }
                }
            }
            equaRectangularMap.SetData<Color>(mapColorData);
            return equaRectangularMap;
        }

        /// <summary>
        /// Ok so this is a destination pixel version this doesn't handle mipmaps.
        /// </summary>
        public static Texture2D[] GetMapFacesTextureArrayFromEquaRectangularMap(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize)
        {
            Color[] mapColorData = new Color[equaRectangularMap.Width * equaRectangularMap.Height];
            equaRectangularMap.GetData<Color>(mapColorData);
            int eqw = equaRectangularMap.Width;
            int eqh = equaRectangularMap.Height;
            Texture2D[] textureFaces = new Texture2D[6];
            var wh = new Vector2(faceSize - 1, faceSize - 1);
            for (int index = 0; index < 6; index++)
            {
                Color[] faceColorData = new Color[faceSize * faceSize];
                textureFaces[index] = new Texture2D(gd, faceSize, faceSize);
                // for each face set its pixels from the equarectangularmap.
                for (int y = 0; y < faceSize; y++)
                {
                    for (int x = 0; x < faceSize; x++)
                    {
                        var fuv = new Vector2(x, y) / wh;
                        var v = UvFaceToCubeMapVector(fuv, index);
                        var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                        var eqIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                        faceColorData[x + (y * faceSize)] = mapColorData[eqIndex];
                    }
                }
                textureFaces[index].SetData<Color>(faceColorData);
            }
            return textureFaces;
        }

        /// <summary>
        /// thanks to pumkin pudding for this function.
        /// </summary>
        private static Vector3 EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(Vector2 uvCoords)
        {
            float pi = 3.14159265358f;
            Vector3 v = new Vector3(0.0f, 0.0f, 0.0f);
            Vector2 uv = uvCoords;
            uv *= new Vector2(2.0f * pi, pi);
            float siny = (float)Math.Sin(uv.Y);
            v.X = -(float)Math.Sin(uv.X) * siny;
            v.Y = (float)Math.Cos(uv.Y);
            v.Z = -(float)Math.Cos(uv.X) * siny;
            return v;
        }

        /// <summary>
        /// Gets cube uv from normal.
        /// </summary>
        private static Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(Vector3 normal)
        {
            Vector2 INVERT_ATAN = new Vector2(0.1591f, 0.3183f);
            Vector2 uv = new Vector2((float)Math.Atan2(normal.Z, normal.X), (float)Math.Asin(normal.Y));
            uv *= INVERT_ATAN;
            uv += new Vector2(0.5f, 0.5f);
            return uv;
        }

        /// <summary>
        /// https://www.gamedev.net/forums/topic/687535-implementing-a-cube-map-lookup-function/ // oh so nice i found it i really didn't want to figure out how to write this myself.
        /// </summary>
        private static Vector2 CubeMapVectorToUvFace(Vector3 v, out int faceIndex)
        {
            Vector3 vAbs = Abs(v);
            float ma;
            Vector2 uv;
            if (vAbs.Z >= vAbs.X && vAbs.Z >= vAbs.Y)
            {
                faceIndex = v.Z < 0.0 ? FACE_BACK : FACE_FRONT; //5 : 4;  // z major axis.
                ma = 0.5f / vAbs.Z;
                uv = new Vector2(v.Z < 0.0f ? -v.X : v.X, -v.Y);
            }
            else if (vAbs.Y >= vAbs.X)
            {
                faceIndex = v.Y < 0.0f ? FACE_BOTTOM : FACE_TOP; // 3 : 2; // y major axis.
                ma = 0.5f / vAbs.Y;
                uv = new Vector2(v.X, v.Y < 0.0 ? -v.Z : v.Z);
            }
            else
            {
                faceIndex = v.X < 0.0 ? FACE_LEFT : FACE_RIGHT; // 1 : 0; // x major axis.
                ma = 0.5f / vAbs.X;
                uv = new Vector2(v.X < 0.0 ? v.Z : -v.Z, -v.Y);
            }
            return uv * ma + new Vector2(0.5f, 0.5f);
        }

        /// <summary>
        /// Gets the Cube Face enum from the corresponding integer used in a switch case.
        /// </summary>
        private static CubeMapFace GetFaceFromInt(int face)
        {
            CubeMapFace f;
            switch (face)
            {
                case 0:
                    f = CubeMapFace.NegativeX;
                    break;
                case 1:
                    f = CubeMapFace.NegativeY;
                    break;
                case 2:
                    f = CubeMapFace.NegativeZ;
                    break;
                case 3:
                    f = CubeMapFace.PositiveX;
                    break;
                case 4:
                    f = CubeMapFace.PositiveY;
                    break;
                case 5:
                    f = CubeMapFace.PositiveZ;
                    break;
                default:
                    f = CubeMapFace.PositiveZ;
                    break;
            }
            return f;
        }

        /// <summary>
        /// This is the reverse of the cubemapVectorToUvFace.
        /// </summary>
        private static Vector3 UvFaceToCubeMapVector(Vector2 uv, int faceIndex)
        {
            var u = uv.X * 2f - 1.0f;
            var v = uv.Y * 2f - 1.0f;
            Vector3 dir = new Vector3(0f, 0f, 1f);

            switch (faceIndex)
            {
                case FACE_LEFT:
                    dir = new Vector3(-1f, v, u);
                    break;
                case FACE_BOTTOM:
                    dir = new Vector3(-u, -1f, v);
                    break;
                case FACE_BACK:
                    dir = new Vector3(-u, v, 1f);
                    break;
                case FACE_RIGHT:
                    dir = new Vector3(1f, v, -u);
                    break;
                case FACE_TOP:
                    dir = new Vector3(u, 1f, -v);
                    break;
                case FACE_FRONT:
                    dir = new Vector3(u, v, -1f);
                    break;
                default:
                    dir = new Vector3(u, v, -1f);
                    break;
            }
            dir.Normalize();
            return dir;
        }

        private static Vector3 Abs(Vector3 v)
        {
            if (v.X < 0) v.X = -v.X;
            if (v.Y < 0) v.Y = -v.Y;
            if (v.Z < 0) v.Z = -v.Z;
            return v;
        }

        private static Vector2 UvFromTexturePixel(Texture2D texture, Vector2 pixel)
        {
            return pixel / new Vector2(texture.Width - 1, texture.Height - 1);
        }
    }
}


// First were ill tackle the diffuse portion and get a irradiance map.
// for the moment im using a ix sided low dynamic range (LDR)
/*

 https://learnopengl.com/PBR/IBL/Diffuse-irradiance
 cubemap environment map  > irradiance map

 https://learnopengl.com/PBR/IBL/Specular-IBL

 http://www.hdrlabs.com/sibl/archive.html  equirectangular map  https://floyd.lbl.gov/radiance/refer/Notes/picture_format.html
what the CubeMapGen code does http://www.rorydriscoll.com/2012/01/15/cubemap-texel-solid-angle/
cross image ldr maps http://www.humus.name/index.php?page=Textures
discussion https://stackoverflow.com/questions/29678510/convert-21-equirectangular-panorama-to-cube-map lots of code.
https://stackoverflow.com/questions/34250742/converting-a-cubemap-into-equirectangular-panorama?noredirect=1&lq=1
http://www.adriancourreges.com/blog/2016/09/09/doom-2016-graphics-study/

*/
/* Hlsl stuff that is related gonna have to figure out how to fit this in as well as other stuff.

TextureCube CubeMap;
sampler CubeMapSampler = sampler_state
{
texture = <CubeMap>;
magfilter = linear; /// set it to point ect
minfilter = linear;
mipfilter = linear;
AddressU = clamp;
AddressV = clamp;
//AddressW = clamp;
};

// Placed after point lights.

//____________________________________________________________________
// Enviromental reflections and refraction.
//____________________________________________________________________

float maxMipLevel = 9;
float mipLevel = roughness * maxMipLevel;
float scaledRoughness = (roughness * 0.5f + 0.5f);
float3 F = xfresnelSchlickRoughness(max(dot(N, V), 0.0f), F0, roughness);
float3 InvF = (1.0f - F);
float3 kS = F;
float3 kD = InvF * dielectricity;
float3 NR = N * roughness + R * smoothness;
float3 irradiance = texCUBElod(CubeMapSampler, float4(NR, maxMipLevel * roughness)).rgb;
float3 diffuse = (irradiance.r + irradiance.g + irradiance.b) * 0.3333f * albedo; //albedo * irradiance;
float3 specular = irradiance * albedo; 
float3 ambient = (kD * diffuse + kS * specular); // * occlusion;
// add env light and point lights
float3 outColor = ambient + Lo;


// used to determine the inflection placement position from point a to a normal opposite the plane. \|/
float3 InflectionPositionFromPlane(float3 anyPositionOnPlaneP, float3 theSurfaceNormalN, float3 theCameraPostionC)
{
// also gives the length when placed againsts a unit normal so any unit n * a distance is the distance to that normals plane no matter the normals direction. 
float camToPlaneDist = dot(theSurfaceNormalN, theCameraPostionC - anyPositionOnPlaneP);
return theCameraPostionC - theSurfaceNormalN * camToPlaneDist * 2;
}

// thanks to pumkin pudding for this function.
float3 Function2dSphericalUvCoordinatesTo3dCubeMapNormal(float2 uvCoords)
{
float pi = 3.14159265358f;
float3 v = float3(0.0f, 0.0f, 0.0f);
float2 uv = uvCoords;
uv *= float2(2.0f * pi, pi);
float siny = sin(uv.y);
v.x = -sin(uv.x) * siny;
v.y = cos(uv.y);
v.z = -cos(uv.x) * siny;
return v;
}

// thanks to pumkin pudding for this function.
float2 Function3dCubeMapNormalTo2dSphericalUvCoordinates(float3 normal)
{
const float2 INVERT_ATAN = float2(0.1591f, 0.3183f);
float2 uv = float2(atan2(normal.z, normal.x), asin(normal.y));
uv *= INVERT_ATAN;
uv += 0.5f;
return uv;
}


        ///// <summary>
        ///// Ok so this is a destination pixel version this doesn't handle mipmaps correctly that needs to be fixed up.
        ///// </summary>
        //public static TextureCube GetCubeMapFromEquaRectangularMap(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize)
        //{
        //    TextureCube cubeMap;
        //    cubeMap = new TextureCube(gd, faceSize, true, SurfaceFormat.Color);
        //    Color[] mapColorData = new Color[equaRectangularMap.Width * equaRectangularMap.Height];
        //    equaRectangularMap.GetData<Color>(mapColorData);
        //    int eqw = equaRectangularMap.Width;
        //    int eqh = equaRectangularMap.Height;
        //    var eqLevelCount = equaRectangularMap.LevelCount;
        //    // ...
        //    Texture2D[] textureFaces = new Texture2D[6];
        //    var wh = new Vector2(faceSize - 1, faceSize - 1);
        //    for (int index = 0; index < 6; index++)
        //    {
        //        var cubeMapFace = GetFaceFromInt(index);
        //        Color[] faceData = new Color[faceSize * faceSize];
        //        textureFaces[index] = new Texture2D(gd, faceSize, faceSize);
        //        // For each face set its pixels from the equaRectangularMap.
        //        for (int y = 0; y < faceSize; y++)
        //        {
        //            for (int x = 0; x < faceSize; x++)
        //            {
        //                var fuv = new Vector2(x, y) / wh;
        //                var v = UvFaceToCubeMapVector(fuv, index);
        //                var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
        //                var eqIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
        //                faceData[x + (y * faceSize)] = mapColorData[eqIndex];
        //            }
        //        }
        //        textureFaces[index].SetData<Color>(faceData);
        //    }
        //    for (int index = 0; index < 6; index++)
        //    {
        //        // Set Texture To Cube Map Face. 
        //        var t = textureFaces[index];
        //        var face = GetFaceFromInt(index);
        //        for (int level = 0; level < t.LevelCount; level += 1)
        //        {
        //            // Allocations here so be mindful of when you load.
        //            var data = new Color[(t.Width >> level) * (t.Height >> level)];
        //            t.GetData(level, null, data, 0, data.Length);
        //            cubeMap.SetData(face, level, null, data, 0, data.Length);
        //        }
        //    }
        //    return cubeMap;
        //}

        //public static void SetTextureToCubeMapFace(TextureCube cubeMap, Texture2D textureFace, CubeMapFace faceId)
        //{
        //    for (int level = 0; level < textureFace.LevelCount; level += 1)
        //    {
        //        // Allocations here so be mindful of when you load.
        //        var faceData = new Color[(textureFace.Width >> level) * (textureFace.Height >> level)];
        //        textureFace.GetData(level, null, faceData, 0, faceData.Length);
        //        cubeMap.SetData(faceId, level, null, faceData, 0, faceData.Length);
        //    }
        //}

*/

