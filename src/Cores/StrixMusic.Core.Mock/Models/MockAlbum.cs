﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StrixMusic.CoreInterfaces;
using StrixMusic.CoreInterfaces.Enums;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.Core.Mock.Models
{
    /// <inheritdoc cref="IAlbum"/>
    public class MockAlbum : IAlbum
    {
        private IReadOnlyList<ITrack> _track;

        /// <inheritdoc/>
        [JsonProperty("id")]
        public string IdJson { get; set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty("name")]
        public string NameJson { get; set; } = string.Empty;

        /// <inheritdoc/>
        [JsonIgnore]
        public List<ITrack> TracksJson { get; set; } = new List<ITrack>();

        /// <inheritdoc/>
        [JsonProperty("track_ids")]
        public IEnumerable<string>? TrackIds { get; set; }

        /// <inheritdoc/>
        public IArtist Artist => throw new NotImplementedException();

        /// <inheritdoc/>
        public int TotalTracksCount => throw new NotImplementedException();

        /// <inheritdoc/>
        public ICore SourceCore => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Id => IdJson;

        /// <inheritdoc/>
        public Uri Url => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Name => NameJson;

        /// <inheritdoc/>
        public IReadOnlyList<IImage> Images => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Description => throw new NotImplementedException();

        /// <inheritdoc/>
        public PlaybackState PlaybackState => throw new NotImplementedException();

        /// <inheritdoc/>
        public TimeSpan Duration => throw new NotImplementedException();

        /// <inheritdoc/>
        public IReadOnlyList<ITrack> Tracks => throw new NotImplementedException();

        /// <inheritdoc/>
        public IPlayableCollectionGroup RelatedItems => throw new NotImplementedException();

        /// <inheritdoc/>
        public event EventHandler<CollectionChangedEventArgs<ITrack>> TracksChanged;

        /// <inheritdoc/>
        public event EventHandler<PlaybackState> PlaybackStateChanged;

        /// <inheritdoc/>
        public event EventHandler<string> NameChanged;

        /// <inheritdoc/>
        public event EventHandler<string> DescriptionChanged;

        /// <inheritdoc/>
        public event EventHandler<Uri> UrlChanged;

        /// <inheritdoc/>
        public event EventHandler<CollectionChangedEventArgs<IImage>> ImagesChanged;

        /// <inheritdoc/>
        public Task PauseAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task PlayAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<ITrack>> PopulateTracksAsync(int limit, int offset)
        {
            _track = TracksJson;
            return Task.FromResult(_track);
        }
    }
}