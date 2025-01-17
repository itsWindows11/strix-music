﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using OwlCore;
using OwlCore.AbstractStorage;
using StrixMusic.Sdk.FileMetadata.Models;

namespace StrixMusic.Sdk.FileMetadata.Scanners
{
    /// <summary>
    /// Handles extracting playlist metadata from files.
    /// </summary>
    public sealed partial class PlaylistMetadataScanner : IDisposable
    {
        private static readonly string[] _supportedPlaylistFileFormats = { ".zpl", ".wpl", ".smil", ".m3u", ".m3u8", ".vlc", ".xspf", ".asx", ".mpcpl", ".fpl", ".pls", ".aimppl4" };
        private readonly SemaphoreSlim _batchLock;
        private readonly string _emitDebouncerId = Guid.NewGuid().ToString();
        private readonly List<PlaylistMetadata> _batchMetadataToEmit = new List<PlaylistMetadata>();

        private CancellationTokenSource? _scanningCancellationTokenSource;
        private int _filesProcessed;
        private int _totalFiles;

        /// <summary>
        /// Creates a new instance <see cref="PlaylistMetadataScanner"/>.
        /// </summary>
        public PlaylistMetadataScanner()
        {
            _batchLock = new SemaphoreSlim(1, 1);

            AttachEvents();
        }

        private void AttachEvents()
        {
            // TODO attach file system events
        }

        private void DetachEvents()
        {
            // TODO attach file system events
        }

        /// <summary>
        /// Flag that tells if the scanner is initialized or not.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Playlist metadata scanning completed.
        /// </summary>
        public event EventHandler? PlaylistMetadataScanCompleted;

        /// <summary>
        /// Raised when a new playlist with metadata is discovered.
        /// </summary>
        public event EventHandler<IEnumerable<PlaylistMetadata>>? PlaylistMetadataAdded;

        /// <summary>
        /// Raised when a previously scanned file has been removed from the file system.
        /// </summary>
        public event EventHandler<IEnumerable<PlaylistMetadata>>? PlaylistMetadataRemoved;

        /// <summary>
        /// Raised when the number of files processed has changed.
        /// </summary>
        public event EventHandler<int>? FilesProcessedChanged;

        /// <summary>
        /// Raised when the number of files found has changed.
        /// </summary>
        public event EventHandler<int>? FilesFoundChanged;

        /// <summary>
        /// Scans the given <paramref name="files"/> for playlist metadata, linking it to the given <paramref name="fileMetadata"/>.
        /// </summary>
        /// <param name="files">The files to scan for playlists.</param>
        /// <param name="fileMetadata">The file metadata to use when linking playlist data.</param>
        /// <returns>An <see cref="IEnumerable{PlaylistMetadata}"/> with playlist data linked to the given <paramref name="fileMetadata"/>.</returns>
        public Task<IEnumerable<PlaylistMetadata>> ScanPlaylists(IEnumerable<IFileData> files, IEnumerable<Models.FileMetadata> fileMetadata)
        {
            return ScanPlaylists(files, fileMetadata, new CancellationToken());
        }

        /// <summary>
        /// Scans the given <paramref name="files"/> for playlist metadata, linking it to the given <paramref name="fileMetadata"/>.
        /// </summary>
        /// <param name="files">The files to scan for playlists.</param>
        /// <param name="fileMetadata">The file metadata to use when linking playlist data.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels the ongoing task.</param>
        /// <returns>An <see cref="IEnumerable{PlaylistMetadata}"/> with playlist data linked to the given <paramref name="fileMetadata"/>.</returns>
        public async Task<IEnumerable<PlaylistMetadata>> ScanPlaylists(IEnumerable<IFileData> files, IEnumerable<Models.FileMetadata> fileMetadata, CancellationToken cancellationToken)
        {
            _filesProcessed = 0;

            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            _scanningCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var playlists = files.Where(c => _supportedPlaylistFileFormats.Contains(c.FileExtension)).ToList();
            _totalFiles = playlists.Count;
            FilesFoundChanged?.Invoke(this, playlists.Count);

            var scannedMetadata = new List<PlaylistMetadata>();

            if (_totalFiles == 0)
            {
                PlaylistMetadataScanCompleted?.Invoke(this, EventArgs.Empty);
                return scannedMetadata;
            }

            foreach (var item in playlists)
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                var playlistMetadata = await ProcessFile(item, fileMetadata);

                if (playlistMetadata != null)
                    scannedMetadata.Add(playlistMetadata);
            }

            return scannedMetadata;
        }

        private async Task<PlaylistMetadata?> ProcessFile(IFileData file, IEnumerable<Models.FileMetadata> files)
        {
            var playlistMetadata = await ScanPlaylistMetadata(file, files);

            await _batchLock.WaitAsync();

            if (playlistMetadata != null)
            {
                _batchMetadataToEmit.Add(playlistMetadata);
                FilesProcessedChanged?.Invoke(this, ++_filesProcessed);
            }

            _batchLock.Release();

            _ = HandleChanged();

            return playlistMetadata;
        }

        private async Task HandleChanged()
        {
            await _batchLock.WaitAsync();

            if (_totalFiles != _filesProcessed && _batchMetadataToEmit.Count < 100 && !await Flow.Debounce(_emitDebouncerId, TimeSpan.FromSeconds(5)))
                return;

            Guard.IsNotNull(_scanningCancellationTokenSource, nameof(_scanningCancellationTokenSource));

            if (_scanningCancellationTokenSource.Token.IsCancellationRequested)
                _scanningCancellationTokenSource.Token.ThrowIfCancellationRequested();

            PlaylistMetadataAdded?.Invoke(this, _batchMetadataToEmit.ToArray());

            _batchMetadataToEmit.Clear();

            _batchLock.Release();

            if (_totalFiles == _filesProcessed)
                PlaylistMetadataScanCompleted?.Invoke(this, EventArgs.Empty);
        }

        private enum AimpplPlaylistMode
        {
            Summary,
            Settings,
            Content,
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!IsInitialized)
                return;

            DetachEvents();
            _scanningCancellationTokenSource?.Cancel();

            IsInitialized = false;
        }
    }
}
