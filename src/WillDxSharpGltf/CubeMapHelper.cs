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
        public const int FACE_LEFT = 0; // NegativeX
        public const int FACE_BOTTOM = 1; // NegativeY
        public const int FACE_BACK = 2; // NegativeZ
        public const int FACE_RIGHT = 3; // PositiveX
        public const int FACE_TOP = 4; // PositiveY
        public const int FACE_FRONT = 5; // PositiveZ

        public const float PI = (float)Math.PI;

        /// <summary>
        /// Set faces to cubemap by name, neg xyz,  pos  xyz.  tested passes.
        /// </summary>
        public static TextureCube GetCubeMapFromIndividualFaces(GraphicsDevice gd, int size, TextureCube map, Texture2D[] textureList)
        {
            return GetCubeMapFromIndividualDirectionFaces(gd, size, map, textureList[0], textureList[1], textureList[2], textureList[3], textureList[4], textureList[5]);
        }

        /// <summary>
        /// Set faces to cubemap by name, neg xyz,  pos  xyz.  tested passes.
        /// </summary>
        public static TextureCube GetCubeMapFromIndividualFaces(GraphicsDevice gd, int size, TextureCube map, Texture2D textureLeft, Texture2D textureBottom, Texture2D textureBack, Texture2D textureRight, Texture2D textureTop, Texture2D textureFront)
        {
            return GetCubeMapFromIndividualDirectionFaces(gd, size, map, textureLeft, textureBottom, textureBack, textureRight, textureTop, textureFront);
        }

        /// <summary>
        /// Set faces to cubemap, neg xyz,  pos  xyz. tested passes
        /// </summary>
        public static TextureCube GetCubeMapFromIndividualDirectionFaces(GraphicsDevice gd, int size, TextureCube map, Texture2D textureNegativeX, Texture2D textureNegativeY, Texture2D textureNegativeZ, Texture2D texturePositiveX, Texture2D texturePositiveY, Texture2D texturePositiveZ)
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
        /// This sets a individual texture that represents a face and its mipmaps to a cubemap face at level.   tested passes
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
        /// This is a destination pixel version this version attempts to include mip levels. test passes  v2 
        /// </summary>
        public static TextureCube GetCubeMapFromEquaRectangularMap(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize)
        {
            TextureCube cubeMap = new TextureCube(gd, faceSize, true, SurfaceFormat.Color);
            var cmLevelCount = cubeMap.LevelCount;
            int eqw = equaRectangularMap.Width;
            int eqh = equaRectangularMap.Height;
            var eqColorData = new Color[(equaRectangularMap.Width) * (equaRectangularMap.Height)];
            equaRectangularMap.GetData(0, null, eqColorData, 0, eqColorData.Length);
            for (int level = 0; level < cmLevelCount; level += 1)
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    var adjFaceSize = faceSize >> level;
                    var faceWh = new Vector2(adjFaceSize, adjFaceSize);
                    var faceData = new Color[adjFaceSize * adjFaceSize];
                    for (int y = 0; y < adjFaceSize; y++)
                    {
                        for (int x = 0; x < adjFaceSize; x++)
                        {
                            var fuv = new Vector2(x, y) / faceWh;
                            var v = UvFaceToCubeMapVector(fuv, faceIndex);
                            var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                            var eqPixelIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                            var facePixelIndex = x + (y * adjFaceSize);
                            faceData[facePixelIndex] = eqColorData[eqPixelIndex];
                        }
                    }
                    var cubeMapFace = GetFaceFromInt(faceIndex);
                    cubeMap.SetData(cubeMapFace, level, null, faceData, 0, faceData.Length);
                }
            }
            return cubeMap;
        }

        /// <summary>  
        /// This is a destination pixel version this version attempts to include mip levels. test passes  v2 
        /// </summary>
        public static TextureCube GetCubeMapFromEquaRectangularVector4Map(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize)
        {
            TextureCube cubeMap = new TextureCube(gd, faceSize, true, SurfaceFormat.Vector4);
            var cmLevelCount = cubeMap.LevelCount;
            int eqw = equaRectangularMap.Width;
            int eqh = equaRectangularMap.Height;
            var eqColorData = new Vector4[(equaRectangularMap.Width) * (equaRectangularMap.Height)];
            equaRectangularMap.GetData(0, null, eqColorData, 0, eqColorData.Length);
            for (int level = 0; level < cmLevelCount; level += 1)
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    var adjFaceSize = faceSize >> level;
                    var faceWh = new Vector2(adjFaceSize, adjFaceSize);
                    var faceData = new Vector4[adjFaceSize * adjFaceSize];
                    for (int y = 0; y < adjFaceSize; y++)
                    {
                        for (int x = 0; x < adjFaceSize; x++)
                        {
                            var fuv = new Vector2(x, y) / faceWh;
                            var v = UvFaceToCubeMapVector(fuv, faceIndex);
                            var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                            var eqPixelIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                            var facePixelIndex = x + (y * adjFaceSize);
                            faceData[facePixelIndex] = eqColorData[eqPixelIndex];
                        }
                    }
                    var cubeMapFace = GetFaceFromInt(faceIndex);
                    cubeMap.SetData(cubeMapFace, level, null, faceData, 0, faceData.Length);
                }
            }
            return cubeMap;
        }

        /// <summary>  
        /// This is a destination pixel version this version attempts to include mip levels and use them for convolution in diffuse and specular. test passes  v2 
        /// Reference here. Cubemap convolution  https://learnopengl.com/PBR/IBL/Diffuse-irradiance  
        /// Lo(p,ωo)=kdcπ∫ΩLi(p,ωi)n⋅ωidωi and  Epics... specular  Lo(p,ωo)=∫ΩLi(p,ωi)dωi∗∫Ωfr(p,ωi,ωo)n⋅ωidωi
        /// The convoluted irradiance diffuse map is fairly simple and in earnest i really only need one i think however the specular lobe map varys with roughness.
        /// </summary>
        public static void GetCubeMapsPreFilteredDiffuseAndSpecularFromEquaRectangularMap(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize, out TextureCube cubeMapPreFilteredDiffuse, out TextureCube cubeMapPreFilteredSpecular)
        {
            TextureCube cubeMapSpecular = new TextureCube(gd, faceSize, true, SurfaceFormat.Color);
            TextureCube cubeMapDiffuse = new TextureCube(gd, faceSize, true, SurfaceFormat.Color);
            var cmLevelCount = cubeMapSpecular.LevelCount;
            int eqw = equaRectangularMap.Width;
            int eqh = equaRectangularMap.Height;
            var rawFaceDataList = new List<Color[]>();
            var eqColorData = new Color[(equaRectangularMap.Width) * (equaRectangularMap.Height)];
            equaRectangularMap.GetData(0, null, eqColorData, 0, eqColorData.Length);
            // first reflection level.
            for (int level = 0; level < 1; level += 1)
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    var adjFaceSize = faceSize >> level;
                    var faceWh = new Vector2(adjFaceSize, adjFaceSize);
                    var faceColorDataSpecular = new Color[adjFaceSize * adjFaceSize];
                    var faceColorDataDiffuse = new Color[adjFaceSize * adjFaceSize];
                    Console.WriteLine($"1  level: {level}   faceIndex: {faceIndex}"); //  Line: {y} of {adjFaceSize}");
                    for (int y = 0; y < adjFaceSize; y++)
                    {
                        for (int x = 0; x < adjFaceSize; x++)
                        {
                            var facePixelIndex = x + (y * adjFaceSize);
                            var fuv = new Vector2(x, y) / faceWh;
                            var v = UvFaceToCubeMapVector(fuv, faceIndex);
                            var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                            var eqPixelIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                            faceColorDataSpecular[facePixelIndex] = eqColorData[eqPixelIndex];
                            // ... 
                            SpecularLobeVectorChange(v, 30, .90f, faceColorDataDiffuse, facePixelIndex, eqColorData, eqw, eqh);
                        }
                    }
                    var cubeMapFace = GetFaceFromInt(faceIndex);
                    rawFaceDataList.Add(faceColorDataSpecular); // save the data to the list.
                    cubeMapSpecular.SetData(cubeMapFace, level, null, faceColorDataSpecular, 0, faceColorDataSpecular.Length);
                    cubeMapDiffuse.SetData(cubeMapFace, level, null, faceColorDataDiffuse, 0, faceColorDataDiffuse.Length);
                }
            }
            // rest of the specular lobe reflection levels.
            for (int level = 1; level < cmLevelCount; level += 1)
            {
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    var adjFaceSize = faceSize >> level;
                    var faceWh = new Vector2(adjFaceSize, adjFaceSize);
                    var faceColorDataSpecular = new Color[adjFaceSize * adjFaceSize];
                    var faceColorDataDiffuse = new Color[adjFaceSize * adjFaceSize];
                    Console.WriteLine($"2  level: {level}   faceIndex: {faceIndex}");
                    for (int y = 0; y < adjFaceSize; y++)
                    {
                        for (int x = 0; x < adjFaceSize; x++)
                        {
                            var facePixelIndex = x + (y * adjFaceSize);
                            var fuv = new Vector2(x, y) / faceWh;
                            var v = UvFaceToCubeMapVector(fuv, faceIndex);
                            float roughness = level / (cmLevelCount - 1);
                            SpecularLobeVectorChange(v, 10, level / (cmLevelCount -1), faceColorDataSpecular, facePixelIndex, eqColorData, eqw, eqh);
                            SpecularLobeVectorChange(v, 10, .90f, faceColorDataDiffuse, facePixelIndex, eqColorData, eqw, eqh);
                        }
                    }
                    var cubeMapFace = GetFaceFromInt(faceIndex);
                    cubeMapSpecular.SetData(cubeMapFace, level, null, faceColorDataSpecular, 0, faceColorDataSpecular.Length);
                    cubeMapDiffuse.SetData(cubeMapFace, level, null, faceColorDataDiffuse, 0, faceColorDataDiffuse.Length);
                }
            }
            cubeMapPreFilteredSpecular = cubeMapSpecular;
            cubeMapPreFilteredDiffuse = cubeMapDiffuse;
        }

        public static void SpecularLobeVectorChange(Vector3 n, int numOfSamples, float roughness, Color[] faceColorData, int facePixelIndex, Color[] eqColorData, int equaRectangularMapWidth, int equaRectangularMapHeght)
        {
            float radiansRange = roughness * PI;
            float sampleCount = numOfSamples* roughness;
            float outwardStepCount = sampleCount / 4f;
            var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(n);
            var eqPixelIndex = (int)(uv.X * equaRectangularMapWidth) + ((int)(uv.Y * equaRectangularMapHeght) * equaRectangularMapWidth);
            Color c = eqColorData[eqPixelIndex];
            Vector3 cumulative = new Vector3(c.R, c.G, c.B);
            float cwsum = 1.0f;
            // calculate a pixel of radians this isn't going to be exact.
            var pixRad = 1f / equaRectangularMapHeght * (PI / 2f);
            float rotation = 0;
            //
            for (int currentSc = 0; currentSc < sampleCount; currentSc++)
            {
                float a = (currentSc / sampleCount);
                a *= a;
                float inv = 1.0f - a;
                float outwardRadians = a * radiansRange;
                rotation += PI / 3.0f;
                Matrix moffset = Matrix.CreateRotationY(outwardRadians);
                Matrix rot = Matrix.CreateRotationZ(rotation);
                Matrix multip = moffset * rot;
                var v = Vector3.Transform(n, multip);
                uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                eqPixelIndex = (int)(uv.X * equaRectangularMapWidth) + ((int)(uv.Y * equaRectangularMapHeght) * equaRectangularMapWidth);
                c = eqColorData[eqPixelIndex];
                cumulative += new Vector3(c.R * inv, c.G * inv, c.B * inv);
                cwsum += inv;
                //Console.WriteLine($" ,                      cumulative: {cumulative}   cwsum: {cwsum}");
            }
            cumulative = cumulative / cwsum;
            //Console.WriteLine($" cumulative: {cumulative}   cwsum: {cwsum}");
            faceColorData[facePixelIndex] = new Color(cumulative.X *255, cumulative.Y *255, cumulative.Z * 255, 255);
        }

        //public static void SpecularLobeVectorChange(Vector3 v, int numOfSamples, float roughness, Color[] faceColorData, int facePixelIndex, Color[] eqColorData, int equaRectangularMapWidth, int equaRectangularMapHeght)
        //{
        //    BuildSpecularPreFilterPixelByTheBook(v, roughness, faceColorData, facePixelIndex, eqColorData, equaRectangularMapWidth, equaRectangularMapHeght);
        //}

        /// <summary>
        /// this seems a bit overly complicated but i can move this to a pixel shader simply so.
        /// </summary>
        private static void BuildSpecularPreFilterPixelByTheBook(Vector3 N, float roughness, Color[] faceColorData, int facePixelIndex, Color[] eqColorData, int equaRectangularMapWidth, int equaRectangularMapHeght)
        {
            Vector3 R = N;
            Vector3 V = R;
            const uint SAMPLE_COUNT = 4u;
            float totalWeight = 0.0f;
            Vector3 prefilteredColor = Vector3.Zero;
            for (uint i = 0u; i < SAMPLE_COUNT; ++i)
            {
                Vector2 Xi = HammersleyNoBitOps(i, SAMPLE_COUNT);
                Vector3 H = ImportanceSampleGGX(Xi, N, roughness);
                Vector3 L = Vector3.Normalize(2.0f * Vector3.Dot(V, H) * H - V);
                float NdotL = Math.Max( Vector3.Dot(N, L) , 0.0f );
                if (NdotL > 0.0)
                {
                    var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(L);
                    var eqPixelIndex = (int)(uv.X * equaRectangularMapWidth) + ((int)(uv.Y * equaRectangularMapHeght) * equaRectangularMapWidth);
                    Color c = eqColorData[eqPixelIndex];
                    prefilteredColor.X = (byte)((float)(c.R) * NdotL);
                    prefilteredColor.Y = (byte)((float)(c.G) * NdotL);
                    prefilteredColor.Z = (byte)((float)(c.B) * NdotL);
                    totalWeight += NdotL;
                }
            }
            prefilteredColor = prefilteredColor / totalWeight;
            faceColorData[facePixelIndex] = new Color(prefilteredColor.X, prefilteredColor.Y, prefilteredColor.Z, 1.0f);
        }

        // GGX Importance sampling
        private static Vector3 ImportanceSampleGGX(Vector2 Xi, Vector3 N, float roughness)
        {
            float a = roughness * roughness;
            float phi = 2.0f * PI * Xi.X;
            float cosTheta = (float)Math.Sqrt((1.0f - Xi.Y) / (1.0f + (a * a - 1.0f) * Xi.Y));
            float sinTheta = (float)Math.Sqrt(1.0f - cosTheta * cosTheta);
            // from spherical coordinates to cartesian coordinates
            Vector3 H;
            H.X = (float)Math.Cos(phi) * sinTheta;
            H.Y = (float)Math.Sin(phi) * sinTheta;
            H.Z = cosTheta;
            // from tangent-space vector to world-space sample vector
            Vector3 up = Abs(N.Z) < 0.999f ? new Vector3(0.0f, 0.0f, 1.0f) : new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 tangent = Vector3.Normalize( Vector3.Cross(up, N));
            Vector3 bitangent = Vector3.Cross(N, tangent);
            Vector3 sampleVec = tangent * H.X + bitangent * H.Y + N * H.Z;
            return Vector3.Normalize(sampleVec);
        }

        /// <summary>
        /// The GLSL Hammersley function gives us the low-discrepancy sample x of the total sample set of size N. 
        /// https://learnopengl.com/PBR/IBL/Specular-IBL
        /// </summary>
        private static Vector2 HammersleyNoBitOps(uint x, uint N)
        {
            uint baseVal = 2u;
            uint x2 = x;
            float invBase = 1.0f / (float)(baseVal);
            float denom = 1.0f;
            float vanDerResult = 0.0f;
            for (uint i = 0u; i < 32u; ++i)
            {
                if (x > 0u)
                {
                    denom = Mod((float)(x), 2.0f);
                    vanDerResult += denom * invBase;
                    invBase = invBase / 2.0f;
                    x = (uint)((float)(x) / 2.0f);
                }
            }
            return new Vector2((float)(x2) / (float)(N), vanDerResult);
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
            var faceWh = new Vector2(faceSize , faceSize );  // var faceWh = new Vector2(faceSize - 1, faceSize - 1);
            for (int index = 0; index < 6; index++)
            {
                Color[] faceColorData = new Color[faceSize * faceSize];
                textureFaces[index] = new Texture2D(gd, faceSize, faceSize);
                // for each face set its pixels from the equarectangularmap.
                for (int y = 0; y < faceSize; y++)
                {
                    for (int x = 0; x < faceSize; x++)
                    {
                        var fuv = new Vector2(x, y) / faceWh;
                        var v = UvFaceToCubeMapVector(fuv, index);
                        var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                        //var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(v); // works too same.
                        var eqIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                        var facePixelIndex = x + (y * faceSize);
                        faceColorData[facePixelIndex] = mapColorData[eqIndex];
                    }
                }
                textureFaces[index].SetData<Color>(faceColorData);
            }
            return textureFaces;
        }

        public static Texture2D[] GetMapFacesTextureArrayFromEquaRectangularVector4Map(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize)
        {
            Vector4[] mapColorData = new Vector4[equaRectangularMap.Width * equaRectangularMap.Height];
            equaRectangularMap.GetData<Vector4>(mapColorData);
            int eqw = equaRectangularMap.Width;
            int eqh = equaRectangularMap.Height;
            Texture2D[] textureFaces = new Texture2D[6];
            var faceWh = new Vector2(faceSize, faceSize);  // var faceWh = new Vector2(faceSize - 1, faceSize - 1);
            for (int index = 0; index < 6; index++)
            {
                Vector4[] faceColorData = new Vector4[faceSize * faceSize];
                textureFaces[index] = new Texture2D(gd, faceSize, faceSize, false, SurfaceFormat.Vector4);
                // for each face set its pixels from the equarectangularmap.
                for (int y = 0; y < faceSize; y++)
                {
                    for (int x = 0; x < faceSize; x++)
                    {
                        var fuv = new Vector2(x, y) / faceWh;
                        var v = UvFaceToCubeMapVector(fuv, index);
                        var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                        //var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(v); // works too same.
                        var eqIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                        var facePixelIndex = x + (y * faceSize);
                        faceColorData[facePixelIndex] = mapColorData[eqIndex];
                    }
                }
                textureFaces[index].SetData<Vector4>(faceColorData);
            }
            return textureFaces;
        }

        /// <summary>
        /// This doesn't handle mip maps it wouldn't make much sense for it to.   Mapping is correct in this one.
        /// </summary>
        public static Texture2D GetEquaRectangularMapFromSixImageFaces(GraphicsDevice gd, int outputWidth, int outputHeight, Texture2D[] textureList)
        {
            return GetEquaRectangularMapFromSixImageFaces(gd, outputWidth, outputHeight, textureList[0], textureList[1], textureList[2], textureList[3], textureList[4], textureList[5]);
        }
        /// <summary>
        /// This doesn't handle mip maps it wouldn't make much sense for it to.   Mapping is correct in this one.
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
                        case FACE_LEFT:
                            mapColorData[eqIndex] = leftColorData[findex]; // NegativeX;
                            break;
                        case FACE_BOTTOM:
                            mapColorData[eqIndex] = bottomColorData[findex]; // NegativeY;
                            break;
                        case FACE_BACK:
                            mapColorData[eqIndex] = backColorData[findex]; // NegativeZ;
                            break;
                        case FACE_RIGHT:
                            mapColorData[eqIndex] = rightColorData[findex]; // PositiveX;
                            break;
                        case FACE_TOP:
                            mapColorData[eqIndex] = topColorData[findex]; // PositiveY;
                            break;
                        case FACE_FRONT:
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
        /// thanks to pumkin pudding for this function.  Modifyed to flip y sign and x z cpu side.
        /// </summary>
        public static Vector3 EquaRectangularMapUvCoordinatesTo3dCubeMapNormal(Vector2 uvCoords)
        {
            float pi = 3.14159265358f;
            Vector3 v = new Vector3(0.0f, 0.0f, 0.0f);
            Vector2 uv = uvCoords;
            uv *= new Vector2(2.0f * pi, pi);
            float siny = (float)Math.Sin(uv.Y);
            v.X = -(float)Math.Sin(uv.X) * siny;
            v.Y = (float)Math.Cos(uv.Y);
            v.Z = -(float)Math.Cos(uv.X) * siny;
            //v = new Vector3(v.Z, -v.Y, v.X);
            return v;
        }

        public static Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(Vector3 a_coords)
        {
            float PI = 3.141592653589793f;
            Vector3 a_coords_n = Vector3.Normalize(a_coords);
            float lon = (float)Math.Atan2(a_coords_n.Z, a_coords_n.X);
            float lat = (float)Math.Acos(a_coords_n.Y);
            Vector2 sphereCoords = new Vector2(lon, lat) * (1.0f / PI);
            return new Vector2(sphereCoords.X * 0.5f + 0.5f, 1f - sphereCoords.Y);
        }

        /// <summary>
        /// Gets cube uv from normal.   Ok at this point im not 100% convinced this is actually aligned properly to a dx cubemap normal.
        /// </summary>
        public static Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(Vector3 normal)
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
        public static Vector2 CubeMapVectorToUvFace(Vector3 v, out int faceIndex)
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
        /// This is the reverse of the cubemapVectorToUvFace. Modifyed to flip y sign and x z cpu side.
        /// </summary>
        public static Vector3 UvFaceToCubeMapVector(Vector2 uv, int faceIndex)
        {
            var u = uv.X * 2f - 1.0f;
            var v = uv.Y * 2f - 1.0f;
            Vector3 dir = new Vector3(0f, 0f, 1f);
            switch (faceIndex)
            {
                case FACE_BACK: 
                    dir = new Vector3(-1f, v, -u); 
                    break;
                case FACE_TOP: 
                    dir = new Vector3(v, -1f, u); 
                    break;
                case FACE_LEFT: 
                    dir = new Vector3(u, v, -1f); 
                    break;
                case FACE_FRONT: 
                    dir = new Vector3(1f, v, u);
                    break;
                case FACE_BOTTOM:
                    dir = new Vector3(-v, 1f, u); 
                    break;
                case FACE_RIGHT:
                    dir = new Vector3(-u, v, 1f); 
                    break;
                default:
                    dir = new Vector3(-1f, -1f, -1f); // na
                    break;
            }
            //dir = new Vector3(dir.Z, -dir.Y, dir.X);
            dir.Normalize();
            return dir;
        }

        /// <summary>
        /// Gets the Cube Face enum from the corresponding integer used in a switch case.
        /// </summary>
        public static CubeMapFace GetFaceFromInt(int face)
        {
            CubeMapFace f;
            switch (face)
            {
                case FACE_LEFT:
                    f = CubeMapFace.NegativeX;
                    break;
                case FACE_BOTTOM:
                    f = CubeMapFace.NegativeY;
                    break;
                case FACE_BACK:
                    f = CubeMapFace.NegativeZ;
                    break;
                case FACE_RIGHT:
                    f = CubeMapFace.PositiveX;
                    break;
                case FACE_TOP:
                    f = CubeMapFace.PositiveY;
                    break;
                case FACE_FRONT:
                    f = CubeMapFace.PositiveZ;
                    break;
                default:
                    f = CubeMapFace.PositiveZ;
                    break;
            }
            return f;
        }

        public static Vector2 UvFromTexturePixel(Texture2D texture, Vector2 pixel)
        {
            return pixel / new Vector2(texture.Width - 1, texture.Height - 1);
        }

        /// <summary>
        /// Required when obtaining a inflected position from a camera thru a plane typically used to place a camera to gain a reflection snapshot for static or dynamic water reflections.
        /// </summary>
        public static Vector3 InflectionPositionFromPlane(Vector3 theCameraPostion, Vector3 thePlanesSurfaceNormal, Vector3 anyPositionOnThePlane)
        {
            // the dot product gives the length, when placed againsts a unit normal so any unit n * a distance is the distance to that normals plane no matter the normals direction. 
            float camToPlaneDist = Vector3.Dot(thePlanesSurfaceNormal, theCameraPostion - anyPositionOnThePlane);
            return theCameraPostion - thePlanesSurfaceNormal * camToPlaneDist * 2;
        }

        public static RenderTargetCube GetEnvRenderTargetCube(GraphicsDevice graphicsDevice, int size, bool mipmaps)
        {
            return new RenderTargetCube(graphicsDevice, size, mipmaps, SurfaceFormat.Color, DepthFormat.Depth24);
        }

        /// <summary>
        /// This returns a perspective projection matrix suitable for a rendertarget cube
        /// </summary>
        public static Matrix GetRenderTargetCubeProjectionMatrix(float near, float far)
        {
            return Matrix.CreatePerspectiveFieldOfView((float)MathHelper.Pi * .5f, 1, near, far);
        }

        public static Matrix CreateAndSetCubeFaceView(Vector3 position, Vector3 forward, Vector3 up)
        {
            return CreateLhLookAt(position, forward + position, up);
        }

        /// <summary>
        /// Creates a opposite monogame createlookat version or a left handed matrix.
        /// This returns a matrix suitable for a render target cube.
        /// </summary>
        public static Matrix CreateLhLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var vector = Vector3.Normalize(cameraPosition - cameraTarget);
            var vector2 = -Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
            var vector3 = Vector3.Cross(-vector, vector2);
            Matrix result = Matrix.Identity;
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M14 = 0f;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M24 = 0f;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M34 = 0f;
            result.M41 = -Vector3.Dot(vector2, cameraPosition);
            result.M42 = -Vector3.Dot(vector3, cameraPosition);
            result.M43 = -Vector3.Dot(vector, cameraPosition);
            result.M44 = 1f;
            return result;
        }

        /// <summary>
        /// Takes a screen position Point and reurns a ray in world space using viewport . unproject(...) , 
        /// The near and far are the z plane depth values used and found in your projection matrix.
        /// </summary>
        public static Ray GetScreenPointAsRayInto3dWorld(this Point screenPosition, Matrix projectionMatrix, Matrix viewMatrix, Matrix world, float near, float far, GraphicsDevice device)
        {
            return GetScreenVector2AsRayInto3dWorld(screenPosition.ToVector2(), projectionMatrix, viewMatrix, world, near, far, device);
        }

        /// <summary>
        /// Or not ?
        /// Takes a screen position Vector2 and reurns a ray in world space using viewport . unproject(...) , 
        /// The near and far are the z plane depth values used and found in your projection matrix.
        /// </summary>
        public static Ray GetScreenVector2AsRayInto3dWorld(this Vector2 screenPosition, Matrix projectionMatrix, Matrix viewMatrix, Matrix world, float near, float far, GraphicsDevice device)
        {
            Vector3 farScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, far); // the projection matrice's far plane value.
            Vector3 nearScreenPoint = new Vector3(screenPosition.X, screenPosition.Y, near); // must be more then zero.
            Vector3 nearWorldPoint = device.Viewport.Unproject(nearScreenPoint, projectionMatrix, viewMatrix, world);
            Vector3 farWorldPoint = device.Viewport.Unproject(farScreenPoint, projectionMatrix, viewMatrix, world);
            Vector3 worldRaysNormal = Vector3.Normalize(farWorldPoint - nearWorldPoint);

            return new Ray(nearWorldPoint, worldRaysNormal);
        }

        /// <summary>
        /// Shortcut.
        /// </summary>
        public static void SaveTexture2D(string path, Texture2D t)
        {
            using (System.IO.Stream fs = System.IO.File.OpenWrite(path))
            {
                t.SaveAsPng(fs, t.Width, t.Height);
            }
        }

        private static Vector3 Abs(Vector3 v)
        {
            if (v.X < 0) v.X = -v.X;
            if (v.Y < 0) v.Y = -v.Y;
            if (v.Z < 0) v.Z = -v.Z;
            return v;
        }
        private static float Mod(float x, float y)
        {
            return x - y * (float)((int)(x / y));
        }
        private static float Floor(float n)
        {
            return (float)(int)(n);
        }
        private static float Abs(float n)
        {
            if (n < 0) return -n; else return n;
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

#define PI 3.141592653589793
                inline float2 RadialCoords(float3 a_coords)
                {
                    float3 a_coords_n = normalize(a_coords);
                    float lon = atan2(a_coords_n.z, a_coords_n.x);
                    float lat = acos(a_coords_n.y);
                    float2 sphereCoords = float2(lon, lat) * (1.0 / PI);
                    return float2(sphereCoords.x * 0.5 + 0.5, 1 - sphereCoords.y);
                }

*/

