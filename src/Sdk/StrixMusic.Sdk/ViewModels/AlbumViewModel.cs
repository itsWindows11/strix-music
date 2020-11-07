﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.Collections;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions.SdkMember;
using StrixMusic.Sdk.MediaPlayback;
using StrixMusic.Sdk.MediaPlayback.LocalDevice;
using StrixMusic.Sdk.Services.MediaPlayback;

namespace StrixMusic.Sdk.ViewModels
{
    /// <summary>
    /// Contains bindable information about an <see cref="IAlbum"/>.
    /// </summary>
    public class AlbumViewModel : MergeableObjectViewModel<IAlbum>, IAlbum, ITrackCollectionViewModel
    {
        private readonly IAlbum _album;
        private readonly IPlaybackHandlerService _playbackHandler;
        private ArtistViewModel _artist; // TODO: Expose this field from readonly property

        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumViewModel"/> class.
        /// </summary>
        /// <param name="album"><inheritdoc cref="IAlbum"/></param>
        public AlbumViewModel(IAlbum album)
        {
            _album = album;

            SourceCores = _album.GetSourceCores<ICoreAlbum>().Select(MainViewModel.GetLoadedCore).ToList();

            _playbackHandler = Ioc.Default.GetService<IPlaybackHandlerService>();

            Images = new SynchronizedObservableCollection<IImage>(_album.Images);
            Tracks = new SynchronizedObservableCollection<TrackViewModel>();

            if (_album.RelatedItems != null)
                RelatedItems = new PlayableCollectionGroupViewModel(_album.RelatedItems);

            _artist = new ArtistViewModel(_album.Artist);

            PauseAsyncCommand = new AsyncRelayCommand(PauseAsync);
            PlayAsyncCommand = new AsyncRelayCommand(PlayAsync);
            ChangeNameAsyncCommand = new AsyncRelayCommand<string>(ChangeNameAsync);
            ChangeDescriptionAsyncCommand = new AsyncRelayCommand<string?>(ChangeDescriptionAsync);
            ChangeDurationAsyncCommand = new AsyncRelayCommand<TimeSpan>(ChangeDurationAsync);
            PopulateMoreTracksCommand = new AsyncRelayCommand<int>(PopulateMoreTracksAsync);

            AttachEvents();
        }

        private void AttachEvents()
        {
            _album.PlaybackStateChanged += AlbumPlaybackStateChanged;
            _album.DescriptionChanged += AlbumDescriptionChanged;
            _album.DatePublishedChanged += AlbumDatePublishedChanged;
            _album.NameChanged += AlbumNameChanged;
            _album.UrlChanged += AlbumUrlChanged;
            _album.TrackItemsCountChanged += AlbumOnTrackItemsCountChanged;
        }

        private void DetachEvents()
        {
            _album.PlaybackStateChanged -= AlbumPlaybackStateChanged;
            _album.DescriptionChanged -= AlbumDescriptionChanged;
            _album.DatePublishedChanged -= AlbumDatePublishedChanged;
            _album.NameChanged -= AlbumNameChanged;
            _album.UrlChanged -= AlbumUrlChanged;
        }

        /// <inheritdoc />
        public event EventHandler<PlaybackState>? PlaybackStateChanged
        {
            add => _album.PlaybackStateChanged += value;

            remove => _album.PlaybackStateChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string>? NameChanged
        {
            add => _album.NameChanged += value;

            remove => _album.NameChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string?> DescriptionChanged
        {
            add => _album.DescriptionChanged += value;

            remove => _album.DescriptionChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<Uri?> UrlChanged
        {
            add => _album.UrlChanged += value;

            remove => _album.UrlChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<TimeSpan>? DurationChanged
        {
            add => _album.DurationChanged += value;

            remove => _album.DurationChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<DateTime?> DatePublishedChanged
        {
            add => _album.DatePublishedChanged += value;

            remove => _album.DatePublishedChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<int> TrackItemsCountChanged
        {
            add => _album.TrackItemsCountChanged += value;

            remove => _album.TrackItemsCountChanged -= value;
        }

        private void AlbumUrlChanged(object sender, Uri? e) => Url = e;

        private void AlbumNameChanged(object sender, string e) => Name = e;

        private void AlbumDescriptionChanged(object sender, string? e) => Description = e;

        private void AlbumPlaybackStateChanged(object sender, PlaybackState e) => PlaybackState = e;

        private void AlbumDatePublishedChanged(object sender, DateTime? e) => DatePublished = e;

        private void AlbumOnTrackItemsCountChanged(object sender, int e) => TotalTracksCount = e;

        /// <inheritdoc />
        public string Id => _album.Id;

        /// <inheritdoc cref="ISdkMember{T}.SourceCores" />
        public IReadOnlyList<ICore> SourceCores { get; }

        /// <summary>
        /// The merged sources for this album.
        /// </summary>
        public IReadOnlyList<ICoreAlbum> Sources => _album.GetSources<ICoreAlbum>();

        /// <summary>
        /// The merged sources for this album.
        /// </summary>
        IReadOnlyList<ICoreAlbum> ISdkMember<ICoreAlbum>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreImageCollection> ISdkMember<ICoreImageCollection>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreTrackCollection> ISdkMember<ICoreTrackCollection>.Sources => Sources;

        /// <inheritdoc />
        IReadOnlyList<ICoreAlbumCollectionItem> ISdkMember<ICoreAlbumCollectionItem>.Sources => Sources;

        /// <inheritdoc />
        public TimeSpan Duration => _album.Duration;

        /// <inheritdoc />
        public IPlayableCollectionGroup? RelatedItems { get; }

        /// <inheritdoc />
        public SynchronizedObservableCollection<IImage> Images { get; }

        /// <inheritdoc />
        public SynchronizedObservableCollection<string>? Genres => _album.Genres;

        /// <summary>
        /// The tracks for this album.
        /// </summary>
        public SynchronizedObservableCollection<TrackViewModel> Tracks { get; }

        /// <inheritdoc />
        public string Name
        {
            get => _album.Name;
            private set => SetProperty(() => _album.Name, value);
        }

        /// <inheritdoc />
        public int TotalTracksCount
        {
            get => _album.TotalTracksCount;
            private set => SetProperty(() => _album.TotalTracksCount, value);
        }

        /// <inheritdoc cref="IAlbum.Artist" />
        public ArtistViewModel Artist
        {
            get => _artist;
            set => SetProperty(ref _artist, new ArtistViewModel(value));
        }

        /// <inheritdoc />
        IArtist IAlbum.Artist => Artist;

        /// <inheritdoc />
        public Uri? Url
        {
            get => _album.Url;
            private set => SetProperty(() => _album.Url, value);
        }

        /// <inheritdoc />
        public DateTime? DatePublished
        {
            get => _album.DatePublished;
            set => SetProperty(() => _album.DatePublished, value);
        }

        /// <inheritdoc />
        public string? Description
        {
            get => _album.Description;
            private set => SetProperty(() => _album.Description, value);
        }

        /// <inheritdoc />
        public PlaybackState PlaybackState
        {
            get => _album.PlaybackState;
            private set => SetProperty(() => _album.PlaybackState, value);
        }

        /// <inheritdoc />
        public bool IsPlayAsyncSupported
        {
            get => _album.IsPlayAsyncSupported;
            set => SetProperty(() => _album.IsPlayAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsPauseAsyncSupported
        {
            get => _album.IsPauseAsyncSupported;
            set => SetProperty(() => _album.IsPauseAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsChangeNameAsyncSupported
        {
            get => _album.IsChangeNameAsyncSupported;
            set => SetProperty(() => _album.IsChangeNameAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsChangeDescriptionAsyncSupported
        {
            get => _album.IsChangeDescriptionAsyncSupported;
            set => SetProperty(() => _album.IsChangeDescriptionAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsChangeDatePublishedAsyncSupported
        {
            get => _album.IsChangeDatePublishedAsyncSupported;
            set => SetProperty(() => _album.IsChangeDatePublishedAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsChangeDurationAsyncSupported
        {
            get => _album.IsChangeDurationAsyncSupported;
            set => SetProperty(() => _album.IsChangeDurationAsyncSupported, value);
        }

        /// <inheritdoc />
        public Task PauseAsync() => _album.PauseAsync();

        /// <inheritdoc />
        public async Task PlayAsync()
        {
            if (MainViewModel.Singleton?.ActiveDevice?.Type == DeviceType.Remote)
            {
                await _album.PlayAsync();
                return;
            }

            if (_playbackHandler.PlaybackState == PlaybackState.Paused)
            {
                await _playbackHandler.ResumeAsync();
            }
            else
            {
                await _playbackHandler.PauseAsync();

                // The actual playback
                _playbackHandler.ClearNext();
                _playbackHandler.ClearPrevious();

                // Make sure we have all tracks
                while (Tracks.Count < TotalTracksCount)
                {
                    await PopulateMoreTracksAsync(100);
                }

                // Insert the items into queue
                foreach (var item in Tracks)
                {
                    var index = Tracks.IndexOf(item);

                    // TODO: Use core of active device.

                    var mediaSource = await SourceCores[0].GetMediaSource(item.Model.GetSources<ICoreTrack>()[0]);

                    if (mediaSource is null)
                        continue;

                    _playbackHandler.InsertNext(index, mediaSource);
                }

                if (MainViewModel.Singleton?.LocalDevice?.Model is StrixDevice device)
                {
                    device.SetPlaybackData(_album, Tracks[0].Model);
                }

                // Play the first thing in the album.
                await _playbackHandler.PlayFromNext(0);
            }
        }

        /// <inheritdoc />
        public Task ChangeNameAsync(string name) => _album.ChangeNameAsync(name);

        /// <inheritdoc />
        public Task ChangeDescriptionAsync(string? description) => _album.ChangeDescriptionAsync(description);

        /// <inheritdoc />
        public Task ChangeDurationAsync(TimeSpan duration) => _album.ChangeDurationAsync(duration);

        /// <inheritdoc />
        public Task<bool> IsAddImageSupported(int index) => _album.IsAddImageSupported(index);

        /// <inheritdoc />
        public Task<bool> IsAddTrackSupported(int index) => _album.IsAddTrackSupported(index);

        /// <inheritdoc />
        public Task<bool> IsAddGenreSupported(int index) => _album.IsAddGenreSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveImageSupported(int index) => _album.IsRemoveImageSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveGenreSupported(int index) => _album.IsRemoveGenreSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveTrackSupported(int index) => _album.IsRemoveTrackSupported(index);

        /// <inheritdoc />
        public Task ChangeDatePublishedAsync(DateTime datePublished) => _album.ChangeDatePublishedAsync(datePublished);

        /// <inheritdoc />
        public Task<IReadOnlyList<ITrack>> GetTracksAsync(int limit, int offset) => _album.GetTracksAsync(limit, offset);

        /// <inheritdoc />
        public async Task PopulateMoreTracksAsync(int limit)
        {
            foreach (var item in await _album.GetTracksAsync(limit, Tracks.Count))
            {
                Tracks.Add(new TrackViewModel(item));
            }
        }

        /// <inheritdoc />
        public Task AddTrackAsync(ITrack track, int index)
        {
            return _album.AddTrackAsync(track, index);
        }

        /// <inheritdoc />
        public Task RemoveTrackAsync(int index)
        {
            return _album.RemoveTrackAsync(index);
        }

        /// <inheritdoc />
        public IAsyncRelayCommand<int> PopulateMoreTracksCommand { get; }

        /// <summary>
        /// Attempts to play the album.
        /// </summary>
        public IAsyncRelayCommand PlayAsyncCommand { get; }

        /// <summary>
        /// Attempts to pause the album, if playing.
        /// </summary>
        public IAsyncRelayCommand PauseAsyncCommand { get; }

        /// <summary>
        /// Attempts to change the name of the album, if supported.
        /// </summary>
        public IAsyncRelayCommand ChangeNameAsyncCommand { get; }

        /// <summary>
        /// Attempts to change the description of the album, if supported.
        /// </summary>
        public IAsyncRelayCommand ChangeDescriptionAsyncCommand { get; }

        /// <summary>
        /// Attempts to change the duration of the album, if supported.
        /// </summary>
        public IAsyncRelayCommand ChangeDurationAsyncCommand { get; }
    }
}
