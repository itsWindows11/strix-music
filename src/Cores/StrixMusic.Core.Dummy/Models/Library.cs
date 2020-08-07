﻿using System;
using System.Collections.Generic;
using System.Linq;
using StrixMusic.Core.Dummy.Implementations;
using StrixMusic.Core.Dummy.Models;
using StrixMusic.CoreInterfaces.Enums;
using StrixMusic.CoreInterfaces.Interfaces;

namespace StrixMusic.Core.Dummy
{
    /// <summary>
    /// The root type for the Library stored in JSON.
    /// </summary>
    public class Library : IPlayableCollectionGroup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Library"/> class.
        /// </summary>
        /// <param name="tracks">The tracks in the Library.</param>
        /// <param name="albums">The albums in the Library.</param>
        /// <param name="artists">The artists in the Library.</param>
        public Library(List<DummyTrack> tracks, List<DummyAlbum> albums, List<DummyArtist> artists, DummyCore core)
        {
            DummyCore = core;
            List<IPlayableCollectionBase> items = new List<IPlayableCollectionBase>();
            items.Add(new TrackCollection(tracks, DummyCore!));
            items.Add(new AlbumCollection(albums, DummyCore!));
            items.Add(new ArtistCollection(artists, DummyCore!));
            Items = items;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IPlayableCollectionBase> Items { get; }

        /// <inheritdoc/>
        public string Id => string.Empty;

        /// <inheritdoc/>
        public string Name => "Library";

        /// <inheritdoc/>
        public IReadOnlyList<IImage> Images => throw new NotImplementedException();

        /// <inheritdoc/>
        public Uri Url => throw new NotImplementedException();

        /// <inheritdoc/>
        public string Description => throw new NotImplementedException();

        /// <inheritdoc/>
        public IUserProfile? Owner => throw new NotImplementedException();

        /// <inheritdoc/>
        public PlaybackState State => throw new NotImplementedException();

        /// <inheritdoc/>
        public ITrack? PlayingTrack => throw new NotImplementedException();

        /// <inheritdoc/>
        public int Count { get => Items?.Count() ?? 0; set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public ICore SourceCore => DummyCore!;

        /// <summary>
        /// The <see cref="DummyCore"/> the library is from.
        /// </summary>
        public DummyCore? DummyCore { get; set; }

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