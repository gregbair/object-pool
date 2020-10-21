// Copyright (c) Greg Bair. All rights reserved.
// Licensed under MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ObjectPool
{
    /// <summary>
    /// A proxy to wrap objects of type <typeparamref name="TObject"/>.
    /// </summary>
    /// <typeparam name="TObject">The type of object to wrap.</typeparam>
    public class PooledObjectWrapper<TObject>
        where TObject : class, IDisposable
    {
        /// <summary>
        /// Gets the ID of this proxy.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the actual object that's wrapped in this proxy.
        /// </summary>
        public TObject Actual { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledObjectWrapper{TObject}"/> class.
        /// </summary>
        /// <param name="actual">The object to be wrapped.</param>
        public PooledObjectWrapper(TObject actual)
        {
            Actual = actual ?? throw new ArgumentNullException(nameof(actual));
            Id = Guid.NewGuid();
        }
    }
}