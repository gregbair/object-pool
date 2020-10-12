// Copyright (c) Greg Bair. All rights reserved.
// Licensed under MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace ObjectPool
{
    /// <summary>
    /// Interface for implementing an object pool.
    /// </summary>
    /// <typeparam name="T">The type of object to pool.</typeparam>
    public interface IObjectPool<T> : IDisposable
        where T : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Gets or sets the connection activator.
        /// </summary>
        Func<T, bool> ConnectionActivator { get; set; }

        /// <summary>
        /// Gets or sets the connection passivator.
        /// </summary>
        Func<T, bool> ConnectionPassivator { get; set; }

        /// <summary>
        /// Gets the available count of objects in the pool.
        /// </summary>
        uint AvailableCount { get; }

        /// <summary>
        /// Gets the active count of objects in the pool.
        /// </summary>
        uint ActiveCount { get; }

        /// <summary>
        /// Initialize this object pool.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Retrieves the object from the pool.
        /// </summary>
        /// <returns>An object of type <typeparamref name="T" />.</returns>
        Task<T> GetObjectAsync();

        /// <summary>
        /// Returns the object to the pool.
        /// </summary>
        /// <param name="obj">An object of type <typeparamref name="T" />.</param>
        void ReturnObject(T obj);
    }
}