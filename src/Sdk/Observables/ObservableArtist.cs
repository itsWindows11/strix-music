﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.Collections;
using StrixMusic.Sdk.Enums;
using StrixMusic.Sdk.Events;
using StrixMusic.Sdk.Interfaces;

namespace StrixMusic.Sdk.Observables
{
    /// <summary>
    /// Contains bindable information about an <see cref="IArtist"/>.
    /// </summary>
    public class ObservableArtist : ObservableMergeableObject<IArtist>, IArtist
    {
        private readonly IArtist _artist;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableArtist"/> class.
        /// </summary>
        /// <param name="artist">The <see cref="IArtist"/> to wrap.</param>
        public ObservableArtist(IArtist artist)
        {
            _artist = artist;

            SourceCore = MainViewModel.GetLoadedCore(_artist.SourceCore);

            if (_artist.RelatedItems != null)
                RelatedItems = new ObservableCollectionGroup(_artist.RelatedItems);

            Tracks = new SynchronizedObservableCollection<ITrack>();
            Albums = new SynchronizedObservableCollection<IAlbum>();

            PlayAsyncCommand = new AsyncRelayCommand(PlayAsync);
            PauseAsyncCommand = new AsyncRelayCommand(PauseAsync);
            ChangeNameAsyncCommand = new AsyncRelayCommand<string>(ChangeNameAsync);
            ChangeDescriptionAsyncCommand = new AsyncRelayCommand<string?>(ChangeDescriptionAsync);
            ChangeDurationAsyncCommand = new AsyncRelayCommand<TimeSpan>(ChangeDurationAsync);
            PopulateMoreAlbumsCommand = new AsyncRelayCommand<int>(PopulateMoreAlbumsAsync);
            PopulateMoreTracksCommand = new AsyncRelayCommand<int>(PopulateMoreTracksAsync);

            AttachEvents();
        }

        private void AttachEvents()
        {
            _artist.AlbumsChanged += Artist_AlbumsChanged;
            _artist.TracksChanged += Artist_TracksChanged;
            _artist.PlaybackStateChanged += Artist_PlaybackStateChanged;
            _artist.DescriptionChanged += Artist_DescriptionChanged;
            _artist.NameChanged += Artist_NameChanged;
            _artist.UrlChanged += Artist_UrlChanged;
        }

        private void DetachEvents()
        {
            _artist.AlbumsChanged -= Artist_AlbumsChanged;
            _artist.TracksChanged -= Artist_TracksChanged;
            _artist.PlaybackStateChanged -= Artist_PlaybackStateChanged;
            _artist.DescriptionChanged -= Artist_DescriptionChanged;
            _artist.NameChanged -= Artist_NameChanged;
            _artist.UrlChanged -= Artist_UrlChanged;
        }

        /// <inheritdoc />
        public event EventHandler<TimeSpan>? DurationChanged
        {
            add => _artist.DurationChanged += value;

            remove => _artist.DurationChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<PlaybackState>? PlaybackStateChanged
        {
            add => _artist.PlaybackStateChanged += value;

            remove => _artist.PlaybackStateChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string>? NameChanged
        {
            add => _artist.NameChanged += value;

            remove => _artist.NameChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<string?> DescriptionChanged
        {
            add => _artist.DescriptionChanged += value;

            remove => _artist.DescriptionChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<Uri?> UrlChanged
        {
            add => _artist.UrlChanged += value;

            remove => _artist.UrlChanged -= value;
        }

        /// <inheritdoc />
        public event EventHandler<CollectionChangedEventArgs<ITrack>>? TracksChanged
        {
            add => _artist.TracksChanged += value;
            remove => _artist.TracksChanged -= value;
        }

        private void Artist_TracksChanged(object sender, CollectionChangedEventArgs<ITrack> e)
        {
            foreach (var item in e.AddedItems)
            {
                Tracks.Insert(item.Index, item.Data);
            }

            foreach (var item in e.RemovedItems)
            {
                Tracks.RemoveAt(item.Index);
            }
        }

        private void Artist_AlbumsChanged(object sender, CollectionChangedEventArgs<IAlbum> e)
        {
            foreach (var item in e.AddedItems)
            {
                Albums.Insert(item.Index, item.Data);
            }

            foreach (var item in e.RemovedItems)
            {
                Albums.RemoveAt(item.Index);
            }
        }

        /// <inheritdoc />
        public event EventHandler<CollectionChangedEventArgs<IAlbum>>? AlbumsChanged
        {
            add => _artist.AlbumsChanged += value;
            remove => _artist.AlbumsChanged -= value;
        }

        private void Artist_UrlChanged(object sender, Uri? e) => Url = e;

        private void Artist_NameChanged(object sender, string e) => Name = e;

        private void Artist_DescriptionChanged(object sender, string? e) => Description = e;

        private void Artist_PlaybackStateChanged(object sender, PlaybackState e) => PlaybackState = e;

        /// <inheritdoc />
        public ICore SourceCore { get; }

        /// <inheritdoc />
        public string Id => _artist.Id;

        /// <inheritdoc />
        public int TotalAlbumsCount => _artist.TotalAlbumsCount;

        /// <inheritdoc />
        public int TotalTracksCount => _artist.TotalTracksCount;

        /// <inheritdoc cref="IPlayable.Duration" />
        public TimeSpan Duration => _artist.Duration;

        /// <inheritdoc />
        public IPlayableCollectionGroup? RelatedItems { get; }

        /// <summary>
        /// The artist's albums.
        /// </summary>
        public SynchronizedObservableCollection<IAlbum> Albums { get; }

        /// <summary>
        /// The tracks released by this artist.
        /// </summary>
        public SynchronizedObservableCollection<ITrack> Tracks { get; }

        /// <inheritdoc />
        public SynchronizedObservableCollection<IImage> Images => _artist.Images;

        /// <inheritdoc />
        public SynchronizedObservableCollection<string>? Genres => _artist.Genres;

        /// <inheritdoc />
        public string Name
        {
            get => _artist.Name;
            private set => SetProperty(() => _artist.Name, value);
        }

        /// <inheritdoc />
        public Uri? Url
        {
            get => _artist.Url;
            private set => SetProperty(() => _artist.Url, value);
        }

        /// <inheritdoc />
        public string? Description
        {
            get => _artist.Description;
            private set => SetProperty(() => _artist.Description, value);
        }

        /// <inheritdoc />
        public PlaybackState PlaybackState
        {
            get => _artist.PlaybackState;
            private set => SetProperty(() => _artist.PlaybackState, value);
        }

        /// <inheritdoc />
        public bool IsPlayAsyncSupported
        {
            get => _artist.IsPlayAsyncSupported;
            set => SetProperty(() => _artist.IsPlayAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsPauseAsyncSupported
        {
            get => _artist.IsPauseAsyncSupported;
            set => SetProperty(() => _artist.IsPauseAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsChangeNameAsyncSupported
        {
            get => _artist.IsChangeNameAsyncSupported;
            set => SetProperty(() => _artist.IsChangeNameAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsChangeDescriptionAsyncSupported
        {
            get => _artist.IsChangeDescriptionAsyncSupported;
            set => SetProperty(() => _artist.IsChangeDescriptionAsyncSupported, value);
        }

        /// <inheritdoc />
        public bool IsChangeDurationAsyncSupported
        {
            get => _artist.IsChangeDurationAsyncSupported;
            set => SetProperty(() => _artist.IsChangeDurationAsyncSupported, value);
        }

        /// <inheritdoc />
        public Task PlayAsync() => _artist.PlayAsync();

        /// <inheritdoc />
        public Task PauseAsync() => _artist.PauseAsync();

        /// <inheritdoc />
        public Task ChangeNameAsync(string name) => _artist.ChangeNameAsync(name);

        /// <inheritdoc />
        public Task ChangeDescriptionAsync(string? description) => _artist.ChangeDescriptionAsync(description);

        /// <inheritdoc />
        public Task ChangeDurationAsync(TimeSpan duration) => _artist.ChangeDurationAsync(duration);

        /// <inheritdoc />
        public Task<bool> IsAddImageSupported(int index) => _artist.IsAddImageSupported(index);

        /// <inheritdoc />
        public Task<bool> IsAddAlbumSupported(int index) => _artist.IsAddAlbumSupported(index);

        /// <inheritdoc />
        public Task<bool> IsAddTrackSupported(int index) => _artist.IsAddTrackSupported(index);

        /// <inheritdoc />
        public Task<bool> IsAddGenreSupported(int index) => _artist.IsAddGenreSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveImageSupported(int index) => _artist.IsRemoveImageSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveTrackSupported(int index) => _artist.IsRemoveTrackSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveAlbumSupported(int index) => _artist.IsRemoveAlbumSupported(index);

        /// <inheritdoc />
        public Task<bool> IsRemoveGenreSupported(int index) => _artist.IsRemoveGenreSupported(index);

        /// <inheritdoc />
        public IAsyncEnumerable<IAlbum> GetAlbumsAsync(int limit, int offset) => _artist.GetAlbumsAsync(limit, offset);

        /// <inheritdoc />
        public IAsyncEnumerable<ITrack> GetTracksAsync(int limit, int offset) => _artist.GetTracksAsync(limit, offset);

        /// <summary>
        /// Populates the next set of albums into the collection.
        /// </summary>
        /// <param name="limit">The number of items to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task PopulateMoreAlbumsAsync(int limit)
        {
            // TODO
            return Task.CompletedTask;
        }

        /// <summary>
        /// Populates the next set of tracks into the collection.
        /// </summary>
        /// <param name="limit">The number of items to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task PopulateMoreTracksAsync(int limit)
        {
            // TODO
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task AddTrackAsync(IPlayableCollectionGroup track, int index) => _artist.AddTrackAsync(track, index);

        /// <inheritdoc />
        public Task AddAlbumAsync(IAlbum album, int index) => _artist.AddAlbumAsync(album, index);

        /// <inheritdoc />
        public Task RemoveTrackAsync(int index) => _artist.RemoveTrackAsync(index);

        /// <inheritdoc />
        public Task RemoveAlbumAsync(int index) => _artist.RemoveAlbumAsync(index);

        /// <summary>
        /// <inheritdoc cref="PopulateMoreAlbumsAsync"/>
        /// </summary>
        public IAsyncRelayCommand<int> PopulateMoreAlbumsCommand { get; }

        /// <summary>
        /// <inheritdoc cref="PopulateMoreTracksAsync"/>
        /// </summary>
        public IAsyncRelayCommand<int> PopulateMoreTracksCommand { get; }

        /// <summary>
        /// Attempts to play the artist.
        /// </summary>
        public IAsyncRelayCommand PlayAsyncCommand { get; }

        /// <summary>
        /// Attempts to pause the artist, if playing.
        /// </summary>
        public IAsyncRelayCommand PauseAsyncCommand { get; }

        /// <summary>
        /// Attempts to change the name of the artist, if supported.
        /// </summary>
        public IAsyncRelayCommand ChangeNameAsyncCommand { get; }

        /// <summary>
        /// Attempts to change the description of the artist, if supported.
        /// </summary>
        public IAsyncRelayCommand ChangeDescriptionAsyncCommand { get; }

        /// <summary>
        /// Attempts to change the duration of the artist, if supported.
        /// </summary>
        public IAsyncRelayCommand ChangeDurationAsyncCommand { get; }
    }
}
