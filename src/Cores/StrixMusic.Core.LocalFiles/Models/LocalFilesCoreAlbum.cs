﻿using OwlCore.Collections;
using OwlCore.Events;
using StrixMusic.Core.LocalFiles.Backing.Models;
using StrixMusic.Core.LocalFiles.Backing.Services;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StrixMusic.Core.LocalFiles.Models
{
    ///NOTE: There are some methods set to throw empty stuff temporarily although they are supported, so the playback can be implemented.
    /// <summary>
    /// A LocalFileCore implementation of <see cref="ICoreAlbum"/>.
    /// </summary>
    public class LocalFilesCoreAlbum : ICoreAlbum
    {
        private readonly AlbumMetadata _albumMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFilesCoreAlbum"/> class.
        /// </summary>
        /// <param name="sourceCore">The core that created this object.</param>
        public LocalFilesCoreAlbum(ICore sourceCore, AlbumMetadata albumMetadata, int totalTracksCount)
        {
            SourceCore = sourceCore;
            _albumMetadata = albumMetadata;
            TotalArtistItemsCount = _albumMetadata.TotalTracksCount ?? 0;
            TotalTracksCount = totalTracksCount;
        }

        /// <inheritdoc/>
        public event EventHandler<PlaybackState>? PlaybackStateChanged;

        /// <inheritdoc/>
        public event EventHandler<string>? NameChanged;

        /// <inheritdoc/>
        public event EventHandler<string?>? DescriptionChanged;

        /// <inheritdoc/>
        public event EventHandler<Uri?>? UrlChanged;

        /// <inheritdoc/>
        public event EventHandler<DateTime?>? DatePublishedChanged;

        /// <inheritdoc/>
        public event EventHandler<TimeSpan>? DurationChanged;

        /// <inheritdoc />
        public event EventHandler<DateTime?>? LastPlayedChanged;

        /// <inheritdoc />
        public event EventHandler<int>? TrackItemsCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreTrack>? TrackItemsChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreImage>? ImagesChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreArtistCollectionItem>? ArtistItemsChanged;

        /// <inheritdoc/>
        public int TotalTracksCount { get; }

        /// <inheritdoc/>
        public ICore SourceCore { get; }

        /// <inheritdoc/>
        public string Id => _albumMetadata.Id;

        /// <inheritdoc/>
        public Uri? Url => new Uri("http://google.com");

        /// <inheritdoc/>
        public string Name => _albumMetadata.Title ?? "No Title";

        /// <inheritdoc/>
        public DateTime? DatePublished => _albumMetadata.DatePublished;

        /// <inheritdoc/>
        public string? Description => _albumMetadata.Description;

        /// <inheritdoc/>
        public PlaybackState PlaybackState { get; private set; }

        /// <inheritdoc/>
        public TimeSpan Duration => _albumMetadata.Duration ?? new TimeSpan(0, 0, 0);

        /// <inheritdoc />
        public DateTime? LastPlayed { get; }

        /// <inheritdoc />
        public DateTime? AddedAt { get; }

        /// <inheritdoc />
        public int TotalImageCount { get; } = 3;

        /// <inheritdoc />
        public int TotalArtistItemsCount { get; }

        /// <inheritdoc/>
        public ICorePlayableCollectionGroup? RelatedItems { get; }

        /// <inheritdoc/>
        public SynchronizedObservableCollection<string>? Genres { get; } = new SynchronizedObservableCollection<string>();

        /// <inheritdoc/>
        public bool IsPlayAsyncSupported => true;

        /// <inheritdoc/>
        public bool IsPauseAsyncSupported => true;

        /// <inheritdoc/>
        public bool IsChangeNameAsyncSupported => false;

        /// <inheritdoc/>
        public bool IsChangeDatePublishedAsyncSupported => false;

        /// <inheritdoc/>
        public bool IsChangeDescriptionAsyncSupported => false;

        /// <inheritdoc/>
        public bool IsChangeDurationAsyncSupported => false;

        /// <inheritdoc/>
        public Task<bool> IsAddGenreSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddTrackSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddImageSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveTrackSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveImageSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveGenreSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task ChangeDescriptionAsync(string? description)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc/>
        public Task ChangeDurationAsync(TimeSpan duration)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc/>
        public Task ChangeDatePublishedAsync(DateTime datePublished)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc/>
        public Task ChangeNameAsync(string name)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc/>
        public Task PauseAsync()
        {
            PlaybackState = PlaybackState.Paused;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task PlayAsync()
        {
            PlaybackState = PlaybackState.Playing;

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreTrack> GetTracksAsync(int limit, int offset)
        {
            var tracksList = SourceCore.GetService<TrackService>();

            var tracks = await tracksList.GetTracksByAlbumId(Id, offset, limit);

            foreach (var track in tracks)
            {
                yield return new LocalFilesCoreTrack(SourceCore, track);
            }
        }

        /// <inheritdoc />
        public Task AddTrackAsync(ICoreTrack track, int index)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc />
        public Task RemoveTrackAsync(int index)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreImage> GetImagesAsync(int limit, int offset)
        {
            await Task.FromResult(false);//temporary for playback
            yield break;
        }

        /// <inheritdoc />
        public Task AddImageAsync(ICoreImage image, int index)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc />
        public Task RemoveArtistItemAsync(int index)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc />
        public Task<bool> IsAddArtistItemSupported(int index)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveArtistItemSupported(int index)
        {
            return Task.FromResult(false);//temporary for playback
        }

        /// <inheritdoc />
        public event EventHandler<int>? ArtistItemsCountChanged;

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreArtistCollectionItem> GetArtistItemsAsync(int limit, int offset)
        {
            var artistService = SourceCore.GetService<ArtistService>();

            var artists = await artistService.GetArtistsByAlbumId(Id, offset, limit);

            foreach (var artist in artists)
            {
                // just to test
                var tracks = await SourceCore.GetService<TrackService>().GetTracksByAlbumId(artist.Id, 0, 1000);
                yield return new LocalFilesCoreArtist(SourceCore, artist, tracks.Count);
            }
        }

        /// <inheritdoc />
        public Task AddArtistItemAsync(ICoreArtistCollectionItem artist, int index)
        {
            return Task.FromResult(false);
        }
    }
}
