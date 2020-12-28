﻿using System.Collections.Generic;
using System.Linq;
using StrixMusic.Sdk.Services.Navigation;
using StrixMusic.Sdk.Uno.Controls;
using StrixMusic.Sdk.Uno.Services.Localization;
using StrixMusic.Shells.ZuneDesktop.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StrixMusic.Shells.ZuneDesktop.Controls
{
    /// <summary>
    /// The Settings page in the ZuneDesktop shell.
    /// </summary>
    public sealed partial class SettingsView : UserControl
    {
        private readonly INavigationService<Control> _navigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// </summary>
        public SettingsView()
        {
            this.InitializeComponent();

            _navigationService = Shell.Ioc.GetRequiredService<INavigationService<Control>>();

            var localizationService = Shell.Ioc.GetRequiredService<LocalizationResourceLoader>();

            _displayPages = _displayPages.Select(x => localizationService["StrixMusic.Shells.ZuneDesktop/ZuneSettings", x].ToUpper()).ToList();
        }

        private ZuneDesktopSettingsViewModel? ViewModel => DataContext as ZuneDesktopSettingsViewModel;

        /// <remarks>
        /// Translated in constructor.
        /// </remarks>
        private readonly IEnumerable<string> _displayPages = new string[]
        {
            "Background",
            "Scale",
        };

        private readonly List<string> _behaviorPages = new List<string>()
        {
            "MERGE BY",
        };

        private void SaveClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Save settings changes
            _navigationService.GoBack();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            //TODO: Revert unsaved changes
            _navigationService.GoBack();
        }

        private void NavigateToDisplay(object sender, RoutedEventArgs e)
        {
            PagesList.ItemsSource = _displayPages;
        }

        private void NavigateToBehavior(object sender, RoutedEventArgs e)
        {
            PagesList.ItemsSource = _behaviorPages;
        }
    }
}
