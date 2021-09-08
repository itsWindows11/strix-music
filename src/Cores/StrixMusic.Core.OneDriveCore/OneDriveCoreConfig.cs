﻿using StrixMusic.Core.LocalFiles;
using StrixMusic.Sdk.Data.Core;
using System;
using System.Collections.Generic;
using System.Text;
using OwlCore.AbstractUI.Models;
using StrixMusic.Core.OneDriveCore.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Diagnostics;
using OwlCore.AbstractStorage;
using StrixMusic.Core.OneDriveCore.Storage;
using StrixMusic.Sdk.Components;
using StrixMusic.Sdk.Components.Explorers;

namespace StrixMusic.Core.OneDriveCore
{
    ///  <inheritdoc/>
    public class OneDriveCoreConfig : LocalFilesCoreConfig
    {
        private AbstractTextBox _clientIdTb;
        private AbstractTextBox _tenantTb;
        private AbstractTextBox _redirectUriTb;

        /// <summary>
        /// Initializes a new instance of the <see cref="OneDriveCoreConfig"/> class.
        /// </summary>
        public OneDriveCoreConfig(ICore sourceCore)
            : base(sourceCore)
        {
            SetupAbstractUISettings();
        }

        public override Task SetupConfigurationServices(IServiceCollection services)
        {
            services.AddScoped(typeof(AuthenticationManager));
            services.AddScoped(typeof(OneDriveCoreStorageService));
            services.AddScoped(typeof(FolderExplorerUIHandler));
            services.AddScoped(x => new FolderExplorer(Services));

            Services = services.BuildServiceProvider();

            return Task.CompletedTask;
        }

        ///<inheritdoc/>
        public override void SetupAbstractUISettings()
        {
            _clientIdTb = new AbstractTextBox("ClientId", string.Empty)
            {
                PlaceholderText = "Enter client id here.",
            };

            _tenantTb = new AbstractTextBox("Tenant Id", string.Empty)
            {
                PlaceholderText = "Enter tenant id here.",
            };

            _redirectUriTb = new AbstractTextBox("Redirect Uri", string.Empty)
            {
                PlaceholderText = "Enter redirect uri (If Any).",
            };

            var startButton = new AbstractButton("Start", "Get Started");

            startButton.Clicked += StartButton_Clicked;

            AbstractUIElements = new List<AbstractUIElementGroup>
            {
                new AbstractUIElementGroup("SettingsGroup")
                {
                    Title="OneDrive Settings.",

                    Items = new List<AbstractUIElement>
                    {
                        _clientIdTb,
                        _tenantTb,
                        _redirectUriTb,
                        startButton
                    },
                }
            };
        }

        private async void StartButton_Clicked(object sender, EventArgs e)
        {
            Guard.IsNotNull(Services, "Services are null.");

            var authManager = Services.GetService<AuthenticationManager>();

            if (!string.IsNullOrWhiteSpace(_clientIdTb.Value) && !string.IsNullOrWhiteSpace(_tenantTb.Value))
            {
                Guard.IsNotNull(authManager, nameof(authManager));
                authManager.Init(_clientIdTb.Value, _tenantTb.Value, _redirectUriTb.Value);

                var client = await authManager.GenerateGraphToken();

                var oneDriveService = Services.GetService<OneDriveCoreStorageService>();
                Guard.IsNotNull(oneDriveService, nameof(oneDriveService));

                oneDriveService.Init(client);

                var rootFolder = await oneDriveService.GetRootFolderAsync();

                await InitFileExplorer(rootFolder, true);
            }
            else
            {
                // TODO: Show error.
            }
        }

        private Task InitFileExplorer(IFolderData folder, bool isRoot = false)
        {
            Guard.IsNotNull(Services, nameof(Services));

            var fileExplorerService = Services.GetService<FolderExplorer>();
            var folderExplorerUIHandler = Services.GetService<FolderExplorerUIHandler>();

            Guard.IsNotNull(fileExplorerService, nameof(fileExplorerService));

            _ = fileExplorerService.SetupFileExplorerAsync(folder, isRoot);

            Guard.IsNotNull(folderExplorerUIHandler, nameof(folderExplorerUIHandler));

            folderExplorerUIHandler.FolderExplorerUIUpdated += FolderExplorerUIHandler_FolderExplorerUIUpdated;

            return Task.CompletedTask;
        }

        private void FolderExplorerUIHandler_FolderExplorerUIUpdated(object sender, AbstractUIElementGroup e)
        {
            AbstractUIElements = new List<AbstractUIElementGroup>
            {
                e
            };

            AbstractUIElementChanged();
        }
    }
}
