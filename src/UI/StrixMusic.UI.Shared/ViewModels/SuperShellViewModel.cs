﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore;
using OwlCore.Provisos;
using StrixMusic.Sdk;
using StrixMusic.Sdk.Data;
using StrixMusic.Sdk.Data.Core;
using StrixMusic.Sdk.Services;
using StrixMusic.Sdk.ViewModels;

namespace StrixMusic.Shared.ViewModels
{
    /// <summary>
    /// ViewModel used by the <see cref="SuperShell"/>.
    /// </summary>
    public class SuperShellViewModel : ObservableObject, IAsyncInit, IDisposable
    {
        private readonly MainViewModel _mainViewModel;
        private readonly LoadedServicesItemViewModel _addNewItem;
        private readonly ICoreManagementService _coreManagementService;
        private bool _disposedValue;
        private bool _isShowingAddNew;
        private int _selectedTabIndex;
        private CoreViewModel? _currentCoreConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="SuperShellViewModel"/> class.
        /// </summary>
        public SuperShellViewModel()
        {
            _mainViewModel = Ioc.Default.GetRequiredService<MainViewModel>();
            _coreManagementService = Ioc.Default.GetRequiredService<ICoreManagementService>();

            Services = new ObservableCollection<LoadedServicesItemViewModel>();
            AvailableServices = new ObservableCollection<AvailableServicesItemViewModel>();

            // TODO nuke when switching to NavView for SuperShell.
            CancelAddNewCommand = new RelayCommand(() => IsShowingAddNew = false);
            CancelConfigCoreCommand = new RelayCommand(() => CurrentCoreConfig = null);

            foreach (var coreVm in _mainViewModel.Cores)
            {
                Services.Add(new LoadedServicesItemViewModel(false, coreVm));
            }

            _addNewItem = new LoadedServicesItemViewModel(true, null);
            Services.Add(_addNewItem);

            AttachEvents();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SuperShellViewModel"/> class.
        /// </summary>
        ~SuperShellViewModel()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public async Task InitAsync()
        {
            if (IsInitialized)
                return;

            IsInitialized = true;

            var availableCores = await _coreManagementService.GetCoreRegistryAsync();

            foreach (var coreAssemblyInfo in availableCores)
            {
                AvailableServices.Add(new AvailableServicesItemViewModel(coreAssemblyInfo));
            }

            foreach (var core in _mainViewModel.Cores)
                AttachEvents(core);
        }

        private void AttachEvents()
        {
            _addNewItem.NewItemRequested += AddNewItem_NewItemRequested;
            _mainViewModel.Cores.CollectionChanged += Cores_CollectionChanged;

            foreach(var loadedService in Services)
            {
                loadedService.ConfigRequested += LoadedService_ConfigRequested;
            }

            Guard.IsNotNull(CurrentWindow.AppFrame.ContentOverlay, nameof(CurrentWindow.AppFrame.ContentOverlay));
            CurrentWindow.AppFrame.ContentOverlay.Closed += ContentOverlay_Closed;
        }

        private void AttachEvents(ICore core)
        {
            core.CoreStateChanged += Core_CoreStateChanged;
        }

        private void DetachEvents()
        {
            _addNewItem.NewItemRequested -= AddNewItem_NewItemRequested;
            _mainViewModel.Cores.CollectionChanged -= Cores_CollectionChanged;

            foreach (var loadedService in Services)
            {
                loadedService.ConfigRequested += LoadedService_ConfigRequested;
            }

            Guard.IsNotNull(CurrentWindow.AppFrame.ContentOverlay, nameof(CurrentWindow.AppFrame.ContentOverlay));
            CurrentWindow.AppFrame.ContentOverlay.Closed -= ContentOverlay_Closed;
        }

        private void DetachEvents(ICore core)
        {
            core.CoreStateChanged -= Core_CoreStateChanged;
        }

        private void LoadedService_ConfigRequested(object sender, EventArgs e)
        {
            var viewModel = (LoadedServicesItemViewModel)sender;
            Guard.IsNotNull(viewModel.Core, nameof(viewModel.Core));

            CurrentCoreConfig = viewModel.Core;
        }

        private async void ContentOverlay_Closed(object sender, EventArgs e)
        {
            foreach (var core in _mainViewModel.Cores)
            {
                if (core.CoreState == CoreState.Unloaded || core.CoreState == CoreState.NeedsSetup)
                {
                    await core.DisposeAsync();
                }
            }

            IsShowingAddNew = false;
        }

        private void AddNewItem_NewItemRequested(object sender, EventArgs e)
        {
            IsShowingAddNew = true;
        }

        private async void Core_CoreStateChanged(object sender, CoreState e)
        {
            var core = (ICore)sender;

            var mainPage = Ioc.Default.GetRequiredService<MainPage>();
            var mainViewModel = Ioc.Default.GetRequiredService<MainViewModel>();
            var relevantCore = mainViewModel.Cores.First(x => x.InstanceId == core.InstanceId);

            if (e == CoreState.NeedsSetup)
            {
                _ = Threading.OnPrimaryThread(() => CurrentCoreConfig = relevantCore);
            }

            if (CurrentCoreConfig?.InstanceId != relevantCore.InstanceId)
                return;

            if (e == CoreState.Configured || e == CoreState.Unloaded)
            {
                await Threading.OnPrimaryThread(() => CurrentCoreConfig = null);
            }
        }

        private void Cores_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is CoreViewModel coreVm)
                    {
                        AttachEvents(coreVm);
                        Services.Insert(0, new LoadedServicesItemViewModel(false, coreVm));
                    }
                }

                IsShowingAddNew = false;
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CoreViewModel coreVm)
                    {
                        var serviceToRemove = Services.First(x => x.Core?.InstanceId == coreVm.InstanceId);
                        Services.Remove(serviceToRemove);
                    }
                }
            }
        }

        /// <summary>
        /// The services displayed in the app.
        /// </summary>
        public ObservableCollection<LoadedServicesItemViewModel> Services { get; }

        /// <summary>
        /// The services that are available to be added.
        /// </summary>
        public ObservableCollection<AvailableServicesItemViewModel> AvailableServices { get; }

        /// <summary>
        /// When fired, the user has canceled adding a new item.
        /// </summary>
        public IRelayCommand CancelAddNewCommand { get; }

        /// <summary>
        /// When fired, the user has canceled configuring a core.
        /// </summary>
        public IRelayCommand CancelConfigCoreCommand { get; }

        /// <summary>
        /// If true, the user has selected to add a new item and the UI should reflect this.
        /// </summary>
        public bool IsShowingAddNew
        {
            get => _isShowingAddNew;
            set => SetProperty(ref _isShowingAddNew, value);
        }

        /// <summary>
        /// If true, the view should display configuration
        /// </summary>
        public CoreViewModel? CurrentCoreConfig
        {
            get => _currentCoreConfig;
            set => SetProperty(ref _currentCoreConfig, value);
        }

        /// <summary>
        /// The index of the currently selected tab.
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        /// <inheritdoc/>
        public bool IsInitialized { get; private set; }

        /// <inheritdoc cref="Dispose"/>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;

                DetachEvents();

                foreach (var core in _mainViewModel.Cores)
                    DetachEvents(core);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}