﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace SharpGLTF.Runtime
{
    /// <summary>
    /// Replaces <see cref="ModelMeshPart"/>.
    /// </summary>    
    sealed class RuntimeModelMeshPart
    {
        #region lifecycle

        internal RuntimeModelMeshPart(RuntimeModelMesh parent)
        {
            _Parent = parent;
        }

        #endregion

        #region data

        private readonly RuntimeModelMesh _Parent;

        private Effect _Effect;
        private BlendState _Blend = BlendState.Opaque;
        private RasterizerState _Rasterizer = RasterizerState.CullCounterClockwise;

        private IndexBuffer _SharedIndexBuffer;
        private int _IndexOffset;
        private int _PrimitiveCount;        

        private VertexBuffer _SharedVertexBuffer;
        private int _VertexOffset;
        private int _VertexCount;

        private Microsoft.Xna.Framework.BoundingSphere _Bounds;

        public object Tag { get; set; }

        #endregion

        #region properties

        public GraphicsDevice Device => _Parent._GraphicsDevice;

        public Effect Effect
        {
            get => _Effect;
            set
            {
                if (_Effect == value) return;
                _Effect = value;
                _Parent.InvalidateEffectCollection(); // if we change this property, we need to invalidate the parent's effect collection.
            }
        }

        public BlendState Blending
        {
            get => _Blend;
            set => _Blend = value;
        }

        public RasterizerState Rasterizer
        {
            get => _Rasterizer;
            set => _Rasterizer = value;
        }        

        public Microsoft.Xna.Framework.BoundingSphere BoundingSphere
        {
            get => _Bounds;
            internal set => _Bounds = value;
        }

        #endregion

        #region API

        public void SetVertexBuffer(VertexBuffer vb, int offset, int count)
        {
            this._SharedVertexBuffer = vb;
            this._VertexOffset = offset;
            this._VertexCount = count;            
        }

        public void SetIndexBuffer(IndexBuffer ib, int offset, int count)
        {
            this._SharedIndexBuffer = ib;
            this._IndexOffset = offset;
            this._PrimitiveCount = count;            
        }

        public void Draw(GraphicsDevice device)
        {
            if (_PrimitiveCount > 0)
            {
                device.SetVertexBuffer(_SharedVertexBuffer);
                device.Indices = _SharedIndexBuffer;

                device.BlendState = _Blend;
                device.RasterizerState = _Rasterizer;

                foreach(var pass in _Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, _VertexOffset, _IndexOffset, _PrimitiveCount);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Replaces <see cref="ModelMesh"/>
    /// </summary>
    sealed class RuntimeModelMesh
    {
        #region lifecycle

        public RuntimeModelMesh(GraphicsDevice graphicsDevice)
        {            
            this._GraphicsDevice = graphicsDevice;
        }

        #endregion

        #region data

        internal GraphicsDevice _GraphicsDevice;

        private readonly List<RuntimeModelMeshPart> _Primitives = new List<RuntimeModelMeshPart>();
        private IReadOnlyList<Effect> _Effects;


        private IReadOnlyList<RuntimeModelMeshPart> _OpaquePrimitives;
        private IReadOnlyList<Effect> _OpaqueEffects;

        private IReadOnlyList<RuntimeModelMeshPart> _TranslucidPrimitives;
        private IReadOnlyList<Effect> _TranslucidEffects;

        private Microsoft.Xna.Framework.BoundingSphere? _Sphere;

        #endregion

        #region  properties
        public string Name { get; set; }
        public object Tag { get; set; }

        public IReadOnlyCollection<Effect> OpaqueEffects
        {
            get
            {
                if (_OpaqueEffects != null) return _OpaqueEffects;

                // Create the shared effects collection on demand.

                _OpaqueEffects = GetOpaqueParts()
                    .Select(item => item.Effect)
                    .Distinct()
                    .ToArray();

                return _OpaqueEffects;
            }
        }

        public IReadOnlyCollection<Effect> TranslucidEffects
        {
            get
            {
                if (_TranslucidEffects != null) return _TranslucidEffects;

                // Create the shared effects collection on demand.

                _TranslucidEffects = GetTranslucidParts()
                    .Select(item => item.Effect)
                    .Distinct()
                    .ToArray();

                return _TranslucidEffects;
            }
        }        

        public Microsoft.Xna.Framework.BoundingSphere BoundingSphere
        {
            set => _Sphere = value;

            get
            {
                if (_Sphere.HasValue) return _Sphere.Value;

                foreach(var part in _Primitives)
                {
                    if (_Sphere.HasValue) _Sphere = Microsoft.Xna.Framework.BoundingSphere.CreateMerged(_Sphere.Value, part.BoundingSphere);
                    else _Sphere = part.BoundingSphere;
                }

                return _Sphere.Value;
            }            
        }        

        #endregion

        #region API

        public RuntimeModelMeshPart CreateMeshPart()
        {
            var primitive = new RuntimeModelMeshPart(this);

            _Primitives.Add(primitive);

            _OpaquePrimitives = null;
            _TranslucidPrimitives = null;

            InvalidateEffectCollection();

            _Sphere = null;

            return primitive;
        }

        internal void InvalidateEffectCollection()
        {            
            _OpaqueEffects = null;            
            _TranslucidEffects = null;
        }

        private IReadOnlyList<RuntimeModelMeshPart> GetOpaqueParts()
        {
            if (_OpaquePrimitives != null) return _OpaquePrimitives;
            _OpaquePrimitives = _Primitives.Where(item => item.Blending == BlendState.Opaque).ToArray();
            return _OpaquePrimitives;
        }

        private IReadOnlyList<RuntimeModelMeshPart> GetTranslucidParts()
        {
            if (_TranslucidPrimitives != null) return _TranslucidPrimitives;
            _TranslucidPrimitives = _Primitives.Where(item => item.Blending != BlendState.Opaque).ToArray();
            return _TranslucidPrimitives;
        }

        public void DrawOpaque()
        {            
            _GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            

            foreach (var part in GetOpaqueParts()) part.Draw(_GraphicsDevice);
        }

        public void DrawTranslucid()
        {
            _GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            foreach (var part in GetTranslucidParts()) part.Draw(_GraphicsDevice);
        }        

        #endregion
    }    
}
