// Copyright (c) Greg Bair. All rights reserved.
// Licensed under MIT license. See LICENSE file in the project root for full license information.

using Optional;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObjectPool
{
    /// <summary>
    /// Interface for implementing an object pool.
    /// </summary>
    /// <typeparam name="TObject">The type of object to pool.</typeparam>
    public interface IObjectPool<TObject> : IDisposable
        where TObject : class, IDisposable
    {
        /// <summary>
        /// Gets the connection activator.
        /// </summary>
        Func<TObject, bool> ObjectActivator { get; }

        /// <summary>
        /// Gets the connection passivator.
        /// </summary>
        Func<TObject, bool> ObjectPassivator { get; }

        /// <summary>
        /// Gets the available count of objects in the pool.
        /// </summary>
        uint AvailableCount { get; }

        /// <summary>
        /// Gets the active count of objects in the pool.
        /// </summary>
        uint ActiveCount { get; }

        /// <summary>
        /// Retrieves the object from the pool.
        /// </summary>
        /// <param name="token">A <see cref="CancellationToken"/> used for timeouts.</param>
        /// <returns>An object of type <typeparamref name="TObject" />.</returns>
        Task<Option<TObject>> GetObjectAsync(CancellationToken token = default);

        /// <summary>
        /// Returns the object to the pool.
        /// </summary>
        /// <param name="obj">A wrapper to return to the pool.</param>
        void ReturnObject(PooledObjectProxy<TObject> obj);
    }
}