﻿using StrixMusic.Sdk;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StrixMusic.Shells.ZuneDesktop.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QuickplayContent : UserControl
    {
        private bool _isSecondaryActive = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuickplayContent"/> class.
        /// </summary>
        public QuickplayContent()
        {
            this.InitializeComponent();
            _ = MainViewModel.Singleton?.Library?.PopulateMoreTracksCommand.ExecuteAsync(20);
            _ = MainViewModel.Singleton?.Library?.PopulateMoreAlbumsCommand.ExecuteAsync(20);
            _ = MainViewModel.Singleton?.Library?.PopulateMoreArtistsCommand.ExecuteAsync(20);
        }

        private MainViewModel? ViewModel => DataContext as MainViewModel;

        /// <summary>
        /// Runs any animations for when the <see cref="QuickplayContent"/> enters view.
        /// </summary>
        public void RunEnterViewAnimation()
        {
            LoadInView.Begin();
        }

        private void Rectangle_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (_isSecondaryActive)
            {
                VisualStateManager.GoToState(RootControl, "Main", true);
                VisualStateManager.GoToState(RootControl, "SecondaryHover", true);
            }
            else
            {
                VisualStateManager.GoToState(RootControl, "Secondary", true);
                VisualStateManager.GoToState(RootControl, "MainHover", true);
            }

            _isSecondaryActive = !_isSecondaryActive;
        }

        private void Rectangle_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_isSecondaryActive)
            {
                VisualStateManager.GoToState(RootControl, "SecondaryHover", true);
            }
            else
            {
                VisualStateManager.GoToState(RootControl, "MainHover", true);
            }
        }

        private void Rectangle_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (_isSecondaryActive)
            {
                VisualStateManager.GoToState(RootControl, "SecondaryNoHover", true);
            }
            else
            {
                VisualStateManager.GoToState(RootControl, "MainNoHover", true);
            }
        }
    }
}
