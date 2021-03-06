﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using OwlCore;
using OwlCore.AbstractStorage;
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Sdk.Services.FileMetadataManager.MetadataScanner;
using StrixMusic.Sdk.Services.FileMetadataManager.Models;
using StrixMusic.Sdk.Services.FileMetadataManager.Repositories;

namespace StrixMusic.Sdk.Services.FileMetadataManager
{
    /// <summary>
    /// The service that helps in interacting with the saved file core playlists information.
    /// </summary>
    public class PlaylistRepository : IPlaylistRepository
    {
        private const string PLAYLISTS_DATA_FILENAME = "Playlists.bin";

        private readonly ConcurrentDictionary<string, PlaylistMetadata> _inMemoryMetadata;
        private readonly SemaphoreSlim _storageMutex;
        private readonly FileMetadataScanner _fileMetadataScanner;
        private readonly string _debouncerId;
        private IFolderData? _folderData;

        /// <summary>
        /// Creates a new instance of <see cref="PlaylistRepository"/>.
        /// </summary>
        ///  <param name="fileMetadataScanner">The file scanner instance to source metadata from.</param>
        internal PlaylistRepository(FileMetadataScanner fileMetadataScanner)
        {
            Guard.IsNotNull(fileMetadataScanner, nameof(fileMetadataScanner));

            _fileMetadataScanner = fileMetadataScanner;

            _inMemoryMetadata = new ConcurrentDictionary<string, PlaylistMetadata>();
            _fileMetadataScanner = fileMetadataScanner;
            _storageMutex = new SemaphoreSlim(1, 1);
            _debouncerId = Guid.NewGuid().ToString();

            AttachEvents();
        }

        /// <inheritdoc />
        public async Task InitAsync()
        {
            Guard.IsFalse(IsInitialized, nameof(IsInitialized));
            IsInitialized = true;

            //await LoadDataFromDisk();
        }

        private void AttachEvents()
        {
            _fileMetadataScanner.FileMetadataAdded += FileMetadataScanner_FileMetadataAdded;
            _fileMetadataScanner.FileMetadataRemoved += FileMetadataScanner_FileMetadataRemoved;
        }

        private void DetachEvents()
        {
            _fileMetadataScanner.FileMetadataAdded -= FileMetadataScanner_FileMetadataAdded;
            _fileMetadataScanner.FileMetadataRemoved -= FileMetadataScanner_FileMetadataRemoved;
        }

        private async void FileMetadataScanner_FileMetadataRemoved(object sender, IEnumerable<FileMetadata> e)
        {
            var removedPlaylists = new List<PlaylistMetadata>();

            foreach (var metadata in e)
            {
                if (metadata.PlaylistMetadata != null)
                {
                    Guard.IsNotNullOrWhiteSpace(metadata.PlaylistMetadata.Id, nameof(metadata.PlaylistMetadata.Id));

                    await _storageMutex.WaitAsync();

                    if (!_inMemoryMetadata.TryAdd(metadata.PlaylistMetadata.Id, metadata.PlaylistMetadata))
                        ThrowHelper.ThrowInvalidOperationException($"Tried adding an item that already exists in {nameof(_inMemoryMetadata)}");

                    _storageMutex.Release();
                    removedPlaylists.Add(metadata.PlaylistMetadata);
                }
            }

            if (removedPlaylists.Count > 0)
            {
                _ = CommitChangesAsync();
                MetadataRemoved?.Invoke(this, removedPlaylists);
            }
        }

        private async void FileMetadataScanner_FileMetadataAdded(object sender, IEnumerable<FileMetadata> e)
        {
            var addedPlaylists = new List<PlaylistMetadata>();

            foreach (var metadata in e)
            {
                if (metadata.PlaylistMetadata != null)
                {
                    Guard.IsNotNullOrWhiteSpace(metadata.PlaylistMetadata.Id, nameof(metadata.PlaylistMetadata.Id));

                    await _storageMutex.WaitAsync();

                    if (!_inMemoryMetadata.TryAdd(metadata.PlaylistMetadata.Id, metadata.PlaylistMetadata))
                        ThrowHelper.ThrowInvalidOperationException($"Tried adding an item that already exists in {nameof(_inMemoryMetadata)}");

                    _storageMutex.Release();
                    addedPlaylists.Add(metadata.PlaylistMetadata);
                }
            }

            if (addedPlaylists.Count > 0)
            {
                _ = CommitChangesAsync();
                MetadataAdded?.Invoke(this, addedPlaylists);
            }
        }

        /// <inheritdoc />
        public event EventHandler<IEnumerable<PlaylistMetadata>>? MetadataUpdated;

        /// <inheritdoc />
        public event EventHandler<IEnumerable<PlaylistMetadata>>? MetadataAdded;

        /// <inheritdoc />
        public event EventHandler<IEnumerable<PlaylistMetadata>>? MetadataRemoved;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<(PlaylistMetadata Playlist, TrackMetadata Track)>? TracksChanged;

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Sets the root folder to operate in when saving data.
        /// </summary>
        /// <param name="rootFolder">The root folder to save data in.</param>
        public void SetDataFolder(IFolderData rootFolder)
        {
            _folderData = rootFolder;
        }

        /// <inheritdoc />
        public async Task AddOrUpdatePlaylist(PlaylistMetadata playlistMetadata)
        {
            Guard.IsNotNullOrWhiteSpace(playlistMetadata.Id, nameof(playlistMetadata.Id));

            var isUpdate = false;
            await _storageMutex.WaitAsync();

            _inMemoryMetadata.AddOrUpdate(
                playlistMetadata.Id,
                addValueFactory: id =>
                {
                    isUpdate = false;
                    return playlistMetadata;
                },
                updateValueFactory: (id, existing) =>
                {
                    isUpdate = true;
                    return playlistMetadata;
                });

            _storageMutex.Release();

            _ = CommitChangesAsync();

            if (isUpdate)
                MetadataUpdated?.Invoke(this, playlistMetadata.IntoList());
            else
                MetadataAdded?.Invoke(this, playlistMetadata.IntoList());
        }

        /// <inheritdoc />
        public async Task RemovePlaylist(PlaylistMetadata playlistMetadata)
        {
            Guard.IsNotNullOrWhiteSpace(playlistMetadata.Id, nameof(playlistMetadata.Id));

            await _storageMutex.WaitAsync();
            var removed = _inMemoryMetadata.TryRemove(playlistMetadata.Id, out _);
            _storageMutex.Release();

            if (removed)
            {
                _ = CommitChangesAsync();
                MetadataRemoved?.Invoke(this, playlistMetadata.IntoList());
            }
        }

        /// <inheritdoc />
        public Task<PlaylistMetadata?> GetPlaylistById(string id)
        {
            _inMemoryMetadata.TryGetValue(id, out var metadata);

            return Task.FromResult<PlaylistMetadata?>(metadata);
        }

        /// <inheritdoc />
        public Task<IReadOnlyList<PlaylistMetadata>> GetPlaylists(int offset, int limit)
        {
            var allPlaylists = _inMemoryMetadata.Values.ToList();

            if (limit == -1)
                return Task.FromResult<IReadOnlyList<PlaylistMetadata>>(allPlaylists.GetRange(offset, allPlaylists.Count - offset));

            return Task.FromResult<IReadOnlyList<PlaylistMetadata>>(allPlaylists.GetRange(offset, limit));
        }

        /// <summary>
        /// Gets the existing repo data stored on disk.
        /// </summary>
        /// <returns>The <see cref="PlaylistMetadata"/> collection.</returns>
        private async Task LoadDataFromDisk()
        {
            Guard.IsNotNull(_folderData, nameof(_folderData));

            var fileData = await _folderData.CreateFileAsync(PLAYLISTS_DATA_FILENAME, CreationCollisionOption.OpenIfExists);

            Guard.IsNotNull(fileData, nameof(fileData));

            using var stream = await fileData.GetStreamAsync(FileAccessMode.ReadWrite);
            var bytes = await stream.ToBytesAsync();

            if (bytes.Length == 0)
                return;

            var data = MessagePackSerializer.Deserialize<List<PlaylistMetadata>>(bytes, MessagePack.Resolvers.ContractlessStandardResolver.Options);

            foreach (var item in data)
            {
                Guard.IsNotNullOrWhiteSpace(item?.Id, nameof(item.Id));

                if (!_inMemoryMetadata.TryAdd(item.Id, item))
                    ThrowHelper.ThrowInvalidOperationException($"Item already added to {nameof(_inMemoryMetadata)}");
            }
        }

        private async Task CommitChangesAsync()
        {
            if (!await Flow.Debounce(_debouncerId, TimeSpan.FromSeconds(5)) || _inMemoryMetadata.IsEmpty)
                return;

            await _storageMutex.WaitAsync();

            Guard.IsNotNull(_folderData, nameof(_folderData));
            var bytes = MessagePackSerializer.Serialize(_inMemoryMetadata.ToList(), MessagePack.Resolvers.ContractlessStandardResolver.Options);

            var fileData = await _folderData.CreateFileAsync(PLAYLISTS_DATA_FILENAME, CreationCollisionOption.OpenIfExists);
            await fileData.WriteAllBytesAsync(bytes);

            _storageMutex.Release();
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
                _inMemoryMetadata.Clear();
                _storageMutex.Dispose();
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
        ~PlaylistRepository()
        {
            Dispose(false);
        }
    }
}
