﻿using StrixMusic.Sdk.ViewModels;
using StrixMusic.Sdk.WinUI.Controls.Items.Abstract;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StrixMusic.Sdk.WinUI.Controls.Items
{
    /// <summary>
    /// A Templated <see cref="Control"/> for displaying an <see cref="AlbumViewModel"/> in a list.
    /// </summary>
    public partial class AlbumItem : ItemControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlbumItem"/> class.
        /// </summary>
        public AlbumItem()
        {
            this.DefaultStyleKey = typeof(AlbumItem);
            AttachEvents();
        }

        private void AttachEvents()
        {
            Loaded += AlbumItem_Loaded;
            Unloaded += AlbumItem_Unloaded;
        }

        private void DetachEvents()
        {
            Unloaded -= AlbumItem_Unloaded;
        }

        /// <summary>
        /// ViewModel holding the data for <see cref="AlbumItem" />
        /// </summary>
        public AlbumViewModel Album
        {
            get { return (AlbumViewModel)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        /// <summary>
        /// Dependency property for <ses cref="AlbumViewModel" />.
        /// </summary>
        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AlbumProperty =
            DependencyProperty.Register(nameof(Album), typeof(AlbumViewModel), typeof(AlbumItem), new PropertyMetadata(0));

        private void AlbumItem_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DetachEvents();
        }

        private void AlbumItem_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Loaded -= AlbumItem_Loaded;
        }
    }
}
