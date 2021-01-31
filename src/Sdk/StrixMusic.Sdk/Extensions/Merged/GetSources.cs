﻿using System.Collections.Generic;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Data.Merged;

namespace StrixMusic.Sdk.Extensions
{
    internal static partial class Merged
    {
        /// <summary>
        /// Syntax sugar for getting and casting the sources of an <see cref="IMerged{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The explicit source type to get sources from.</typeparam>
        /// <param name="merged">The <see cref="IMerged{T}"/> to operate on.</param>
        /// <returns>The sources of the given <see cref="IMerged{T}"/></returns>
        internal static IReadOnlyList<TSource> GetSources<TSource>(this IMerged<TSource> merged)
            where TSource : ICoreMember
        {
            return merged.Sources;
        }
    }
}