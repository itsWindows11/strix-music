﻿using System;
using System.Collections.Generic;

namespace StrixMusic.CoreInterfaces.Interfaces
{
    /// <summary>
    /// Interface representing lyrics for a <see cref="ITrack"/>.
    /// </summary>
    public interface ILyrics
    {
        /// <summary>
        /// Timestamped lyrics. The Key is a point in the song, and the value is the lyric at that position.
        /// </summary>
        Dictionary<TimeSpan, string>? TimedLyrics { get; }

        /// <summary>
        /// A simple text wall containing all the lyrics for this song.
        /// </summary>
        string? TextLyrics { get; }

        /// <summary>
        /// The track that these lyrics belong to.
        /// </summary>
        ITrack Track { get; }
    }
}
