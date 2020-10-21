// Copyright (c) Greg Bair. All rights reserved.
// Licensed under MIT license. See LICENSE file in the project root for full license information.

using Castle.DynamicProxy;
using System;

namespace ObjectPool
{
    /// <summary>
    /// Intercepts calls for <typeparamref name="TProxy"/>.
    /// </summary>
    /// <typeparam name="TProxy">The type to intercept calls for.</typeparam>
    internal class PooledObjectInterceptor<TProxy> : IInterceptor
        where TProxy : class, IDisposable
    {
        private readonly IObjectPool<TProxy> _pool;
        private readonly PooledObjectProxy<TProxy> _proxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledObjectInterceptor{TProxy}"/> class.
        /// </summary>
        /// <param name="pool">The pool to which this interceptor should return objects to.</param>
        /// <param name="proxy">The proxy object wrapper.</param>
        public PooledObjectInterceptor(IObjectPool<TProxy> pool, PooledObjectProxy<TProxy> proxy)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
        }

        /// <inheritdoc/>
        public void Intercept(IInvocation invocation)
        {
            if (invocation is null)
            {
                throw new ArgumentNullException(nameof(invocation));
            }

            if (!invocation.Method.Name.Equals("Dispose", StringComparison.OrdinalIgnoreCase))
            {
                invocation.Proceed();
            }
            else
            {
                _pool.ReturnObject(_proxy);
            }
        }
    }
}