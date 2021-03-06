﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GLTFMATERIAL = SharpGLTF.Schema2.Material;

namespace SharpGLTF.Runtime.Content
{
    public class PBREffectsLoaderContext : LoaderContext
    {
        #region lifecycle        

        public PBREffectsLoaderContext(GraphicsDevice device) : base(device)
        {            
        }

        #endregion

        #region effects creation        

        protected override Effect CreateEffect(GLTFMATERIAL srcMaterial, bool isSkinned)
        {
            if (srcMaterial.Unlit)
            {
                var ueffect = new UnlitEffect(this.Device);

                TransferChannel(ueffect.BaseColorMap, srcMaterial, "BaseColor", Vector4.One);
                TransferChannel(ueffect.EmissiveMap, srcMaterial, "Emissive", Vector3.Zero);
                TransferChannel(ueffect.OcclusionMap, srcMaterial, "Occlusion", 0);
                if (ueffect.OcclusionMap.Texture == null) ueffect.OcclusionMap.Scale = 0;

                return ueffect;
            }

            PBREffect effect = null;

            if (srcMaterial.FindChannel("SpecularGlossiness") != null)
            {
                var xeffect = new PBRSpecularGlossinessEffect(this.Device);
                effect = xeffect;

                TransferChannel(xeffect.DiffuseMap, srcMaterial, "Diffuse", Vector4.One);
                TransferChannel(xeffect.SpecularGlossinessMap, srcMaterial, "SpecularGlossiness", Vector4.Zero);
            }
            else
            {
                var xeffect = new PBRMetallicRoughnessEffect(this.Device);
                effect = xeffect;

                TransferChannel(xeffect.BaseColorMap, srcMaterial, "BaseColor", Vector4.One);
                TransferChannel(xeffect.MetalRoughnessMap, srcMaterial, "MetallicRoughness", Vector2.One);
            }            

            TransferChannel(effect.NormalMap, srcMaterial, "Normal", 1);
            TransferChannel(effect.EmissiveMap, srcMaterial, "Emissive", Vector3.Zero);
            TransferChannel(effect.OcclusionMap, srcMaterial, "Occlusion", 0);            
            if (effect.OcclusionMap.Texture == null) effect.OcclusionMap.Scale = 0;            

            return effect;
        }

        #endregion

        #region meshes creation

        protected override void WriteMeshPrimitive(MeshPrimitiveReader srcPrimitive, Effect effect, BlendState blending, bool doubleSided)
        {
            if (srcPrimitive.IsSkinned) WriteMeshPrimitive<VertexSkinned>(effect, blending, doubleSided, srcPrimitive);
            else WriteMeshPrimitive<VertexRigid>(effect, blending, doubleSided, srcPrimitive);
        }

        #endregion

        #region gltf helpers
        
        private void TransferChannel(EffectTexture2D.Scalar1 dst, GLTFMATERIAL src, string name, float defval)
        {            
            dst.Texture = UseTexture(src, name);
            dst.Sampler = UseSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        private void TransferChannel(EffectTexture2D.Scalar2 dst, GLTFMATERIAL src, string name, Vector2 defval)
        {
            dst.Texture = UseTexture(src, name);
            dst.Sampler = UseSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        private void TransferChannel(EffectTexture2D.Scalar3 dst, GLTFMATERIAL src, string name, Vector3 defval)
        {
            dst.Texture = UseTexture(src, name);
            dst.Sampler = UseSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        private void TransferChannel(EffectTexture2D.Scalar4 dst, GLTFMATERIAL src, string name, Vector4 defval)
        {
            dst.Texture = UseTexture(src, name);
            dst.Sampler = UseSampler(src, name);
            dst.Scale = GetScaler(src, name, defval);
            dst.SetIndex = GetTextureSet(src, name);
            dst.Transform = GetTransform(src, name);
        }

        private float GetScaler(GLTFMATERIAL srcMaterial, string name, float defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return param.X;
        }

        private (Vector3 u, Vector3 v) GetTransform(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return (Vector3.UnitX,Vector3.UnitY);

            if (channel.Value.TextureTransform == null) return (Vector3.UnitX, Vector3.UnitY);

            var S = System.Numerics.Matrix3x2.CreateScale(channel.Value.TextureTransform.Scale);
            var R = System.Numerics.Matrix3x2.CreateRotation(-channel.Value.TextureTransform.Rotation);
            var T = System.Numerics.Matrix3x2.CreateTranslation(channel.Value.TextureTransform.Offset);
            
            var X = S * R * T;

            return (new Vector3(X.M11, X.M21, X.M31), new Vector3(X.M12, X.M22, X.M32));
        }

        private int GetTextureSet(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return 0;

            if (channel.Value.TextureTransform == null) return channel.Value.TextureCoordinate;

            return channel.Value.TextureTransform.TextureCoordinateOverride ?? channel.Value.TextureCoordinate;
        }

        private Vector2 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector2 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector2(param.X, param.Y);
        }

        private Vector3 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector3 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector3(param.X, param.Y, param.Z);
        }

        private Vector4 GetScaler(GLTFMATERIAL srcMaterial, string name, Vector4 defval)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return defval;
            var param = channel.Value.Parameter;

            return new Vector4(param.X, param.Y, param.Z, param.W);
        }

        private Texture2D UseTexture(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return null;
            if (channel.Value.Texture == null) return null;
            if (channel.Value.Texture.PrimaryImage == null) return null;
            if (channel.Value.Texture.PrimaryImage.Content.IsEmpty) return null;

            return UseTexture(channel.Value, null);
        }

        private SamplerState UseSampler(GLTFMATERIAL srcMaterial, string name)
        {
            var channel = srcMaterial.FindChannel(name);

            if (!channel.HasValue) return null;
            if (channel.Value.Texture == null) return null;

            return UseSampler(channel.Value.TextureSampler);
        }

        #endregion

        #region vertex types

        [System.Diagnostics.DebuggerDisplay("{_ToDebugString(),nq}")]
        struct VertexRigid : IVertexType
        {
            #region debug

            private string _ToDebugString()
            {
                var p = $"{Position.X:N5} {Position.Y:N5} {Position.Z:N5}";
                var n = $"{Normal.X:N2} {Normal.Y:N2} {Normal.Z:N2}";
                var t = $"{Tangent.X:N2} {Tangent.Y:N2} {Tangent.Z:N2} {Tangent.W:N1}";
                var uv0 = $"{TextureCoordinate0.X:N3} {TextureCoordinate0.Y:N3}";
                var uv1 = $"{TextureCoordinate1.X:N3} {TextureCoordinate1.Y:N3}";

                return $"𝐏:{p}   𝚴:{n}   𝚻:{t}   𝐂:{Color.PackedValue:X}   𝐔𝐕₀:{uv0}   𝐔𝐕₁:{uv1}";
            }

            #endregion

            #region static

            private static VertexDeclaration _VDecl = CreateVertexDeclaration();

            public static VertexDeclaration CreateVertexDeclaration()
            {
                int offset = 0;

                var a = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
                offset += 3 * 4;

                var b = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
                offset += 3 * 4;

                var c = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0);
                offset += 4 * 4;

                var d = new VertexElement(offset, VertexElementFormat.Color, VertexElementUsage.Color, 0);
                offset += 4 * 1;

                var e = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
                offset += 2 * 4;

                var f = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1);
                offset += 2 * 4;

                return new VertexDeclaration(a, b, c, d, e, f);
            }

            #endregion

            #region data

            public VertexDeclaration VertexDeclaration => _VDecl;

            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Tangent;
            public Color Color;
            public Vector2 TextureCoordinate0;
            public Vector2 TextureCoordinate1;

            #endregion
        }

        [System.Diagnostics.DebuggerDisplay("{_ToDebugString(),nq}")]
        struct VertexSkinned : IVertexType
        {
            #region debug

            private string _ToDebugString()
            {
                var p = $"{Position.X:N5} {Position.Y:N5} {Position.Z:N5}";
                var n = $"{Normal.X:N2} {Normal.Y:N2} {Normal.Z:N2}";
                var t = $"{Tangent.X:N2} {Tangent.Y:N2} {Tangent.Z:N2} {Tangent.W:N1}";
                var uv0 = $"{TextureCoordinate0.X:N3} {TextureCoordinate0.Y:N3}";
                var uv1 = $"{TextureCoordinate1.X:N3} {TextureCoordinate1.Y:N3}";
                var jv = BlendIndices.ToVector4();
                var j = $"{jv.X:N3} {jv.Y:N3} {jv.Z:N3} {jv.W:N3}";

                return $"𝐏:{p}   𝚴:{n}   𝚻:{t}   𝐂:{Color.PackedValue:X}   𝐔𝐕₀:{uv0}   𝐔𝐕₁:{uv1}   𝐉𝐖:{j}";
            }

            #endregion

            #region static

            private static VertexDeclaration _VDecl = CreateVertexDeclaration();

            public static VertexDeclaration CreateVertexDeclaration()
            {
                int offset = 0;

                var a = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);
                offset += 3 * 4;

                var b = new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);
                offset += 3 * 4;

                var c = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0);
                offset += 4 * 4;

                var d = new VertexElement(offset, VertexElementFormat.Color, VertexElementUsage.Color, 0);
                offset += 4 * 1;

                var e = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
                offset += 2 * 4;

                var f = new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1);
                offset += 2 * 4;

                var g = new VertexElement(offset, VertexElementFormat.Byte4, VertexElementUsage.BlendIndices, 0);
                offset += 4 * 1;

                var h = new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0);
                offset += 4 * 4;

                return new VertexDeclaration(a, b, c, d, e, f, g, h);
            }

            #endregion

            #region data

            public VertexDeclaration VertexDeclaration => _VDecl;

            public Vector3 Position;
            public Vector3 Normal;
            public Vector4 Tangent;
            public Color Color;
            public Vector2 TextureCoordinate0;
            public Vector2 TextureCoordinate1;
            public Microsoft.Xna.Framework.Graphics.PackedVector.Byte4 BlendIndices;
            public Vector4 BlendWeight;

            #endregion
        }

        #endregion
    }
}