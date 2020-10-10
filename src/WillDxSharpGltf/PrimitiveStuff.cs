//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework
{

    /// <summary>
    /// Made this years ago kinda janky i could do it better now if i ever take the time. Last updated jan 2019.
    /// This is a cube sphere or a sky sphere. A face resolution of 2 is also a cube or sky cube.
    /// It can use 6 seperate images on 6 faces or a cross or blender block type texture..
    /// Both Sphere and skyShere Uses CCW culling in regular operation.
    /// It generates positions normals texture and tangents for normal maping.
    /// It tesselates face points into sphereical coordinates on creation.
    /// It can also switch tangent or normal directions or u v that shouldn't be needed though.
    /// </summary>
    public class SpherePNTT
    {
        bool changeToSkySphere = false;
        bool useSingleImageTexture = false;
        bool blenderStyleElseCross = false;
        bool flipTangentSign = false;
        bool flipVerticeWindingToCW = false;
        bool flipNormalDirection = false;
        bool flipU = false;
        bool flipV = false;
        int verticeFaceResolution = 3;
        float scale = 1f;

        int verticeFaceDrawOffset = 0;
        int indiceFaceDrawOffset = 0;
        int verticesPerFace = 0;
        int indicesPerFace = 0;
        int primitivesPerFace = 0;

        // face identifiers
        const int FaceFront = 0;
        const int FaceBack = 1;
        const int FaceLeft = 2;
        const int FaceRight = 3;
        const int FaceTop = 4;
        const int FaceBottom = 5;

        public VertexPositionColorNormalTextureTangent[] vertices = new VertexPositionColorNormalTextureTangent[24];
        public int[] indices = new int[36];

        /// <summary>
        /// Defaults to a single image hexahedron used on all faces. 
        /// Use the other overloads if you want something more specific like a sphere by increasing vertexResolutionPerFace.
        /// The spheres are counter clockwise wound that can be changed by setting the skysphere bool or fliping the winding direction.
        /// The skySphere is clockwise wound normally 
        /// if you are using opposite or strange orders for normals or whatever this has option to match just about everything.
        /// </summary>
        public SpherePNTT()
        {
            CreateSixFaceSphere(true, false, false, false, false, false, false, false, verticeFaceResolution, scale);
        }
        // seperate faces
        public SpherePNTT(bool changeToSkySphere)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, false, false, false, false, false, verticeFaceResolution, scale);
        }
        // seperate faces at resolution
        public SpherePNTT(bool changeToSkySphere, int vertexResolutionPerFace, float scale)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, false, false, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, int vertexResolutionPerFace, float scale, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, false, flipWindingDirection, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, int vertexResolutionPerFace, float scale, bool flipNormalDirection, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, false, false, flipNormalDirection, flipWindingDirection, false, false, false, vertexResolutionPerFace, scale);
        }
        /// <summary>
        /// Set the type, if the faces are in a single image or six seperate images and if the single image is a cross or blender type image.
        /// Additionally specify the number of vertices per face this value is squared as it is used for rows and columns.
        /// </summary>
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, int vertexResolutionPerFace, float scale)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, false, false, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, int vertexResolutionPerFace, float scale, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, false, flipWindingDirection, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, int vertexResolutionPerFace, float scale, bool flipNormalDirection, bool flipWindingDirection)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, flipNormalDirection, flipWindingDirection, false, false, false, vertexResolutionPerFace, scale);
        }
        public SpherePNTT(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleSkyBox, bool flipNormalDirection, bool flipWindingDirection, bool flipTangentDirection, bool flipTextureDirectionU, bool flipTextureDirectionV, int vertexResolutionPerFace, float scale)
        {
            CreateSixFaceSphere(changeToSkySphere, changeToSingleImageTexture, blenderStyleSkyBox, flipNormalDirection, flipWindingDirection, flipTangentDirection, flipTextureDirectionU, flipTextureDirectionV, vertexResolutionPerFace, scale);
        }

        void CreateSixFaceSphere(bool changeToSkySphere, bool changeToSingleImageTexture, bool blenderStyleElseCross, bool flipNormalDirection, bool flipWindingDirection, bool flipTangentDirection, bool flipU, bool flipV, int vertexResolutionPerFace, float scale)
        {
            this.scale = scale;
            this.changeToSkySphere = changeToSkySphere;
            this.useSingleImageTexture = changeToSingleImageTexture;
            this.blenderStyleElseCross = blenderStyleElseCross;
            this.flipVerticeWindingToCW = flipWindingDirection;
            this.flipNormalDirection = flipNormalDirection;
            this.flipTangentSign = flipTangentDirection;
            this.flipU = flipU;
            this.flipV = flipV;
            if (vertexResolutionPerFace < 2)
                vertexResolutionPerFace = 2;
            this.verticeFaceResolution = vertexResolutionPerFace;
            Vector3 offset = new Vector3(.5f, .5f, .5f);
            // 8 vertice points ill label them, then reassign them for clarity.
            Vector3 LT_f = new Vector3(0, 1, 0) - offset; Vector3 A = LT_f * scale;
            Vector3 LB_f = new Vector3(0, 0, 0) - offset; Vector3 B = LB_f * scale;
            Vector3 RT_f = new Vector3(1, 1, 0) - offset; Vector3 C = RT_f * scale;
            Vector3 RB_f = new Vector3(1, 0, 0) - offset; Vector3 D = RB_f * scale;
            Vector3 LT_b = new Vector3(0, 1, 1) - offset; Vector3 E = LT_b * scale;
            Vector3 LB_b = new Vector3(0, 0, 1) - offset; Vector3 F = LB_b * scale;
            Vector3 RT_b = new Vector3(1, 1, 1) - offset; Vector3 G = RT_b * scale;
            Vector3 RB_b = new Vector3(1, 0, 1) - offset; Vector3 H = RB_b * scale;
            if (flipVerticeWindingToCW)
            {
                LT_f = new Vector3(0, 1, 0) - offset; H = LT_f * scale;
                LB_f = new Vector3(0, 0, 0) - offset; G = LB_f * scale;
                RT_f = new Vector3(1, 1, 0) - offset; F = RT_f * scale;
                RB_f = new Vector3(1, 0, 0) - offset; E = RB_f * scale;
                LT_b = new Vector3(0, 1, 1) - offset; D = LT_b * scale;
                LB_b = new Vector3(0, 0, 1) - offset; C = LB_b * scale;
                RT_b = new Vector3(1, 1, 1) - offset; B = RT_b * scale;
                RB_b = new Vector3(1, 0, 1) - offset; A = RB_b * scale;
            }

            // Six faces to a cube or sphere
            // each face of the cube wont actually share vertices as each will use its own texture.
            // unless it is actually using single skybox texture

            // we will need to precalculate the grids size now
            int vw = vertexResolutionPerFace;
            int vh = vertexResolutionPerFace;
            int vlen = vw * vh * 6; // the extra six here is the number of faces
            int iw = vw - 1;
            int ih = vh - 1;
            int ilen = iw * ih * 6 * 6; // the extra six here is the number of faces
            vertices = new VertexPositionColorNormalTextureTangent[vlen];
            indices = new int[ilen];
            verticeFaceDrawOffset = vlen = vw * vh;
            indiceFaceDrawOffset = ilen = iw * ih * 6;
            verticesPerFace = vertexResolutionPerFace * vertexResolutionPerFace;
            indicesPerFace = iw * ih * 6;
            primitivesPerFace = iw * ih * 2; // 2 triangles per quad

            if (changeToSkySphere)
            {
                // passed uv texture coordinates.
                Vector2 uv0 = new Vector2(1f, 1f);
                Vector2 uv1 = new Vector2(0f, 1f);
                Vector2 uv2 = new Vector2(1f, 0f);
                Vector2 uv3 = new Vector2(0f, 0f);
                SetFaceGrid(FaceFront, D, B, C, A, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBack, F, H, E, G, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceLeft, B, F, A, E, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceRight, H, D, G, C, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceTop, C, A, G, E, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBottom, H, F, D, B, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
            }
            else // regular cube 
            {
                Vector2 uv0 = new Vector2(0f, 0f);
                Vector2 uv1 = new Vector2(0f, 1f);
                Vector2 uv2 = new Vector2(1f, 0f);
                Vector2 uv3 = new Vector2(1f, 1f);
                SetFaceGrid(FaceFront, A, B, C, D, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBack, G, H, E, F, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceLeft, E, F, A, B, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceRight, C, D, G, H, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceTop, E, A, G, C, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
                SetFaceGrid(FaceBottom, B, F, D, H, uv0, uv1, uv2, uv3, vertexResolutionPerFace);
            }
        }

        void SetFaceGrid(int faceMultiplier, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, int vertexResolution)
        {
            if (useSingleImageTexture)
                UvSkyTextureReassignment(faceMultiplier, ref uv0, ref uv1, ref uv2, ref uv3);
            int vw = vertexResolution;
            int vh = vertexResolution;
            int vlen = vw * vh;
            int iw = vw - 1;
            int ih = vh - 1;
            int ilen = iw * ih * 6;
            // actual start index's
            int vIndex = faceMultiplier * vlen;
            int iIndex = faceMultiplier * ilen;
            // we now must build the grid/
            float ratio = 1f / (float)(vertexResolution - 1);
            // well do it all simultaneously no point in spliting it up
            for (int y = 0; y < vertexResolution; y++)
            {
                float ratioY = (float)y * ratio;
                for (int x = 0; x < vertexResolution; x++)
                {
                    // index
                    int index = vIndex + (y * vertexResolution + x);
                    float ratioX = (float)x * ratio;
                    // calculate uv_n_p tangent comes later
                    var uv = InterpolateUv(uv0, uv1, uv2, uv3, ratioX, ratioY);
                    var n = InterpolateToNormal(v0, v1, v2, v3, ratioX, ratioY);
                    var p = n * .5f * scale; // displace to distance
                    if (changeToSkySphere)
                        n = -n;
                    if (flipNormalDirection)
                        n = -n;
                    // handle u v fliping if its desired.
                    if (flipU)
                        uv.X = 1.0f - uv.X;
                    if (flipV)
                        uv.Y = 1.0f - uv.Y;
                    // assign
                    vertices[index].Position = p;
                    vertices[index].Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                    vertices[index].TextureCoordinate = uv;
                    vertices[index].Normal = n;
                }
            }

            // ToDo... 
            // We could loop all the vertices which are nearly the exact same and make sure they are the same place but seperate.
            // sort of redundant but floating point errors happen under interpolation, well get back to that later on.
            // not sure i really need to it looks pretty spot on.

            // ok so now we have are positions our normal and uv per vertice we need to loop again and handle the tangents
            for (int y = 0; y < (vertexResolution - 1); y++)
            {
                for (int x = 0; x < (vertexResolution - 1); x++)
                {
                    //
                    int indexV0 = vIndex + (y * vertexResolution + x);
                    int indexV1 = vIndex + ((y + 1) * vertexResolution + x);
                    int indexV2 = vIndex + (y * vertexResolution + (x + 1));
                    int indexV3 = vIndex + ((y + 1) * vertexResolution + (x + 1));
                    var p0 = vertices[indexV0].Position;
                    var p1 = vertices[indexV1].Position;
                    var p2 = vertices[indexV2].Position;
                    var p3 = vertices[indexV3].Position;
                    var t = -(p0 - p1);
                    if (changeToSkySphere)
                        t = -t;
                    t.Normalize();
                    if (flipTangentSign)
                        t = -t;
                    vertices[indexV0].Tangent = t; vertices[indexV1].Tangent = t; vertices[indexV2].Tangent = t; vertices[indexV3].Tangent = t;
                    //
                    // set our indices while were at it.
                    int indexI = iIndex + ((y * (vertexResolution - 1) + x) * 6);
                    int via = indexV0, vib = indexV1, vic = indexV2, vid = indexV3;
                    indices[indexI + 0] = via; indices[indexI + 1] = vib; indices[indexI + 2] = vic;
                    indices[indexI + 3] = vic; indices[indexI + 4] = vib; indices[indexI + 5] = vid;
                }
            }
        }

        // this allows for the use of a single texture skybox.
        void UvSkyTextureReassignment(int faceMultiplier, ref Vector2 uv0, ref Vector2 uv1, ref Vector2 uv2, ref Vector2 uv3)
        {
            if (useSingleImageTexture)
            {
                Vector2 tupeBuvwh = new Vector2(.250000000f, .333333333f); // this is a 8 square left sided skybox
                Vector2 tupeAuvwh = new Vector2(.333333333f, .500000000f); // this is a 6 square blender type skybox
                Vector2 currentuvWH = tupeBuvwh;
                Vector2 uvStart = Vector2.Zero;
                Vector2 uvEnd = Vector2.Zero;

                // crossstyle
                if (blenderStyleElseCross == false)
                {
                    currentuvWH = tupeBuvwh;
                    switch (faceMultiplier)
                    {
                        case FaceFront:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBack:
                            uvStart = new Vector2(currentuvWH.X * 3f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceRight:
                            uvStart = new Vector2(currentuvWH.X * 2f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceLeft:
                            uvStart = new Vector2(currentuvWH.X * 0f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceTop:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBottom:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 2f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                    }
                    if (changeToSkySphere)
                    {
                        uv0 = new Vector2(uvEnd.X, uvEnd.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv3 = new Vector2(uvStart.X, uvStart.Y);
                    }
                    else
                    {
                        uv0 = new Vector2(uvStart.X, uvStart.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv3 = new Vector2(uvEnd.X, uvEnd.Y);
                    }
                }
                else
                {
                    currentuvWH = tupeAuvwh;
                    switch (faceMultiplier)
                    {
                        case FaceLeft:
                            uvStart = new Vector2(currentuvWH.X * 0f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBack:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceRight:
                            uvStart = new Vector2(currentuvWH.X * 2f, currentuvWH.Y * 0f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceBottom:
                            uvStart = new Vector2(currentuvWH.X * 0f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceTop:
                            uvStart = new Vector2(currentuvWH.X * 1f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                        case FaceFront:
                            uvStart = new Vector2(currentuvWH.X * 2f, currentuvWH.Y * 1f);
                            uvEnd = uvStart + currentuvWH;
                            break;
                    }
                    if (changeToSkySphere)
                    {
                        uv0 = new Vector2(uvEnd.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv3 = new Vector2(uvStart.X, uvStart.Y);
                    }
                    else
                    {
                        uv0 = new Vector2(uvStart.X, uvStart.Y); uv1 = new Vector2(uvStart.X, uvEnd.Y); uv2 = new Vector2(uvEnd.X, uvStart.Y); uv3 = new Vector2(uvEnd.X, uvEnd.Y);
                    }
                }
            }
        }

        Vector3 InterpolateToNormal(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float timeX, float timeY)
        {
            var y0 = ((v1 - v0) * timeY + v0);
            var y1 = ((v3 - v2) * timeY + v2);
            var n = ((y1 - y0) * timeX + y0) * 10f; // * 10f ensure its sufficiently denormalized.
            n.Normalize();
            return n;
        }
        Vector2 InterpolateUv(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float timeX, float timeY)
        {
            var y0 = ((v1 - v0) * timeY + v0);
            var y1 = ((v3 - v2) * timeY + v2);
            return ((y1 - y0) * timeX + y0);
        }

        public void Draw(GraphicsDevice gd, Effect effect)
        {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, (indices.Length / 3), VertexPositionColorNormalTextureTangent.VertexDeclaration);
            }
        }

        /// <summary>
        /// Seperate faced cube or sphere or sky
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void Draw(GraphicsDevice gd, Effect effect, Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D top, Texture2D bottom)
        {
            int FaceFront = 0;
            int FaceBack = 1;
            int FaceLeft = 2;
            int FaceRight = 3;
            int FaceTop = 4;
            int FaceBottom = 5;
            for (int t = 0; t < 6; t++)
            {
                if (t == FaceFront) effect.Parameters["TextureA"].SetValue(front);
                if (t == FaceBack) effect.Parameters["TextureA"].SetValue(back);
                if (t == FaceLeft) effect.Parameters["TextureA"].SetValue(left);
                if (t == FaceRight) effect.Parameters["TextureA"].SetValue(right);
                if (t == FaceTop) effect.Parameters["TextureA"].SetValue(top);
                if (t == FaceBottom) effect.Parameters["TextureA"].SetValue(bottom);
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// Single texture multi faced cube or sphere or sky
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void Draw(GraphicsDevice gd, Effect effect, TextureCube cubeTexture)
        {
            effect.Parameters["CubeMap"].SetValue(cubeTexture);
            for (int t = 0; t < 6; t++)
            {
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void DrawWithBasicEffect(GraphicsDevice gd, BasicEffect effect, Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D top, Texture2D bottom)
        {
            int FaceFront = 0;
            int FaceBack = 1;
            int FaceLeft = 2;
            int FaceRight = 3;
            int FaceTop = 4;
            int FaceBottom = 5;
            for (int t = 0; t < 6; t++)
            {
                if (t == FaceFront) effect.Texture = front;
                if (t == FaceBack) effect.Texture = back;
                if (t == FaceLeft) effect.Texture = left;
                if (t == FaceRight) effect.Texture = right;
                if (t == FaceTop) effect.Texture = top;
                if (t == FaceBottom) effect.Texture = bottom;
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        /// <summary>
        /// Single texture multi faced cube or sphere or sky
        /// This method is pretty dependant on being able to pass to textureA not good but....
        /// </summary>
        public void DrawWithBasicEffect(GraphicsDevice gd, BasicEffect effect, Texture2D cubeTexture)
        {
            effect.Texture = cubeTexture;
            for (int t = 0; t < 6; t++)
            {
                int ifoffset = t * indicesPerFace;
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, ifoffset, primitivesPerFace, VertexPositionColorNormalTextureTangent.VertexDeclaration);
                }
            }
        }

        public Vector3 Norm(Vector3 n)
        {
            return Vector3.Normalize(n);
        }

        /// <summary>
        /// Positional cross product, Counter Clock wise positive.
        /// </summary>
        public static Vector3 CrossVectors3d(Vector3 a, Vector3 b, Vector3 c)
        {
            // no point in doing reassignments the calculation is straight forward.
            return new Vector3
                (
                ((b.Y - a.Y) * (c.Z - b.Z)) - ((c.Y - b.Y) * (b.Z - a.Z)),
                ((b.Z - a.Z) * (c.X - b.X)) - ((c.Z - b.Z) * (b.X - a.X)),
                ((b.X - a.X) * (c.Y - b.Y)) - ((c.X - b.X) * (b.Y - a.Y))
                );
        }

        /// <summary>
        /// use the vector3 cross
        /// </summary>
        public static Vector3 CrossXna(Vector3 a, Vector3 b, Vector3 c)
        {
            var v1 = a - b;
            var v2 = c - b;

            return Vector3.Cross(v1, v2);
        }
    }

    public struct VertexPositionColorNormalTextureTangent : IVertexType
    {
        public Vector3 Position;
        public Vector4 Color;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector3 Tangent;

        public static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
              new VertexElement(VertexElementByteOffset.PositionStartOffset(), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector4(), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector2(), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
              new VertexElement(VertexElementByteOffset.OffsetVector3(), VertexElementFormat.Vector3, VertexElementUsage.Normal, 1)
        );
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }
    }
    /// <summary>
    /// This is a helper struct for tallying byte offsets
    /// </summary>
    public struct VertexElementByteOffset
    {
        public static int currentByteSize = 0;
        //[STAThread]
        public static int PositionStartOffset() { currentByteSize = 0; var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(int n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(float n) { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector2 n) { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Color n) { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector3 n) { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int Offset(Vector4 n) { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }

        public static int OffsetInt() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetFloat() { var s = sizeof(float); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetColor() { var s = sizeof(int); currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector2() { var s = sizeof(float) * 2; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector3() { var s = sizeof(float) * 3; currentByteSize += s; return currentByteSize - s; }
        public static int OffsetVector4() { var s = sizeof(float) * 4; currentByteSize += s; return currentByteSize - s; }
    }
}
