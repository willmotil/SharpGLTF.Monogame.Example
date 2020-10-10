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

        /// <summary>
        /// Set faces to cubemap by name, neg xyz,  pos  xyz.  tested passes.
        /// </summary>
        public static TextureCube SetIndividualFacesToCubeMap(GraphicsDevice gd, int size, TextureCube map, Texture2D textureLeft, Texture2D textureBottom, Texture2D textureBack, Texture2D textureRight, Texture2D textureTop, Texture2D textureFront)
        {
            return SetIndividualCubeFacesToCubeMap(gd, size, map, textureLeft, textureBottom, textureBack, textureRight, textureTop, textureFront);
        }

        /// <summary>
        /// Set faces to cubemap, neg xyz,  pos  xyz. tested passes
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
        /// This sets a individual texture and its mipmaps to a cubemap.   tested passes
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

        ///// <summary>
        ///// Ok so this is a destination pixel version this version attempts to include mip levels.    testing Fails.
        ///// However lots of dimensional variables to account for here.
        ///// </summary>
        //public static TextureCube GetCubeMapFromEquaRectangularMap(GraphicsDevice gd, Texture2D equaRectangularMap, int faceSize)
        //{
        //    TextureCube cubeMap = new TextureCube(gd, faceSize, true, SurfaceFormat.Color);
        //    var eqLevelCount = equaRectangularMap.LevelCount;
        //    for (int level = 0; level < eqLevelCount; level += 1)
        //    {
        //        int eqw = equaRectangularMap.Width >> level;
        //        int eqh = equaRectangularMap.Height >> level;
        //        var mapColorData = new Color[(equaRectangularMap.Width >> level) * (equaRectangularMap.Height >> level)];
        //        equaRectangularMap.GetData(level, null, mapColorData, 0, mapColorData.Length);
        //        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        //        {
        //            var adjFaceSize = faceSize >> level;
        //            var wh = new Vector2(adjFaceSize - 1, adjFaceSize - 1);
        //            var faceData = new Color[adjFaceSize * adjFaceSize];
        //            for (int y = 0; y < adjFaceSize; y++)
        //            {
        //                for (int x = 0; x < adjFaceSize; x++)
        //                {
        //                    var fuv = new Vector2(x, y) / wh;
        //                    var v = UvFaceToCubeMapVector(fuv, faceIndex);
        //                    var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
        //                    var eqIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
        //                    faceData[x + (y * adjFaceSize)] = mapColorData[eqIndex];
        //                }
        //            }
        //            var cubeMapFace = GetFaceFromInt(faceIndex);
        //            cubeMap.SetData(cubeMapFace, level, null, faceData, 0, faceData.Length);
        //        }
        //    }
        //    return cubeMap;
        //}

        /// <summary>  v2
        /// Ok so this is a destination pixel version this version attempts to include mip levels.    testing Fails.
        /// However lots of dimensional variables to account for here.
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
                //int eqw = equaRectangularMap.Width >> level;
                //int eqh = equaRectangularMap.Height >> level;
                //var mapColorData = new Color[(equaRectangularMap.Width >> level) * (equaRectangularMap.Height >> level)];
                //equaRectangularMap.GetData(level, null, mapColorData, 0, mapColorData.Length);
                for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                {
                    var adjFaceSize = faceSize >> level;
                    var faceWh = new Vector2(adjFaceSize, adjFaceSize);//new Vector2(adjFaceSize , adjFaceSize );//new Vector2(adjFaceSize - 1, adjFaceSize - 1);
                    //if (faceWh.X < 1) faceWh.X = 1;
                    //if (faceWh.Y < 1) faceWh.Y = 1;
                    var faceData = new Color[adjFaceSize * adjFaceSize];
                    for (int y = 0; y < adjFaceSize; y++)
                    {
                        for (int x = 0; x < adjFaceSize; x++)
                        {
                            var fuv = new Vector2(x, y) / faceWh;
                            var v = UvFaceToCubeMapVector(fuv, faceIndex);
                            var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinates(v);
                            var eqIndex = (int)(uv.X * eqw) + ((int)(uv.Y * eqh) * eqw);
                            faceData[x + (y * adjFaceSize)] = eqColorData[eqIndex];
                        }
                    }
                    var cubeMapFace = GetFaceFromInt(faceIndex);
                    cubeMap.SetData(cubeMapFace, level, null, faceData, 0, faceData.Length);
                }
            }
            return cubeMap;
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
                        //var uv = CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(v);
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
        /// Gets cube uv from normal.   Ok at this point im not 100% convinced this is actually aligned properly to a dx cubemap normal.
        /// </summary>
        private static Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinates(Vector3 normal)
        {
            Vector2 INVERT_ATAN = new Vector2(0.1591f, 0.3183f);
            Vector2 uv = new Vector2((float)Math.Atan2(normal.Z, normal.X), (float)Math.Asin(normal.Y));
            uv *= INVERT_ATAN;
            uv += new Vector2(0.5f, 0.5f);
            return uv;
        }

        private static Vector2 CubeMapNormalTo2dEquaRectangularMapUvCoordinatesAlt(Vector3 a_coords)
        {
            float PI = 3.141592653589793f;
            Vector3 a_coords_n = Vector3.Normalize(a_coords);
            float lon = (float)Math.Atan2(a_coords_n.Z, a_coords_n.X);
            float lat = (float)Math.Acos(a_coords_n.Y);
            Vector2 sphereCoords = new Vector2(lon, lat) * (1.0f / PI);
            return new Vector2(sphereCoords.X * 0.5f + 0.5f, 1f - sphereCoords.Y);
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
                case FACE_BACK: // FACE_LEFT:
                    dir = new Vector3(-1f, v, -u); // back
                    break;
                case FACE_TOP: // FACE_BOTTOM:
                    dir = new Vector3(v, -1f, u); // top
                    break;
                case FACE_LEFT: // FACE_BACK:
                    dir = new Vector3(u, v, -1f); // left
                    break;
                case FACE_FRONT: // FACE_RIGHT:
                    dir = new Vector3(1f, v, u); // front
                    break;
                case FACE_BOTTOM: // FACE_TOP:
                    dir = new Vector3(-v, 1f, u); // bottom
                    break;
                case FACE_RIGHT: // FACE_FRONT:
                    dir = new Vector3(-u, v, 1f); // right
                    break;
                default:
                    dir = new Vector3(-1f, -1f, -1f); // na
                    break;
            }
            dir.Normalize();
            return dir;
        }

        //private static Vector3 UvFaceToCubeMapVector(Vector2 uv, int faceIndex)
        //{
        //    var u = uv.X * 2f - 1.0f;
        //    var v = uv.Y * 2f - 1.0f;
        //    //var u = uv.X - .5f;
        //    //var v = uv.Y - .5f;
        //    Vector3 dir = new Vector3(0f, 0f, 1f);

        //    switch (faceIndex)
        //    {
        //        case FACE_LEFT:
        //            dir = new Vector3(-1f, v, u);
        //            break;
        //        case FACE_BOTTOM:
        //            dir = new Vector3(-u, -1f, v);
        //            break;
        //        case FACE_BACK:
        //            dir = new Vector3(u, v, -1f);
        //            break;
        //        case FACE_RIGHT:
        //            dir = new Vector3(1f, v, -u);
        //            break;
        //        case FACE_TOP:
        //            dir = new Vector3(u, 1f, -v);
        //            break;
        //        case FACE_FRONT:
        //            dir = new Vector3(-u, v, 1f);
        //            break;
        //        default:
        //            dir = new Vector3(-1f, v, u);
        //            break;
        //    }
        //    dir.Normalize();
        //    return dir;
        //}

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

        /// <summary>
        /// Required when obtaining a inflected position from a camera thru a plane typically used to place a camera to gain a reflection snapshot for static or dynamic water reflections.
        /// </summary>
        private static Vector3 InflectionPositionFromPlane(Vector3 theCameraPostion, Vector3 thePlanesSurfaceNormal, Vector3 anyPositionOnThePlane)
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

        public static void SaveTexture2D(string path, Texture2D t)
        {
            using (System.IO.Stream fs = System.IO.File.OpenWrite(path))
            {
                t.SaveAsPng(fs, t.Width, t.Height);
            }
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

