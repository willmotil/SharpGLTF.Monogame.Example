﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;

using SharpGLTF.Schema2;

using SRCMESH = SharpGLTF.Schema2.Mesh;
using SRCPRIM = SharpGLTF.Schema2.MeshPrimitive;
using SRCMATERIAL = SharpGLTF.Schema2.Material;

using MODELMESH = SharpGLTF.Runtime.Content.RuntimeModelMesh;

namespace SharpGLTF.Runtime.Content
{
    /// <summary>
    /// Helper class used to import a glTF meshes and materials into MonoGame
    /// </summary>
    public abstract class LoaderContext
    {
        #region lifecycle

        public static LoaderContext CreateLoaderContext(GraphicsDevice device)
        {
            if (device.GraphicsProfile == GraphicsProfile.HiDef) return new PBREffectsLoaderContext(device);

            return new BasicEffectsLoaderContext(device);
        }

        public LoaderContext(GraphicsDevice device)
        {
            _Device = device;
        }

        #endregion

        #region data

        private GraphicsDevice _Device;

        private GraphicsResourceTracker _Disposables;
        
        private EffectsFactory _MatFactory;

        // gathers all meshes using shared vertex and index buffers whenever possible.
        private MeshPrimitiveWriter _MeshWriter;
        private int _CurrentMeshIndex;

        // used as a container to a default material;
        private SharpGLTF.Schema2.ModelRoot _DummyModel; 

        #endregion

        #region properties

        protected GraphicsDevice Device => _Device;

        internal IReadOnlyList<GraphicsResource> Disposables => _Disposables.Disposables;

        #endregion

        #region API

        internal void Reset()
        {
            _Disposables = new GraphicsResourceTracker();
            _MatFactory = new EffectsFactory(_Device, _Disposables);
            _MeshWriter = new MeshPrimitiveWriter();
        }

        public MonoGameDeviceContent<MonoGameModelTemplate> LoadDeviceModel(string modelFilepath)
        {
            return MonoGameModelTemplate.LoadDeviceModel(_Device, modelFilepath, this);
        }

        public MonoGameDeviceContent<MonoGameModelTemplate> CreateDeviceModel(ModelRoot model)
        {
            return MonoGameModelTemplate.CreateDeviceModel(_Device, model, this);
        }

        #endregion

        #region Mesh API

        internal void _WriteMesh(SRCMESH srcMesh)
        {
            if (_Device == null) throw new InvalidOperationException();            

            var srcPrims = _GetValidPrimitives(srcMesh)
                .ToDictionary(item => item, item => new MeshPrimitiveReader(item));

            MeshPrimitiveReader.GenerateNormalsAndTangents(srcPrims.Values);
            
            foreach (var srcPrim in srcPrims)
            {
                _CurrentMeshIndex = srcMesh.LogicalIndex;

                _WriteMeshPrimitive(srcPrim.Value, srcPrim.Key.Material);                
            }            
        }

        private static IEnumerable<SRCPRIM> _GetValidPrimitives(SRCMESH srcMesh)
        {
            foreach (var srcPrim in srcMesh.Primitives)
            {
                var ppp = srcPrim.GetVertexAccessor("POSITION");
                if (ppp.Count < 3) continue;

                if (srcPrim.DrawPrimitiveType == Schema2.PrimitiveType.POINTS) continue;
                if (srcPrim.DrawPrimitiveType == Schema2.PrimitiveType.LINES) continue;
                if (srcPrim.DrawPrimitiveType == Schema2.PrimitiveType.LINE_LOOP) continue;
                if (srcPrim.DrawPrimitiveType == Schema2.PrimitiveType.LINE_STRIP) continue;

                yield return srcPrim;
            }
        }

        private void _WriteMeshPrimitive(MeshPrimitiveReader srcPrim, SRCMATERIAL srcMaterial)
        {
            if (srcMaterial == null) srcMaterial = GetDefaultMaterial();

            var effect = _MatFactory.GetMaterial(srcMaterial, srcPrim.IsSkinned);

            if (effect == null)
            {
                effect = CreateEffect(srcMaterial, srcPrim.IsSkinned);
                _MatFactory.Register(srcMaterial, srcPrim.IsSkinned, effect);
            }

            var blending = BlendState.Opaque;

            if (effect is AnimatedEffect animEffect)
            {
                animEffect.AlphaCutoff = -1;

                if (srcMaterial.Alpha == AlphaMode.BLEND)
                {
                    blending = BlendState.NonPremultiplied;
                    animEffect.AlphaBlend = true;
                }
                if (srcMaterial.Alpha == AlphaMode.MASK)
                {
                    animEffect.AlphaCutoff = srcMaterial.AlphaCutoff;
                }
            }

            if (effect is PBREffect pbrEffect)            
            {
                pbrEffect.NormalMode = srcMaterial.DoubleSided ? GeometryNormalMode.DoubleSided : GeometryNormalMode.Reverse;                
            }
            
            WriteMeshPrimitive(srcPrim, effect, blending, srcMaterial.DoubleSided);
        }        

        protected abstract void WriteMeshPrimitive(MeshPrimitiveReader srcPrimitive, Effect effect, BlendState blending, bool doubleSided);

        protected void WriteMeshPrimitive<TVertex>(Effect effect, BlendState blending, bool doubleSided, MeshPrimitiveReader primitive)
            where TVertex : unmanaged, IVertexType
        {
            _MeshWriter.WriteMeshPrimitive<TVertex>(_CurrentMeshIndex, effect, blending, doubleSided, primitive);
        }

        #endregion

        #region EFfects API

        /// <summary>
        /// Called when finding a new material that needs to be converted to an Effect.
        /// </summary>
        /// <param name="srcMaterial">The material to convert.</param>
        /// <param name="isSkinned">Indicates that the material is used in a skinned mesh.</param>
        /// <returns>An effect to be used in place of <paramref name="srcMaterial"/>. </returns>
        protected abstract Effect CreateEffect(Material srcMaterial, bool isSkinned);

        protected virtual Texture2D UseTexture(MaterialChannel? channel, string name)
        {
            return _MatFactory.UseTexture(channel, name);
        }

        protected virtual SamplerState UseSampler(Schema2.TextureSampler gltfSampler)
        {
            // glTF default is LinearWrap
            if (gltfSampler == null) return SamplerState.LinearWrap;

            // First we check if we can use one of the SamplerState predefined values.
            if (gltfSampler.MinFilter == TextureMipMapFilter.DEFAULT && gltfSampler.MagFilter == TextureInterpolationFilter.DEFAULT)
            {
                if (gltfSampler.WrapS == TextureWrapMode.CLAMP_TO_EDGE && gltfSampler.WrapT == TextureWrapMode.CLAMP_TO_EDGE)
                {
                    return SamplerState.LinearClamp;
                }

                if (gltfSampler.WrapS == TextureWrapMode.REPEAT && gltfSampler.WrapT == TextureWrapMode.REPEAT)
                {
                    return SamplerState.LinearWrap;
                }                
            }

            // if we cannot use a predefined value, we have to create a new SamplerState.
            return _MatFactory.UseSampler(gltfSampler);
        }

        

        #endregion

        #region resources API

        internal IReadOnlyDictionary<int, MODELMESH> CreateRuntimeModels()
        {
            return _MeshWriter.GetRuntimeMeshes(_Device, _Disposables);
        }

        private Material GetDefaultMaterial()
        {
            if (_DummyModel == null)  // !=
            {
                _DummyModel = ModelRoot.CreateModel();
                _DummyModel.CreateMaterial("Default");
            }
            
            return _DummyModel.LogicalMaterials[0];
        }           

        #endregion
    }

    
}
