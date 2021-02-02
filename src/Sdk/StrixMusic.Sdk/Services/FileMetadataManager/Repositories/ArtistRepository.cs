﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractStorage;
using StrixMusic.Sdk.Services.FileMetadataManager.MetadataScanner;
using StrixMusic.Sdk.Services.FileMetadataManager.Models;

namespace StrixMusic.Sdk.Services.FileMetadataManager
{
    /// <summary>
    /// Service to retrieve artist records.
    /// </summary>
    public class ArtistRepository : IMetadataRepository
    {
        private const string ARTIST_DATA_FILENAME = "ArtistMeta.bin";
        private readonly FileMetadataScanner _fileMetadataScanner;
        private string? _pathToMetadataFile;

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Creates a new instance for <see cref="TrackRepository"/>.
        /// </summary>
        ///  <param name="fileMetadataScanner">The file scanner instance to use when </param>
        public ArtistRepository(FileMetadataScanner fileMetadataScanner)
        {
            _fileMetadataScanner = fileMetadataScanner;
        }

        private void AttachEvents()
        {
            _fileMetadataScanner.FileMetadataAdded += FileMetadataAdded;
            _fileMetadataScanner.FileMetadataRemoved += FileMetadataRemoved;
        }

        private void DetachEvents()
        {
            _fileMetadataScanner.FileMetadataAdded -= FileMetadataAdded;
            _fileMetadataScanner.FileMetadataRemoved -= FileMetadataRemoved;
        }

        private void FileMetadataAdded(object sender, FileMetadata e)
        {
            // todo
        }

        private void FileMetadataRemoved(object sender, FileMetadata e)
        {
            // todo
        }

        /// <summary>
        /// Sets the root folder to operate in when saving data.
        /// </summary>
        /// <param name="rootFolder">The root folder to work in.</param>
        public void SetDataFolder(IFolderData rootFolder)
        {
            _pathToMetadataFile = $"{rootFolder.Path}\\{ARTIST_DATA_FILENAME}";
        }

        /// <summary>
        /// Gets all <see cref="TrackMetadata"/> over the file system.
        /// </summary>
        /// <param name="offset">The starting index for retrieving items.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IReadOnlyList<ArtistMetadata>> GetArtistMetadata(int offset, int limit)
        {
            Guard.IsNotNullOrWhiteSpace(_pathToMetadataFile, nameof(_pathToMetadataFile));

            //if (!File.Exists(_pathToMetadataFile))
            //    throw new FileNotFoundException(_pathToMetadatafile);

            //var bytes = File.ReadAllBytes(_pathToMetadatafile);
            //var artistMetadataLst = MessagePackSerializer.Deserialize<IReadOnlyList<ArtistMetadata>>(bytes, MessagePack.Resolvers.ContractlessStandardResolver.Options);

            //return Task.FromResult<IReadOnlyList<ArtistMetadata>>(artistMetadataLst.Skip(offset).Take(limit).ToList());

            var allArtists = await _fileMetadataScanner.GetUniqueArtistMetadata();

            if (limit == -1)
            {
                return allArtists;
            }
            else
            {
                var filteredArtists = allArtists.Skip(offset).Take(limit).ToList();

                return filteredArtists;
            }
        }

        /// <summary>
        /// Gets the filtered artist by album ids.
        /// </summary>
        /// <param name="albumId">The artist Id.</param>
        /// <param name="offset">The starting index for retrieving items.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The filtered <see cref="IReadOnlyList{ArtistMetadata}"/>></returns>
        public async Task<IReadOnlyList<ArtistMetadata>> GetArtistsByAlbumId(string albumId, int offset, int limit)
        {
            var filteredArtists = new List<ArtistMetadata>();

            var artists = await GetArtistMetadata(offset, limit);

            foreach (var item in artists)
            {
                if (item.AlbumIds != null && item.AlbumIds.Contains(albumId))
                {
                    filteredArtists.Add(item);
                }
            }

            return filteredArtists;
        }

        /// <summary>
        /// Gets the filtered artist by album ids.
        /// </summary>
        /// <param name="trackId">The artist Id.</param>
        /// <param name="offset">The starting index for retrieving items.</param>
        /// <param name="limit">The maximum number of items to return.</param>
        /// <returns>The filtered <see cref="IReadOnlyList{ArtistMetadata}"/>></returns>
        public async Task<IReadOnlyList<ArtistMetadata>> GetArtistsByTrackId(string trackId, int offset, int limit)
        {
            var filteredArtists = new List<ArtistMetadata>();

            var artists = await GetArtistMetadata(0, -1);

            foreach (var item in artists)
            {
                if (item?.AlbumIds != null && item.AlbumIds.Contains(trackId))
                {
                    filteredArtists.Add(item);
                }
            }

            return filteredArtists.Skip(offset).Take(limit).ToList();
        }

        /// <inheritdoc />
        public async Task InitAsync()
        {
            await Task.Run(ScanForArtists);
            IsInitialized = true;
        }

        /// <summary>
        /// Create or Update <see cref="ArtistMetadata"/> information in files.
        /// </summary>
        /// <returns>The <see cref="ArtistMetadata"/> collection.</returns>
        private async Task ScanForArtists()
        {
            if (!File.Exists(_pathToMetadataFile))
                File.Create(_pathToMetadataFile).Close(); // creates the file and closes the file stream.

            // NOTE: Make sure you have already scanned the file metadata.
            var metadata = await _fileMetadataScanner.GetUniqueArtistMetadata();

            if (metadata != null && metadata.Count > 0)
            {
                var bytes = MessagePackSerializer.Serialize(metadata, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                File.WriteAllBytes(_pathToMetadataFile, bytes);
            }
        }

        private void ReleaseUnmanagedResources()
        {
            DetachEvents();
        }

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsInitialized)
                return;

            ReleaseUnmanagedResources();
            if (disposing)
            {
                // dispose any objects you created here
            }

            IsInitialized = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~ArtistRepository()
        {
            Dispose(false);
        }
    }
}