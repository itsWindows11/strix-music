﻿using System;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.ViewModels.Bindables
{
    /// <summary>
    /// Used to bind search results across multiple cores to the View model.
    /// </summary>
    public class ObservableSearchResults : ObservableCollectionGroup
    {
        /// <summary>
        /// Bindable wrapper for <see cref="ISearchResults"/>.
        /// </summary>
        public ObservableSearchResults(ISearchResults searchResults)
            : base(searchResults)
        {
        }
    }
}