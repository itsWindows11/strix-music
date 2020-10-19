﻿using Microsoft.Extensions.DependencyInjection;
using StrixMusic.Sdk.Services.Navigation;
using StrixMusic.Sdk.Uno.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace StrixMusic.Shells.Strix.Styles
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
            INavigationService<Control> navigationService = StrixShellIoc.Ioc.GetService<INavigationService<Control>>();
            navigationService.NavigateTo(typeof(ArtistView), false, e.ClickedItem);
        }
    }
}