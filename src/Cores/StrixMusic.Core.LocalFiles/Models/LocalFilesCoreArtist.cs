﻿using Microsoft.Toolkit.Diagnostics;
using OwlCore.Collections;
using OwlCore.Events;
using StrixMusic.Core.LocalFiles.Backing.Models;
using StrixMusic.Core.LocalFiles.Backing.Services;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StrixMusic.Core.LocalFiles.Models
{
    /// <inheritdoc/>
    public class LocalFilesCoreArtist : ICoreArtist
    {
        ///NOTE: There are some methods set to NotSupported temporarily although they are supported, so the playback can be implemented.
        private ArtistMetadata _artistMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalFilesCoreArtist"/> class.
        /// </summary>
        /// <param name="sourceCore">The source core.</param>
        public LocalFilesCoreArtist(ICore sourceCore, ArtistMetadata artistMetadata, int totalTracksCount)
        {
            SourceCore = sourceCore;
            _artistMetadata = artistMetadata;
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
        public event EventHandler<TimeSpan>? DurationChanged;

        /// <inheritdoc />
        public event EventHandler<DateTime?>? LastPlayedChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? AlbumItemsCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? TrackItemsCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreImage>? ImagesChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreAlbumCollectionItem>? AlbumItemsChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreTrack>? TrackItemsChanged;

        /// <inheritdoc/>
        public string Id => _artistMetadata.Id;

        /// <inheritdoc/>
        public int TotalAlbumItemsCount => 0;

        /// <inheritdoc />
        public int TotalImageCount { get; } = 0;

        /// <inheritdoc/>
        public int TotalTracksCount { get; }

        /// <inheritdoc/>
        public ICore SourceCore { get; }

        /// <inheritdoc/>
        public Uri? Url => _artistMetadata.Url ?? new Uri("http://google.com");

        /// <inheritdoc/>
        public string? Name => _artistMetadata.Name;

        //This is incorrect value.
        /// <inheritdoc/>
        public string Description => _artistMetadata.Name ?? "No Description";

        /// <inheritdoc/>
        public PlaybackState PlaybackState => PlaybackState.None;

        /// <inheritdoc/>
        public TimeSpan Duration => new TimeSpan(0, 0, 0);

        /// <inheritdoc />
        public DateTime? LastPlayed { get; }

        /// <inheritdoc />
        public DateTime? AddedAt { get; }

        /// <inheritdoc/>
        public ICorePlayableCollectionGroup? RelatedItems => null;

        /// <inheritdoc/>
        public SynchronizedObservableCollection<string>? Genres { get; }

        /// <inheritdoc/>
        public bool IsPlayAsyncSupported => true;

        /// <inheritdoc/>
        public bool IsPauseAsyncSupported => true;

        /// <inheritdoc/>
        public bool IsChangeNameAsyncSupported => false;

        /// <inheritdoc/>
        public bool IsChangeDescriptionAsyncSupported => false;

        /// <inheritdoc/>
        public bool IsChangeDurationAsyncSupported => false;

        /// <inheritdoc/>
        public Task<bool> IsAddImageSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddTrackSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddAlbumItemSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddGenreSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveImageSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveTrackSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveAlbumItemSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveGenreSupported(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task ChangeDescriptionAsync(string? description)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc/>
        public Task ChangeDurationAsync(TimeSpan duration)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc/>
        public Task ChangeNameAsync(string name)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc/>
        public Task PauseAsync()
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc/>
        public Task PlayAsync()
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreAlbumCollectionItem> GetAlbumItemsAsync(int limit, int offset)
        {
            var albumsService = SourceCore.GetService<AlbumService>();

            var albums = await albumsService.GetAlbumsByArtistId(Id, offset, limit);

            foreach (var album in albums)
            {
                // just to test
                var tracks = await SourceCore.GetService<TrackService>().GetTracksByAlbumId(album.Id, 0, 1000);
                yield return new LocalFilesCoreAlbum(SourceCore, album, tracks.Count);
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreTrack> GetTracksAsync(int limit, int offset)
        {
            var trackService = SourceCore.GetService<TrackService>();

            var albums = await trackService.GetTracksByArtistId(Id, offset, limit);

            foreach (var album in albums)
            {
                yield return new LocalFilesCoreTrack(SourceCore, album);
            }
        }

        /// <inheritdoc />
        public Task AddTrackAsync(ICoreTrack track, int index)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc />
        public Task AddAlbumItemAsync(ICoreAlbumCollectionItem album, int index)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc />
        public Task RemoveTrackAsync(int index)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc />
        public Task RemoveAlbumItemAsync(int index)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc />
        public Task AddImageAsync(ICoreImage image, int index)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index)
        {
            throw new NotSupportedException();//temporary for playback
        }

        /// <inheritdoc />
        public IAsyncEnumerable<ICoreImage> GetImagesAsync(int limit, int offset)
        {
            throw new NotSupportedException(); //temporary for playback
        }
    }
}
