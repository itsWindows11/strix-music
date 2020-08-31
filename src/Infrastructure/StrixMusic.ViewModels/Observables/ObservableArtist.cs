﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.Input;
using StrixMusic.CoreInterfaces;
using StrixMusic.CoreInterfaces.Enums;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.ViewModels.Bindables
{
    /// <summary>
    /// Contains bindable information about an <see cref="IArtist"/>.
    /// </summary>
    public class ObservableArtist : ObservableMergeableObject<IArtist>
    {
        private readonly IArtist _artist;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableArtist"/> class.
        /// </summary>
        /// <param name="artist">The <see cref="IArtist"/> to wrap.</param>
        public ObservableArtist(IArtist artist)
        {
            _artist = artist;

            SourceCore = new ObservableCore(_artist.SourceCore);

            PlayAsyncCommand = new AsyncRelayCommand(PlayAsync);
            PauseAsyncCommand = new AsyncRelayCommand(PauseAsync);

            Tracks = new ObservableCollection<ObservableTrack>(_artist.Tracks.Select(x => new ObservableTrack(x)));
            Albums = new ObservableCollection<ObservableAlbum>(_artist.Albums.Select(x => new ObservableAlbum(x)));
            Images = new ObservableCollection<IImage>(_artist.Images);
            RelatedItems = new ObservableCollectionGroup(_artist.RelatedItems);

            AttachEvents();
        }

        private void AttachEvents()
        {
            _artist.PlaybackStateChanged += Artist_PlaybackStateChanged;
            _artist.TracksChanged += Artist_RelatedTracksChanged;
            _artist.AlbumsChanged += Artist_AlbumsChanged;
            _artist.DescriptionChanged += Artist_DescriptionChanged;
            _artist.NameChanged += Artist_NameChanged;
            _artist.UrlChanged += Artist_UrlChanged;
            _artist.ImagesChanged += Artist_ImagesChanged;
        }

        private void DetachEvents()
        {
            _artist.PlaybackStateChanged -= Artist_PlaybackStateChanged;
            _artist.TracksChanged -= Artist_RelatedTracksChanged;
            _artist.AlbumsChanged -= Artist_AlbumsChanged;
            _artist.DescriptionChanged -= Artist_DescriptionChanged;
            _artist.NameChanged -= Artist_NameChanged;
            _artist.UrlChanged -= Artist_UrlChanged;
            _artist.ImagesChanged -= Artist_ImagesChanged;
        }

        private void Artist_ImagesChanged(object sender, CollectionChangedEventArgs<IImage> e)
        {
            foreach (var item in e.RemovedItems)
            {
                Images.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                Images.Add(item);
            }
        }

        private void Artist_UrlChanged(object sender, Uri? e)
        {
            Url = e;
        }

        private void Artist_NameChanged(object sender, string e)
        {
            Name = e;
        }

        private void Artist_DescriptionChanged(object sender, string? e)
        {
            Description = e;
        }

        private void Artist_AlbumsChanged(object sender, CollectionChangedEventArgs<IAlbum> e)
        {
            foreach (var item in e.RemovedItems)
            {
                Albums.Remove(new ObservableAlbum(item));
            }

            foreach (var item in e.AddedItems)
            {
                Albums.Add(new ObservableAlbum(item));
            }
        }

        private void Artist_RelatedTracksChanged(object sender, CollectionChangedEventArgs<ITrack> e)
        {
            foreach (var item in e.RemovedItems)
            {
                Tracks.Remove(new ObservableTrack(item));
            }

            foreach (var item in e.AddedItems)
            {
                Tracks.Add(new ObservableTrack(item));
            }
        }

        private void Artist_PlaybackStateChanged(object sender, PlaybackState e)
        {
            PlaybackState = e;
        }

        /// <inheritdoc cref="IAlbumCollection.Albums"/>
        public ObservableCollection<ObservableAlbum> Albums { get; }

        /// <inheritdoc cref="IAlbumCollection.TotalAlbumsCount"/>
        public int TotalAlbumsCount => _artist.TotalAlbumsCount;

        /// <inheritdoc cref="ITrackCollection.Tracks"/>
        public ObservableCollection<ObservableTrack> Tracks { get; }

        /// <inheritdoc cref="ITrackCollection.TotalTracksCount"/>
        public int TotalTracksCount => _artist.TotalTracksCount;

        /// <inheritdoc cref="IPlayable.Url"/>
        public Uri? Url
        {
            get => _artist.Url;
            private set => SetProperty(() => _artist.Url, value);
        }

        /// <inheritdoc cref="IPlayable.SourceCore"/>
        public ObservableCore SourceCore { get; }

        /// <inheritdoc cref="IPlayable.Id"/>
        public string Id => _artist.Id;

        /// <inheritdoc cref="IPlayable.Name"/>
        public string Name
        {
            get => _artist.Name;
            private set => SetProperty(() => _artist.Name, value);
        }

        /// <inheritdoc cref="IPlayable.Images"/>
        public ObservableCollection<IImage> Images { get; }

        /// <inheritdoc cref="IPlayable.Description"/>
        public string? Description
        {
            get => _artist.Description;
            private set => SetProperty(() => _artist.Description, value);
        }

        /// <inheritdoc cref="IPlayable.PlaybackState"/>
        public PlaybackState PlaybackState
        {
            get => _artist.PlaybackState;
            private set => SetProperty(() => _artist.PlaybackState, value);
        }

        /// <inheritdoc cref="IAlbumCollection.AlbumsChanged"/>
        public event EventHandler<CollectionChangedEventArgs<IAlbum>>? AlbumsChanged
        {
            add
            {
                _artist.AlbumsChanged += value;
            }

            remove
            {
                _artist.AlbumsChanged -= value;
            }
        }

        /// <inheritdoc cref="ITrackCollection.TracksChanged"/>
        public event EventHandler<CollectionChangedEventArgs<ITrack>>? TracksChanged
        {
            add
            {
                _artist.TracksChanged += value;
            }

            remove
            {
                _artist.TracksChanged -= value;
            }
        }

        /// <inheritdoc cref="IPlayable.PlaybackStateChanged"/>
        public event EventHandler<PlaybackState>? PlaybackStateChanged
        {
            add
            {
                _artist.PlaybackStateChanged += value;
            }

            remove
            {
                _artist.PlaybackStateChanged -= value;
            }
        }

        /// <inheritdoc cref="IPlayable.NameChanged"/>
        public event EventHandler<string>? NameChanged
        {
            add
            {
                _artist.NameChanged += value;
            }

            remove
            {
                _artist.NameChanged -= value;
            }
        }

        /// <inheritdoc cref="IPlayable.DescriptionChanged"/>
        public event EventHandler<string?> DescriptionChanged
        {
            add
            {
                _artist.DescriptionChanged += value;
            }

            remove
            {
                _artist.DescriptionChanged -= value;
            }
        }

        /// <inheritdoc cref="IPlayable.UrlChanged"/>
        public event EventHandler<Uri?> UrlChanged
        {
            add
            {
                _artist.UrlChanged += value;
            }

            remove
            {
                _artist.UrlChanged -= value;
            }
        }

        /// <inheritdoc cref="IPlayable.Images"/>
        public event EventHandler<CollectionChangedEventArgs<IImage>>? ImagesChanged
        {
            add
            {
                _artist.ImagesChanged += value;
            }

            remove
            {
                _artist.ImagesChanged -= value;
            }
        }

        /// <summary>
        /// Attempts to pause the artist, if playing.
        /// </summary>
        public IAsyncRelayCommand PauseAsyncCommand { get; }

        /// <inheritdoc cref="IPlayable.PauseAsync"/>
        public Task PauseAsync()
        {
            return _artist.PauseAsync();
        }

        /// <summary>
        /// Attempts to play the artist.
        /// </summary>
        public IAsyncRelayCommand PlayAsyncCommand { get; }

        /// <inheritdoc cref="IPlayable.Duration" />
        public TimeSpan Duration => _artist.Duration;

        /// <inheritdoc cref="IRelatedCollectionGroups.RelatedItems"/>
        public ObservableCollectionGroup RelatedItems { get; }

        /// <inheritdoc cref="IPlayable.PlayAsync"/>
        public Task PlayAsync()
        {
            return _artist.PlayAsync();
        }

        /// <inheritdoc cref="IAlbumCollection.PopulateAlbumsAsync(int, int)"/>
        public Task<IReadOnlyList<IAlbum>> PopulateAlbumsAsync(int limit, int offset = 0)
        {
            return _artist.PopulateAlbumsAsync(limit, offset);
        }

        /// <inheritdoc cref="ITrackCollection.PopulateTracksAsync(int, int)"/>
        public Task<IReadOnlyList<ITrack>> PopulateTracksAsync(int limit, int offset = 0)
        {
            return _artist.PopulateTracksAsync(limit, offset);
        }
    }
}