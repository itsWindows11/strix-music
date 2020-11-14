﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OwlCore.Collections;
using OwlCore.Extensions;
using StrixMusic.Sdk.Data.Base;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Sdk.Data.Merged
{
    /// <summary>
    /// Merged multiple <see cref="ICoreAlbum"/> into a single <see cref="IAlbum"/>
    /// </summary>
    public sealed class MergedAlbum : IAlbum, IMerged<ICoreAlbum>
    {
        private readonly ICoreAlbum _preferredSource;
        private readonly List<ICoreAlbum> _sources;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergedImage"/> class.
        /// </summary>
        public MergedAlbum(IEnumerable<ICoreAlbum> sources)
        {
            if (sources == null)
                throw new ArgumentNullException(nameof(sources));

            var coreAlbums = sources as ICoreAlbum[] ?? sources.ToArray();
            _sources = coreAlbums.ToList();

            Images = new SynchronizedObservableCollection<IImage>();

            Artist = new MergedArtist(_sources.Select(x => x.Artist).ToList());

            RelatedItems = new MergedPlayableCollectionGroup(coreAlbums.Select(x => x.RelatedItems).PruneNull().ToList());

            // TODO: Get the actual preferred source.
            _preferredSource = _sources[0];
        }

        /// <inheritdoc cref="ISdkMember{T}.SourceCores" />
        public IReadOnlyList<ICore> SourceCores => Sources.Select(x => x.SourceCore).ToList();

        /// <inheritdoc/>
        public IReadOnlyList<ICoreAlbum> Sources => _sources;

        /// <inheritdoc/>
        public IArtist Artist { get; }

        /// <inheritdoc/>
        public IPlayableCollectionGroup? RelatedItems { get; }

        /// <inheritdoc/>
        public DateTime? DatePublished => _preferredSource.DatePublished;

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
        public int TotalTracksCount => Sources.Sum(x => x.TotalTracksCount);

        /// <inheritdoc/>
        public string Id => _preferredSource.Id;

        /// <inheritdoc/>
        public string Name => _preferredSource.Name;

        /// <inheritdoc/>
        public string? Description => _preferredSource.Description;

        /// <inheritdoc/>
        public PlaybackState PlaybackState => _preferredSource.PlaybackState;

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
        public SynchronizedObservableCollection<IImage> Images { get; }

        /// <inheritdoc/>
        public Uri? Url => _preferredSource.Url;

        /// <inheritdoc />
        public event EventHandler<DateTime?> DatePublishedChanged
        {
            add => _preferredSource.DatePublishedChanged += value;
            remove => _preferredSource.DatePublishedChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string> NameChanged
        {
            add => _preferredSource.NameChanged += value;
            remove => _preferredSource.NameChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string?> DescriptionChanged
        {
            add => _preferredSource.DescriptionChanged += value;
            remove => _preferredSource.DescriptionChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<PlaybackState> PlaybackStateChanged
        {
            add => _preferredSource.PlaybackStateChanged += value;
            remove => _preferredSource.PlaybackStateChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<Uri?> UrlChanged
        {
            add => _preferredSource.UrlChanged += value;
            remove => _preferredSource.UrlChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<TimeSpan>? DurationChanged
        {
            add => _preferredSource.DurationChanged += value;
            remove => _preferredSource.DurationChanged -= value;
        }

        /// <inheritdoc/>
        public void AddSource(ICoreAlbum itemToMerge)
        {
            _sources.Add(itemToMerge);
        }

        /// <inheritdoc/>
        public bool Equals(ICoreAlbum other)
        {
            return other?.Name == Name;
        }

        /// <inheritdoc/>
        public Task RemoveTrackAsync(int index) => _preferredSource.RemoveTrackAsync(index);

        /// <inheritdoc/>
        public Task AddTrackAsync(ITrack track, int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public event EventHandler<int>? TrackItemsCountChanged;

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

        /// <inheritdoc/>
        Task IPlayable.ChangeNameAsync(string name)
        {
            return _preferredSource.ChangeNameAsync(name);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ITrack>> GetTracksAsync(int limit, int offset)
        {
            var tracks = new List<ICoreTrack>();
            var trackCollections = new List<ICoreTrackCollection>();

            // Using one source for now.
            await foreach (var item in _preferredSource.GetTracksAsync(limit, offset))
            {
                if (item is ICoreTrack track)
                    tracks.Add(track);

                if (item is ICoreTrackCollection collection)
                    trackCollections.Add(collection);
            }

            // Turn each item into an Sdk Member
            var mergedTracks = tracks.Select(x => new MergedTrack(new ICoreTrack[] { x }));
            return mergedTracks.ToList();
        }

        /// <inheritdoc/>
        public Task<bool> IsAddGenreSupported(int index) => _preferredSource.IsAddGenreSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsAddImageSupported(int index) => _preferredSource.IsAddImageSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsAddTrackSupported(int index) => _preferredSource.IsAddTrackSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveGenreSupported(int index) => _preferredSource.IsRemoveGenreSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveImageSupported(int index) => _preferredSource.IsRemoveImageSupported(index);

        /// <inheritdoc/>
        public Task<bool> IsRemoveTrackSupported(int index) => _preferredSource.IsRemoveTrackSupported(index);

        /// <inheritdoc/>
        public Task PauseAsync() => _preferredSource.PauseAsync();

        /// <inheritdoc/>
        Task IPlayable.PlayAsync() => _preferredSource.PlayAsync();
    }
}
