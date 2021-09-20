﻿using Microsoft.Toolkit.Diagnostics;
using OwlCore;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.MediaPlayback;
using StrixMusic.Sdk.Services.MediaPlayback;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media;
using Windows.Storage.Streams;

namespace StrixMusic.Sdk.Uno.Services.MediaPlayback
{
    /// <summary>
    /// Integrates an <see cref="IPlaybackHandlerService"/> with the system media transport controls.
    /// </summary>
    public class SystemMediaTransportControlsHandler : IDisposable
    {
        private readonly IPlaybackHandlerService _playbackHandlerService;
        private readonly SystemMediaTransportControls _systemMediaTransportControls;

        /// <summary>
        /// Creates a new instance of <see cref="SystemMediaTransportControlsHandler"/>
        /// </summary>
        /// <param name="playbackHandlerService">The playback handler to use for display with the system transport controls.</param>
        public SystemMediaTransportControlsHandler(IPlaybackHandlerService playbackHandlerService)
        {
            _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            _playbackHandlerService = playbackHandlerService;

            AttachEvents();
        }

        private void AttachEvents()
        {
            _playbackHandlerService.CurrentItemChanged += PlaybackHandlerService_CurrentItemChanged;
            _playbackHandlerService.PlaybackStateChanged += PlaybackHandlerService_PlaybackStateChanged;
            _playbackHandlerService.RepeatStateChanged += PlaybackHandlerService_RepeatStateChanged;
            _playbackHandlerService.ShuffleStateChanged += PlaybackHandlerService_ShuffleStateChanged;
            _playbackHandlerService.PositionChanged += PlaybackHandlerService_PositionChanged;

            _systemMediaTransportControls.ShuffleEnabledChangeRequested += SystemMediaTransportControls_ShuffleEnabledChangeRequested;
            _systemMediaTransportControls.AutoRepeatModeChangeRequested += SystemMediaTransportControls_AutoRepeatModeChangeRequested;
            _systemMediaTransportControls.PlaybackPositionChangeRequested += SystemMediaTransportControls_PlaybackPositionChangeRequested;
            _systemMediaTransportControls.PlaybackRateChangeRequested += SystemMediaTransportControls_PlaybackRateChangeRequested;
            _systemMediaTransportControls.ButtonPressed += SystemMediaTransportControls_ButtonPressed;
        }

        private void DetachEvents()
        {
            _playbackHandlerService.CurrentItemChanged -= PlaybackHandlerService_CurrentItemChanged;
            _playbackHandlerService.PlaybackStateChanged -= PlaybackHandlerService_PlaybackStateChanged;
            _playbackHandlerService.RepeatStateChanged -= PlaybackHandlerService_RepeatStateChanged;
            _playbackHandlerService.ShuffleStateChanged -= PlaybackHandlerService_ShuffleStateChanged;
            _playbackHandlerService.PositionChanged -= PlaybackHandlerService_PositionChanged;

            _systemMediaTransportControls.ShuffleEnabledChangeRequested -= SystemMediaTransportControls_ShuffleEnabledChangeRequested;
            _systemMediaTransportControls.AutoRepeatModeChangeRequested -= SystemMediaTransportControls_AutoRepeatModeChangeRequested;
            _systemMediaTransportControls.PlaybackPositionChangeRequested -= SystemMediaTransportControls_PlaybackPositionChangeRequested;
            _systemMediaTransportControls.PlaybackRateChangeRequested -= SystemMediaTransportControls_PlaybackRateChangeRequested;
            _systemMediaTransportControls.ButtonPressed -= SystemMediaTransportControls_ButtonPressed;
        }

        private void SystemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            _ = Threading.OnPrimaryThread(() =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        _playbackHandlerService.ResumeAsync();
                        break;
                    case SystemMediaTransportControlsButton.Stop:
                    case SystemMediaTransportControlsButton.Pause:
                        _playbackHandlerService.PauseAsync();
                        break;
                    case SystemMediaTransportControlsButton.FastForward:
                        _playbackHandlerService.SeekAsync(_playbackHandlerService.Position + TimeSpan.FromSeconds(5));
                        break;
                    case SystemMediaTransportControlsButton.Rewind:
                        _playbackHandlerService.SeekAsync(_playbackHandlerService.Position - TimeSpan.FromSeconds(5));
                        break;
                    case SystemMediaTransportControlsButton.Next:
                        _playbackHandlerService.NextAsync();
                        break;
                    case SystemMediaTransportControlsButton.Previous:
                        _playbackHandlerService.PreviousAsync();
                        break;
                    case SystemMediaTransportControlsButton.Record:
                    case SystemMediaTransportControlsButton.ChannelUp:
                    case SystemMediaTransportControlsButton.ChannelDown:
                        break;
                }
            });
        }

        private void SystemMediaTransportControls_PlaybackRateChangeRequested(SystemMediaTransportControls sender, PlaybackRateChangeRequestedEventArgs args)
        {
            _playbackHandlerService.ChangePlaybackSpeedAsync(args.RequestedPlaybackRate);
        }

        private void SystemMediaTransportControls_AutoRepeatModeChangeRequested(SystemMediaTransportControls sender, AutoRepeatModeChangeRequestedEventArgs args)
        {
            _playbackHandlerService.SetRepeatStateAsync((RepeatState)args.RequestedAutoRepeatMode);
        }

        private void SystemMediaTransportControls_PlaybackPositionChangeRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args)
        {
            _playbackHandlerService.SeekAsync(args.RequestedPlaybackPosition);
        }

        private void SystemMediaTransportControls_ShuffleEnabledChangeRequested(SystemMediaTransportControls sender, ShuffleEnabledChangeRequestedEventArgs args)
        {
            if (args.RequestedShuffleEnabled != _playbackHandlerService.ShuffleState)
                _playbackHandlerService.ToggleShuffleAsync();
        }

        private void PlaybackHandlerService_PositionChanged(object sender, TimeSpan e)
        {
            _systemMediaTransportControls.UpdateTimelineProperties(new SystemMediaTransportControlsTimelineProperties
            {
                Position = e,
                EndTime = _playbackHandlerService.CurrentItem?.Track.Duration ?? TimeSpan.MaxValue,
                StartTime = TimeSpan.Zero,
            });
        }

        private void PlaybackHandlerService_ShuffleStateChanged(object sender, bool e)
        {
            _systemMediaTransportControls.ShuffleEnabled = e;
        }

        private void PlaybackHandlerService_RepeatStateChanged(object sender, RepeatState e)
        {
            _systemMediaTransportControls.AutoRepeatMode = (MediaPlaybackAutoRepeatMode)e;
        }

        private void PlaybackHandlerService_PlaybackStateChanged(object sender, PlaybackState e)
        {
            if (e == PlaybackState.Queued)
                return;

            _systemMediaTransportControls.IsPlayEnabled = e == PlaybackState.Paused;
            _systemMediaTransportControls.IsPauseEnabled = e == PlaybackState.Playing;
            _systemMediaTransportControls.IsStopEnabled = e == PlaybackState.Playing;
            _systemMediaTransportControls.IsRewindEnabled = e == PlaybackState.Playing;
            _systemMediaTransportControls.IsFastForwardEnabled = e == PlaybackState.Playing;
            _systemMediaTransportControls.IsChannelUpEnabled = false;
            _systemMediaTransportControls.IsChannelDownEnabled = false;
            _systemMediaTransportControls.IsRecordEnabled = false;

            _systemMediaTransportControls.PlaybackStatus = e switch
            {
                PlaybackState.None => MediaPlaybackStatus.Stopped,
                PlaybackState.Failed => MediaPlaybackStatus.Closed,
                PlaybackState.Playing => MediaPlaybackStatus.Playing,
                PlaybackState.Paused => MediaPlaybackStatus.Paused,
                PlaybackState.Loading => MediaPlaybackStatus.Changing,
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<MediaPlaybackStatus>(),
            };
        }

        private async void PlaybackHandlerService_CurrentItemChanged(object sender, IMediaSourceConfig? e)
        {
            var updater = _systemMediaTransportControls.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            var musicProperties = updater.MusicProperties;

            if (e is null)
            {
                updater.ClearAll();
                updater.Update();
                return;
            }

            PlaybackHandlerService_PlaybackStateChanged(sender, _playbackHandlerService.PlaybackState);
            _systemMediaTransportControls.IsNextEnabled = _playbackHandlerService.NextItems.Count > 0;
            _systemMediaTransportControls.IsPreviousEnabled = _playbackHandlerService.PreviousItems.Count > 0;

            // Images
            // Just the first, we don't care about the size.
            await foreach (var image in e.Track.GetImagesAsync(1, 0))
            {
                updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(image.Uri);
                break;
            }

            // Genres
            musicProperties.Genres.Clear();
            await foreach (var genre in e.Track.GetGenresAsync(e.Track.TotalGenreCount, 0))
                musicProperties.Genres.Add(genre.Name);

            // Track info
            musicProperties.TrackNumber = (uint)(e.Track.TrackNumber ?? 0);
            musicProperties.Title = e.Track.Name;

            // Artist info
            // Just the first (primary) artist.
            await foreach (var artist in e.Track.GetArtistItemsAsync(1, 0))
            {
                musicProperties.Artist = artist.Name;
                break;
            }

            // Album info
            musicProperties.AlbumTrackCount = (uint)(e.Track.Album?.TotalTrackCount ?? 0);
            musicProperties.AlbumTitle = e.Track.Album?.Name ?? string.Empty;

            updater.Update();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DetachEvents();
        }
    }
}