﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StrixMusic.CoreInterfaces.Enums;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.Core.Dummy.Implementations
{
    /// <inheritdoc/>
    public class DummyAlbum : IAlbum
    {
        /// <inheritdoc/>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <inheritdoc/>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public IList<ITrack> Tracks => (IList<ITrack>)DummyTracks;

        /// <summary>
        /// List of full <see cref="DummyTrack"/>s to be used within the DummyCore.
        /// </summary>
        [JsonIgnore]
        public List<DummyTrack> DummyTracks { get; set; } = new List<DummyTrack>();

        /// <summary>
        /// List of the Ids of <see cref="DummyTrack"/>s on the <see cref="DummyAlbum"/>.
        /// </summary>
        [JsonProperty("track_ids")]
        public IEnumerable<string> TrackIds { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public IArtist Artist => DummyArtist;

        /// <summary>
        /// The full <see cref="DummyArtist"/> of the album.
        /// </summary>
        [JsonIgnore]
        public DummyArtist DummyArtist { get; set; }

        /// <summary>
        /// The Id of the album's artist.
        /// </summary>
        [JsonProperty("artist_id")]
        public string ArtistId { get; set; }

        /// <inheritdoc/>
        public IList<IImage> Images => throw new NotImplementedException();

        /// <inheritdoc/>
        public Uri Url => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Description => throw new NotImplementedException();

        /// <inheritdoc/>
        public IUserProfile Owner => throw new NotImplementedException();

        /// <inheritdoc/>
        public PlaybackState State => throw new NotImplementedException();

        /// <inheritdoc/>
        public ITrack PlayingTrack => throw new NotImplementedException();

        /// <inheritdoc/>
        public ICore SourceCore { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public void Play()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            throw new NotImplementedException();
        }
    }
}