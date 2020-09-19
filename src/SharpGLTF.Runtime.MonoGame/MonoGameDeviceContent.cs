﻿using System;

namespace SharpGLTF.Runtime
{
    /// <summary>
    /// A wrapper that contains an object with disposable resources.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoGameDeviceContent<T> : IDisposable
        where T:class
    {
        #region lifecycle

        internal MonoGameDeviceContent(T instance, IDisposable[] disposables)
        {
            _Content = instance;
            _Disposables = disposables;
        }

        public void Dispose()
        {
            _Content = null;
            if (_Disposables == null) return;

            foreach (var d in _Disposables) d.Dispose();
            _Disposables = null;
        }

        ~MonoGameDeviceContent()
        {
            System.Diagnostics.Debug.Assert(_Disposables == null, "Not disposed correctly");
        }

        #endregion

        #region data

        /// <summary>
        /// The actual object.
        /// </summary>
        private T _Content;

        /// <summary>
        /// The disposable resources associated with <see cref="_Content"/>.
        /// </summary>
        private IDisposable[] _Disposables;

        #endregion

        #region properties

        public static implicit operator T(MonoGameDeviceContent<T> value) { return value?.Content; }

        public T Content => _Content;

        #endregion       
    }
}
