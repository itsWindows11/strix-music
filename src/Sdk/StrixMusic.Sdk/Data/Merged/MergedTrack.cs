﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.Collections;
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Sdk.Data.Base;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Sdk.Data.Merged
{
    /// <summary>
    /// A concrete class that merged multiple <see cref="ICoreTrack"/>s.
    /// </summary>
    public class MergedTrack : ITrack, IMerged<ICoreTrack>
    {
        private readonly ICoreTrack _preferredSource;
        private readonly List<ICoreTrack> _sources;

        private readonly MergedCollectionMap<IArtistCollection, ICoreArtistCollection, IArtistCollectionItem, ICoreArtistCollectionItem> _artistMap;

        private readonly MergedCollectionMap<IImageCollection, ICoreImageCollection, IImage, ICoreImage> _imageCollectionMap;
        private readonly List<ICore> _sourceCores;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedTrack"/> class.
        /// </summary>
        /// <param name="tracks">The <see cref="ICoreTrack"/>s to merge together.</param>
        public MergedTrack(IEnumerable<ICoreTrack> tracks)
        {
            _sources = tracks?.ToList() ?? ThrowHelper.ThrowArgumentNullException<List<ICoreTrack>>(nameof(tracks));
            _sourceCores = _sources.Select(x => x.SourceCore).ToList();

            // TODO: Use top Preferred core.
            _preferredSource = _sources.First();

            _artistMap = new MergedCollectionMap<IArtistCollection, ICoreArtistCollection, IArtistCollectionItem, ICoreArtistCollectionItem>(this);
            _imageCollectionMap = new MergedCollectionMap<IImageCollection, ICoreImageCollection, IImage, ICoreImage>(this);

            foreach (var item in _sources)
            {
                TotalArtistItemsCount += item.TotalArtistItemsCount;
                TotalImageCount += item.TotalImageCount;

                if (item.IsExplicit)
                    IsExplicit = true;
            }

            AttachEvents(_preferredSource);
        }

        private void AttachEvents(ICoreTrack source)
        {
            AttachPlayableEvents(source);

            source.LanguageChanged += LanguageChanged;
            source.LyricsChanged += Source_LyricsChanged;
            source.IsExplicitChanged += IsExplicitChanged;
            source.AlbumChanged += Source_AlbumChanged;
            source.TrackNumberChanged += TrackNumberChanged;

            _imageCollectionMap.ItemsChanged += ImageCollectionMap_ItemsChanged;
            _imageCollectionMap.ItemsCountChanged += ImageCollectionMap_ItemsCountChanged;
            _artistMap.ItemsChanged += ArtistMap_ItemsChanged;
            _artistMap.ItemsCountChanged += ArtistMap_ItemsCountChanged;
        }

        private void DetachEvents(ICoreTrack source)
        {
            DetachPlayableEvents(source);

            source.LanguageChanged -= LanguageChanged;
            source.LyricsChanged -= Source_LyricsChanged;
            source.IsExplicitChanged -= IsExplicitChanged;
            source.AlbumChanged -= Source_AlbumChanged;
            source.TrackNumberChanged -= TrackNumberChanged;

            _imageCollectionMap.ItemsChanged -= ImageCollectionMap_ItemsChanged;
            _imageCollectionMap.ItemsCountChanged -= ImageCollectionMap_ItemsCountChanged;
            _artistMap.ItemsChanged -= ArtistMap_ItemsChanged;
            _artistMap.ItemsCountChanged -= ArtistMap_ItemsCountChanged;
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

        private void ImageCollectionMap_ItemsCountChanged(object sender, int e)
        {
            TotalImageCount = e;
            ImagesCountChanged?.Invoke(this, e);
        }

        private void ArtistMap_ItemsCountChanged(object sender, int e)
        {
            TotalArtistItemsCount = e;
            ArtistItemsCountChanged?.Invoke(this, e);
        }

        private void ImageCollectionMap_ItemsChanged(object sender, IReadOnlyList<CollectionChangedEventItem<IImage>> addedItems, IReadOnlyList<CollectionChangedEventItem<IImage>> removedItems)
        {
            ImagesChanged?.Invoke(this, addedItems, removedItems);
        }

        private void ArtistMap_ItemsChanged(object sender, IReadOnlyList<CollectionChangedEventItem<IArtistCollectionItem>> addedItems, IReadOnlyList<CollectionChangedEventItem<IArtistCollectionItem>> removedItems)
        {
            ArtistItemsChanged?.Invoke(this, addedItems, removedItems);
        }

        private void Source_LyricsChanged(object sender, ICoreLyrics? e)
        {
            if (e is null)
                return;

            var merged = new MergedLyrics(e);
            LyricsChanged?.Invoke(this, merged);
        }

        private void Source_AlbumChanged(object sender, ICoreAlbum? e)
        {
            if (e is null)
                return;

            var merged = new MergedAlbum(e.IntoList());
            AlbumChanged?.Invoke(this, merged);
        }

        /// <inheritdoc/>
        public event EventHandler<IAlbum?>? AlbumChanged;

        /// <inheritdoc />
        public event EventHandler<int?>? TrackNumberChanged;

        /// <inheritdoc />
        public event EventHandler<PlaybackState>? PlaybackStateChanged;

        /// <inheritdoc />
        public event EventHandler<CultureInfo?>? LanguageChanged;

        /// <inheritdoc />
        public event EventHandler<ILyrics?>? LyricsChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsExplicitChanged;

        /// <inheritdoc />
        public event EventHandler<string>? NameChanged;

        /// <inheritdoc />
        public event EventHandler<string?>? DescriptionChanged;

        /// <inheritdoc />
        public event EventHandler<Uri?>? UrlChanged;

        /// <inheritdoc />
        public event EventHandler<TimeSpan>? DurationChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ArtistItemsCountChanged;

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IImage>? ImagesChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IArtistCollectionItem>? ArtistItemsChanged;

        /// <inheritdoc cref="ISdkMember{T}.SourceCores"/>
        public IReadOnlyList<ICore> SourceCores => _sourceCores;

        /// <inheritdoc />
        IReadOnlyList<ICoreTrack> ISdkMember<ICoreTrack>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreGenreCollection> ISdkMember<ICoreGenreCollection>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreArtistCollection> ISdkMember<ICoreArtistCollection>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreImageCollection> ISdkMember<ICoreImageCollection>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreArtistCollectionItem> ISdkMember<ICoreArtistCollectionItem>.Sources => Sources;

        /// <summary>
        /// The original sources for this merged item.
        /// </summary>
        public IReadOnlyList<ICoreTrack> Sources => _sources;

        /// <inheritdoc/>
        public string Id => _preferredSource.Id;

        /// <inheritdoc/>
        public Uri? Url => _preferredSource.Url;

        /// <inheritdoc/>
        public string Name => _preferredSource.Name;

        /// <inheritdoc/>
        public TrackType Type => _preferredSource.Type;

        /// <inheritdoc/>
        public int TotalArtistItemsCount { get; private set; }

        /// <inheritdoc />
        public int TotalImageCount { get; private set; }

        /// <inheritdoc/>
        public SynchronizedObservableCollection<string>? Genres { get; }

        /// <inheritdoc/>
        public IPlayableCollectionGroup? RelatedItems { get; }

        /// <inheritdoc/>
        public IAlbum? Album { get; }

        /// <inheritdoc/>
        public int? TrackNumber => _preferredSource.TrackNumber;

        /// <inheritdoc/>
        public int? DiscNumber => _preferredSource.DiscNumber;

        /// <inheritdoc/>
        public CultureInfo? Language => _preferredSource.Language;

        /// <inheritdoc/>
        public ILyrics? Lyrics { get; }

        /// <inheritdoc/>
        public bool IsExplicit { get; }

        /// <inheritdoc/>
        public string? Description => _preferredSource.Description;

        /// <inheritdoc/>
        public PlaybackState PlaybackState => _preferredSource.PlaybackState;

        /// <inheritdoc/>
        public TimeSpan Duration => _preferredSource.Duration;

        /// <inheritdoc/>
        public bool IsChangeAlbumAsyncSupported => _preferredSource.IsChangeAlbumAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeTrackNumberAsyncSupported => _preferredSource.IsChangeTrackNumberAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeLanguageAsyncSupported => _preferredSource.IsChangeLanguageAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeLyricsAsyncSupported => _preferredSource.IsChangeLyricsAsyncSupported;

        /// <inheritdoc/>
        public bool IsChangeIsExplicitAsyncSupported => _preferredSource.IsChangeIsExplicitAsyncSupported;

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
        public Task<bool> IsRemoveImageSupported(int index) => _imageCollectionMap.IsRemoveItemSupport(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveGenreSupported(int index) => _preferredSource.IsRemoveGenreSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveArtistSupported(int index) => _artistMap.IsRemoveItemSupport(index);

        /// <inheritdoc/>
        public Task PauseAsync() => _preferredSource.PauseAsync();

        /// <inheritdoc/>
        public Task PlayAsync() => _preferredSource.PlayAsync();

        /// <inheritdoc/>
        public Task ChangeAlbumAsync(IAlbum? album)
        {
            return _sources.InParallel(x =>
            {
                if (!x.IsChangeAlbumAsyncSupported)
                    return Task.CompletedTask;

                var sourceAlbum = album?.GetSources<ICoreAlbum>().First(y => y.SourceCore == x.SourceCore);
                return x.ChangeAlbumAsync(sourceAlbum);
            });
        }

        /// <inheritdoc/>
        public Task ChangeTrackNumberAsync(int? trackNumber)
        {
            return _sources.InParallel(x => x.ChangeTrackNumberAsync(trackNumber));
        }

        /// <inheritdoc/>
        public Task ChangeLanguageAsync(CultureInfo language)
        {
            return _preferredSource.ChangeLanguageAsync(language);
        }

        /// <inheritdoc/>
        public Task ChangeLyricsAsync(ILyrics? lyrics)
        {
            var sourceToChange = lyrics?.GetSources().First(x => x.SourceCore == _preferredSource.SourceCore);

            Guard.IsNotNull(sourceToChange, nameof(sourceToChange));

            return _preferredSource.ChangeLyricsAsync(sourceToChange);
        }

        /// <inheritdoc/>
        public Task ChangeIsExplicitAsync(bool isExplicit) => _preferredSource.ChangeIsExplicitAsync(isExplicit);

        /// <inheritdoc/>
        public Task ChangeNameAsync(string name) => _preferredSource.ChangeNameAsync(name);

        /// <inheritdoc/>
        public Task ChangeDescriptionAsync(string? description) => _preferredSource.ChangeDescriptionAsync(description);

        /// <inheritdoc/>
        public Task ChangeDurationAsync(TimeSpan duration) => _preferredSource.ChangeDurationAsync(duration);

        /// <inheritdoc/>
        public Task<bool> IsAddArtistSupported(int index) => _preferredSource.IsAddArtistSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsAddImageSupported(int index) => _preferredSource.IsAddImageSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsAddGenreSupported(int index) => _preferredSource.IsAddGenreSupported(index);

        /// <inheritdoc/>
        public Task<IReadOnlyList<IArtistCollectionItem>> GetArtistItemsAsync(int limit, int offset) => _artistMap.GetItems(limit, offset);

        /// <inheritdoc/>
        public Task AddArtistItemAsync(IArtistCollectionItem artist, int index) => _artistMap.InsertItem(artist, index);

        /// <inheritdoc/>
        public Task RemoveArtistAsync(int index) => _preferredSource.RemoveArtistAsync(index);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as ICoreTrack);

        /// <inheritdoc/>
        public override int GetHashCode() => _preferredSource.Id.GetHashCode();

        /// <inheritdoc />
        public Task<IReadOnlyList<IImage>> GetImagesAsync(int limit, int offset) => _imageCollectionMap.GetItems(limit, offset);

        /// <inheritdoc />
        public Task AddImageAsync(IImage image, int index) => _imageCollectionMap.InsertItem(image, index);

        /// <inheritdoc />
        public Task RemoveImageAsync(int index) => _imageCollectionMap.RemoveAt(index);

        /// <inheritdoc />
        public void AddSource(ICoreTrack itemToMerge)
        {
            Guard.IsNotNull(itemToMerge, nameof(itemToMerge));

            _sources.Add(itemToMerge);
            _sourceCores.Add(itemToMerge.SourceCore);

            _artistMap.AddSource(itemToMerge);
            _imageCollectionMap.AddSource(itemToMerge);
        }

        /// <inheritdoc />
        public void RemoveSource(ICoreTrack itemToRemove)
        {
            Guard.IsNotNull(itemToRemove, nameof(itemToRemove));
            
            _sources.Remove(itemToRemove);
            _sourceCores.Remove(itemToRemove.SourceCore);

            _artistMap.RemoveSource(itemToRemove);
            _imageCollectionMap.RemoveSource(itemToRemove);
        }

        /// <inheritdoc />
        public bool Equals(ICoreTrack? other)
        {
            return other?.Name == Name &&
                   other?.TrackNumber == TrackNumber &&
                   other?.Type == Type &&
                   other?.DiscNumber == DiscNumber &&
                   other?.Duration == Duration &&
                   Album is MergedAlbum album &&
                   !(other?.Album is null) &&
                   album.Equals(other.Album);
        }
    }
}