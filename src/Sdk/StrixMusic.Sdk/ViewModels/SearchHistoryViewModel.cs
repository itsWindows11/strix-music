﻿using System.Collections.Generic;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Data.Merged;
using StrixMusic.Sdk.Extensions;

namespace StrixMusic.Sdk.ViewModels
{
    /// <summary>
    /// Used to bind search history across multiple cores to the View model.
    /// </summary>
    public class SearchHistoryViewModel : PlayableCollectionGroupViewModel, ISearchHistory
    {
        private readonly ISearchHistory _searchHistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecentlyPlayedViewModel"/> class.
        /// </summary>
        /// <param name="searchHistory">The <see cref="IRecentlyPlayed"/> to wrap.</param>
        public SearchHistoryViewModel(ISearchHistory searchHistory)
            : base(searchHistory)
        {
            _searchHistory = searchHistory;
        }

        /// <inheritdoc />
        IReadOnlyList<ICoreSearchHistory> IMerged<ICoreSearchHistory>.Sources => this.GetSources<ICoreSearchHistory>();

        /// <inheritdoc />
        public bool Equals(ICoreSearchHistory other) => _searchHistory.Equals(other);
    }
}