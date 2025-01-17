﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Sdk.AppModels;
using StrixMusic.Sdk.BaseModels;
using StrixMusic.Sdk.CoreModels;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Sdk.AdapterModels
{
    /// <summary>
    /// A concrete class that merged multiple <see cref="ICorePlaylistCollection"/>s.
    /// </summary>
    public class MergedPlaylistCollection : IPlaylistCollection, IMergedMutable<ICorePlaylistCollection>
    {
        private readonly List<ICorePlaylistCollection> _sources;
        private readonly ICorePlaylistCollection _preferredSource;
        private readonly MergedCollectionMap<IPlaylistCollection, ICorePlaylistCollection, IPlaylistCollectionItem, ICorePlaylistCollectionItem> _playlistMap;
        private readonly MergedCollectionMap<IImageCollection, ICoreImageCollection, IImage, ICoreImage> _imageMap;
        private readonly MergedCollectionMap<IUrlCollection, ICoreUrlCollection, IUrl, ICoreUrl> _urlMap;

        /// <summary>
        /// Creates a new instance of <see cref="MergedPlaylistCollection"/>.
        /// </summary>
        public MergedPlaylistCollection(IEnumerable<ICorePlaylistCollection> collections, MergedCollectionConfig config)
        {
            _sources = collections.ToList();

            _preferredSource = _sources[0];

            _imageMap = new MergedCollectionMap<IImageCollection, ICoreImageCollection, IImage, ICoreImage>(this, config);
            _urlMap = new MergedCollectionMap<IUrlCollection, ICoreUrlCollection, IUrl, ICoreUrl>(this, config);
            _playlistMap = new MergedCollectionMap<IPlaylistCollection, ICorePlaylistCollection, IPlaylistCollectionItem, ICorePlaylistCollectionItem>(this, config);

            foreach (var item in _sources)
            {
                TotalPlaylistItemsCount += item.TotalPlaylistItemsCount;
                TotalImageCount += item.TotalImageCount;
                TotalUrlCount += item.TotalUrlCount;
            }

            Name = _preferredSource.Name;
            Description = _preferredSource.Description;
            PlaybackState = _preferredSource.PlaybackState;
            Duration = _preferredSource.Duration;
            LastPlayed = _preferredSource.LastPlayed;
            AddedAt = _preferredSource.AddedAt;

            AttachEvents(_preferredSource);
        }

        private void AttachEvents(ICorePlaylistCollection source)
        {
            AttachPlayableEvents(source);

            source.IsPlayPlaylistCollectionAsyncAvailableChanged += IsPlayPlaylistCollectionAsyncAvailableChanged;
            source.IsPausePlaylistCollectionAsyncAvailableChanged += IsPausePlaylistCollectionAsyncAvailableChanged;

            _playlistMap.ItemsChanged += PlaylistMap_ItemsChanged;
            _playlistMap.ItemsCountChanged += PlaylistMap_ItemsCountChanged;
            _imageMap.ItemsChanged += ImageCollectionMap_ItemsChanged;
            _imageMap.ItemsCountChanged += ImageCollectionMap_ItemsCountChanged;
            _urlMap.ItemsChanged += UrlCollectionMap_ItemsChanged;
            _urlMap.ItemsCountChanged += UrlCollectionMap_ItemsCountChanged;
        }

        private void DetachEvents(ICorePlaylistCollection source)
        {
            DetachPlayableEvents(source);

            source.IsPlayPlaylistCollectionAsyncAvailableChanged -= IsPlayPlaylistCollectionAsyncAvailableChanged;
            source.IsPausePlaylistCollectionAsyncAvailableChanged -= IsPausePlaylistCollectionAsyncAvailableChanged;

            _playlistMap.ItemsChanged -= PlaylistMap_ItemsChanged;
            _playlistMap.ItemsCountChanged -= PlaylistMap_ItemsCountChanged;
            _imageMap.ItemsChanged -= ImageCollectionMap_ItemsChanged;
            _imageMap.ItemsCountChanged -= ImageCollectionMap_ItemsCountChanged;
            _urlMap.ItemsChanged -= UrlCollectionMap_ItemsChanged;
            _urlMap.ItemsCountChanged -= UrlCollectionMap_ItemsCountChanged;
        }

        private void AttachPlayableEvents(IPlayableBase source)
        {
            source.PlaybackStateChanged += PlaybackStateChanged;
            source.NameChanged += NameChanged;
            source.DescriptionChanged += DescriptionChanged;
            source.DurationChanged += DurationChanged;
            source.LastPlayedChanged += LastPlayedChanged;
            source.IsChangeNameAsyncAvailableChanged += IsChangeNameAsyncAvailableChanged;
            source.IsChangeDurationAsyncAvailableChanged += IsChangeDurationAsyncAvailableChanged;
            source.IsChangeDescriptionAsyncAvailableChanged += IsChangeDescriptionAsyncAvailableChanged;
        }

        private void DetachPlayableEvents(IPlayableBase source)
        {
            source.PlaybackStateChanged -= PlaybackStateChanged;
            source.NameChanged -= NameChanged;
            source.DescriptionChanged -= DescriptionChanged;
            source.DurationChanged -= DurationChanged;
            source.LastPlayedChanged -= LastPlayedChanged;
            source.IsChangeNameAsyncAvailableChanged -= IsChangeNameAsyncAvailableChanged;
            source.IsChangeDurationAsyncAvailableChanged -= IsChangeDurationAsyncAvailableChanged;
            source.IsChangeDescriptionAsyncAvailableChanged -= IsChangeDescriptionAsyncAvailableChanged;
        }

        private void PlaylistMap_ItemsCountChanged(object sender, int e)
        {
            TotalPlaylistItemsCount = e;
            PlaylistItemsCountChanged?.Invoke(this, e);
        }

        private void ImageCollectionMap_ItemsCountChanged(object sender, int e)
        {
            TotalImageCount = e;
            ImagesCountChanged?.Invoke(this, e);
        }

        private void UrlCollectionMap_ItemsCountChanged(object sender, int e)
        {
            TotalUrlCount = e;
            UrlsCountChanged?.Invoke(this, e);
        }

        private void ImageCollectionMap_ItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<IImage>> addedItems, IReadOnlyList<CollectionChangedItem<IImage>> removedItems)
        {
            ImagesChanged?.Invoke(this, addedItems, removedItems);
        }

        private void UrlCollectionMap_ItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<IUrl>> addedItems, IReadOnlyList<CollectionChangedItem<IUrl>> removedItems)
        {
            UrlsChanged?.Invoke(this, addedItems, removedItems);
        }

        private void PlaylistMap_ItemsChanged(object sender, IReadOnlyList<CollectionChangedItem<IPlaylistCollectionItem>> addedItems, IReadOnlyList<CollectionChangedItem<IPlaylistCollectionItem>> removedItems)
        {
            PlaylistItemsChanged?.Invoke(this, addedItems, removedItems);
        }

        /// <inheritdoc />
        public event EventHandler<PlaybackState>? PlaybackStateChanged;

        /// <inheritdoc />
        public event EventHandler<string>? NameChanged;

        /// <inheritdoc />
        public event EventHandler<string?>? DescriptionChanged;

        /// <inheritdoc />
        public event EventHandler<TimeSpan>? DurationChanged;

        /// <inheritdoc />
        public event EventHandler<DateTime?>? LastPlayedChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsPlayPlaylistCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsPausePlaylistCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeNameAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDescriptionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDurationAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? UrlsCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? PlaylistItemsCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IImage>? ImagesChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IUrl>? UrlsChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IPlaylistCollectionItem>? PlaylistItemsChanged;

        /// <inheritdoc/>
        public event EventHandler<DownloadInfo>? DownloadInfoChanged;
        
        /// <inheritdoc cref="IMerged.SourcesChanged"/>
        public event EventHandler? SourcesChanged;

        /// <inheritdoc />
        public string Id => _preferredSource.Id;

        /// <inheritdoc />
        public string Name { get; internal set; }

        /// <inheritdoc />
        public string? Description { get; internal set; }

        /// <inheritdoc />
        public PlaybackState PlaybackState { get; internal set; }

        /// <inheritdoc/>
        public DownloadInfo DownloadInfo => default;

        /// <inheritdoc />
        public TimeSpan Duration { get; internal set; }

        /// <inheritdoc />
        public DateTime? LastPlayed { get; internal set; }

        /// <inheritdoc />
        public DateTime? AddedAt { get; internal set; }

        /// <inheritdoc />
        public int TotalPlaylistItemsCount { get; internal set; }

        /// <inheritdoc />
        public int TotalImageCount { get; internal set; }

        /// <inheritdoc />
        public int TotalUrlCount { get; internal set; }

        /// <inheritdoc />
        public bool IsPlayPlaylistCollectionAsyncAvailable => _preferredSource.IsPlayPlaylistCollectionAsyncAvailable;

        /// <inheritdoc />
        public bool IsPausePlaylistCollectionAsyncAvailable => _preferredSource.IsPausePlaylistCollectionAsyncAvailable;

        /// <inheritdoc />
        public bool IsChangeNameAsyncAvailable => _preferredSource.IsChangeNameAsyncAvailable;

        /// <inheritdoc />
        public bool IsChangeDescriptionAsyncAvailable => _preferredSource.IsChangeDescriptionAsyncAvailable;

        /// <inheritdoc />
        public bool IsChangeDurationAsyncAvailable => _preferredSource.IsChangeDurationAsyncAvailable;

        /// <inheritdoc />
        public Task PlayPlaylistCollectionAsync(CancellationToken cancellationToken) => _preferredSource.PlayPlaylistCollectionAsync(cancellationToken);

        /// <inheritdoc />
        public Task PausePlaylistCollectionAsync(CancellationToken cancellationToken) => _preferredSource.PausePlaylistCollectionAsync(cancellationToken);

        /// <inheritdoc />
        public Task PlayPlaylistCollectionAsync(IPlaylistCollectionItem playlistItem, CancellationToken cancellationToken = default)
        {
            var targetCore = _preferredSource.SourceCore;

            ICorePlaylistCollectionItem? source = null;

            if (playlistItem is IPlaylist playlist)
                source = playlist.GetSources<ICorePlaylist>().FirstOrDefault(x => x.SourceCore.InstanceId == targetCore.InstanceId);

            if (playlistItem is IPlaylistCollection collection)
                source = collection.GetSources<ICorePlaylistCollection>().FirstOrDefault(x => x.SourceCore.InstanceId == targetCore.InstanceId);

            Guard.IsNotNull(source, nameof(source));

            return _preferredSource.PlayPlaylistCollectionAsync(source, cancellationToken);
        }

        /// <inheritdoc />
        public Task ChangeNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return _preferredSource.ChangeNameAsync(name, cancellationToken);
        }

        /// <inheritdoc />
        public Task ChangeDescriptionAsync(string? description, CancellationToken cancellationToken = default)
        {
            return _preferredSource.ChangeDescriptionAsync(description, cancellationToken);
        }

        /// <inheritdoc />
        public Task ChangeDurationAsync(TimeSpan duration, CancellationToken cancellationToken = default)
        {
            return _preferredSource.ChangeDurationAsync(duration, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> IsAddPlaylistItemAvailableAsync(int index, CancellationToken cancellationToken = default) => _playlistMap.IsAddItemAvailableAsync(index, cancellationToken);

        /// <inheritdoc />
        public Task<bool> IsRemovePlaylistItemAvailableAsync(int index, CancellationToken cancellationToken = default) => _playlistMap.IsRemoveItemAvailableAsync(index, cancellationToken);

        /// <inheritdoc />
        public Task<bool> IsAddImageAvailableAsync(int index, CancellationToken cancellationToken = default) => _imageMap.IsAddItemAvailableAsync(index, cancellationToken);

        /// <inheritdoc />
        public Task<bool> IsRemoveImageAvailableAsync(int index, CancellationToken cancellationToken = default) => _imageMap.IsRemoveItemAvailableAsync(index, cancellationToken);

        /// <inheritdoc />
        public Task<bool> IsAddUrlAvailableAsync(int index, CancellationToken cancellationToken = default) => _urlMap.IsAddItemAvailableAsync(index, cancellationToken);

        /// <inheritdoc />
        public Task<bool> IsRemoveUrlAvailableAsync(int index, CancellationToken cancellationToken = default) => _urlMap.IsRemoveItemAvailableAsync(index, cancellationToken);

        /// <inheritdoc cref="IMerged{T}.Sources" />
        public IReadOnlyList<ICorePlaylistCollection> Sources => _sources;

        /// <inheritdoc />
        IReadOnlyList<ICorePlaylistCollectionItem> IMerged<ICorePlaylistCollectionItem>.Sources => _sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreImageCollection> IMerged<ICoreImageCollection>.Sources => _sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreUrlCollection> IMerged<ICoreUrlCollection>.Sources => _sources;

        /// <inheritdoc />
        IReadOnlyList<ICorePlaylistCollection> IMerged<ICorePlaylistCollection>.Sources => _sources;

        /// <inheritdoc />
        public IAsyncEnumerable<IPlaylistCollectionItem> GetPlaylistItemsAsync(int limit, int offset, CancellationToken cancellationToken = default) => _playlistMap.GetItemsAsync(limit, offset, cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<IImage> GetImagesAsync(int limit, int offset, CancellationToken cancellationToken = default) => _imageMap.GetItemsAsync(limit, offset, cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<IUrl> GetUrlsAsync(int limit, int offset, CancellationToken cancellationToken = default) => _urlMap.GetItemsAsync(limit, offset, cancellationToken);

        /// <inheritdoc />
        public Task AddPlaylistItemAsync(IPlaylistCollectionItem playlistItem, int index, CancellationToken cancellationToken = default) => _playlistMap.InsertItemAsync(playlistItem, index, cancellationToken);

        /// <inheritdoc />
        public Task AddImageAsync(IImage image, int index, CancellationToken cancellationToken = default) => _imageMap.InsertItemAsync(image, index, cancellationToken);

        /// <inheritdoc />
        public Task AddUrlAsync(IUrl url, int index, CancellationToken cancellationToken = default) => _urlMap.InsertItemAsync(url, index, cancellationToken);

        /// <inheritdoc />
        public Task RemovePlaylistItemAsync(int index, CancellationToken cancellationToken = default) => _playlistMap.RemoveAtAsync(index, cancellationToken);

        /// <inheritdoc />
        public Task RemoveImageAsync(int index, CancellationToken cancellationToken = default) => _imageMap.RemoveAtAsync(index, cancellationToken);

        /// <inheritdoc />
        public Task RemoveUrlAsync(int index, CancellationToken cancellationToken = default) => _urlMap.RemoveAtAsync(index, cancellationToken);

        /// <inheritdoc/>
        public Task StartDownloadOperationAsync(DownloadOperation operation, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void AddSource(ICorePlaylistCollection itemToMerge)
        {
            Guard.IsNotNull(itemToMerge, nameof(itemToMerge));

            _sources.Add(itemToMerge);

            _playlistMap.AddSource(itemToMerge);
            _imageMap.AddSource(itemToMerge);
            _urlMap.AddSource(itemToMerge);
        }

        /// <inheritdoc />
        public void RemoveSource(ICorePlaylistCollection itemToRemove)
        {
            Guard.IsNotNull(itemToRemove, nameof(itemToRemove));

            _sources.Remove(itemToRemove);

            _playlistMap.RemoveSource(itemToRemove);
            _imageMap.RemoveSource(itemToRemove);
            _urlMap.RemoveSource(itemToRemove);
        }

        /// <inheritdoc />
        public bool Equals(ICorePlaylistCollection? other)
        {
            return other?.Name == Name;
        }

        /// <inheritdoc />
        public bool Equals(ICorePlaylistCollectionItem other) => Equals(other as ICorePlaylistCollection);

        /// <inheritdoc />
        public bool Equals(ICoreImageCollection other) => Equals(other as ICorePlaylistCollection);

        /// <inheritdoc />
        public bool Equals(ICoreUrlCollection other) => Equals(other as ICorePlaylistCollection);

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            DetachEvents(_preferredSource);

            await _sources.InParallel(x => x.DisposeAsync().AsTask());

            await _playlistMap.DisposeAsync();
            await _imageMap.DisposeAsync();
            await _urlMap.DisposeAsync();
        }
    }
}
