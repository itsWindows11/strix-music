﻿using Windows.UI.Xaml.Controls;

namespace StrixMusic.Sdk.Uno.Controls
{
    /// <summary>
    /// A Templated <see cref="Control"/> for the NowPlaying bar in a Shell.
    /// </summary>
    public sealed partial class NowPlayingBar : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NowPlayingBar"/> class.
        /// </summary>
        public NowPlayingBar()
        {
            this.DefaultStyleKey = typeof(NowPlayingBar);
        }
    }
}