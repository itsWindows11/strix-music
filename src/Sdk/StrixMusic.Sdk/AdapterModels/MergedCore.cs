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
using OwlCore.Events;
using OwlCore.Extensions;
using StrixMusic.Sdk.AppModels;
using StrixMusic.Sdk.CoreModels;

namespace StrixMusic.Sdk.AdapterModels
{
    /// <summary>
    /// Aggregates many <see cref="ICore"/> instances into one instance.
    /// All data returned and emitted by this instance will contained merged data from the provided sources.
    /// </summary>
    public sealed class MergedCore : IStrixDataRoot, IMergedMutable<ICore>
    {
        private readonly List<ICore> _sources;
        private readonly List<IDevice> _devices;

        private readonly MergedLibrary _library;
        private MergedDiscoverables? _discoverables;
        private MergedPlayableCollectionGroup? _pins;
        private MergedRecentlyPlayed? _recentlyPlayed;
        private MergedSearch? _search;

        /// <summary>
        /// Initializes a new instance of <see cref="MergedCore"/>.
        /// </summary>
        public MergedCore(IEnumerable<ICore> cores, MergedCollectionConfig config)
        {
            _sources = cores.ToList();
            Guard.HasSizeGreaterThan(_sources, 0, nameof(_sources));

            foreach (var core in _sources)
                CheckSdkVersion(core.Registration);

            MergeConfig = config;

            _library = new MergedLibrary(_sources.Select(x => x.Library), config);

            // These items have no notification support for changing after construction,
            // so they need to be initialized every time in case a new source is added with a non-null value.
            if (_sources.Any(x => x.Discoverables != null))
                _discoverables = new MergedDiscoverables(_sources.Select(x => x.Discoverables).PruneNull(), config);

            if (_sources.Any(x => x.Pins != null))
                _pins = new MergedPlayableCollectionGroup(_sources.Select(x => x.Pins).PruneNull(), config);

            if (_sources.Any(x => x.RecentlyPlayed != null))
                _recentlyPlayed = new MergedRecentlyPlayed(_sources.Select(x => x.RecentlyPlayed).PruneNull(), config);

            if (_sources.Any(x => x.Search != null))
                _search = new MergedSearch(_sources.Select(x => x.Search).PruneNull(), config);

            _devices = new List<IDevice>(_sources.SelectMany(x => x.Devices, (_, device) => new DeviceAdapter(device)));

            foreach (var source in _sources)
                AttachEvents(source);
        }

        private void AttachEvents(ICore core)
        {
            core.DevicesChanged += Core_DevicesChanged;
        }

        private void DetachEvents(ICore core)
        {
            core.DevicesChanged -= Core_DevicesChanged;
        }

        /// <inheritdoc />
        public event CollectionChangedEventHandler<IDevice>? DevicesChanged;

        /// <inheritdoc cref="IMerged.SourcesChanged" />
        public event EventHandler? SourcesChanged;

        /// <inheritdoc />
        public event EventHandler<IPlayableCollectionGroup>? PinsChanged;

        /// <inheritdoc />
        public event EventHandler<ISearch>? SearchChanged;

        /// <inheritdoc />
        public event EventHandler<IRecentlyPlayed>? RecentlyPlayedChanged;

        /// <inheritdoc />
        public event EventHandler<IDiscoverables>? DiscoverablesChanged;

        private void Core_DevicesChanged(object sender, IReadOnlyList<CollectionChangedItem<ICoreDevice>> addedItems, IReadOnlyList<CollectionChangedItem<ICoreDevice>> removedItems)
        {
            var itemsToAdd = addedItems.Select(x => new CollectionChangedItem<IDevice>(new DeviceAdapter(x.Data), x.Index)).ToList();
            var itemsToRemove = removedItems.Select(x => new CollectionChangedItem<IDevice>(new DeviceAdapter(x.Data), x.Index)).ToList();

#warning TODO: Compute actual indices for merged device list.
            foreach (var item in itemsToRemove)
                _devices.RemoveAt(item.Index);

            foreach (var item in itemsToAdd)
                _devices.InsertOrAdd(item.Index, item.Data);

            DevicesChanged?.Invoke(this, itemsToAdd, itemsToRemove);
        }

        /// <inheritdoc cref="IMerged{T}.Sources"/>
        public IReadOnlyList<ICore> Sources => _sources;

        /// <inheritdoc />
        public MergedCollectionConfig MergeConfig { get; }

        /// <inheritdoc />
        public IReadOnlyList<IDevice> Devices => _devices;

        /// <inheritdoc />
        public ILibrary Library => _library;

        /// <inheritdoc />
        public IPlayableCollectionGroup? Pins => _pins;

        /// <inheritdoc />
        public ISearch? Search => _search;

        /// <inheritdoc />
        public IRecentlyPlayed? RecentlyPlayed => _recentlyPlayed;

        /// <inheritdoc />
        public IDiscoverables? Discoverables => _discoverables;

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            foreach (var core in _sources)
                await InitCore(core, cancellationToken);

            IsInitialized = true;
        }

        private async Task InitCore(ICore core, CancellationToken cancellationToken)
        {
        Begin:
            var setupCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (core.CoreState == CoreState.Unloaded || core.CoreState == CoreState.NeedsConfiguration)
            {
                try
                {
                    await core.InitAsync(setupCancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    if (!setupCancellationTokenSource.IsCancellationRequested)
                        setupCancellationTokenSource.Cancel();
                }
            }

            if (setupCancellationTokenSource.IsCancellationRequested || core.CoreState == CoreState.Unloaded)
            {
                setupCancellationTokenSource.Dispose();
                RemoveSource(core);
                return;
            }

            if (core.CoreState == CoreState.NeedsConfiguration)
            {
                // If the user needs to provide information for setup to continue, wait for another state change.
                // Display of the core's AbstractConfigPanel handled by the host application.
                await Flow.EventAsTask<CoreState>(cb => core.CoreStateChanged += cb, cb => core.CoreStateChanged -= cb, TimeSpan.FromMinutes(10));
                goto Begin;
            }

            setupCancellationTokenSource.Dispose();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Cores can be merged, but are never matched conditionally.
        /// </remarks>
        public bool Equals(ICore other) => false;

        /// <inheritdoc />
        public void AddSource(ICore itemToMerge)
        {
            if (_sources.Contains(itemToMerge))
                ThrowHelper.ThrowArgumentException(nameof(itemToMerge), "Cannot add the same source twice.");

            CheckSdkVersion(itemToMerge.Registration);

            _devices.AddRange(itemToMerge.Devices.Select(x => new DeviceAdapter(x)));
            _library.AddSource(itemToMerge.Library);

            if (itemToMerge.Discoverables is not null)
            {
                if (_discoverables is not null)
                {
                    _discoverables.AddSource(itemToMerge.Discoverables);
                }
                else
                {
                    _discoverables = new MergedDiscoverables(itemToMerge.Discoverables.IntoList(), MergeConfig);
                    DiscoverablesChanged?.Invoke(this, _discoverables);
                }
            }

            if (itemToMerge.RecentlyPlayed is not null)
            {
                if (_recentlyPlayed is not null)
                {
                    _recentlyPlayed.AddSource(itemToMerge.RecentlyPlayed);
                }
                else
                {
                    _recentlyPlayed = new MergedRecentlyPlayed(itemToMerge.RecentlyPlayed.IntoList(), MergeConfig);
                    RecentlyPlayedChanged?.Invoke(this, _recentlyPlayed);
                }
            }

            if (itemToMerge.Pins is not null)
            {
                if (_pins is not null)
                {
                    _pins.AddSource(itemToMerge.Pins);
                }
                else
                {
                    _pins = new MergedPlayableCollectionGroup(itemToMerge.Pins.IntoList(), MergeConfig);
                    PinsChanged?.Invoke(this, _pins);
                }
            }

            if (itemToMerge.Search is not null)
            {
                if (_search is not null)
                {
                    _search.AddSource(itemToMerge.Search);
                }
                else
                {
                    _search = new MergedSearch(itemToMerge.Search.IntoList(), MergeConfig);
                    SearchChanged?.Invoke(this, _search);
                }
            }

            _sources.Add(itemToMerge);
            AttachEvents(itemToMerge);

            SourcesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void RemoveSource(ICore itemToRemove)
        {
            if (!_sources.Contains(itemToRemove))
                ThrowHelper.ThrowArgumentException(nameof(itemToRemove), "Cannot remove an item that doesn't exist in the collection.");

            _devices.RemoveAll(x => itemToRemove.Devices.Any(y => y.Id == x.Id) && x.SourceCore?.InstanceId == itemToRemove.InstanceId);
            _library.RemoveSource(itemToRemove.Library);

            if (itemToRemove.Discoverables is not null)
            {
                Guard.IsNotNull(_discoverables);
                _discoverables.RemoveSource(itemToRemove.Discoverables);
            }

            if (itemToRemove.RecentlyPlayed is not null)
            {
                Guard.IsNotNull(_recentlyPlayed);
                _recentlyPlayed.RemoveSource(itemToRemove.RecentlyPlayed);
            }

            if (itemToRemove.Pins is not null)
            {
                Guard.IsNotNull(_pins);
                _pins.RemoveSource(itemToRemove.Pins);
            }

            if (itemToRemove.Search is not null)
            {
                Guard.IsNotNull(_search);
                _search.RemoveSource(itemToRemove.Search);
            }

            _sources.Remove(itemToRemove);
            DetachEvents(itemToRemove);
            SourcesChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (IsInitialized)
                return;

            foreach (var source in _sources)
            {
                DetachEvents(source);
                await source.DisposeAsync();
            }

            IsInitialized = false;
        }

        private void CheckSdkVersion(CoreMetadata coreMetadata)
        {
            var currentSdkVersion = typeof(ICore).Assembly.GetName().Version;
            if (coreMetadata.SdkVer != currentSdkVersion)
                throw new IncompatibleSdkVersionException(coreMetadata.SdkVer, coreMetadata.DisplayName);
        }
    }
}
