﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using StrixMusic.CoreInterfaces;
using StrixMusic.CoreInterfaces.Enums;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.Core.Mock.Models
{
    /// <inheritdoc cref="IAlbum"/>
    public class MockArtist : IArtist
    {
        private List<IAlbum> _albums = new List<IAlbum>();
        private List<ITrack> _itracks = new List<ITrack>();
        private List<IImage> _images = new List<IImage>();
        private List<IPlayableCollectionGroup> _relatedItems = new List<IPlayableCollectionGroup>();
        private string _id;
        private Uri _url;
        private string _name;
        private string _description;
        private TimeSpan _duration;
        private PlaybackState _playbackState;
        private TimeSpan _timeSpan;
        private int _totalTracksCount;
        private int _totalAlbumCount;

        /// <inheritdoc cref="IAlbum.Albums"/>
        public IReadOnlyList<IAlbum> Albums => _albums;

        /// <inheritdoc cref="IAlbum.TotalAlbumsCount"/>
        public int TotalAlbumsCount => _totalTracksCount;

        /// <inheritdoc cref="IAlbum.Tracks"/>
        public IReadOnlyList<ITrack> Tracks => _itracks;

        /// <inheritdoc cref="IAlbum.TotalTracksCount"/>
        public int TotalTracksCount => _totalTracksCount;

        /// <inheritdoc cref="IAlbum.SourceCore"/>
        public ICore SourceCore => throw new NotImplementedException();

        /// <inheritdoc cref="IAlbum.Id"/>
        public string Id => _id;

        /// <inheritdoc cref="IAlbum.Url"/>
        public Uri Url => _url;

        /// <inheritdoc cref="IAlbum.Name"/>
        public string Name => _name;

        /// <inheritdoc cref="IAlbum.Images"/>
        public IReadOnlyList<IImage> Images => _images;

        /// <inheritdoc cref="IAlbum.Description"/>
        public string Description => _description;

        /// <inheritdoc cref="IAlbum.PlaybackState"/>
        public PlaybackState PlaybackState => _playbackState;

        /// <inheritdoc cref="IAlbum.Duration"/>
        public TimeSpan Duration => _duration;

        /// <inheritdoc cref="IAlbum.RelatedItems"/>
        public IReadOnlyList<IPlayableCollectionGroup> RelatedItems => _relatedItems;

        /// <inheritdoc cref="IAlbum.TotalRelatedItemsCount"/>
        public int TotalRelatedItemsCount => _totalTracksCount;

        /// <inheritdoc cref="IAlbum.AlbumsChanged"/>
        public event EventHandler<CollectionChangedEventArgs<IAlbum>> AlbumsChanged;

        /// <inheritdoc cref="IAlbum.TracksChanged"/>
        public event EventHandler<CollectionChangedEventArgs<ITrack>> TracksChanged;

        /// <inheritdoc cref="IAlbum.PlaybackStateChanged"/>
        public event EventHandler<PlaybackState> PlaybackStateChanged;

        /// <inheritdoc cref="IAlbum.NameChanged"/>
        public event EventHandler<string> NameChanged;

        /// <inheritdoc cref="IAlbum.DescriptionChanged"/>
        public event EventHandler<string> DescriptionChanged;

        /// <inheritdoc cref="IAlbum.UrlChanged"/>
        public event EventHandler<Uri> UrlChanged;

        /// <inheritdoc cref="IAlbum.ImagesChanged"/>
        public event EventHandler<CollectionChangedEventArgs<IImage>> ImagesChanged;

        /// <inheritdoc cref="IAlbum.RelatedItemsChanged"/>
        public event EventHandler<CollectionChangedEventArgs<IPlayableCollectionGroup>> RelatedItemsChanged;


        /// <inheritdoc cref="IAlbum.PauseAsync"/>
        public Task PauseAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IAlbum.PlayAsync"/>
        public Task PlayAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IAlbum.PopulateAlbumsAsync"/>
        public Task PopulateAlbumsAsync(int limit, int offset = 0)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IAlbum.PopulateRelatedItemsAsync"/>
        public Task PopulateRelatedItemsAsync(int limit, int offset = 0)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc cref="IAlbum.PopulateTracksAsync"/>
        public Task PopulateTracksAsync(int limit, int offset = 0)
        {
            throw new NotImplementedException();
        }
    }
}
