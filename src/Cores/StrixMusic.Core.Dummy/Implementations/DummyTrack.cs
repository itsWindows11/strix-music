﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.Core.Dummy.Implementations
{
    /// <inheritdoc/>
    public class DummyTrack : ITrack
    {
        /// <inheritdoc/>
        [JsonProperty("id")]
        public string Id { get; }

        /// <inheritdoc/>
        public List<IImage> Images => throw new NotImplementedException();

        /// <inheritdoc/>
        public Uri Url => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Type => "song";

        /// <inheritdoc/>
        [JsonProperty("title")]
        public string Title { get; }

        /// <inheritdoc/>
        public IArtist Artist => DummyArtist;

        /// <summary>
        /// Full <see cref="DummyArtist"/> to be used within the DummyCore.
        /// </summary>
        public DummyArtist DummyArtist { get; }

        /// <summary>
        /// The Id of the <see cref="DummyArtist"/>.
        /// </summary>
        [JsonProperty("artist_id")]
        public string ArtistId { get; }

        /// <inheritdoc/>
        public IAlbum Album => DummyAlbum;

        /// <summary>
        /// The Id of the <see cref="DummyAlbum"/>.
        /// </summary>
        [JsonProperty("album_id")]
        public string AlbumId { get; set; }

        /// <summary>
        /// Full <see cref="DummyAlbum"/> to be used within the DummyCore.
        /// </summary>
        public DummyAlbum DummyAlbum { get; }

        /// <inheritdoc/>
        public DateTime? DatePublished => throw new NotImplementedException();

        /// <inheritdoc/>
        public IList<string> Genre => throw new NotImplementedException();

        /// <inheritdoc/>
        public int? TrackNumber => throw new NotImplementedException();

        /// <inheritdoc/>
        public int PlayCount => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Language => throw new NotImplementedException();

        /// <inheritdoc/>
        public ILyrics Lyrics => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool IsExplicit { get; }

        /// <inheritdoc/>
        public long Duration { get; }

        /// <inheritdoc/>
        public string Description => throw new NotImplementedException();

        /// <inheritdoc/>
        public IDevice Device { get; private set; }

        /// <inheritdoc/>
        public ICore SourceCore { get; }
    }
}