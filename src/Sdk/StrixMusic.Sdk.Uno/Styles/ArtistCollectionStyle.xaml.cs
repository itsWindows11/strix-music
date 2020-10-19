﻿using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using StrixMusic.Sdk.Services.Navigation;
using StrixMusic.Sdk.Uno.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace StrixMusic.Sdk.Uno.Styles
{
    public sealed partial class ArtistCollectionStyle : ResourceDictionary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtistCollectionStyle"/> class.
        /// </summary>
        public ArtistCollectionStyle()
        {
            this.InitializeComponent();
        }

        private void OpenArtist(object sender, ItemClickEventArgs e)
        {
            INavigationService<Control> navigationService = DefaultShellIoc.Ioc.GetService<INavigationService<Control>>();
            navigationService.NavigateTo(typeof(ArtistView), false, e.ClickedItem);
        }
    }
}