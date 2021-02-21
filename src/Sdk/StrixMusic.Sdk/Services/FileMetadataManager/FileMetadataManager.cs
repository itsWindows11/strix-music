﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using OwlCore.AbstractStorage;
using OwlCore.Extensions;
using StrixMusic.Sdk.Services.FileMetadataManager.MetadataScanner;
using StrixMusic.Sdk.Services.FileMetadataManager.Models;
using StrixMusic.Sdk.Services.StorageService;

namespace StrixMusic.Sdk.Services.FileMetadataManager
{
    /// <inheritdoc />
    public class FileMetadataManager : IFileMetadataManager
    {
        private readonly string _instanceId;
        private readonly IFolderData _rootFolder;
        private readonly FileMetadataScanner _fileMetadataScanner;

        /// <summary>
        /// Creates a new instance of <see cref="FileMetadataManager"/>.
        /// </summary>
        /// <param name="instanceId">A unique identifier that helps decide where the scanned data is stored.</param>
        /// <param name="rootFolder">The folder where data is scanned.</param>
        public FileMetadataManager(string instanceId, IFolderData rootFolder)
        {
            _instanceId = instanceId;
            _rootFolder = rootFolder;

            _fileMetadataScanner = new FileMetadataScanner(_rootFolder);

            Albums = new AlbumRepository(_fileMetadataScanner);
            Artists = new ArtistRepository(_fileMetadataScanner);
            Tracks = new TrackRepository(_fileMetadataScanner);
            Playlists = new PlaylistRepository();

            AttachEvents();
        }

        private static async Task<IFolderData> GetDataStorageFolder(string instanceId)
        {
            var primaryFileSystemService = Ioc.Default.GetRequiredService<IFileSystemService>();

            var path = Path.Combine(primaryFileSystemService.RootFolder.Path, nameof(FileMetadataManager), instanceId);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var folderData = await primaryFileSystemService.GetFolderFromPathAsync(path);

            Guard.IsNotNull(folderData, nameof(folderData));

            return folderData;
        }

        private void AttachEvents()
        {
            // todo, attach changed events?
            _fileMetadataScanner.FileMetadataAdded += FileMetadataScanner_FileMetadataAdded;
            _fileMetadataScanner.FileMetadataUpdated += FileMetadataScanner_FileMetadataUpdated;
        }

        private void FileMetadataScanner_FileMetadataUpdated(object sender, FileMetadata e)
        {
            FileMetadataUpdated?.Invoke(this, e);
        }

        private void FileMetadataScanner_FileMetadataAdded(object sender, FileMetadata e)
        {
            var fileMetadata = new FileMetadata();

            if (Tracks.AddOrSkipTrackMetadata(e.TrackMetadata))
                fileMetadata.TrackMetadata = e.TrackMetadata;

            if (Albums.AddOrSkipAlbumMetadata(e.AlbumMetadata))
                fileMetadata.AlbumMetadata = e.AlbumMetadata;

            if (Artists.AddOrSkipArtistMetadata(e.ArtistMetadata))
                fileMetadata.ArtistMetadata = e.ArtistMetadata;

            FileMetadataAdded?.Invoke(sender, fileMetadata);
        }

        private void DetachEvents()
        {
            _fileMetadataScanner.FileMetadataAdded -= FileMetadataScanner_FileMetadataAdded;
        }

        ///<inheritdoc />
        public event EventHandler<FileMetadata>? FileMetadataAdded;

        ///<inheritdoc />
        public event EventHandler<FileMetadata>? FileMetadataUpdated;

        ///<inheritdoc />
        public event EventHandler<FileMetadata>? FileMetadataRemoved;

        /// <inheritdoc />
        public event EventHandler? ScanningStarted;

        /// <inheritdoc />
        public event EventHandler? ScanningCompleted;

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public AlbumRepository Albums { get; private set; }

        /// <inheritdoc />
        public ArtistRepository Artists { get; private set; }

        /// <inheritdoc />
        public PlaylistRepository Playlists { get; private set; }

        /// <inheritdoc />
        public TrackRepository Tracks { get; private set; }

        /// <inheritdoc />
        public async Task InitAsync()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;

            var dataFolder = await GetDataStorageFolder(_instanceId);

            _fileMetadataScanner.CacheFolder = dataFolder;
            await _fileMetadataScanner.InitAsync();

            // Perform initialization tasks for all repos in parallel.
            var repositories = new IMetadataRepository[]
            {
                Albums,
                Artists,
                Tracks,
                Playlists,
            };

            await repositories.InParallel(x =>
            {
                x.SetDataFolder(dataFolder);
                return x.InitAsync();
            });

            ScanningCompleted?.Invoke(this, new EventArgs());
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            DetachEvents();

            Albums.Dispose();
            Artists.Dispose();
            Playlists.Dispose();
            Tracks.Dispose();
            _fileMetadataScanner?.Dispose();

            return default;
        }
    }
}