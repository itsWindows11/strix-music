﻿using System.Collections.Generic;
using OwlCore.Extensions;
using StrixMusic.Sdk.Services;

namespace StrixMusic.Cores.LocalFiles.Services
{
    /// <inheritdoc />
    internal class LocalFilesCoreSettingsService : SettingsServiceBase
    {
        /// <summary>
        /// Creates a new instance of <see cref="LocalFilesCoreSettingsService"/>.
        /// </summary>
        /// <param name="instanceId">The ID of the current <see cref="LocalFilesCore"/> instance.</param>
        public LocalFilesCoreSettingsService(string instanceId)
        {
            Id = instanceId;
        }

        /// <inheritdoc />
        public override string Id { get; }

        /// <inheritdoc/>
        public override IEnumerable<SettingsKeysBase> SettingsKeys => new LocalFilesCoreSettingsKeys().IntoList();
    }
}