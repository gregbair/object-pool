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
    public class PooledObjectInterceptor<TProxy> : IInterceptor
        where TProxy : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PooledObjectInterceptor{TProxy}"/> class.
        /// </summary>
        /// <param name="pool">The pool to which this interceptor should return objects to.</param>
        /// <param name="actual">The actual object that is proxied.</param>
        public PooledObjectInterceptor(IObjectPool<TProxy> pool, TProxy actual)
        {
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _actual = actual ?? throw new ArgumentNullException(nameof(actual));
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
                _pool.ReturnObject(_actual);
            }
        }

        private readonly IObjectPool<TProxy> _pool;
        private readonly TProxy _actual;
    }
}