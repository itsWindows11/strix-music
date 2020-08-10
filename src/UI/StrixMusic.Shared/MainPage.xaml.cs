﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using StrixMusic.Helpers;
using StrixMusic.Services.Settings;
using StrixMusic.Services.StorageService;
using StrixMusic.Services.SuperShell;
using StrixMusic.Shell.Default.Controls;
using StrixMusic.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StrixMusic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        /// <summary>
        /// Fires when the Super buttons is clicked. Temporary until a proper trigger mechanism is found for touch devices.
        /// </summary>
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Ioc.Default.GetService<ISuperShellService>().Show(SuperShellDisplays.Settings);
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;
            Unloaded += MainPage_Unloaded;

            await Initialize();
            AttachEvents();

            // Events must be attached before initializing this.
            await Ioc.Default.GetService<IFileSystemService>().Init();
        }

        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            DetachEvents();
        }

        private void AttachEvents()
        {
            Ioc.Default.GetService<ISettingsService>().SettingChanged += SettingsService_SettingChanged;

            var fileSystemSvc = Ioc.Default.GetService<IFileSystemService>();

            fileSystemSvc.FolderScanStarted += FileSystemSvc_FolderScanStarted;
            fileSystemSvc.FolderDeepScanCompleted += FileSystemSvc_FolderDeepScanCompleted;
            fileSystemSvc.FileScanStarted += FileSystemSvc_FileScanStarted;
        }

        private void DetachEvents()
        {
            Unloaded -= MainPage_Unloaded;

            Ioc.Default.GetService<ISettingsService>().SettingChanged -= SettingsService_SettingChanged;

            var fileSystemSvc = Ioc.Default.GetService<IFileSystemService>();

            fileSystemSvc.FolderScanStarted -= FileSystemSvc_FolderScanStarted;
            fileSystemSvc.FolderDeepScanCompleted -= FileSystemSvc_FolderDeepScanCompleted;
            fileSystemSvc.FileScanStarted -= FileSystemSvc_FileScanStarted;
        }

        private void FileSystemSvc_FolderDeepScanCompleted(object sender, StrixMusic.CoreInterfaces.Interfaces.Storage.IFolderData e)
        {
#if !__ANDROID__
            Debug.WriteLine($"Deep scan of folder {e.Name} completed");
#endif
        }

        private void FileSystemSvc_FileScanStarted(object sender, FileScanStateEventArgs e)
        {
#if !__ANDROID__
            Debug.WriteLine($"Scanning file {e.FileData.Name}");
#endif
        }

        private void FileSystemSvc_FolderScanStarted(object sender, StrixMusic.CoreInterfaces.Interfaces.Storage.IFolderData e)
        {
#if !__ANDROID__
            Debug.WriteLine($"Scanning folder {e.Name}");
#endif
        }

        private async void SettingsService_SettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (e.Key == nameof(SettingsKeys.PreferredShell))
            {
                await SetupPreferredShell();
            }
        }

        private async Task Initialize()
        {
            // TODO: Remove or replace.
            await SetupPreferredShell();

            SuperShellDisplay.Content = new SuperShell();
        }

        private async Task SetupPreferredShell()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // Removes the current shell.
                ShellDisplay.Content = null;

                // Removes old resource(s).
                foreach (var dict in App.Current.Resources.MergedDictionaries)
                {
                    Match shellMatch = Regex.Match(dict.Source.AbsoluteUri, Constants.Shells.ShellResourceDictionaryRegex);
                    if (shellMatch.Success)
                    {
                        // Skips removing the default ResourceDictionary.
                        if (shellMatch.Groups[1].Value == Constants.Shells.DefaultShellName)
                            continue;

                        App.Current.Resources.MergedDictionaries.Remove(dict);
                        break;
                    }
                }

                // Gets the preferred shell from settings.
                var preferredShell = await Ioc.Default.GetService<ISettingsService>().GetValue<string>(nameof(SettingsKeys.PreferredShell));

                // Makes sure the saved shell is valid, falls back to Default.
                if (!Constants.Shells.LoadedShells.Contains(preferredShell))
                {
                    preferredShell = Constants.Shells.DefaultShellName;
                }

                if (preferredShell != Constants.Shells.DefaultShellName)
                {
                    // Loads the preferred shell
                    var resourcePath = $"{Constants.ResourcesPrefix}{Constants.Shells.ShellNamespacePrefix}.{preferredShell}/{Constants.Shells.ShellResourcesSuffix}";
                    var resourceDictionary = new ResourceDictionary() { Source = new Uri(resourcePath) };
                    App.Current.Resources.MergedDictionaries.Add(resourceDictionary);
                }

                ShellDisplay.Content = CreateShellControl();
            });
        }

        private ShellControl CreateShellControl()
        {
            ShellControl shell = new ShellControl
            {
                DataContext = Ioc.Default.GetService<MainViewModel>(),
            };
            return shell;
        }
    }
}
