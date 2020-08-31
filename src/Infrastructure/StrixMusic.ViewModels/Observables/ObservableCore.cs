﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using StrixMusic.CoreInterfaces;
using StrixMusic.CoreInterfaces.Interfaces;
using StrixMusic.CoreInterfaces.Interfaces.CoreConfig;

namespace StrixMusic.ViewModels.Bindables
{
    /// <summary>
    /// Contains information about a <see cref="ICore"/>
    /// </summary>
    public class ObservableCore : ObservableObject
    {
        private readonly ICore _core;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCore"/> class.
        /// </summary>
        /// <param name="core">The base <see cref="ICore"/></param>
        public ObservableCore(ICore core)
        {
            _core = core;

            Devices = new ObservableCollection<ObservableDevice>(_core.Devices.Select(x => new ObservableDevice(x)));
            Library = new ObservableLibrary(_core.Library);
            RecentlyPlayed = new ObservableRecentlyPlayed(_core.RecentlyPlayed);
            Discoverables = new ObservableCollectionGroup(_core.Discoverables);

            AttachEvents();
        }

        /// <inheritdoc cref="ICore.CoreStateChanged" />
        public event EventHandler<CoreState>? CoreStateChanged
        {
            add
            {
                _core.CoreStateChanged += value;
            }

            remove
            {
                _core.CoreStateChanged -= value;
            }
        }

        /// <inheritdoc cref="ICore.DevicesChanged" />
        public event EventHandler<CollectionChangedEventArgs<IDevice>>? DevicesChanged
        {
            add
            {
                _core.DevicesChanged += value;
            }

            remove
            {
                _core.DevicesChanged -= value;
            }
        }

        /// <inheritdoc cref="ICore.SearchAutoCompleteChanged" />
        private void AttachEvents()
        {
            _core.CoreStateChanged += Core_CoreStateChanged;
            _core.DevicesChanged += Core_DevicesChanged;
        }

        private void DetachEvents()
        {
            _core.CoreStateChanged -= Core_CoreStateChanged;
            _core.DevicesChanged -= Core_DevicesChanged;
        }

        private void Core_DevicesChanged(object sender, CollectionChangedEventArgs<IDevice> e)
        {
            foreach (var item in e.AddedItems)
            {
                Devices.Add(new ObservableDevice(item));
            }

            foreach (var item in e.RemovedItems)
            {
                Devices.Remove(new ObservableDevice(item));
            }
        }

        /// <inheritdoc cref="ICore.CoreState" />
        private void Core_CoreStateChanged(object sender, CoreState e) => CoreState = e;

        /// <inheritdoc cref="ICore.Devices"/>
        public ObservableCollection<ObservableDevice> Devices { get; }

        /// <inheritdoc cref="ICore.Library" />
        public ObservableLibrary Library { get; }

        /// <inheritdoc cref="ICore.RecentlyPlayed" />
        public ObservableRecentlyPlayed RecentlyPlayed { get; }

        /// <inheritdoc cref="ICore.Discoverables" />
        public ObservableCollectionGroup Discoverables { get; }

        /// <inheritdoc cref="ICore.GetSearchAutoCompleteAsync" />
        public Task<IAsyncEnumerable<string>> GetSearchAutoCompleteAsync(string query)
        {
            return _core.GetSearchAutoCompleteAsync(query);
        }

        /// <inheritdoc cref="ICore.GetSearchResultsAsync" />
        public Task<ISearchResults> GetSearchResultsAsync(string query)
        {
            return _core.GetSearchResultsAsync(query);
        }

        /// <inheritdoc cref="ICore.InitAsync" />
        public Task InitAsync()
        {
            return _core.InitAsync();
        }

        /// <inheritdoc cref="ICore.DisposeAsync" />
        public ValueTask DisposeAsync()
        {
            return _core.DisposeAsync();
        }

        /// <inheritdoc cref="ICore.CoreState" />
        public CoreState CoreState
        {
            get => _core.CoreState;
            set => SetProperty(() => _core.CoreState, value);
        }

        /// <inheritdoc cref="ICore.CoreConfig" />
        public ICoreConfig CoreConfig => _core.CoreConfig;

        /// <inheritdoc cref="ICore.Name" />
        public string Name => _core.Name;

        /// <inheritdoc cref="ICore.User" />
        public IUser User => _core.User;
    }
}