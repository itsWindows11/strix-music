﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using OwlCore.AbstractUI.Models;
using OwlCore.AbstractUI.ViewModels;
using OwlCore.Events;
using StrixMusic.Sdk.AppModels;
using StrixMusic.Sdk.CoreModels;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Sdk.ViewModels
{
    /// <summary>
    /// A ViewModel for <see cref="ICore"/>.
    /// </summary>
    public sealed class CoreViewModel : ObservableObject, ISdkViewModel, ICore
    {
        private readonly ICore _core;
        private readonly SynchronizationContext _syncContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreViewModel"/> class.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> to wrap around.</param>
        public CoreViewModel(ICore core)
            : this(core, core.Registration)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreViewModel"/> class.
        /// </summary>
        /// <param name="core">The <see cref="ICore"/> to wrap around.</param>
        /// <param name="coreMetadata">The metadata that was used to construct this core instance.</param>
        public CoreViewModel(ICore core, CoreMetadata coreMetadata)
        {
            _syncContext = SynchronizationContext.Current;

            _core = core;

            DisplayName = coreMetadata.DisplayName;
            LogoUri = coreMetadata.LogoUri;

            CoreState = _core.CoreState;

            AbstractConfigPanel = new AbstractUICollectionViewModel(_core.AbstractConfigPanel);

            AttachEvents();
        }

        private void AttachEvents()
        {
            _core.CoreStateChanged += Core_CoreStateChanged;
            _core.InstanceDescriptorChanged += Core_InstanceDescriptorChanged;
            _core.AbstractConfigPanelChanged += OnAbstractConfigPanelChanged;
        }

        private void OnAbstractConfigPanelChanged(object sender, EventArgs e) => _syncContext.Post(_ =>
        {
            AbstractConfigPanel = new AbstractUICollectionViewModel(_core.AbstractConfigPanel);
            OnPropertyChanged(nameof(AbstractConfigPanel));
        }, null);

        private void DetachEvents()
        {
            _core.CoreStateChanged -= Core_CoreStateChanged;
            _core.InstanceDescriptorChanged -= Core_InstanceDescriptorChanged;
            _core.AbstractConfigPanelChanged -= OnAbstractConfigPanelChanged;
        }

        private void Core_InstanceDescriptorChanged(object sender, string e) => _syncContext.Post(_ =>
        {
            OnPropertyChanged(nameof(InstanceDescriptor));
            InstanceDescriptorChanged?.Invoke(sender, e);
        }, null);

        /// <inheritdoc cref="ICore.CoreState" />
        private void Core_CoreStateChanged(object sender, CoreState e)
        {
            CoreState = e;

            _syncContext.Post(_ =>
            {
                OnPropertyChanged(nameof(CoreState));

                OnPropertyChanged(nameof(IsCoreStateUnloaded));
                OnPropertyChanged(nameof(IsCoreStateConfiguring));
                OnPropertyChanged(nameof(IsCoreStateConfigured));
                OnPropertyChanged(nameof(IsCoreStateLoading));
                OnPropertyChanged(nameof(IsCoreStateLoaded));
            }, null);
        }

        /// <inheritdoc />
        public string InstanceId => _core.InstanceId;

        /// <inheritdoc />
        public string InstanceDescriptor => _core.InstanceDescriptor;

        /// <inheritdoc />
        AbstractUICollection ICore.AbstractConfigPanel => _core.AbstractConfigPanel;

        /// <inheritdoc cref="ICore.AbstractConfigPanel"/>
        public AbstractUICollectionViewModel AbstractConfigPanel { get; private set; }

        /// <inheritdoc />
        public MediaPlayerType PlaybackType => _core.PlaybackType;

        /// <summary>
        /// A local path or url pointing to a SVG file containing the logo for this core.
        /// </summary>
        public Uri LogoUri { get; }

        /// <summary>
        /// The user-friendly name of the core.
        /// </summary>
        public string DisplayName { get; }

        /// <inheritdoc cref="ICore.User" />
        public ICoreUser? User => _core.User;

        /// <inheritdoc cref="ICore.CoreState" />
        public CoreState CoreState { get; internal set; }

        /// <inheritdoc />
        public ICore SourceCore => _core.SourceCore;

        /// <summary>
        /// True when <see cref="CoreState"/> is <see cref="AppModels.CoreState.Unloaded"/>.
        /// </summary>
        public bool IsCoreStateUnloaded => CoreState == CoreState.Unloaded;

        /// <summary>
        /// True when <see cref="CoreState"/> is <see cref="AppModels.CoreState.NeedsConfiguration"/>.
        /// </summary>
        public bool IsCoreStateConfiguring => CoreState == CoreState.NeedsConfiguration;

        /// <summary>
        /// True when <see cref="CoreState"/> is <see cref="AppModels.CoreState.Configured"/>.
        /// </summary>
        public bool IsCoreStateConfigured => CoreState == CoreState.Configured;

        /// <summary>
        /// True when <see cref="CoreState"/> is <see cref="AppModels.CoreState.Loading"/>.
        /// </summary>
        public bool IsCoreStateLoading => CoreState == CoreState.Loading;

        /// <summary>
        /// True when <see cref="CoreState"/> is <see cref="AppModels.CoreState.Loaded"/>.
        /// </summary>
        public bool IsCoreStateLoaded => CoreState == CoreState.Loaded;

        /// <inheritdoc cref="ICore.CoreStateChanged" />
        public event EventHandler<CoreState>? CoreStateChanged
        {
            add => _core.CoreStateChanged += value;
            remove => _core.CoreStateChanged -= value;
        }

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreDevice>? DevicesChanged;

        /// <inheritdoc />
        public event EventHandler? AbstractConfigPanelChanged;

        /// <inheritdoc />
        public event EventHandler<string>? InstanceDescriptorChanged;

        /// <inheritdoc />
        IReadOnlyList<ICoreDevice> ICore.Devices => _core.Devices;

        /// <inheritdoc cref="ICore.Library" />
        ICoreLibrary ICore.Library => _core.Library;

        /// <inheritdoc />
        ICoreSearch? ICore.Search { get; }

        /// <inheritdoc cref="ICore.RecentlyPlayed" />
        ICoreRecentlyPlayed? ICore.RecentlyPlayed => _core.RecentlyPlayed;

        /// <inheritdoc cref="ICore.Discoverables" />
        ICoreDiscoverables? ICore.Discoverables => _core.Discoverables;

        /// <inheritdoc cref="ICore.Pins" />
        ICorePlayableCollectionGroup? ICore.Pins => _core.Pins;

        /// <inheritdoc cref="CoreMetadata"/>
        public CoreMetadata Registration => _core.Registration;

        /// <inheritdoc cref="IAsyncDisposable.DisposeAsync" />
        public async ValueTask DisposeAsync()
        {
            DetachEvents();

            await _core.DisposeAsync();
        }

        /// <inheritdoc/>
        public Task<ICoreModel?> GetContextByIdAsync(string id, CancellationToken cancellationToken = default) => _core.GetContextByIdAsync(id, cancellationToken);

        /// <inheritdoc />
        public Task<IMediaSourceConfig?> GetMediaSourceAsync(ICoreTrack track, CancellationToken cancellationToken = default) => _core.GetMediaSourceAsync(track, cancellationToken);

        /// <inheritdoc />
        public Task InitAsync(CancellationToken cancellationToken = default) => _core.InitAsync(cancellationToken);

        /// <inheritdoc />
        public bool IsInitialized => _core.IsInitialized;
    }
}
