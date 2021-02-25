﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XNAV2 = Microsoft.Xna.Framework.Vector2;
using XNAV3 = Microsoft.Xna.Framework.Vector3;
using XNAV4 = Microsoft.Xna.Framework.Vector4;
using XNAQ = Microsoft.Xna.Framework.Quaternion;
using XNA4X4 = Microsoft.Xna.Framework.Matrix;

using MonoScene.Graphics.Content;

namespace MonoScene.Graphics.Pipeline
{
    static class Converters
    {
        public static XNAV2 ToXna(this Assimp.Vector2D v) { return new XNAV2(v.X, v.Y); }

        public static XNAV3 ToXna(this Assimp.Vector3D v) { return new XNAV3(v.X, v.Y, v.Z); }

        public static XNAV3 ToXnaVector(this Assimp.Color3D v) { return new XNAV3(v.R, v.G, v.B); }
        public static XNAV4 ToXnaVector(this Assimp.Color4D v) { return new XNAV4(v.R, v.G, v.B, v.A); }

        public static XNAQ ToXna(this Assimp.Quaternion q) { return new XNAQ(q.X, q.Y, q.Z, q.W); }        

        public static XNA4X4 ToXna(this Assimp.Matrix4x4 m)
        {
            // Assimp matrix needs to be transposed relative to Xna Matrix.

            return new XNA4X4
                (m.A1, m.B1, m.C1, m.D1
                , m.A2, m.B2, m.C2, m.D2
                , m.A3, m.B3, m.C3, m.D3
                , m.A4, m.B4, m.C4, m.D4);
        }

        public static (XNAV3 U, XNAV3 V) ToXna(this System.Numerics.Matrix3x2 m)
        {
            var u = new XNAV3(m.M11, m.M21, m.M31);
            var v = new XNAV3(m.M12, m.M22, m.M32);

            return (u, v);
        }

        public static MaterialContent ToXna(this Assimp.Material srcMaterial)
        {
            var dstMaterial = new MaterialContent();
            dstMaterial.Name = srcMaterial.Name;

            dstMaterial.DoubleSided = srcMaterial.HasTwoSided;
            dstMaterial.Mode = srcMaterial.BlendMode == Assimp.BlendMode.Default ? MaterialBlendMode.Opaque : MaterialBlendMode.Blend;
            dstMaterial.AlphaCutoff = 0.5f;            

            if (srcMaterial.IsPBRMaterial)
            {
                dstMaterial.PreferredShading = "MetallicRoughness";
                SetTexture(dstMaterial, "Normals", srcMaterial.PBR.TextureNormalCamera);
                SetTexture(dstMaterial, "BaseColor", srcMaterial.PBR.TextureBaseColor);
                SetTexture(dstMaterial, "MetallicRoughness", srcMaterial.PBR.TextureMetalness);
                SetTexture(dstMaterial, "Emissive", srcMaterial.PBR.TextureEmissionColor);
            }
            else
            {
                dstMaterial.PreferredShading = "MetallicRoughness";
                SetTexture(dstMaterial, "BaseColor", srcMaterial.TextureDiffuse);
            }

            return dstMaterial;
        }

        public static IReadOnlyList<IMeshDecoder<MaterialContent>> ToXna(this IReadOnlyList<Assimp.Mesh> srcMeshes, IReadOnlyList<Assimp.Material> srcMaterials)
        {
            var dstMaterials = srcMaterials
                .Select(item => item.ToXna())
                .ToArray();

            var dstMeshes = srcMeshes
                .Select(item => new _MeshDecoder<MaterialContent>(item, dstMaterials[item.MaterialIndex]))
                .Cast<IMeshDecoder<MaterialContent>>()
                .ToArray();

            return dstMeshes;
        }

        private static void SetTexture(MaterialContent dstMaterial, string slot, Assimp.TextureSlot srcSlot)
        {
            var dstChannel = dstMaterial.UseChannel(slot);

            dstChannel.VertexIndexSet = srcSlot.UVIndex;            
        }
    }
}