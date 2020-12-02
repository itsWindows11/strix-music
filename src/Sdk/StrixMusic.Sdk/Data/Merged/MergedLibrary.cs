﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Diagnostics;
using StrixMusic.Sdk.Data.Base;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;

namespace StrixMusic.Sdk.Data.Merged
{
    /// <summary>
    /// A concrete class that merges multiple <see cref="ILibraryBase"/>.
    /// </summary>
    public class MergedLibrary : MergedPlayableCollectionGroupBase<ICoreLibrary>, ILibrary, IMerged<ICoreLibrary>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedLibrary"/> class.
        /// </summary>
        /// <param name="sources">The <see cref="ICoreLibrary"/> objects to merge.</param>
        public MergedLibrary(IEnumerable<ICoreLibrary> sources)
            : base(sources.ToArray())
        {
        }

        /// <inheritdoc />
        IReadOnlyList<ICoreLibrary> ISdkMember<ICoreLibrary>.Sources => StoredSources;

        /// <inheritdoc cref="Equals(object?)" />
        public bool Equals(ICoreLibrary? other)
        {
            return other?.Name == Name;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is ICoreLibrary other && Equals(other));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return PreferredSource.GetHashCode();
        }
    }
}