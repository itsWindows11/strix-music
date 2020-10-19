﻿using StrixMusic.Sdk.Core.Data;

namespace StrixMusic.Sdk.Core.ViewModels
{
    /// <summary>
    /// Used to bind search results across multiple cores to the View model.
    /// </summary>
    public class SearchResultsViewModel : PlayableCollectionGroupViewModel
    {
        /// <summary>
        /// Bindable wrapper for <see cref="ISearchResults"/>.
        /// </summary>
        public SearchResultsViewModel(ISearchResults searchResults)
            : base(searchResults)
        {
        }
    }
}