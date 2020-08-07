﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StrixMusic.CoreInterfaces.Enums;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.Core.Dummy.Implementations
{
    /// <inheritdoc/>
    public class DummyArtist : IArtist
    {
        /// <inheritdoc/>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <inheritdoc/>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// List of full <see cref="DummyAlbum"/>s to be used within the DummyCore.
        /// </summary>
        public List<DummyAlbum> DummyAlbums { get; set; } = new List<DummyAlbum>();

        /// <summary>
        /// List of the Ids of <see cref="DummyAlbum"/>s to the <see cref="DummyArtist"/>
        /// </summary>
        [JsonProperty("album_ids")]
        public List<string>? AlbumIds { get; set; }

        /// <inheritdoc/>
        public IReadOnlyList<IImage> Images => throw new NotImplementedException();

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
        public ICore SourceCore => DummyCore!;

        /// <summary>
        /// The <see cref="DummyCore"/> where the <see cref="DummyArtist"/> is from.
        /// </summary>
        public DummyCore? DummyCore { get; set; }

        public IReadOnlyList<ITrack> TopTracks => throw new NotImplementedException();

        public IReadOnlyList<IArtist> RelatedArtists => throw new NotImplementedException();

        /// <inheritdoc/>
        public IReadOnlyList<IAlbum> Albums => DummyAlbums!;

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
