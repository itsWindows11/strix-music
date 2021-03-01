﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.Collections;
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Core.LocalFiles.Services;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;
using StrixMusic.Sdk.Services.FileMetadataManager;
using StrixMusic.Sdk.Services.FileMetadataManager.Models;

namespace StrixMusic.Core.LocalFiles.Models
{
    /// <inheritdoc />
    public class LocalFileCorePlaylist : ICorePlaylist
    {
        private readonly PlaylistMetadata _playlistMetadata;
        private readonly IFileMetadataManager _fileMetadataManager;

        /// <summary>
        /// Creates a new instance of <see cref="LocalFileCorePlaylist"/>
        /// </summary>
        public LocalFileCorePlaylist(ICore sourceCore, PlaylistMetadata playlistMetadata)
        {
            SourceCore = sourceCore;
            _playlistMetadata = playlistMetadata;

            Guard.IsNotNull(playlistMetadata, nameof(playlistMetadata));
            Guard.IsNotNull(playlistMetadata.Id, nameof(playlistMetadata.Id));

            Id = playlistMetadata.Id;
            _fileMetadataManager = sourceCore.GetService<IFileMetadataManager>();

            CalculatePlaylistDuration().FireAndForget();
        }

        private async Task CalculatePlaylistDuration()
        {
            if (_playlistMetadata.TrackIds != null)
            {
                var allTracks = await _fileMetadataManager.Tracks.GetTrackMetadataByIds(_playlistMetadata.TrackIds);

                if (allTracks != null)
                {
                    var durationMilliseconds = 0;

                    foreach (var item in allTracks)
                    {
                        durationMilliseconds += item?.Duration?.Milliseconds ?? 0;
                    }

                    Duration = TimeSpan.FromMilliseconds(durationMilliseconds);

                    TotalTracksCount = allTracks.Count;
                }
            }
        }

        /// <inheritdoc />
        public Task<bool> IsAddImageAvailable(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveImageAvailable(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task RemoveImageAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public int TotalImageCount { get; }

        /// <inheritdoc />
        public event EventHandler<int>? ImagesCountChanged;

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public Uri? Url => _playlistMetadata?.Url ?? null;

        /// <inheritdoc />
        public string Name => _playlistMetadata?.Title ?? "No Title";

        /// <inheritdoc />
        public string? Description => _playlistMetadata.Description;

        /// <inheritdoc />
        public DateTime? LastPlayed { get; }

        /// <inheritdoc />
        public PlaybackState PlaybackState { get; }

        /// <inheritdoc />
        public TimeSpan Duration { get;private  set; }

        /// <inheritdoc />
        public bool IsChangeNameAsyncAvailable => false;

        /// <inheritdoc />
        public bool IsChangeDescriptionAsyncAvailable => false;

        /// <inheritdoc />
        public bool IsChangeDurationAsyncAvailable => false;

        /// <inheritdoc />
        public Task ChangeNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task ChangeDescriptionAsync(string? description)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task ChangeDurationAsync(TimeSpan duration)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public event EventHandler<PlaybackState>? PlaybackStateChanged;

        /// <inheritdoc />
        public event EventHandler<string>? NameChanged;

        /// <inheritdoc />
        public event EventHandler<string?>? DescriptionChanged;

        /// <inheritdoc />
        public event EventHandler<Uri?>? UrlChanged;

        /// <inheritdoc />
        public event EventHandler<TimeSpan>? DurationChanged;

        /// <inheritdoc />
        public event EventHandler<DateTime?>? LastPlayedChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeNameAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDescriptionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsChangeDurationAsyncAvailableChanged;

        /// <inheritdoc />
        public DateTime? AddedAt { get; }

        /// <inheritdoc />
        public int TotalTracksCount { get; private set; }

        /// <inheritdoc />
        public bool IsPlayTrackCollectionAsyncAvailable => false;

        /// <inheritdoc />
        public bool IsPauseTrackCollectionAsyncAvailable => false;

        /// <inheritdoc />
        public Task PlayTrackCollectionAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task PauseTrackCollectionAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task RemoveTrackAsync(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task<bool> IsAddTrackAvailable(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveTrackAvailable(int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public event EventHandler<bool>? IsPlayTrackCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<bool>? IsPauseTrackCollectionAsyncAvailableChanged;

        /// <inheritdoc />
        public event EventHandler<int>? TrackItemsCountChanged;

        /// <inheritdoc />
        public SynchronizedObservableCollection<string>? Genres { get; }

        /// <inheritdoc />
        public Task<bool> IsAddGenreAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> IsRemoveGenreAvailable(int index)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public ICore SourceCore { get; }

        /// <inheritdoc />
        public IAsyncEnumerable<ICoreImage> GetImagesAsync(int limit, int offset)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task AddImageAsync(ICoreImage image, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreImage>? ImagesChanged;

        /// <inheritdoc />
        public async IAsyncEnumerable<ICoreTrack> GetTracksAsync(int limit, int offset)
        {
            var trackIds = _playlistMetadata.TrackIds;

            if (trackIds == null) yield break;

            var tracks = await _fileMetadataManager.Tracks.GetTrackMetadataByIds(trackIds);

            if (tracks == null) yield break;

            foreach (var item in tracks)
            {
                yield return new LocalFilesCoreTrack(SourceCore, item);
            }
        }

        /// <inheritdoc />
        public Task AddTrackAsync(ICoreTrack track, int index)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public Task PlayTrackCollectionAsync(ICoreTrack track)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreTrack>? TrackItemsChanged;

        /// <inheritdoc />
        public ICoreUserProfile? Owner { get; }

        /// <inheritdoc />
        public ICorePlayableCollectionGroup? RelatedItems { get; }
    }
}
