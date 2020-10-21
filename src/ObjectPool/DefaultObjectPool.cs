// Copyright (c) Greg Bair. All rights reserved.
// Licensed under MIT license. See LICENSE file in the project root for full license information.

using Castle.DynamicProxy;
using Optional;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectPool
{
    /// <summary>
    /// Standard implementation of <see cref="IObjectPool{TObject}"/>.
    /// </summary>
    /// <typeparam name="TObject">The type of object to pool.</typeparam>
    public sealed class DefaultObjectPool<TObject> : IObjectPool<TObject>
        where TObject : class, IDisposable
    {
        private readonly IObjectPoolFactory<TObject> _factory;

        private readonly ObjectPoolOptions _options;

        private readonly ProxyGenerator _generator;

        private readonly ConcurrentStack<PooledObjectProxy<TObject>> _available =
            new ConcurrentStack<PooledObjectProxy<TObject>>();

        private readonly ConcurrentDictionary<Guid, PooledObjectProxy<TObject>> _active =
            new ConcurrentDictionary<Guid, PooledObjectProxy<TObject>>();

        private bool _isDisposed;

        /// <inheritdoc />
        public Func<TObject, bool> ObjectActivator { get; }

        /// <inheritdoc/>
        public Func<TObject, bool> ObjectPassivator { get; }

        /// <inheritdoc />
        public int AvailableCount => _available.Count;

        /// <inheritdoc/>
        public int ActiveCount => _active.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultObjectPool{TObject}"/> class.
        /// </summary>
        /// <param name="factory">The factory to produce objects of type <typeparamref name="TObject"/>.</param>
        /// <param name="options">Configuration options for the pool.</param>
        /// <param name="objectActivator">The activator for the object.</param>
        /// <param name="objectPassivator">The passivator for the object.</param>
        public DefaultObjectPool(IObjectPoolFactory<TObject> factory, ObjectPoolOptions? options = null, Func<TObject, bool>? objectActivator = null, Func<TObject, bool>? objectPassivator = null)
        {
            ObjectActivator = objectActivator ?? (_ => true);
            ObjectPassivator = objectPassivator ?? (_ => true);
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _options = options ?? new ObjectPoolOptions();
            _generator = new ProxyGenerator();
        }

        /// <param name="token"></param>
        /// <inheritdoc/>
        public async Task<Option<TObject>> GetObjectAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
            {
                return Option.None<TObject>();
            }

            if (_available.TryPop(out var existing))
            {
                _active.TryAdd(existing.Id, existing);
                return existing.Actual.SomeNotNull();
            }

            var obj = _factory.Create();

            if (ObjectActivator(obj))
            {
                await _factory.ActivateAsync(obj).ConfigureAwait(false);
            }

            var newProxy = new PooledObjectProxy<TObject>(obj);
            _active.TryAdd(newProxy.Id, newProxy);
            var genProxy =
                _generator.CreateInterfaceProxyWithTarget(obj, new PooledObjectInterceptor<TObject>(this, newProxy));

            return genProxy.SomeNotNull();
        }

        /// <inheritdoc/>
        public void ReturnObject(PooledObjectProxy<TObject> proxyObj)
        {
            if (proxyObj is null)
            {
                throw new ArgumentNullException(nameof(proxyObj));
            }

            if (_active.TryGetValue(proxyObj.Id, out var proxy) && _active.TryRemove(proxy.Id, out _))
            {
                _available.Push(proxyObj);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all managed references.
        /// </summary>
        /// <param name="disposing">Whether we're actually disposing.</param>
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (!_active.IsEmpty)
                {
                    throw new InvalidOperationException($"There are still {_active.Count} active object(s)");
                }

                foreach (var proxy in _available)
                {
                    if (ObjectPassivator(proxy.Actual))
                    {
                        proxy.Actual.Dispose();
                    }
                }
            }

            _isDisposed = true;
        }
    }
}