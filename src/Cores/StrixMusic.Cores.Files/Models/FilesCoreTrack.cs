﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.Events;
using StrixMusic.Cores.Files.Services;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;
using StrixMusic.Sdk.Services.FileMetadataManager;
using StrixMusic.Sdk.Services.FileMetadataManager.Models;

namespace StrixMusic.Cores.Files.Models
{
    /// <summary>
    /// Wraps around <see cref="TrackMetadata"/> to provide track information extracted from a file to the Strix SDK.
    /// </summary>
    public sealed class FilesCoreTrack : ICoreTrack
    {
        private readonly IFileMetadataManager _fileMetadataManager;
        private TrackMetadata _trackMetadata;
        private FilesCoreImage? _image;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesCoreTrack"/> class.
        /// </summary>
        /// <param name="sourceCore">The source core.</param>
        /// <param name="trackMetadata">The track metadata to wrap around</param>
        public FilesCoreTrack(ICore sourceCore, TrackMetadata trackMetadata)
        {
            SourceCore = sourceCore;
            _trackMetadata = trackMetadata;

            if (trackMetadata.ImagePath != null)
                _image = InstanceCache.Images.GetOrCreate(Id, SourceCore, new Uri(trackMetadata.ImagePath));

            _fileMetadataManager = SourceCore.GetService<IFileMetadataManager>();
            AttachEvents();
        }

        private void AttachEvents()
        {
            _fileMetadataManager.Tracks.MetadataUpdated += Tracks_MetadataUpdated;
            _fileMetadataManager.Tracks.ArtistItemsChanged += Tracks_ArtistItemsChanged;
        }

        private void DetachEvents()
        {
            _fileMetadataManager.Tracks.MetadataUpdated -= Tracks_MetadataUpdated;
            _fileMetadataManager.Tracks.ArtistItemsChanged -= Tracks_ArtistItemsChanged;
        }

        private void Tracks_ArtistItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<(TrackMetadata Track, ArtistMetadata Artist)>> addedItems, IReadOnlyList<CollectionChangedItem<(TrackMetadata Track, ArtistMetadata Artist)>> removedItems)
        {
            var coreAddedItems = new List<CollectionChangedItem<ICoreArtistCollectionItem>>();
            var coreRemovedItems = new List<CollectionChangedItem<ICoreArtistCollectionItem>>();

            foreach (var item in addedItems)
            {
                if (item.Data.Track.Id == Id)
                {
                    Guard.IsNotNullOrWhiteSpace(item.Data.Artist.Id, nameof(item.Data.Artist.Id));
                    var coreArtist = InstanceCache.Artists.GetOrCreate(item.Data.Artist.Id, SourceCore, item.Data.Artist);
                    coreAddedItems.Add(new CollectionChangedItem<ICoreArtistCollectionItem>(coreArtist, item.Index));
                }
            }

            foreach (var item in removedItems)
            {
                if (item.Data.Track.Id == Id)
                {
                    Guard.IsNotNullOrWhiteSpace(item.Data.Artist.Id, nameof(item.Data.Artist.Id));
                    var coreArtist = InstanceCache.Artists.GetOrCreate(item.Data.Artist.Id, SourceCore, item.Data.Artist);
                    coreRemovedItems.Add(new CollectionChangedItem<ICoreArtistCollectionItem>(coreArtist, item.Index));
                }
            }

            if (coreAddedItems.Count + coreRemovedItems.Count > 0)
                ArtistItemsChanged?.Invoke(this, coreAddedItems, coreRemovedItems);
        }

        private void Tracks_MetadataUpdated(object sender, IEnumerable<TrackMetadata> e)
        {
            foreach (var metadata in e)
            {
                if (metadata.Id != Id)
                    return;

                Guard.IsNotNull(metadata.ArtistIds, nameof(metadata.ArtistIds));

                var previousData = _trackMetadata;
                _trackMetadata = metadata;

                if (metadata.Title != previousData.Title)
                    NameChanged?.Invoke(this, Name);

                if (metadata.ImagePath != previousData.ImagePath)
                    HandleImageChanged(metadata);

                if (metadata.Description != previousData.Description)
                    DescriptionChanged?.Invoke(this, Description);

                if (metadata.DiscNumber != previousData.DiscNumber)
                    TrackNumberChanged?.Invoke(this, TrackNumber);

                if (!Equals(metadata.Language, previousData.Language))
                    LanguageChanged?.Invoke(this, Language);

                if (!ReferenceEquals(metadata.Lyrics, previousData.Lyrics))
                    LyricsChanged?.Invoke(this, Lyrics);

                if (metadata.TrackNumber != previousData.TrackNumber)
                    TrackNumberChanged?.Invoke(this, TrackNumber);

                if (metadata.Duration != previousData.Duration)
                    DurationChanged?.Invoke(this, Duration);

                // TODO genres, post genres do-over

                if (metadata.ArtistIds.Count != (previousData.ArtistIds?.Count ?? 0))
                    ArtistItemsCountChanged?.Invoke(this, metadata.ArtistIds.Count);
            }
        }

        private void HandleImageChanged(TrackMetadata e)
        {
            var previousImage = _image;

            var removed = new List<CollectionChangedItem<ICoreImage>>();
            var added = new List<CollectionChangedItem<ICoreImage>>();

            if (previousImage != null)
                removed.Add(new CollectionChangedItem<ICoreImage>(previousImage, 0));

            // ReSharper disable once ReplaceWithStringIsNullOrEmpty (breaks nullability check)
            if (e.ImagePath != null && e.ImagePath.Length > 0)
            {
                var newImage = new FilesCoreImage(SourceCore, new Uri(e.ImagePath));
                InstanceCache.Images.Replace(Id, newImage);
                added.Add(new CollectionChangedItem<ICoreImage>(newImage, 0));
                _image = newImage;
            }

            if (added.Count > 0 || removed.Count > 0)
                ImagesChanged?.Invoke(this, added, removed);
        }

        /// <inheritdoc/>
        public event EventHandler<ICoreAlbum?>? AlbumChanged;

        /// <inheritdoc/>
        public event EventHandler<int?>? TrackNumberChanged;

        /// <inheritdoc/>
        public event EventHandler<CultureInfo?>? LanguageChanged;

        /// <inheritdoc/>
        public event EventHandler<ICoreLyrics?>? LyricsChanged;

        /// <inheritdoc/>
        public event EventHandler<bool>? IsExplicitChanged;

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
        public event EventHandler<bool>? IsPlayArtistCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsPauseArtistCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeNameAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDescriptionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDurationAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ArtistItemsCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? GenresCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? UrlsCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreArtistCollectionItem>? ArtistItemsChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreGenre>? GenresChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreImage>? ImagesChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreUrl>? UrlsChanged;

        /// <inheritdoc/>
        public string Id => _trackMetadata.Id ?? string.Empty;

        /// <inheritdoc/>
        public TrackType Type => TrackType.Song;

        /// <inheritdoc />
        public int TotalArtistItemsCount => _trackMetadata.ArtistIds?.Count ?? 0;

        /// <inheritdoc />
        public int TotalImageCount => _image != null ? 1 : 0;

        /// <inheritdoc/>
        public int TotalGenreCount => _trackMetadata.Genres?.Count ?? 0;

        /// <inheritdoc />
        public int TotalUrlCount => 0;

        /// <inheritdoc/>
        public ICoreAlbum? Album { get; }

        /// <inheritdoc/>
        /// <remarks>Is not passed into the constructor. Should be set on object creation.</remarks>
        public int? TrackNumber => Convert.ToInt32(_trackMetadata.TrackNumber);

        /// <inheritdoc />
        public int? DiscNumber { get; }

        /// <inheritdoc/>
        public CultureInfo? Language { get; }

        /// <inheritdoc/>
        public ICoreLyrics? Lyrics => null;

        /// <inheritdoc/>
        public bool IsExplicit => false;

        /// <inheritdoc/>
        public ICore SourceCore { get; }

        /// <summary>
        /// The path to the playable music file on disk.
        /// </summary>
        public Uri? LocalTrackPath => _trackMetadata.Url;

        /// <inheritdoc/>
        public string Name => _trackMetadata.Title ?? string.Empty;

        /// <inheritdoc/>
        public string? Description => _trackMetadata.Description;

        /// <inheritdoc/>
        public PlaybackState PlaybackState { get; }

        /// <inheritdoc/>
        public TimeSpan Duration => _trackMetadata.Duration ?? new TimeSpan(0, 0, 0);

        /// <inheritdoc />
        public DateTime? LastPlayed { get; }

        /// <inheritdoc />
        public DateTime? AddedAt { get; }

        /// <inheritdoc/>
        public ICorePlayableCollectionGroup? RelatedItems => null;

        /// <inheritdoc/>
        public bool IsChangeAlbumAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeTrackNumberAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeLanguageAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeLyricsAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeIsExplicitAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsPlayArtistCollectionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsPauseArtistCollectionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeNameAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeDescriptionAsyncAvailable => false;

        /// <inheritdoc/>
        public bool IsChangeDurationAsyncAvailable => false;

        /// <inheritdoc/>
        public Task<bool> IsAddArtistItemAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddImageAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddGenreAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> IsAddUrlAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveArtistItemAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveImageAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveGenreAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveUrlAvailableAsync(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task ChangeAlbumAsync(ICoreAlbum? albums)
        {
            throw new NotSupportedException();
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
        public Task ChangeIsExplicitAsync(bool isExplicit)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeLanguageAsync(CultureInfo language)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeLyricsAsync(ICoreLyrics? lyrics)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeNameAsync(string name)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task ChangeTrackNumberAsync(int? trackNumber)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task PauseArtistCollectionAsync()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task PlayArtistCollectionAsync()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task PlayArtistCollectionAsync(ICoreArtistCollectionItem artistItem)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddArtistItemAsync(ICoreArtistCollectionItem artist, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddImageAsync(ICoreImage image, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddGenreAsync(ICoreGenre genre, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task AddUrlAsync(ICoreUrl image, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveArtistItemAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveGenreAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveUrlAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<ICoreArtistCollectionItem> GetArtistItemsAsync(int limit, int offset)
        {
            var artistRepo = _fileMetadataManager.Artists;
            var artists = await artistRepo.GetArtistsByTrackId(Id, offset, limit);

            foreach (var artist in artists)
            {
                Guard.IsNotNullOrWhiteSpace(artist.Id, nameof(artist.Id));

                yield return InstanceCache.Artists.GetOrCreate(artist.Id, SourceCore, artist);
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreImage> GetImagesAsync(int limit, int offset)
        {
            if (_image != null)
                yield return _image;

            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreGenre> GetGenresAsync(int limit, int offset)
        {
            foreach (var genre in _trackMetadata.Genres ?? Enumerable.Empty<string>())
            {
                yield return new FilesCoreGenre(SourceCore, genre);
            }

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<ICoreUrl> GetUrlsAsync(int limit, int offset = 0)
        {
            return AsyncEnumerable.Empty<ICoreUrl>();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            DetachEvents();
            return default;
        }
    }
}