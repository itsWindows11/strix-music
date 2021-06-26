﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.Collections;
using OwlCore.Events;
using StrixMusic.Core.External.Services;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;
using StrixMusic.Sdk.Services.FileMetadataManager;
using StrixMusic.Sdk.Services.FileMetadataManager.Models;

namespace StrixMusic.Core.External.Models
{
    /// <summary>
    /// Wraps around <see cref="ArtistMetadata"/> to provide artist information extracted from a file to the Strix SDK.
    /// </summary>
    public class ExternalCoreArtist : ICoreArtist
    {
        private readonly IFileMetadataManager _fileMetadataManager;
        private ArtistMetadata _artistMetadata;
        private ExternalCoreImage? _image;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalCoreArtist"/> class.
        /// </summary>
        /// <param name="sourceCore">The source core.</param>
        /// <param name="artistMetadata">The artist metadata to wrap around.</param>
        public ExternalCoreArtist(ICore sourceCore, ArtistMetadata artistMetadata)
        {
            SourceCore = sourceCore;
            _artistMetadata = artistMetadata;
            _fileMetadataManager = SourceCore.GetService<IFileMetadataManager>();

            if (artistMetadata.ImagePath != null)
                _image = InstanceCache.Images.GetOrCreate(Id, SourceCore, new Uri(artistMetadata.ImagePath));

            Genres = new SynchronizedObservableCollection<string>(artistMetadata.Genres);

            AttachEvents();
        }

        private void AttachEvents()
        {
            _fileMetadataManager.Artists.MetadataUpdated += Artists_MetadataUpdated;
            _fileMetadataManager.Artists.TracksChanged += Artists_TracksChanged;
            _fileMetadataManager.Artists.AlbumItemsChanged += Artists_AlbumItemsChanged;
        }

        private void DetachEvents()
        {
            _fileMetadataManager.Artists.MetadataUpdated -= Artists_MetadataUpdated;
            _fileMetadataManager.Artists.TracksChanged -= Artists_TracksChanged;
            _fileMetadataManager.Artists.AlbumItemsChanged -= Artists_AlbumItemsChanged;
        }

        private void Artists_TracksChanged(object sender, IReadOnlyList<CollectionChangedItem<(ArtistMetadata Artist, TrackMetadata Track)>> addedItems, IReadOnlyList<CollectionChangedItem<(ArtistMetadata Artist, TrackMetadata Track)>> removedItems)
        {
            var coreAddedItems = new List<CollectionChangedItem<ICoreTrack>>();
            var coreRemovedItems = new List<CollectionChangedItem<ICoreTrack>>();

            foreach (var item in addedItems)
            {
                if (item.Data.Artist.Id == Id)
                {
                    Guard.IsNotNullOrWhiteSpace(item.Data.Track.Id, nameof(item.Data.Track.Id));
                    var coreTrack = InstanceCache.Tracks.GetOrCreate(item.Data.Track.Id, SourceCore, item.Data.Track);
                    coreAddedItems.Add(new CollectionChangedItem<ICoreTrack>(coreTrack, item.Index));
                }
            }

            foreach (var item in removedItems)
            {
                if (item.Data.Artist.Id == Id)
                {
                    Guard.IsNotNullOrWhiteSpace(item.Data.Track.Id, nameof(item.Data.Track.Id));
                    var coreTrack = InstanceCache.Tracks.GetOrCreate(item.Data.Track.Id, SourceCore, item.Data.Track);
                    coreRemovedItems.Add(new CollectionChangedItem<ICoreTrack>(coreTrack, item.Index));
                }
            }

            if (coreAddedItems.Count > 0 || coreRemovedItems.Count > 0)
                TrackItemsChanged?.Invoke(this, coreAddedItems, coreRemovedItems);
        }

        private void Artists_AlbumItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<(ArtistMetadata Artist, AlbumMetadata Album)>> addedItems, IReadOnlyList<CollectionChangedItem<(ArtistMetadata Artist, AlbumMetadata Album)>> removedItems)
        {
            var coreAddedItems = new List<CollectionChangedItem<ICoreAlbumCollectionItem>>();
            var coreRemovedItems = new List<CollectionChangedItem<ICoreAlbumCollectionItem>>();

            foreach (var item in addedItems)
            {
                if (item.Data.Artist.Id == Id)
                {
                    Guard.IsNotNullOrWhiteSpace(item.Data.Album.Id, nameof(item.Data.Album.Id));
                    var coreAlbum = InstanceCache.Albums.GetOrCreate(item.Data.Album.Id, SourceCore, item.Data.Album);
                    coreAddedItems.Add(new CollectionChangedItem<ICoreAlbumCollectionItem>(coreAlbum, item.Index));
                }
            }

            foreach (var item in removedItems)
            {
                if (item.Data.Artist.Id == Id)
                {
                    Guard.IsNotNullOrWhiteSpace(item.Data.Album.Id, nameof(item.Data.Album.Id));
                    var coreAlbum = InstanceCache.Albums.GetOrCreate(item.Data.Album.Id, SourceCore, item.Data.Album);
                    coreRemovedItems.Add(new CollectionChangedItem<ICoreAlbumCollectionItem>(coreAlbum, item.Index));
                }
            }

            if (coreAddedItems.Count > 0 || coreRemovedItems.Count > 0)
                AlbumItemsChanged?.Invoke(this, coreAddedItems, coreRemovedItems);
        }

        private void Artists_MetadataUpdated(object sender, IEnumerable<ArtistMetadata> e)
        {
            foreach (var metadata in e)
            {
                if (metadata.Id != Id)
                    return;

                Guard.IsNotNull(metadata.AlbumIds, nameof(metadata.AlbumIds));
                Guard.IsNotNull(metadata.TrackIds, nameof(metadata.TrackIds));

                var previousData = _artistMetadata;
                _artistMetadata = metadata;

                if (metadata.Name != previousData.Name)
                    NameChanged?.Invoke(this, Name);

                if (metadata.ImagePath != previousData.ImagePath)
                    HandleImageChanged(metadata);

                if (metadata.Description != previousData.Description)
                    DescriptionChanged?.Invoke(this, Description);

                // TODO genres, post genres do-over

                if (metadata.TrackIds.Count != (previousData.TrackIds?.Count ?? 0))
                    TrackItemsCountChanged?.Invoke(this, metadata.TrackIds.Count);

                if (metadata.AlbumIds.Count != (previousData.AlbumIds?.Count ?? 0))
                    AlbumItemsCountChanged?.Invoke(this, metadata.AlbumIds.Count);
            }
        }

        private void HandleImageChanged(ArtistMetadata e)
        {
            var previousImage = _image;

            var removed = new List<CollectionChangedItem<ICoreImage>>();
            var added = new List<CollectionChangedItem<ICoreImage>>();

            if (previousImage != null)
                removed.Add(new CollectionChangedItem<ICoreImage>(previousImage, 0));

            // ReSharper disable once ReplaceWithStringIsNullOrEmpty (breaks nullability check)
            if (e.ImagePath != null && e.ImagePath.Length > 0)
            {
                var newImage = new ExternalCoreImage(SourceCore, new Uri(e.ImagePath));
                InstanceCache.Images.Replace(Id, newImage);
                added.Add(new CollectionChangedItem<ICoreImage>(newImage, 0));
                _image = newImage;
            }

            if (added.Count > 0 || removed.Count > 0)
            {
                ImagesChanged?.Invoke(this, added, removed);

                if (added.Count != removed.Count)
                    ImagesCountChanged?.Invoke(this, TotalImageCount);
            }
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
        public event EventHandler<bool>? IsPlayTrackCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsPauseTrackCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsPlayAlbumCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsPauseAlbumCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeNameAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDescriptionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDurationAsyncAvailableChanged;

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
        public string Id => _artistMetadata.Id ?? string.Empty;

        /// <inheritdoc />
        public int TotalImageCount => _image != null ? 1 : 0;

        /// <inheritdoc/>
        public int TotalTracksCount => _artistMetadata.TrackIds?.Count ?? 0;

        /// <inheritdoc/>
        public int TotalAlbumItemsCount => _artistMetadata.AlbumIds?.Count ?? 0;

        /// <inheritdoc/>
        public ICore SourceCore { get; }

        /// <inheritdoc/>
        public Uri? Url => null;

        /// <inheritdoc/>
        public string Name => _artistMetadata.Name ?? string.Empty;

        /// <inheritdoc/>
        public string Description => _artistMetadata.Description ?? string.Empty;

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
        public bool IsPlayTrackCollectionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsPauseTrackCollectionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsPlayAlbumCollectionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsPauseAlbumCollectionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeNameAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeDescriptionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeDurationAsyncAvailable => false;

        /// <inheritdoc/>
        public Task<bool> IsAddImageAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddTrackAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddAlbumItemAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddGenreAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveImageAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveTrackAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveAlbumItemAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveGenreAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task ChangeDescriptionAsync(string? description)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeDurationAsync(TimeSpan duration)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeNameAsync(string name)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task PauseTrackCollectionAsync()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task PlayTrackCollectionAsync()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task PauseAlbumCollectionAsync()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task PlayAlbumCollectionAsync()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task PlayTrackCollectionAsync(ICoreTrack track)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task PlayAlbumCollectionAsync(ICoreAlbumCollectionItem albumItem)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddTrackAsync(ICoreTrack track, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddAlbumItemAsync(ICoreAlbumCollectionItem album, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveTrackAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveAlbumItemAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddImageAsync(ICoreImage image, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreAlbumCollectionItem> GetAlbumItemsAsync(int limit, int offset)
        {
            var albumsService = _fileMetadataManager.Albums;

            var albums = await albumsService.GetAlbumsByArtistId(Id, offset, limit);

            foreach (var album in albums)
            {
                Guard.IsNotNull(album.Id, nameof(album.Id));

                yield return InstanceCache.Albums.GetOrCreate(album.Id, SourceCore, album);
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreTrack> GetTracksAsync(int limit, int offset)
        {
            var trackService = _fileMetadataManager.Tracks;
            var tracks = await trackService.GetTracksByArtistId(Id, offset, limit);

            foreach (var track in tracks)
            {
                Guard.IsNotNullOrWhiteSpace(track?.Id, nameof(track.Id));
                yield return InstanceCache.Tracks.GetOrCreate(track.Id, SourceCore, track);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreImage> GetImagesAsync(int limit, int offset)
        {
            if (_image != null)
                yield return _image;

            await Task.CompletedTask;
        }

        private void ReleaseUnmanagedResources()
        {
            DetachEvents();
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                Genres?.Dispose();
            }
        }

        /// <inheritdoc />
        ~ExternalCoreArtist()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            return default;
        }
    }
}
