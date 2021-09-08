﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractStorage;

namespace StrixMusic.Sdk.Components.Explorers
{
    /// <inheritdoc/>
    public class FolderExplorer : IFolderExplorer
    {
        private IServiceProvider _services;

        private FolderExplorerUIHandler? _folderExplorerUIHandler;

        ///<inheritdoc />
        public Stack<IFolderData> FolderStack { get; private set; }

        ///<inheritdoc />
        public IFolderData? PreviousFolder { get; private set; }

        ///<inheritdoc />
        public IFolderData? SelectedFolder { get; private set; }

        ///<inheritdoc />
        public IFolderData? CurrentFolder { get; private set; }

        ///<inheritdoc />
        public bool IsRootDirectory { get; private set; }

        ///<inheritdoc />
        public NavigationState NavigationState { get; private set; }

        ///<inheritdoc />
        public event EventHandler<IFolderData>? DirectoryChanged;

        /// <summary>
        /// Creates a new instance of <see cref="FolderExplorer"/>.
        /// </summary>
        /// <param name="services"></param>
        public FolderExplorer(IServiceProvider services)
        {
            _services = services;

            NavigationState = NavigationState.None;
            FolderStack = new Stack<IFolderData>();
        }

        ///<inheritdoc />
        public async Task SetupFileExplorerAsync(IFolderData folder, bool isRoot = false)
        {
            IsRootDirectory = isRoot;
            folder.IsRoot = IsRootDirectory;

            _folderExplorerUIHandler = _services.GetService<FolderExplorerUIHandler>();

            Guard.IsNotNull(_folderExplorerUIHandler, nameof(_folderExplorerUIHandler));

            if (NavigationState != NavigationState.Back)
                FolderStack.Push(folder);

            CurrentFolder = folder;

            var folders = await folder.GetFoldersAsync();

            _folderExplorerUIHandler.SetupFileExplorerUIComponents(folders, IsRootDirectory);

            _folderExplorerUIHandler.DirectoryChanged += FolderUIHandler_ItemTapped;
        }

        private async void FolderUIHandler_ItemTapped(object sender, NavigationEventArgs e)
        {
            Guard.IsNotNull(_folderExplorerUIHandler, nameof(_folderExplorerUIHandler));
            _folderExplorerUIHandler.DirectoryChanged -= FolderUIHandler_ItemTapped;

            if (e.BackNavigationOccurred)
            {
                FolderStack.Pop();
                PreviousFolder = FolderStack.FirstOrDefault();
                NavigationState = NavigationState.Back;

                Guard.IsNotNull(PreviousFolder, nameof(PreviousFolder));

                Guard.IsNotNull(PreviousFolder.IsRoot, nameof(PreviousFolder.IsRoot));

                await SetupFileExplorerAsync(PreviousFolder, PreviousFolder.IsRoot.Value);

                DirectoryChanged?.Invoke(this, PreviousFolder);
            }
            else
            {
                Guard.IsNotNull(e.TappedFolder, nameof(e.TappedFolder));

                NavigationState = NavigationState.Forward;

                Guard.IsNotNull(e.TappedFolder.IsRoot, nameof(e.TappedFolder.IsRoot));
                await SetupFileExplorerAsync(e.TappedFolder, e.TappedFolder.IsRoot.Value);

                DirectoryChanged?.Invoke(this, e.TappedFolder);
            }
        }
    }

}