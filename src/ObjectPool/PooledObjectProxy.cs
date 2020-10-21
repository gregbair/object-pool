// Copyright (c) Greg Bair. All rights reserved.
// Licensed under MIT license. See LICENSE file in the project root for full license information.

using Castle.DynamicProxy;
using System;

namespace ObjectPool
{
    /// <summary>
    /// A proxy that wraps an instance of <typeparamref name="TObject"/>.
    /// </summary>
    /// <typeparam name="TObject">The type of object to wrap.</typeparam>
    internal class PooledObjectProxy<TObject>
        where TObject : class, IDisposable
    {
        /// <summary>
        /// Gets the actual object.
        /// </summary>
        /// <remarks>
        /// This is actually proxied by an <see cref="IProxyGenerator"/>.
        /// </remarks>
        internal TObject Actual { get; }

        private readonly IProxyGenerator _generator = new ProxyGenerator();

        private readonly PooledObjectInterceptor<TObject> _interceptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledObjectProxy{TObject}"/> class.
        /// </summary>
        /// <param name="interceptor">The interceptor to use for this object.</param>
        /// <param name="obj">The actual object to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="interceptor"/> or <paramref name="obj"/> are null.</exception>
        public PooledObjectProxy(PooledObjectInterceptor<TObject> interceptor, TObject obj)
        {
            _interceptor = interceptor ?? throw new ArgumentNullException(nameof(interceptor));

            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            Actual = _generator.CreateInterfaceProxyWithTarget(obj, interceptor);
        }
    }
}