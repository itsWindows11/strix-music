﻿using System;

namespace StrixMusic.Sdk.Services
{
    /// <summary>
    /// Contains metadata for a registered core. Used to identify and instantiate a new core instance.
    /// </summary>
    public class CoreMetadata
    {
        /// <summary>
        /// Creates a new instance of <see cref="CoreMetadata"/>.
        /// </summary>
        /// <param name="id">A unique identifier for this core, including all instances.</param>
        /// <param name="displayName">The user-friendly name of the core.</param>
        /// <param name="logoUri">A local path or url pointing to a SVG file containing the logo for this core.</param>
        public CoreMetadata(string id, string displayName, Uri logoUri)
        {
            Id = id;
            LogoUri = logoUri;
            DisplayName = displayName;
        }

        /// <summary>
        /// A unique identifier for this registered core, regardless of instances.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// A local path or url pointing to a SVG file containing the logo for this core.
        /// </summary>
        public Uri LogoUri { get; }

        /// <summary>
        /// The user-friendly name of the core.
        /// </summary>
        public string DisplayName { get; }
    }
}