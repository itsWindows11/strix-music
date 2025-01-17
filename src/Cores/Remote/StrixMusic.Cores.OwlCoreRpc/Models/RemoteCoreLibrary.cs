﻿// Copyright (c) Arlo Godfrey. All Rights Reserved.
// Licensed under the GNU Lesser General Public License, Version 3.0 with additional terms.
// See the LICENSE, LICENSE.LESSER and LICENSE.ADDITIONAL files in the project root for more information.

using Newtonsoft.Json;
using StrixMusic.Sdk.CoreModels;

namespace StrixMusic.Sdk.Plugins.CoreRemote
{
    /// <summary>
    /// An external, remotely synchronized implementation of <see cref="ICoreLibrary"/>
    /// </summary>
    public sealed class RemoteCoreLibrary : RemoteCorePlayableCollectionGroupBase, ICoreLibrary
    {
        /// <summary>
        /// Creates a new instance of <see cref="RemoteCoreLibrary"/>. Interacts with a remote core, identified by the given parameters.
        /// </summary>
        /// <param name="sourceCoreInstanceId">The ID of the core that created this instance.</param>
        /// <param name="id">Uniquely identifies the instance being remoted.</param>
        [JsonConstructor]
        internal RemoteCoreLibrary(string sourceCoreInstanceId, string id)
            : base(sourceCoreInstanceId, id)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="RemoteCoreLibrary"/>. Wraps around the given <paramref name="library"/> for remote interaction.
        /// </summary>
        /// <param name="library">The library to control remotely.</param>
        internal RemoteCoreLibrary(ICoreLibrary library)
            : base(library)
        {
        }
    }
}
