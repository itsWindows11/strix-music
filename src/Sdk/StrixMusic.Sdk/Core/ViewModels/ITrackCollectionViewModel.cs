﻿using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.Collections;
using StrixMusic.Sdk.Core.Data;

namespace StrixMusic.Sdk.Core.ViewModels
{
    /// <summary>
    /// An observable <see cref="ITrackCollection"/>.
    /// </summary>
    public interface ITrackCollectionViewModel : ITrackCollection
    {
        /// <summary>
        /// The tracks in this collection.
        /// </summary>
        public SynchronizedObservableCollection<TrackViewModel> Tracks { get; }

        /// <summary>
        /// Populates the next set of tracks into the collection.
        /// </summary>
        /// <param name="limit">The number of items to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task PopulateMoreTracksAsync(int limit);

        /// <inheritdoc cref="PopulateMoreTracksAsync" />
        public IAsyncRelayCommand<int> PopulateMoreTracksCommand { get; }
    }
}