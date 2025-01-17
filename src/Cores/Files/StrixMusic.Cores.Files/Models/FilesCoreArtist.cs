﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Cores.Files.Services;
using StrixMusic.Sdk.CoreModels;
using StrixMusic.Sdk.FileMetadata;
using StrixMusic.Sdk.FileMetadata.Models;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Cores.Files.Models
{
    /// <summary>
    /// Wraps around <see cref="ArtistMetadata"/> to provide artist information extracted from a file to the Strix SDK.
    /// </summary>
    public sealed class FilesCoreArtist : ICoreArtist
    {
        private readonly IFileMetadataManager? _fileMetadataManager;
        private ArtistMetadata _artistMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesCoreArtist"/> class.
        /// </summary>
        /// <param name="sourceCore">The source core.</param>
        /// <param name="artistMetadata">The artist metadata to wrap around.</param>
        public FilesCoreArtist(ICore sourceCore, ArtistMetadata artistMetadata)
        {
            SourceCore = sourceCore;
            _artistMetadata = artistMetadata;
            _fileMetadataManager = sourceCore.Cast<FilesCore>().FileMetadataManager;

            AttachEvents();
        }

        private void AttachEvents()
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            _fileMetadataManager.AlbumArtists.MetadataUpdated += Artists_MetadataUpdated;
        }

        private void DetachEvents()
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            _fileMetadataManager.AlbumArtists.MetadataUpdated -= Artists_MetadataUpdated;
        }

        private void Artists_MetadataUpdated(object sender, IEnumerable<ArtistMetadata> e)
        {
            foreach (var metadata in e)
            {
                if (metadata.Id != Id)
                    return;

                Guard.IsNotNull(metadata.AlbumIds, nameof(metadata.AlbumIds));
                Guard.IsNotNull(metadata.TrackIds, nameof(metadata.TrackIds));
                Guard.IsNotNull(metadata.ImageIds, nameof(metadata.ImageIds));
                Guard.IsNotNull(metadata.Genres, nameof(metadata.Genres));

                var previousData = _artistMetadata;
                _artistMetadata = metadata;

                Guard.IsNotNull(previousData.TrackIds, nameof(previousData.TrackIds));
                Guard.IsNotNull(previousData.AlbumIds, nameof(previousData.AlbumIds));
                Guard.IsNotNull(previousData.ImageIds, nameof(previousData.ImageIds));
                Guard.IsNotNull(previousData.Genres, nameof(previousData.Genres));

                if (metadata.Name != previousData.Name)
                    NameChanged?.Invoke(this, Name);

                if (metadata.Description != previousData.Description)
                    DescriptionChanged?.Invoke(this, Description);

                if (metadata.Genres.Count != previousData.Genres.Count)
                    GenresCountChanged?.Invoke(this, metadata.Genres.Count);

                if (metadata.TrackIds.Count != previousData.TrackIds.Count)
                    TracksCountChanged?.Invoke(this, metadata.TrackIds.Count);

                if (metadata.AlbumIds.Count != previousData.AlbumIds.Count)
                    AlbumItemsCountChanged?.Invoke(this, metadata.AlbumIds.Count);

                _ = HandleImagesChanged(previousData.ImageIds, metadata.ImageIds);
                _ = HandleTracksChanged(previousData.TrackIds, metadata.TrackIds);
                _ = HandleAlbumsChanged(previousData.AlbumIds, metadata.AlbumIds);
            }
        }

        private async Task HandleAlbumsChanged(HashSet<string> oldAlbumIds, HashSet<string> newAlbumIds)
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            
            if (oldAlbumIds.OrderBy(s => s).SequenceEqual(newAlbumIds.OrderBy(s => s)))
            {
                // Lists have identical content, so no images have changed.
                return;
            }

            var addedAlbums = newAlbumIds.Except(oldAlbumIds);
            var removedAlbums = oldAlbumIds.Except(newAlbumIds);

            if (oldAlbumIds.Count != newAlbumIds.Count)
            {
                AlbumItemsChanged?.Invoke(this, await TransformAsync(addedAlbums), await TransformAsync(removedAlbums));
                AlbumItemsCountChanged?.Invoke(this, newAlbumIds.Count);
            }

            async Task<IReadOnlyList<CollectionChangedItem<ICoreAlbumCollectionItem>>> TransformAsync(IEnumerable<string> ids)
            {
                var idArray = ids as string[] ?? ids.ToArray();
                var collectionChangedItems = new List<CollectionChangedItem<ICoreAlbumCollectionItem>>(idArray.Length);

                foreach (var id in idArray)
                {
                    var album = await _fileMetadataManager.Albums.GetByIdAsync(id);

                    Guard.IsNotNullOrWhiteSpace(album?.Id, nameof(album.Id));
                    collectionChangedItems.Add(new CollectionChangedItem<ICoreAlbumCollectionItem>(InstanceCache.Albums.GetOrCreate(album.Id, SourceCore, album), collectionChangedItems.Count));
                }

                return collectionChangedItems;
            }
        }

        private async Task HandleTracksChanged(HashSet<string> oldTrackIds, HashSet<string> newTrackIds)
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            if (oldTrackIds.OrderBy(s => s).SequenceEqual(newTrackIds.OrderBy(s => s)))
            {
                // Lists have identical content, so no images have changed.
                return;
            }

            var addedImages = newTrackIds.Except(oldTrackIds);
            var removedImages = oldTrackIds.Except(newTrackIds);

            if (oldTrackIds.Count != newTrackIds.Count)
            {
                TracksChanged?.Invoke(this, await TransformAsync(addedImages), await TransformAsync(removedImages));
                TracksCountChanged?.Invoke(this, newTrackIds.Count);
            }

            async Task<IReadOnlyList<CollectionChangedItem<ICoreTrack>>> TransformAsync(IEnumerable<string> ids)
            {
                var idArray = ids as string[] ?? ids.ToArray();
                var collectionChangedItems = new List<CollectionChangedItem<ICoreTrack>>(idArray.Length);

                foreach (var id in idArray)
                {
                    var track = await _fileMetadataManager.Tracks.GetByIdAsync(id);

                    Guard.IsNotNullOrWhiteSpace(track?.Id, nameof(track.Id));
                    collectionChangedItems.Add(new CollectionChangedItem<ICoreTrack>(InstanceCache.Tracks.GetOrCreate(track.Id, SourceCore, track), collectionChangedItems.Count));
                }

                return collectionChangedItems;
            }
        }

        private async Task HandleImagesChanged(HashSet<string> oldImageIds, HashSet<string> newImageIds)
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            if (oldImageIds.OrderBy(s => s).SequenceEqual(newImageIds.OrderBy(s => s)))
            {
                // Lists have identical content, so no images have changed.
                return;
            }

            var addedImages = newImageIds.Except(oldImageIds);
            var removedImages = oldImageIds.Except(newImageIds);

            if (oldImageIds.Count != newImageIds.Count)
            {
                ImagesChanged?.Invoke(this, await TransformAsync(addedImages), await TransformAsync(removedImages));
                ImagesCountChanged?.Invoke(this, newImageIds.Count);
            }

            async Task<IReadOnlyList<CollectionChangedItem<ICoreImage>>> TransformAsync(IEnumerable<string> ids)
            {
                var idArray = ids as string[] ?? ids.ToArray();
                var collectionChangedItems = new List<CollectionChangedItem<ICoreImage>>(idArray.Length);

                foreach (var id in idArray)
                {
                    var image = await _fileMetadataManager.Images.GetByIdAsync(id);

                    Guard.IsNotNullOrWhiteSpace(image?.Id, nameof(image.Id));
                    collectionChangedItems.Add(new CollectionChangedItem<ICoreImage>(InstanceCache.Images.GetOrCreate(image.Id, SourceCore, image), collectionChangedItems.Count));
                }

                return collectionChangedItems;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<PlaybackState>? PlaybackStateChanged;

        /// <inheritdoc/>
        public event EventHandler<string>? NameChanged;

        /// <inheritdoc/>
        public event EventHandler<string?>? DescriptionChanged;

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
        public event EventHandler<int>? TracksCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreImage>? ImagesChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreAlbumCollectionItem>? AlbumItemsChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreTrack>? TracksChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreGenre>? GenresChanged;

        /// <inheritdoc />
        public event EventHandler<int>? GenresCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreUrl>? UrlsChanged;

        /// <inheritdoc />
        public event EventHandler<int>? UrlsCountChanged;

        /// <inheritdoc/>
        public string Id => _artistMetadata.Id ?? string.Empty;

        /// <inheritdoc/>
        public int TotalTrackCount => _artistMetadata.TrackIds?.Count ?? 0;

        /// <inheritdoc/>
        public int TotalAlbumItemsCount => _artistMetadata.AlbumIds?.Count ?? 0;

        /// <inheritdoc />
        public int TotalGenreCount => _artistMetadata.Genres?.Count ?? 0;

        /// <inheritdoc />
        public int TotalImageCount => _artistMetadata.ImageIds?.Count ?? 0;

        /// <inheritdoc />
        public int TotalUrlCount => 0;

        /// <inheritdoc/>
        public ICore SourceCore { get; }

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
        public Task<bool> IsAddImageAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddTrackAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddAlbumItemAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddGenreAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddUrlAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveImageAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveTrackAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveAlbumItemAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveGenreAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsRemoveUrlAvailableAsync(int index, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task ChangeDescriptionAsync(string? description, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeDurationAsync(TimeSpan duration, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeNameAsync(string name, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <param name="cancellationToken">A cancellation token that may be used to cancel the ongoing task.</param>
        /// <inheritdoc/>
        public Task PauseTrackCollectionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <param name="cancellationToken">A cancellation token that may be used to cancel the ongoing task.</param>
        /// <inheritdoc/>
        public Task PlayTrackCollectionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <param name="cancellationToken">A cancellation token that may be used to cancel the ongoing task.</param>
        /// <inheritdoc/>
        public Task PauseAlbumCollectionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <param name="cancellationToken">A cancellation token that may be used to cancel the ongoing task.</param>
        /// <inheritdoc/>
        public Task PlayAlbumCollectionAsync(CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task PlayTrackCollectionAsync(ICoreTrack track, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task PlayAlbumCollectionAsync(ICoreAlbumCollectionItem albumItem, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddTrackAsync(ICoreTrack track, int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddAlbumItemAsync(ICoreAlbumCollectionItem album, int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveTrackAsync(int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveAlbumItemAsync(int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddGenreAsync(ICoreGenre genre, int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveGenreAsync(int index, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task AddImageAsync(ICoreImage image, int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddUrlAsync(ICoreUrl image, int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveUrlAsync(int index, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreAlbumCollectionItem> GetAlbumItemsAsync(int limit, int offset, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            var albums = await _fileMetadataManager.Albums.GetAlbumsByArtistId(Id, offset, limit);

            foreach (var album in albums)
            {
                Guard.IsNotNull(album.Id, nameof(album.Id));
                yield return InstanceCache.Albums.GetOrCreate(album.Id, SourceCore, album);
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreTrack> GetTracksAsync(int limit, int offset, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            var tracks = await _fileMetadataManager.Tracks.GetTracksByArtistId(Id, offset, limit);

            foreach (var track in tracks)
            {
                Guard.IsNotNullOrWhiteSpace(track?.Id, nameof(track.Id));
                yield return InstanceCache.Tracks.GetOrCreate(track.Id, SourceCore, track);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreImage> GetImagesAsync(int limit, int offset, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(_fileMetadataManager, nameof(_fileMetadataManager));
            if (_artistMetadata.ImageIds == null)
                yield break;

            foreach (var imageId in _artistMetadata.ImageIds)
            {
                var image = await _fileMetadataManager.Images.GetByIdAsync(imageId);
                Guard.IsNotNull(image, nameof(image));

                yield return InstanceCache.Images.GetOrCreate(imageId, SourceCore, image);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreUrl> GetUrlsAsync(int limit, int offset, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreGenre> GetGenresAsync(int limit, int offset, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(_artistMetadata, nameof(_artistMetadata));

            foreach (var genre in _artistMetadata.Genres ?? Enumerable.Empty<string>())
                yield return new FilesCoreGenre(SourceCore, genre);

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            DetachEvents();
            return default;
        }
    }
}
