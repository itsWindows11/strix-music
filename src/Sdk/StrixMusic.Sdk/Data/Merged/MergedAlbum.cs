﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.Collections;
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Sdk.Data.Base;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Sdk.Data.Merged
{
    /// <summary>
    /// Merged multiple <see cref="ICoreAlbum"/> into a single <see cref="IAlbum"/>
    /// </summary>
    public class MergedAlbum : IAlbum, IMerged<ICoreAlbum>, IMerged<ICoreAlbumCollectionItem>
    {
        private readonly ICoreAlbum _preferredSource;
        private readonly List<ICoreAlbum> _sources;
        private readonly List<ICore> _sourceCores;
        private readonly MergedCollectionMap<ITrackCollection, ICoreTrackCollection, ITrack, ICoreTrack> _trackCollectionMap;
        private readonly MergedCollectionMap<IImageCollection, ICoreImageCollection, IImage, ICoreImage> _imageCollectionMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedAlbum"/> class.
        /// </summary>
        public MergedAlbum(IEnumerable<ICoreAlbum> sources)
        {
            _sources = sources?.ToList() ?? ThrowHelper.ThrowArgumentNullException<List<ICoreAlbum>>(nameof(sources));
            _sourceCores = _sources.Select(x => x.SourceCore).ToList();

            Artist = new MergedArtist(_sources.Select(x => x.Artist).ToList());

            var relatedItemsSources = _sources.Select(x => x.RelatedItems).PruneNull().ToList();
            if (relatedItemsSources.Count > 0)
            {
                RelatedItems = new MergedPlayableCollectionGroup(relatedItemsSources);
            }

            // TODO: Get the actual preferred source.
            _preferredSource = _sources[0];

            Name = _preferredSource.Name;
            Url = _preferredSource.Url;
            DatePublished = _preferredSource.DatePublished;
            PlaybackState = _preferredSource.PlaybackState;

            _trackCollectionMap = new MergedCollectionMap<ITrackCollection, ICoreTrackCollection, ITrack, ICoreTrack>(this);
            _imageCollectionMap = new MergedCollectionMap<IImageCollection, ICoreImageCollection, IImage, ICoreImage>(this);

            AttachEvents(_preferredSource);
        }

        private void AttachEvents(ICoreAlbum source)
        {
            AttachPlayableEvents(source);

            source.DatePublishedChanged += DatePublishedChanged;

            _trackCollectionMap.ItemsChanged += TrackCollectionMap_ItemsChanged;
            _trackCollectionMap.ItemsCountChanged += TrackCollectionMap_ItemsCountChanged;
            _imageCollectionMap.ItemsChanged += ImageCollectionMap_ItemsChanged;
            _imageCollectionMap.ItemsCountChanged += ImageCollectionMap_ItemsCountChanged;
        }

        private void DetachEvents(ICoreAlbum source)
        {
            DetachPlayableEvents(source);

            source.DatePublishedChanged -= DatePublishedChanged;

            _trackCollectionMap.ItemsChanged -= TrackCollectionMap_ItemsChanged;
            _trackCollectionMap.ItemsCountChanged -= TrackCollectionMap_ItemsCountChanged;
            _imageCollectionMap.ItemsChanged -= ImageCollectionMap_ItemsChanged;
            _imageCollectionMap.ItemsCountChanged -= ImageCollectionMap_ItemsCountChanged;
        }

        private void AttachPlayableEvents(IPlayable source)
        {
            source.PlaybackStateChanged += PlaybackStateChanged;
            source.NameChanged += NameChanged;
            source.DescriptionChanged += DescriptionChanged;
            source.UrlChanged += UrlChanged;
            source.DurationChanged += DurationChanged;
        }

        private void DetachPlayableEvents(IPlayable source)
        {
            source.PlaybackStateChanged -= PlaybackStateChanged;
            source.NameChanged -= NameChanged;
            source.DescriptionChanged -= DescriptionChanged;
            source.UrlChanged -= UrlChanged;
            source.DurationChanged -= DurationChanged;
        }

        private void TrackCollectionMap_ItemsChanged(object sender, IReadOnlyList<CollectionChangedEventItem<ITrack>> addedItems, IReadOnlyList<CollectionChangedEventItem<ITrack>> removedItems)
        {
            TrackItemsChanged?.Invoke(this, addedItems, removedItems);
        }

        private void ImageCollectionMap_ItemsChanged(object sender, IReadOnlyList<CollectionChangedEventItem<IImage>> addedItems, IReadOnlyList<CollectionChangedEventItem<IImage>> removedItems)
        {
            ImagesChanged?.Invoke(this, addedItems, removedItems);
        }

        private void TrackCollectionMap_ItemsCountChanged(object sender, int e)
        {
            TotalTracksCount = e;
            TrackItemsCountChanged?.Invoke(this, e);
        }

        private void ImageCollectionMap_ItemsCountChanged(object sender, int e)
        {
            TotalImageCount = e;
            ImagesCountChanged?.Invoke(this, e);
        }

        /// <inheritdoc cref="ISdkMember{T}.SourceCores" />
        public IReadOnlyList<ICore> SourceCores => _sourceCores;

        /// <inheritdoc />
        public void AddSource(ICoreAlbumCollectionItem itemToMerge)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RemoveSource(ICoreAlbumCollectionItem itemToRemove)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IReadOnlyList<ICoreAlbumCollectionItem> IMerged<ICoreAlbumCollectionItem>.Sources => Sources;

        /// <inheritdoc/>
        public IReadOnlyList<ICoreAlbum> Sources => _sources;

        /// <inheritdoc/>
        public IArtist Artist { get; }

        /// <inheritdoc/>
        public IPlayableCollectionGroup? RelatedItems { get; }

        /// <inheritdoc/>
        public DateTime? DatePublished { get; internal set; }

        /// <inheritdoc/>
        public bool IsChangeDatePublishedAsyncSupported => _preferredSource.IsChangeDatePublishedAsyncSupported;

        /// <inheritdoc/>
        SynchronizedObservableCollection<string>? IGenreCollectionBase.Genres => _preferredSource.Genres;

        /// <inheritdoc/>
        IReadOnlyList<ICoreAlbumCollectionItem> ISdkMember<ICoreAlbumCollectionItem>.Sources => Sources;

        /// <inheritdoc/>
        IReadOnlyList<ICoreTrackCollection> ISdkMember<ICoreTrackCollection>.Sources => Sources;

        /// <inheritdoc/>
        IReadOnlyList<ICoreImageCollection> ISdkMember<ICoreImageCollection>.Sources => Sources;

        /// <inheritdoc/>
        IReadOnlyList<ICoreAlbum> ISdkMember<ICoreAlbum>.Sources => Sources;

        /// <inheritdoc/>
        public string Id => _preferredSource.Id;

        /// <inheritdoc/>
        public string Name { get; internal set; }

        /// <inheritdoc/>
        public int TotalTracksCount { get; internal set; }

        /// <inheritdoc />
        public int TotalImageCount { get; internal set; }

        /// <inheritdoc/>
        public string? Description { get; internal set; }

        /// <inheritdoc/>
        public PlaybackState PlaybackState { get; internal set; }

        /// <inheritdoc/>
        public TimeSpan Duration => _preferredSource.Duration;

        /// <inheritdoc/>
        public bool IsPlayAsyncSupported => _preferredSource.IsPlayAsyncSupported;

        /// <inheritdoc/>
        public bool IsPauseAsyncSupported => _preferredSource.IsPauseAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeNameAsyncSupported => _preferredSource.IsChangeNameAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeDescriptionAsyncSupported => _preferredSource.IsChangeDescriptionAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeDurationAsyncSupported => _preferredSource.IsChangeDurationAsyncSupported;

        /// <inheritdoc/>
        public Uri? Url { get; internal set; }

        /// <inheritdoc />
        public event EventHandler<DateTime?>? DatePublishedChanged;

        /// <inheritdoc />
        public event EventHandler<string>? NameChanged;

        /// <inheritdoc />
        public event EventHandler<string?>? DescriptionChanged;

        /// <inheritdoc />
        public event EventHandler<PlaybackState>? PlaybackStateChanged;

        /// <inheritdoc />
        public event EventHandler<Uri?>? UrlChanged;

        /// <inheritdoc />
        public event EventHandler<TimeSpan>? DurationChanged;

        /// <inheritdoc />
        public event EventHandler<int>? TrackItemsCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ITrack>? TrackItemsChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IImage>? ImagesChanged;

        /// <inheritdoc/>
        public Task RemoveTrackAsync(int index) => _preferredSource.RemoveTrackAsync(index);

        /// <inheritdoc/>
        public Task AddTrackAsync(ITrack track, int index)
        {
            return _trackCollectionMap.InsertItem(track, index);
        }

        /// <inheritdoc/>
        public Task ChangeDatePublishedAsync(DateTime datePublished)
        {
            return _preferredSource.ChangeDatePublishedAsync(datePublished);
        }

        /// <inheritdoc/>
        public Task ChangeDescriptionAsync(string? description)
        {
            return _preferredSource.ChangeDescriptionAsync(description);
        }

        /// <inheritdoc/>
        public Task ChangeDurationAsync(TimeSpan duration)
        {
            return _preferredSource.ChangeDurationAsync(duration);
        }

        /// <inheritdoc cref="IPlayable.ChangeNameAsync(string)"/>
        public Task ChangeNameAsync(string name)
        {
            return _preferredSource.ChangeNameAsync(name);
        }

        /// <inheritdoc/>
        Task IPlayable.ChangeNameAsync(string name) => ChangeNameAsync(name);

        /// <inheritdoc />
        public Task<IReadOnlyList<IImage>> GetImagesAsync(int limit, int offset)
        {
            return _imageCollectionMap.GetItems(limit, offset);
        }

        /// <inheritdoc />
        public Task AddImageAsync(IImage image, int index)
        {
            return _imageCollectionMap.InsertItem(image, index);
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index)
        {
            return _imageCollectionMap.RemoveAt(index);
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<ITrack>> GetTracksAsync(int limit, int offset) => _trackCollectionMap.GetItems(limit, offset);

        /// <inheritdoc/>
        public Task<bool> IsAddGenreSupported(int index) => _preferredSource.IsAddGenreSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsAddImageSupported(int index) => _imageCollectionMap.IsAddItemSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsAddTrackSupported(int index) => _trackCollectionMap.IsAddItemSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveGenreSupported(int index) => _preferredSource.IsRemoveGenreSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveImageSupported(int index) => _imageCollectionMap.IsRemoveItemSupport(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveTrackSupported(int index) => _trackCollectionMap.IsRemoveItemSupport(index);

        /// <inheritdoc/>
        public Task PauseAsync() => _preferredSource.PauseAsync();

        /// <inheritdoc/>
        public Task PlayAsync() => _preferredSource.PlayAsync();

        /// <inheritdoc/>
        public void AddSource(ICoreAlbum itemToMerge)
        {
            Guard.IsNotNull(itemToMerge, nameof(itemToMerge));

            _sources.Add(itemToMerge);
            _sourceCores.Add(itemToMerge.SourceCore);
            _imageCollectionMap.AddSource(itemToMerge);
            _trackCollectionMap.AddSource(itemToMerge);
        }

        /// <inheritdoc />
        public void RemoveSource(ICoreAlbum itemToRemove)
        {
            Guard.IsNotNull(itemToRemove, nameof(itemToRemove));

            _sources.Remove(itemToRemove);
            _sourceCores.Remove(itemToRemove.SourceCore);
            _imageCollectionMap.RemoveSource(itemToRemove);
            _trackCollectionMap.RemoveSource(itemToRemove);
        }

        /// <inheritdoc/>
        public bool Equals(ICoreAlbum other)
        {
            return other?.Name == Name;
        }

        /// <inheritdoc />
        public bool Equals(ICoreAlbumCollectionItem other)
        {
            return false;
        }
    }
}