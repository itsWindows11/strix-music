﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using StrixMusic.Core.Dummy.Implementations;

namespace StrixMusic.Core.Dummy.Deserialization
{
    /// <summary>
    /// Deserializes the JSON library, then hooks references up by Ids.
    /// </summary>
    public static class Deserializer
    {
        /// <summary>
        /// Deserializes and sets up the DummyCore <see cref="SerializedLibrary"/>.
        /// </summary>
        /// <returns>The <see cref="SerializedLibrary"/> of the DummyCore.</returns>
        public static SerializedLibrary DeserializeLibrary()
        {
            string resource = GetManifestResourceString(Assembly.GetExecutingAssembly(), "StrixMusic.Core.Dummy.Library.json");
            SerializedLibrary lib = JsonConvert.DeserializeObject<SerializedLibrary>(resource);
            return GraphLibrary(lib);
        }

        private static SerializedLibrary GraphLibrary(SerializedLibrary library)
        {
            Dictionary<string, DummyArtist> artists = new Dictionary<string, DummyArtist>();
            Dictionary<string, DummyAlbum> albums = new Dictionary<string, DummyAlbum>();
            Dictionary<string, DummyTrack> tracks = new Dictionary<string, DummyTrack>();

            foreach (var artist in library.Artists!)
            {
                artists.Add(artist.Id, artist);
            }

            foreach (var album in library.Albums!)
            {
                albums.Add(album.Id, album);
                album.DummyArtist = artists[album.ArtistId];
                album.DummyArtist.DummyAlbums.Add(album);
            }

            foreach (var track in library.Tracks!)
            {
                tracks.Add(track.Id, track);
                track.DummyAlbum = albums[track.AlbumId];
                track.DummyArtist = artists[track.ArtistId];
            }

            foreach (var album in library.Albums)
            {
                foreach (var track in album.TrackIds!)
                {
                    album.DummyTracks.Add(tracks[track]);
                }
            }

            return library;
        }

        /// <summary>
        /// Gets an Embedded Resource as a <see cref="string"/>.
        /// </summary>
        /// <param name="assembly">The <see cref="Assembly"/> containing the resource.</param>
        /// <param name="path">The path of the Resource within the <see cref="Assembly"/>.</param>
        /// <returns>The resorce as a <see cref="string"/></returns>
        private static string GetManifestResourceString(this Assembly assembly, string path)
        {
            using Stream stream = assembly.GetManifestResourceStream(path);
            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd().Trim();
        }
    }
}