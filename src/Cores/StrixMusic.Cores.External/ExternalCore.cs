﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.Events;
using OwlCore.Extensions;
using OwlCore.Remoting;
using OwlCore.Remoting.Attributes;
using StrixMusic.Core.External.Models;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Extensions;
using StrixMusic.Sdk.MediaPlayback;

namespace StrixMusic.Core.External
{
    /// <inheritdoc />
    [RemoteOptions(RemotingDirection.Bidirectional)]
    public class ExternalCore : ICore
    {
        private static readonly ConcurrentDictionary<string, ExternalCore> _externalCoreInstances = new ConcurrentDictionary<string, ExternalCore>();
        private readonly MemberRemote _memberRemote;

        /// <summary>
        /// Creates a new instance of <see cref="ExternalCore"/>.
        /// </summary>
        /// <param name="instanceId"></param>
        public ExternalCore(string instanceId)
        {
            _externalCoreInstances.TryAdd(instanceId, this);

            InstanceId = instanceId;

            Devices = new List<ICoreDevice>();
            RecentlyPlayed = new ExternalCoreRecentlyPlayed(instanceId);
            Discoverables = new ExternalCoreDiscoverables(instanceId);
            Library = new ExternalCoreLibrary(instanceId);
            Pins = new ExternalCorePins(instanceId);

            CoreConfig = new ExternalCoreConfig(instanceId);

            _memberRemote = new MemberRemote(this, $"{instanceId}.{nameof(ExternalCore)}"); // finish implementing remote methods, props and locks
        }

        /// <summary>
        /// Gets a created <see cref="ExternalCore"/> instance by instance ID.
        /// </summary>
        /// <returns>The core instance.</returns>
        /// <exception cref="InvalidOperationException"/>
        public static ExternalCore GetInstance(string instanceId)
        {
            if (!_externalCoreInstances.TryGetValue(instanceId, out var value))
                return ThrowHelper.ThrowInvalidOperationException<ExternalCore>($"Could not find a registered {nameof(ExternalCore)} with {nameof(instanceId)} of {instanceId}");

            return value;
        }

        /// <inheritdoc/>
        public string InstanceId { get; }

        /// <inheritdoc/>
        public ICoreConfig CoreConfig { get; }

        /// <inheritdoc />
        public ICore SourceCore => this;

        /// <inheritdoc/>
        [RemoteProperty]
        public CoreState CoreState { get; internal set; } = CoreState.Unloaded;

        /// <inheritdoc />
        [RemoteProperty]
        public string InstanceDescriptor { get; private set; } = string.Empty;

        /// <inheritdoc/>
        [RemoteProperty]
        public ICoreUser? User { get; }

        /// <inheritdoc/>
        public IReadOnlyList<ICoreDevice> Devices { get; }

        /// <inheritdoc/>
        public ICoreLibrary Library { get; }

        /// <inheritdoc />
        public ICoreSearch? Search { get; }

        /// <inheritdoc/>
        public ICoreRecentlyPlayed? RecentlyPlayed { get; }

        /// <inheritdoc/>
        public ICoreDiscoverables? Discoverables { get; }

        /// <inheritdoc/>
        public ICorePlayableCollectionGroup? Pins { get; }

        /// <inheritdoc/>
        public event EventHandler<CoreState>? CoreStateChanged;

        /// <inheritdoc />
        public event CollectionChangedEventHandler<ICoreDevice>? DevicesChanged;

        /// <inheritdoc />
        public event EventHandler<string>? InstanceDescriptorChanged;

        /// <summary>
        /// Change the <see cref="CoreState"/>.
        /// </summary>
        /// <param name="state">The new state.</param>
        internal void ChangeCoreState(CoreState state)
        {
            CoreState = state;
            CoreStateChanged?.Invoke(this, state);
        }

        /// <inheritdoc/>
        public ValueTask DisposeAsync()
        {
            // Dispose any resources not known to the SDK.
            // Do not dispose Library, Devices, etc. manually. The SDK will dispose these for you.
            return default;
        }

        /// <inheritdoc/>
        public async Task InitAsync(IServiceCollection services)
        {
            // ==========================
            // Train of thought for later:
            // ==========================

            // Services - these are pre-injected and provided by the SDK.
            // Are they all needed? Should be provided remotely?

            // Any interface would need a Bidirectional-capable remote proxy class available
            // for all SDK implementors (same class used in client/host from SDK)

            // INotificationsService -- YES! 
            // ILocalizationService -- TBD.
            // IFileSystemService -- TBD.
            //  - For picking a file and keeping access.
            //  - Requires remoting all IFileData and IFolderData implementations

            // ---------------------------

            // Should we call InitAsync manually on every model?
            // Do we even need InitAsync on everything?
            // They should be able to call InitAsync when they need it, like when (/if) an item is retrieved from the API but needs extra init beyond the constructor (see MusicBrainz)
            // Their own API should determine if init async is even needed at all

            // answer:
            // The asynchronous nature of how items returned from an API + remote lock mean they always have the chance to call initasync and we always wait for async calls on their end
            // Remote properties means we don't care how or when it's called or what it even does
            // No.

            // ============
            // TODO:
            // Remove any manually added InitAsync from all ExternalCore models.
            // Remaining remoting implementation for ExternalCore models
            // Generic PlayableCollectionGroup implementation for e.g. RelatedItems
            // Set up two way remote proxy wrapper for INotificationsService.
            // ============
            await _memberRemote.RemoteWaitAsync(nameof(InitAsync));
        }

        /// <inheritdoc/>
        public async Task<ICoreMember?> GetContextById(string id)
        {
            return null;
        }

        /// <inheritdoc/>
        public Task<IMediaSourceConfig?> GetMediaSource(ICoreTrack track)
        {
            return null;
        }
    }
}
