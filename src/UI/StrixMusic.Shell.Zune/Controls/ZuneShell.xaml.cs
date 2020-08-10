﻿using StrixMusic.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace StrixMusic.Shell.Zune.Controls
{
    public sealed partial class ZuneShell : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ZuneShell"/> class.
        /// </summary>
        public ZuneShell()
        {
            this.InitializeComponent();
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RootControl.RequestedTheme = Pivot.SelectedIndex == 0 ? ElementTheme.Dark : ElementTheme.Light;
            Storyboard transition = Pivot.SelectedIndex == 0 ? LeaveLightTheme : EnterLightTheme;
            transition.Begin();
        }

        private MainViewModel? ViewModel => DataContext as MainViewModel;
    }
}