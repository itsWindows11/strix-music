﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using StrixMusic.CoreInterfaces.Interfaces;
using StrixMusic.Services.Settings;

namespace StrixMusix.ViewModels
{
    /// <summary>
    /// The MainViewModel used throughout the app
    /// </summary>
    public class MainViewModel : ObservableRecipient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        /// <param name="service"></param>
        public MainViewModel(ISettingsService settings)
        {
        }

        /// <summary>
        /// A consolidated list of all users in the app
        /// </summary>
        public ObservableCollection<IUser> Users { get; }

        /// <summary>
        /// All available devices.
        /// </summary>
        public ObservableCollection<IDevice> Devices { get; }

        /// <summary>
        /// The consolidated music library across all cores.
        /// </summary>
        public IPlayableCollectionGroup Library { get; set; }

        /// <summary>
        /// The consolidated recently played items across all cores.
        /// </summary>
        public IPlayableCollectionGroup RecentlyPlayed { get; set; }

        /// <summary>
        /// Used to browse and discovered new music.
        /// </summary>
        public ObservableCollection<IPlayableCollectionGroup>? Discoverables { get; }
    }
}
