﻿using StrixMusic.Sdk.Models;
using StrixMusic.Sdk.Models.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace StrixMusic.Sdk.MediaPlayback
{
    /// <summary>
    /// Holds <see cref="ICore"/> information as well as the <see cref="IMediaSourceConfig"/>.
    /// </summary>
    public record PlaybackItem
    {
        /// <summary>
        /// The media source to be played.
        /// </summary>
        public IMediaSourceConfig? MediaConfig { get; set; }

        /// <summary>
        /// The Core to which the track belongs.
        /// </summary>
        public ICore? SourceCore { get; }

        /// <summary>
        /// The track that holds information of from all merged source.
        /// </summary>
        public ITrack? Track { get; }
    }
}
