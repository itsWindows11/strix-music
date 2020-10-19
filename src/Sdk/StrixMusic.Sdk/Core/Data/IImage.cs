﻿using System;

namespace StrixMusic.Sdk.Core.Data
{
    /// <summary>
    /// Contains details about an image.
    /// </summary>
    public interface IImage : ICoreMember
    {
        /// <summary>
        /// Local or remote resource pointing to the image.
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Height of the image.
        /// </summary>
        double Height { get; }

        /// <summary>
        /// Width of the image.
        /// </summary>
        double Width { get; }
    }
}