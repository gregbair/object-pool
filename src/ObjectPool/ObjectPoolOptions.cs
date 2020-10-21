// Copyright (c) Greg Bair. All rights reserved.
// Licensed under MIT license. See LICENSE file in the project root for full license information.

namespace ObjectPool
{
    /// <summary>
    /// Options for an <see cref="DefaultObjectPool{TObject}"/>.
    /// </summary>
    public class ObjectPoolOptions
    {
        /// <summary>
        /// Gets or sets the min number of objects allowed in the pool.
        /// </summary>
        public int MinObjects { get; set; }

        /// <summary>
        /// Gets or sets the max number of objects allowed in the pool.
        /// </summary>
        public int MaxObjects { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to activate the object after creation.
        /// Defaults to true.
        /// </summary>
        public bool ActivateOnCreate { get; set; } = true;
    }
}