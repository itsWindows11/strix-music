﻿using StrixMusic.Sdk.MediaPlayback;
using System;
using System.Threading;
using System.Threading.Tasks;
using StrixMusic.Sdk.AppModels;
using StrixMusic.Sdk.BaseModels;
using StrixMusic.Sdk.CoreModels;

namespace StrixMusic.Cores.OwlCoreRpc.Tests.Mock
{
    public class MockCoreDevice : ICoreDevice
    {
        public MockCoreDevice(ICore sourceCore)
        {
            SourceCore = sourceCore;
            Id = nameof(MockCoreDevice);
            Name = "Mock device";
        }

        public ICoreTrackCollection? PlaybackQueue { get; set; }

        public ICoreTrack? NowPlaying { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }

        public IPlayableBase? PlaybackContext { get; set; }

        public DeviceType Type { get; set; }

        public bool ShuffleState { get; set; }

        public RepeatState RepeatState { get; set; }

        public bool IsSeekAsyncAvailable { get; set; }

        public bool IsResumeAsyncAvailable { get; set; }

        public bool IsPauseAsyncAvailable { get; set; }

        public bool IsChangeVolumeAsyncAvailable { get; set; }

        public bool IsChangePlaybackSpeedAvailable { get; set; }

        public bool IsNextAsyncAvailable { get; set; }

        public bool IsPreviousAsyncAvailable { get; set; }

        public bool IsToggleShuffleAsyncAvailable { get; set; }

        public bool IsToggleRepeatAsyncAvailable { get; set; }

        public TimeSpan Position { get; set; }

        public PlaybackState PlaybackState { get; set; }

        public double Volume { get; set; }

        public double PlaybackSpeed { get; set; }

        public ICore SourceCore { get; set; }

        public event EventHandler<ICoreTrack>? NowPlayingChanged;
        public event EventHandler<bool>? IsActiveChanged;
        public event EventHandler<IPlayableBase?>? PlaybackContextChanged;
        public event EventHandler<bool>? ShuffleStateChanged;
        public event EventHandler<RepeatState>? RepeatStateChanged;
        public event EventHandler<TimeSpan>? PositionChanged;
        public event EventHandler<PlaybackState>? PlaybackStateChanged;
        public event EventHandler<double>? VolumeChanged;
        public event EventHandler<double>? PlaybackSpeedChanged;

        public Task ChangePlaybackSpeedAsync(double speed, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task ChangeVolumeAsync(double volume, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public Task NextAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task PauseAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task PreviousAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task ResumeAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SeekAsync(TimeSpan position, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SwitchToAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task ToggleRepeatAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task ToggleShuffleAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
