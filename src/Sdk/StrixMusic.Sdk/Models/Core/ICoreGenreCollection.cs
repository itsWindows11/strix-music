﻿using OwlCore.Events;
using StrixMusic.Sdk.Models.Base;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StrixMusic.Sdk.Models.Core
{
    /// <inheritdoc cref="IGenreCollectionBase" />
    /// <remarks>This interface should be implemented by a core.</remarks>
    public interface ICoreGenreCollection : ICoreCollection, IGenreCollectionBase, ICoreMember
    {
        /// <summary>
        /// Gets a requested number of <see cref="ICoreGenre"/>s starting at the given offset in the backend.
        /// </summary>
        /// <param name="limit">The max number of items to return.</param>
        /// <param name="offset">Get items starting at this index.</param>
        /// <returns><see cref="IAsyncEnumerable{T}"/> containing the requested items.</returns>
        IAsyncEnumerable<ICoreGenre> GetGenresAsync(int limit, int offset);

        /// <summary>
        /// Adds a new genre to the collection.
        /// </summary>
        /// <param name="genre">The genre to insert.</param>
        /// <param name="index">the position to insert the genre at.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task AddGenreAsync(ICoreGenre genre, int index);

        /// <summary>
        /// Fires when the genres are changed.
        /// </summary>
        event CollectionChangedEventHandler<ICoreGenre>? GenresChanged;
    }
}