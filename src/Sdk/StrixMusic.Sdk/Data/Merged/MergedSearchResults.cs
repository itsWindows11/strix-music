﻿using System;
using System.Collections.Generic;
using System.Linq;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;

namespace StrixMusic.Sdk.Data.Merged
{
    /// <summary>
    /// A concrete class that merges multiple <see cref="ICoreSearchResults"/>.
    /// </summary>
    public class MergedSearchResults : MergedPlayableCollectionGroupBase<ICoreSearchResults>, ISearchResults, IMerged<ICoreSearchResults>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MergedSearchResults"/> class.
        /// </summary>
        /// <param name="searchResults">The search results to merge.</param>
        public MergedSearchResults(IEnumerable<ICoreSearchResults> searchResults)
            : base(searchResults.ToArray())
        {
        }

        /// <inheritdoc />
        IReadOnlyList<ICoreSearchResults> ISdkMember<ICoreSearchResults>.Sources => this.GetSources<ICoreSearchResults>();

        /// <inheritdoc cref="Equals(object?)" />
        public bool Equals(ICoreSearchResults? other)
        {
            // TODO: Merge together based on query (post search refactor)
            // return other?.Name == Name;
            return false;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (obj is ICoreSearchResults other && Equals(other));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return PreferredSource.GetHashCode();
        }
    }
}