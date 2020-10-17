﻿using StrixMusic.Sdk;
using Windows.UI.Xaml.Controls;
using StrixMusic.Sdk.Core.ViewModels;

namespace StrixMusic.Shells.Controls
{
    /// <summary>
    /// A Templated <see cref="Control"/> for showing <see cref="SearchResultsViewModel"/> as a page.
    /// </summary>
    public sealed partial class SearchView : Control
    {
        private string _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchView"/> class.
        /// </summary>
        public SearchView(string query)
        {
            this.DefaultStyleKey = typeof(SearchView);
            _query = query;
            this.DataContextChanged += SearchView_DataContextChanged;
        }

        private async void SearchView_DataContextChanged(Windows.UI.Xaml.DependencyObject sender, Windows.UI.Xaml.DataContextChangedEventArgs args)
        {
            //await ViewModel!.GlobalSearchResultsAsync(_query);
        }

        /// <summary>
        /// The <see cref="MainViewModel"/> for the app.
        /// </summary>
        public MainViewModel? ViewModel => DataContext as MainViewModel;
    }
}
