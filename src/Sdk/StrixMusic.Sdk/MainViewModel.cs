﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OwlCore.Extensions.AsyncExtensions;
using StrixMusic.Sdk.Core.Data;
using StrixMusic.Sdk.Core.Merged;
using StrixMusic.Sdk.Core.ViewModels;

namespace StrixMusic.Sdk
{
    /// <summary>
    /// The MainViewModel used throughout the app
    /// </summary>
    public partial class MainViewModel : ObservableRecipient
    {
        private readonly List<ICore> _cores = new List<ICore>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            Singleton = this;

            Devices = new ObservableCollection<DeviceViewModel>();
            SearchAutoComplete = new ObservableCollection<string>();

            Cores = new ObservableCollection<CoreViewModel>();
            Users = new ObservableCollection<UserProfileViewModel>();
            PlaybackQueue = new ObservableCollection<TrackViewModel>();

            GetSearchResultsAsyncCommand = new AsyncRelayCommand<string>(GlobalSearchResultsAsync);
            GetSearchAutoSuggestAsyncCommand = new RelayCommand<string>(GlobalSearchSuggestions);
        }

        /// <summary>
        /// Initializes and loads the cores given.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InitializeCores(IEnumerable<ICore> cores)
        {
            _cores.AddRange(cores);

            await Cores.InParallel(x => x.DisposeAsync().AsTask());

            // These collections should be empty, no matter how many times the method is called.
            Devices.Clear();
            SearchAutoComplete.Clear();
            Cores.Clear();
            Users.Clear();
            PlaybackQueue.Clear();

            await _cores.InParallel(async core =>
            {
                // Registers itself into Cores
                _ = new CoreViewModel(core);

                await core.InitAsync();

                Users.Add(new UserProfileViewModel(core.User));
            });

            var mergedLibrary = new MergedLibrary(_cores.Select(x => x.Library));
            Library = new LibraryViewModel(mergedLibrary);

            var mergedRecentlyPlayed = new MergedRecentlyPlayed(_cores.Select(x => x.RecentlyPlayed));
            RecentlyPlayed = new RecentlyPlayedViewModel(mergedRecentlyPlayed);

            var mergedDiscoverables = new MergedDiscoverables(_cores.Select(x => x.Discoverables));
            Discoverables = new DiscoverablesViewModel(mergedDiscoverables);
        }

        /// <summary>
        /// Gets search suggestions from all cores and asynchronously populate it into <see cref="SearchAutoComplete"/>.
        /// </summary>
        /// <param name="query">The query to search for.</param>
        public void GlobalSearchSuggestions(string query)
        {
            SearchAutoComplete.Clear();

            Parallel.ForEach(_cores, async core =>
            {
                await foreach (var item in core.GetSearchAutoCompleteAsync(query))
                {
                    if (!SearchAutoComplete.Contains(item))
                        SearchAutoComplete.Add(item);
                }
            });
        }

        /// <summary>
        /// Performs a search on all loaded cores, and loads it into <see cref="SearchResults"/>.
        /// </summary>
        /// <param name="query">The query to search for.</param>
        /// <returns>The merged search results.</returns>
        public async Task<ISearchResults> GlobalSearchResultsAsync(string query)
        {
            var searchResults = await _cores.InParallel(core => core.GetSearchResultsAsync(query));

            var merged = new MergedSearchResults(searchResults);

            SearchResults = new SearchResultsViewModel(merged);

            return merged;
        }

        /// <summary>
        /// The single instance of the <see cref="MainViewModel"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="MainViewModel"/> contains only logic for interacting with instance of cores, and relaying them to the UI.
        /// With how the settings are set up, creating more than one <see cref="MainViewModel"/> off the same instance IDs would result in a
        /// second instance of MainViewModel with identical states and functionality. Therefore, a singleton is desired over multi-instance.
        /// </remarks>
        public static MainViewModel? Singleton { get; private set; }

        /// <summary>
        /// Contains data about the cores that are loaded.
        /// </summary>
        public ObservableCollection<CoreViewModel> Cores { get; }

        /// <summary>
        /// A consolidated list of all users in the app.
        /// </summary>
        public ObservableCollection<UserProfileViewModel> Users { get; }

        /// <summary>
        /// All available devices.
        /// </summary>
        public ObservableCollection<DeviceViewModel> Devices { get; }

        /// <summary>
        /// The consolidated music library across all cores.
        /// </summary>
        public LibraryViewModel? Library { get; private set; }

        /// <summary>
        /// The consolidated recently played items across all cores.
        /// </summary>
        public RecentlyPlayedViewModel? RecentlyPlayed { get; private set; }

        /// <summary>
        /// Used to browse and discovered new music.
        /// </summary>
        public DiscoverablesViewModel? Discoverables { get; private set; }

        /// <summary>
        /// Gets search results for a query.
        /// </summary>
        public IAsyncRelayCommand<string> GetSearchResultsAsyncCommand { get; }

        /// <summary>
        /// Gets autocomplete suggestions for a search query.
        /// </summary>
        public IRelayCommand<string> GetSearchAutoSuggestAsyncCommand { get; }

        /// <summary>
        /// Contains search results.
        /// </summary>
        public SearchResultsViewModel? SearchResults { get; private set; }

        /// <summary>
        /// The autocomplete strings for the search results.
        /// </summary>
        public ObservableCollection<string> SearchAutoComplete { get; }

        /// <summary>
        /// The current playback queue. First item plays next.
        /// </summary>
        public ObservableCollection<TrackViewModel> PlaybackQueue { get; }
    }
}